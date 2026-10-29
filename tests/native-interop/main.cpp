#include <windows.h>
#include <algorithm>
#include <cmath>
#include <cstring>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>
#include <cstdint>

template<class T> T symbol(HMODULE lib,const char *name) {
    auto p=GetProcAddress(lib,name); if(!p)throw std::runtime_error(name);return reinterpret_cast<T>(p);
}
struct Api {
    HMODULE library;
    const char *(__cdecl *error)();
    void (__cdecl *destroy)(void *);
    void (__cdecl *freeBuffer)(void *);
    uint64_t (__cdecl *bufferSize)(const void *);
    uint64_t (__cdecl *objects)();
    uint64_t (__cdecl *buffers)();
    void *(__cdecl *build)(int,double *,const char **);
    explicit Api(const wchar_t *folder) {
        SetDllDirectoryW(folder);
        library=LoadLibraryW((std::wstring(folder)+L"/erod.dll").c_str());
        if(!library)throw std::runtime_error("Could not load erod.dll");
        if(symbol<int(__cdecl*)()>(library,"erodGetApiVersion")()!=2)throw std::runtime_error("ABI mismatch");
        error=symbol<decltype(error)>(library,"erodGetLastError");destroy=symbol<decltype(destroy)>(library,"erodDestroyObject");
        freeBuffer=symbol<decltype(freeBuffer)>(library,"erodFree");bufferSize=symbol<decltype(bufferSize)>(library,"erodGetBufferSize");
        objects=symbol<decltype(objects)>(library,"erodGetOutstandingObjectCount");buffers=symbol<decltype(buffers)>(library,"erodGetOutstandingBufferCount");
        build=symbol<decltype(build)>(library,"erodElasticRodBuild");
    }
    void check() { if(error()[0])throw std::runtime_error(error()); }
};
struct Owner { Api &api;void *pointer;~Owner(){api.destroy(pointer);} };
void expect(bool condition,const char *message) { if(!condition)throw std::runtime_error(message); }

void run(Api &api,const std::wstring &mode) {
    double points[]={0,0,0,1,0,0,2,0,0};const char *error=nullptr;
    if(mode==L"error") {
        auto pointer=api.build(2,points,&error);
        expect(!pointer && error && std::strstr(error,"three points"),"Invalid constructor error contract");
        std::string copy(error);
        expect(copy==api.error(),"Stable error message was lost");
        std::cout<<"PASS stable constructor error: "<<copy<<std::endl;return;
    }
    if(mode==L"hardening") {
        double cases[][9]={{0,0,0,0,0,0,2,0,0}, {0,0,0,1,0,0,0,0,0}, {0,0,0,NAN,0,0,2,0,0}};
        for(auto &input:cases) {
            auto pointer=api.build(3,input,&error);
            expect(!pointer && error && std::strstr(error,"Native precondition failed"),"Geometry failure did not cross native error boundary safely");
            expect(api.objects()==0 && api.buffers()==0,"Failed constructor leaked resources");
        }
        Owner valid{api,api.build(3,points,&error)}; api.check();
        expect(valid.pointer!=nullptr,"Valid call failed after rejected constructors");
        std::cout<<"PASS collapsed, antiparallel and nonfinite geometry rejected through ABI; subsequent call succeeds"<<std::endl;
        return;
    }
    if(mode==L"materials") {
        auto buildGraph=symbol<void*(__cdecl*)(int,int,double*,int*,double*,int,int,int,const char**)>(api.library,"erodXShellBuildFromGraph");
        auto makeMaterial=symbol<void*(__cdecl*)(int,double,double,double*,int,int)>(api.library,"erodMaterialBuild");
        auto copyMaterial=symbol<void*(__cdecl*)(void*)>(api.library,"erodMaterialCopy");
        auto assign=symbol<void(__cdecl*)(void*const*,int,void*)>(api.library,"erodMaterialSetToLinkage");
        double xyz[]={0,0,0,-1,0,0,1,0,0,0,1,0,0,-1,0};
        int edges[]={0,1,0,2,0,3,0,4};double normals[]={0,0,1,0,0,1,0,0,1,0,0,1,0,0,1};
        Owner graph{api,buildGraph(5,4,xyz,edges,normals,8,0,1,&error)};api.check();
        double params[]={0.02,0.01};Owner first{api,makeMaterial(0,200000,0.3,params,2,0)};api.check();
        Owner second{api,copyMaterial(first.pointer)};api.check();
        void *inputs[]={first.pointer,second.pointer};
        assign(inputs,1,graph.pointer);api.check();
        expect(inputs[0]==first.pointer && inputs[1]==second.pointer,"Successful assignment modified input pointers");
        assign(inputs,2,graph.pointer);
        expect(std::strstr(api.error(),"Joint material count mismatch"),"Invalid joint count was not rejected");
        expect(inputs[0]==first.pointer && inputs[1]==second.pointer,"Failed assignment modified input pointers");
        assign(inputs,1,nullptr);
        expect(api.error()[0] && inputs[0]==first.pointer && inputs[1]==second.pointer,"Null-linkage failure modified inputs");
        assign(inputs,1,graph.pointer);api.check();
        std::cout<<"PASS material pointer inputs preserved on success and failure; assignment reusable"<<std::endl;
        return;
    }
    Owner rod{api,api.build(3,points,&error)};api.check();expect(rod.pointer!=nullptr,"No rod");
    auto material=symbol<void(__cdecl*)(void*,int,double,double,double*,int,int)>(api.library,"erodElasticRodSetMaterial");
    double section[]={0.02,0.01};material(rod.pointer,0,200000,0.3,section,2,0);api.check();
    if(mode==L"buffers") {
        auto dofs=symbol<void(__cdecl*)(void*,double**,int*)>(api.library,"erodElasticRodGetDoFs");
        auto mesh=symbol<void(__cdecl*)(void*,double**,int**,int*,int*)>(api.library,"erodElasticRodGetMeshData");
        struct Count{int value;int canary;};
        for(int iteration=0;iteration<250;++iteration) {
            Count n{0,0x12345678};double *values=nullptr;dofs(rod.pointer,&values,&n.value);api.check();
            expect(n.value==11 && n.canary==0x12345678 && api.bufferSize(values)==88,"DOF count/size mismatch");
            api.freeBuffer(values);
            Count nc{0,0x12345678},nq{0,0x12345678};double *xyz=nullptr;int *faces=nullptr;
            mesh(rod.pointer,&xyz,&faces,&nc.value,&nq.value);api.check();
            expect(nc.value==48 && nq.value==32 && nc.canary==0x12345678 && nq.canary==0x12345678,"Mesh count mismatch");
            api.freeBuffer(xyz);api.freeBuffer(faces);expect(api.buffers()==0,"Buffer leak");
        }
        std::cout<<"PASS 250 allocation/copy/free cycles and count canaries"<<std::endl;return;
    }
    if(mode==L"solver") {
        auto solve=symbol<int(__cdecl*)(void*,int,int,int,int*,double*,double,double,int,int,int,int,int,int,int,double**,const char**)>(api.library,"erodElasticRodNewtonSolver");
        int supports[]={0,1,2,3,4,5,6,7,8,9,10};double *report=nullptr;
        auto result=solve(rod.pointer,100,11,0,supports,nullptr,1e-8,1e-8,0,0,1,0,0,0,1,&report,&error);api.check();
        const auto size=api.bufferSize(report);expect(result==1 && size>=56 && size<4016 && (size/8-2)%5==0,"Report length incorrect");
        std::vector<double> copied(report,report+size/8);api.freeBuffer(report);
        expect(copied[0]==1,"Convergence status incorrect");
        std::cout<<"PASS early-convergence report uses actual length: "<<size<<" bytes"<<std::endl;return;
    }
    if(mode==L"bad-dofs") {
        double wrong[10]={};symbol<void(__cdecl*)(void*,double*,int)>(api.library,"erodElasticRodSetDoFs")(rod.pointer,wrong,10);
        expect(std::strstr(api.error(),"incorrect length")!=nullptr,"Missing boundary error");
        std::cout<<"PASS invalid DOFs return an error without an escaping C++ exception"<<std::endl;return;
    }
    if(mode==L"strain") {
        double values[]={0,0,0,1.125,0,0,2.25,0,0,0,0};
        auto set=symbol<void(__cdecl*)(void*,double*,int)>(api.library,"erodElasticRodSetDoFs");
        auto strain=symbol<double(__cdecl*)(void*)>(api.library,"erodElasticRodGetMaxStrain");
        set(rod.pointer,values,11);api.check();expect(std::abs(strain(rod.pointer)-0.125)<1e-10,"Fractional strain truncated");
        values[3]=0.875;values[6]=1.75;set(rod.pointer,values,11);api.check();expect(std::abs(strain(rod.pointer)+0.125)<1e-10,"Compressive strain truncated");
        std::cout<<"PASS positive and negative fractional strain"<<std::endl;return;
    }
    if(mode==L"lifetime") {
        auto copy=symbol<void*(__cdecl*)(void*,const char**)>(api.library,"erodElasticRodCopy");
        auto makeMaterial=symbol<void*(__cdecl*)(int,double,double,double*,int,int)>(api.library,"erodMaterialBuild");
        auto copyMaterial=symbol<void*(__cdecl*)(void*)>(api.library,"erodMaterialCopy");
        auto dofCount=symbol<int(__cdecl*)(void*)>(api.library,"erodElasticRodGetDoFCount");
        auto base=api.objects();
        for(int i=0;i<1000;++i) {
            void *r=copy(rod.pointer,&error);api.check();expect(r!=rod.pointer,"Copy aliased original");api.destroy(r);
            expect(dofCount(r)==-1 && api.error()[0],"Released handle was accepted");
            if(i%50==0) { void *m=makeMaterial(0,200000,0.3,section,2,0);api.check();void *m2=copyMaterial(m);api.check();
            api.destroy(m2);api.destroy(m); } expect(api.objects()==base,"Owned object leak");
        }
        std::cout<<"PASS 1000 rod and 20 material copy-and-destroy cycles; released handles rejected"<<std::endl;return;
    }
    throw std::runtime_error("Unknown test mode");
}
int wmain(int argc,wchar_t **argv) {
    if(argc!=3)return 2;
    try {
        Api api(argv[1]);run(api,argv[2]);
        expect(api.objects()==0 && api.buffers()==0,"Native objects or buffers left outstanding");
        std::cout<<"PASS zero outstanding owned objects and buffers"<<std::endl;
        return 0;
    } catch(const std::exception &error) { std::cerr<<"FAIL: "<<error.what()<<std::endl;return 1; }
}
