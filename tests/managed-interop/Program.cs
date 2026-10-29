using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ErodDataLib.Types;
using ErodModelLib.Creators;
using ErodModelLib.Types;
using Rhino.Geometry;

internal static class Program
{
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] private static extern bool SetDllDirectory(string path);
    public static int Main(string[] args)
    {
        if(args.Length!=1)return 2;
        string binary=Path.GetFullPath(args[0]);
        string rhino=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Rhino 8","System");
        string grasshopper=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Rhino 8","Plug-ins","Grasshopper");
        AppDomain.CurrentDomain.AssemblyResolve+=(sender,e)=> {
            string name=new AssemblyName(e.Name).Name+".dll";
            foreach(string directory in new[]{binary,rhino,grasshopper}) {
                string path=Path.Combine(directory,name);if(File.Exists(path))return Assembly.LoadFrom(path);
            }
            return null;
        };
        SetDllDirectory(binary);
        try { Tests.Run();return 0; } catch(Exception error) { Console.Error.WriteLine(error);return 1; }
    }
}
internal static class Counters
{
    [DllImport("erod.dll",CallingConvention=CallingConvention.Cdecl,EntryPoint="erodGetOutstandingObjectCount")] internal static extern ulong Objects();
    [DllImport("erod.dll",CallingConvention=CallingConvention.Cdecl,EntryPoint="erodGetOutstandingBufferCount")] internal static extern ulong Buffers();
}
internal sealed class TestIO:ModelIO
{
    internal TestIO():base(ElasticModelType.ElasticRod) { }
    public override object Clone()=>new TestIO();
}
// Exercises the real ElasticModel ownership and NewtonSolver paths without
// creating Rhino-native meshes or requiring an interactive Rhino session.
internal sealed class TestRod:ElasticModel
{
    internal bool ThrowOnUpdate;
    internal TestRod():base(new TestIO())
    {
        IntPtr error;Handle=Kernel.ElasticRod.ErodElasticRodBuild(3,new double[]{0,0,0,1,0,0,2,0,0},out error);
        SetMaterial(0,200000,0.3,new[]{0.02,0.01},0);
    }
    private TestRod(TestRod other):base(new TestIO()) { IntPtr error;Handle=Kernel.ElasticRod.ErodElasticRodCopy(other.Handle,out error); }
    protected override void Init() { }
    public override void SetMaterial(int t,double e,double n,double[] p,int a)=>Kernel.ElasticRod.ErodElasticRodSetMaterial(Handle,t,e,n,p,p.Length,a);
    public override void InitMesh() { }
    protected override void GetMeshData(out double[] xyz,out int[] faces) { xyz=new double[0];faces=new int[0]; }
    public override void Update() { if(ThrowOnUpdate)throw new InvalidOperationException("Deliberate managed update failure"); }
    public override int GetDoFCount()=>Kernel.ElasticRod.ErodElasticRodGetDoFCount(Handle);
    public override object Clone()=>new TestRod(this);
    public override double GetEnergy()=>Kernel.ElasticRod.ErodElasticRodGetEnergy(Handle);
    public override double GetBendingEnergy()=>Kernel.ElasticRod.ErodElasticRodGetEnergyBend(Handle);
    public override double GetStretchingEnergy()=>Kernel.ElasticRod.ErodElasticRodGetEnergyStretch(Handle);
    public override double GetTwistingEnergy()=>Kernel.ElasticRod.ErodElasticRodGetEnergyTwist(Handle);
    public override double GetMaxStrain()=>Kernel.ElasticRod.ErodElasticRodGetMaxStrain(Handle);
    public override double[] GetForceVars(bool a=true,bool b=false)=>new double[0];
    public override int[] GetFixedVars(int n,int i,double step=1)=>new int[0];
    public override int[] GetFixedVars(bool temporary,double step=1)=>new int[0];
    public override Line[] GetCablesAsLines()=>new Line[0];
}
internal static class Tests
{
    private static int passed;
    private static void Expect(bool condition,string message) { if(!condition)throw new Exception(message); }
    private static void Throws<T>(Action action) where T:Exception { try { action(); } catch(T) { return; } throw new Exception("Expected "+typeof(T).Name); }
    private static void Collect() { for(int i=0;i<3;++i) { GC.Collect();GC.WaitForPendingFinalizers(); } }
    private static void Check(string name,Action action)
    {
        action();Collect();Expect(Counters.Objects()==0 && Counters.Buffers()==0,"Leak after "+name);
        ++passed;Console.WriteLine("PASS "+name+"; zero outstanding objects/buffers");
    }
    internal static NativeHandle Rod()
    {
        IntPtr error;return Kernel.ElasticRod.ErodElasticRodBuild(3,new double[]{0,0,0,1,0,0,2,0,0},out error);
    }
    private static NativeHandle Graph()
    {
        IntPtr error;
        return Kernel.RodLinkage.ErodXShellBuildFromGraph(5,4,
            new double[]{0,0,0,-1,0,0,1,0,0,0,1,0,0,-1,0},
            new[]{0,1,0,2,0,3,0,4},new double[]{0,0,1,0,0,1,0,0,1,0,0,1,0,0,1},8,0,1,out error);
    }
    [MethodImpl(MethodImplOptions.NoInlining)] private static NativeHandle OrphanedView()
    { var parent=Graph();return Kernel.RodSegment.ErodRodSegmentBuild(parent,0); }
    [MethodImpl(MethodImplOptions.NoInlining)] private static void OrphanedOwners()
    { for(int i=0;i<200;++i) { var handle=Rod();Expect(handle.IsAlive,"Invalid fresh owner"); } }
    private static double[] Stiffness(NativeHandle segment)
    {
        using(var scope=new NativeBufferScope()) {
            IntPtr a,b;int na,nb;Kernel.RodSegment.ErodRodSegmentGetBendingStiffness(segment,out a,out b,out na,out nb);
            var values=new double[na];NativeBufferScope.Copy(a,values,0,na);return values;
        }
    }
    private static double Area(NativeHandle linkage,int segment)
    {
        var view=new RodSegment(linkage,segment);
        double[] data;double[][] points;int[][] edges;view.GetEdgeMaterial(0,out data,out points,out edges);return data[0];
    }
    internal static void Run()
    {
        NativeApi.EnsureVersion();
        Check("invalid inputs and stable managed errors",()=> {
            IntPtr error;
            Throws<NativeInteropException>(()=>Kernel.ElasticRod.ErodElasticRodBuild(2,new double[6],out error));
            Throws<ArgumentException>(()=>Kernel.ElasticRod.ErodElasticRodBuild(3,new double[3],out error));
            using(var rod=Rod()) {
                Throws<NativeInteropException>(()=>Kernel.ElasticRod.ErodElasticRodSetDoFs(rod,new double[10],10));
                Throws<ArgumentException>(()=>Kernel.ElasticRod.ErodElasticRodSetDoFs(rod,new double[10],11));
            }
        });
        Check("zero handles are invalid",()=> {
            foreach(var type in new[]{typeof(ErodModelLib.Types.ElasticRod),typeof(Joint),typeof(RodSegment)}) {
                var value=(Grasshopper.Kernel.Types.IGH_Goo)FormatterServices.GetUninitializedObject(type);
                Expect(!value.IsValid,type.Name+" accepted zero pointer");
            }
            Expect(!default(Material).IsValid,"Default material accepted");
        });
        Check("exception-safe array transfers",()=> {
            using(var rod=Rod()) {
                Throws<InvalidOperationException>(()=> { using(var buffers=new NativeBufferScope()) {
                    IntPtr data;int count;Kernel.ElasticRod.ErodElasticRodGetDoFs(rod,out data,out count);
                    NativeBufferScope.Copy(data,new double[count+1],0,count+1);
                }});
                Expect(Counters.Buffers()==0,"Copy exception leaked buffer");
                Throws<InvalidOperationException>(()=> { using(var buffers=new NativeBufferScope()) {
                    IntPtr a,b;int na,nb;Kernel.ElasticRod.ErodElasticRodGetMeshData(rod,out a,out b,out na,out nb);
                    throw new InvalidOperationException("Deliberate post-transfer failure");
                }});
                Expect(Counters.Buffers()==0,"Multi-buffer exception leaked");
            }
        });
        Check("solver reports use actual sample counts",()=> {
            using(var rod=new TestRod()) {
                var options=new NewtonSolverOpts(100);options.WriteConvergenceReport=2;
                ConvergenceReport report;
                Expect(NewtonSolver.Optimize(rod,Enumerable.Range(0,11).ToArray(),new double[0],options,out report,false),"Rest state did not converge");
                Expect(report.SampleCount==1 && report.Iterations==0 && report.Energy.Length==1,"Wrong report sample count");
                rod.ThrowOnUpdate=true;
                Throws<InvalidOperationException>(()=>NewtonSolver.Optimize(rod,Enumerable.Range(0,11).ToArray(),new double[0],options,out report,true));
                Expect(Counters.Buffers()==0,"Managed update failure leaked report");
            }
        });
        Check("deep model duplication and disposal",()=> {
            using(var original=new TestRod())using(var duplicate=(TestRod)original.Duplicate()) {
                Expect(original.Model!=duplicate.Model,"Duplicate shares owned pointer");
                original.Dispose();Expect(!original.IsValid && duplicate.IsValid,"Dispose affected independent duplicate");
                Throws<ObjectDisposedException>(()=>original.GetDoFCount());Expect(duplicate.GetDoFCount()==11,"Duplicate is invalid");
            }
        });
        Check("material clones own separate native objects",()=> {
            using(var material=new Material(new MaterialIO(0,0,0.02,0.01,200000,0.3)))using(var copy=(Material)material.Duplicate()) {
                Expect(material.Model!=copy.Model,"Material copy aliased");material.Dispose();Expect(copy.IsValid,"Original disposal closed material clone");
            }
        });
        Check("borrowed views retain roots through GC",()=> {
            using(var child=OrphanedView()) { Collect();Expect(Kernel.RodSegment.ErodRodSegmentGetEdgesCount(child)>0,"Borrowed owner collected prematurely"); }
        });
        Check("parent disposal invalidates borrowed views safely",()=> {
            using(var parent=Graph())using(var child=Kernel.RodSegment.ErodRodSegmentBuild(parent,0)) {
                var view=new RodSegment(parent,0);var joint=new Joint(parent,0);
                Throws<Exception>(()=>view.GetEdgeMaterial(-1,out var a,out var b,out var c));
                parent.Dispose();Expect(!view.IsValid && !joint.IsValid,"Disposed parent still reported valid");
                Throws<ObjectDisposedException>(()=>Kernel.RodSegment.ErodRodSegmentGetEdgesCount(child));
                Throws<ObjectDisposedException>(()=>view.GetStartJoint());
                Throws<ObjectDisposedException>(()=>joint.GetAlpha());
            }
        });
        Check("SafeHandle finalizers release abandoned owners",()=> { OrphanedOwners(); });
        Check("dispose racing with native calls",()=> {
            for(int trial=0;trial<100;++trial) {
                var rod=Rod();var start=new ManualResetEventSlim(false);
                var worker=Task.Run(()=> { start.Wait();for(int i=0;i<100;++i) { try { Kernel.ElasticRod.ErodElasticRodGetDoFCount(rod); } catch(ObjectDisposedException) { break; } } });
                start.Set();rod.Dispose();worker.Wait();start.Dispose();
            }
        });
        Check("correct vector ABI",()=> { Expect(Kernel.RodLinkage.ErodXShellStripAutoDiff(new[]{1.0,2.0,3.0},new[]{4.0,5.0,6.0})==32.0,"Vector arguments were not marshalled correctly"); });
        Check("periodic ownership, buffers and cloning",()=> {
            IntPtr error;
            // A closed square plus the overlapping first edge, as required by PeriodicRod.
            using(var rod=Kernel.PeriodicRod.ErodPeriodicElasticRodBuild(6,new double[]{0,0,0,1,0,0,1,1,0,0,1,0,0,0,0,1,0,0},0,out error))
            using(var copy=Kernel.PeriodicRod.ErodPeriodicElasticRodCopy(rod,out error)) {
                Kernel.PeriodicRod.ErodPeriodicElasticRodSetMaterial(rod,0,200000,0.3,new[]{0.02,0.01},2,0);
                using(var scope=new NativeBufferScope()) {
                    IntPtr values;int count;Kernel.PeriodicRod.ErodPeriodicElasticRodGetDoFs(rod,out values,out count);
                    Expect(count==Kernel.PeriodicRod.ErodPeriodicElasticRodGetDoFCount(rod),"Periodic count mismatch");
                    var managed=new double[count];NativeBufferScope.Copy(values,managed,0,count);
                }
                rod.Dispose();Expect(Kernel.PeriodicRod.ErodPeriodicElasticRodGetDoFCount(copy)>0,"Periodic clone was invalidated");
            }
        });
        Check("asymmetric multi-box stiffening",()=> {
            using(var graph=Graph())using(var left=Kernel.RodSegment.ErodRodSegmentBuild(graph,0))
            using(var right=Kernel.RodSegment.ErodRodSegmentBuild(graph,1))using(var up=Kernel.RodSegment.ErodRodSegmentBuild(graph,2)) {
                Kernel.RodLinkage.ErodXShellSetMaterial(graph,0,200000,0.3,new[]{0.02,0.01},2,0);
                double l=Stiffness(left)[2],r=Stiffness(right)[2],u=Stiffness(up)[2];
                var boxes=new[]{new Box(Plane.WorldXY,new Interval(-1.2,-0.01),new Interval(-0.2,0.2),new Interval(-0.3,0.4)),
                                new Box(Plane.WorldXY,new Interval(0.01,1.2),new Interval(-0.2,0.2),new Interval(-0.3,0.4))};
                var packed=NativeChecks.BoxCoordinates(boxes);
                Expect(packed[0]==-1.2 && packed[1]==-0.2 && packed[2]==-0.3,"Box XYZ packing corrupted");
                Kernel.RodLinkage.ErodXShellSetStiffenRegions(graph,3,packed,2);
                Expect(Math.Abs(Stiffness(left)[2]/l-3)<1e-8,"First box failed");
                Expect(Math.Abs(Stiffness(right)[2]/r-3)<1e-8,"Second box stride failed");
                Expect(Math.Abs(Stiffness(up)[2]/u-1)<1e-8,"Unselected segment changed");
            }
        });
        Check("custom section interpolation fails safely",()=> {
            IntPtr error;
            using(var graph=Kernel.RodLinkage.ErodXShellBuildFromGraph(8,7,
                new double[]{0,0,0,2,0,0,-1,0,0,0,1,0,0,-1,0,3,0,0,2,1,0,2,-1,0},
                new[]{0,1,0,2,0,3,0,4,1,5,1,6,1,7},Enumerable.Range(0,8).SelectMany(i=>new[]{0.0,0.0,1.0}).ToArray(),8,0,1,out error)) {
                var contours=new double[]{-0.01,-0.005,0,0.01,-0.005,0,0.01,0.005,0,-0.01,0.005,0,
                    -0.02,-0.01,0,0.02,-0.01,0,0.02,0.01,0,-0.02,0.01,0};
                bool rejected=false;
                try { Kernel.RodLinkage.ErodXShellSetCustomJointMaterial(graph,2,new[]{0,0},new[]{200000.0,200000.0},new[]{0.3,0.3},contours,new[]{4,4},new double[0],new[]{0,0},new[]{0,0}); }
                catch(NativeInteropException ex) { rejected=ex.Message.Contains("cached cross-section"); }
                Expect(rejected,"Unsupported contour interpolation must report a managed error");
            }
        });
        Check("native geometry preconditions become managed errors",()=> {
            IntPtr error;
            foreach(var points in new[]{
                new double[]{0,0,0,0,0,0,2,0,0},
                new double[]{0,0,0,1,0,0,0,0,0}}) {
                Throws<NativeInteropException>(()=> {
                    using(var invalid=Kernel.ElasticRod.ErodElasticRodBuild(3,points,out error)) { }
                });
            }
            using(var rod=Rod()) {
                var bad=new double[]{0,0,0,0,0,0,2,0,0,0,0};
                Throws<NativeInteropException>(()=>Kernel.ElasticRod.ErodElasticRodSetDoFs(rod,bad,bad.Length));
                Expect(Kernel.ElasticRod.ErodElasticRodGetDoFCount(rod)==11,"Read failed after rejected mutation");
            }
        });
        Check("live solver callback timing, errors and unregister",()=> {
            using(var rod=new TestRod()) {
                var options=new NewtonSolverOpts(10); options.Verbose=false; options.WriteConvergenceReport=2;
                var lines=new System.Collections.Generic.List<string>(); bool returned=false, late=false;
                ConvergenceReport report;
                Expect(NewtonSolver.Optimize(rod,Enumerable.Range(0,11).ToArray(),new double[0],options,out report,
                    log: text=> { late |= returned; lines.Add(text); }, updateMesh:false),"Logged solve failed");
                returned=true;
                Expect(!late,"Report delivered after solver returned");
                Expect(lines.Count==report.SampleCount && lines.Count>0,"Live rows do not match native report samples");
                Expect(lines[0].Trim().Split('\t').Length==6,"Malformed progress row");
                int count=lines.Count;
                NewtonSolver.Optimize(rod,Enumerable.Range(0,11).ToArray(),new double[0],options,out report,false);
                Expect(lines.Count==count,"Callback remained registered after solve");
                Expect(NewtonSolver.Optimize(rod,Enumerable.Range(0,11).ToArray(),new double[0],options,out report,
                    log: text=> { throw new InvalidOperationException("Failed display sink"); }, updateMesh:false),"A logging error changed solver outcome");
                rod.ThrowOnUpdate=true;
                Throws<InvalidOperationException>(()=>NewtonSolver.Optimize(rod,Enumerable.Range(0,11).ToArray(),new double[0],options,out report,log: lines.Add));
                rod.ThrowOnUpdate=false;
                count=lines.Count;
                NewtonSolver.Optimize(rod,Enumerable.Range(0,11).ToArray(),new double[0],options,out report,false);
                Expect(lines.Count==count,"Callback survived failed managed update");
            }
        });
        Check("thread-local progress registrations stay isolated",()=> {
            // Existing native benchmarking/material state is shared. Keep native
            // solves serialized while BOTH thread-local log scopes are active.
            using(var first=new TestRod())using(var second=new TestRod())using(var ready=new Barrier(2)) {
                var rods=new[]{first,second};var gate=new object();
                var tasks=Enumerable.Range(0,2).Select(i=>Task.Run(()=> {
                    var options=new NewtonSolverOpts(10);options.Verbose=false;
                    int thread=Thread.CurrentThread.ManagedThreadId, rows=0;bool wrongThread=false;
                    using(var log=new NativeSolverLog(text=> { wrongThread |= Thread.CurrentThread.ManagedThreadId!=thread; rows++; GC.Collect(); })) {
                        if(!ready.SignalAndWait(TimeSpan.FromSeconds(10)))throw new Exception("Log registration barrier timed out");
                        lock(gate) {
                            ConvergenceReport report;
                            NewtonSolver.Optimize(rods[i],Enumerable.Range(0,11).ToArray(),new double[0],options,out report,false);
                        }
                    }
                    Expect(rows>0 && !wrongThread,"Progress routed to the wrong solver thread");
                })).ToArray();
                Task.WaitAll(tasks);
            }
        });
        Check("material assignment preserves input handles and copies values",()=> {
            using(var graph=Graph()) {
                using(var material=new Material(new MaterialIO(0,0,0.02,0.01,200000,0.3))) {
                    var handles=new[]{material.Handle};
                    Kernel.Material.ErodMaterialSetToLinkage(handles,1,graph);
                    Kernel.Material.ErodMaterialSetToLinkage(handles,1,graph);
                    Expect(Math.Abs(Kernel.Material.ErodMaterialGetArea(material.Handle)-0.0002)<1e-8,"Assignment invalidated input material");
                }
                Expect(Math.Abs(Area(graph,0)-0.0002)<1e-8,"Assigned material did not survive temporary disposal");
            }
            IntPtr error;
            using(var graph=Kernel.RodLinkage.ErodXShellBuildFromGraph(8,7,
                new double[]{0,0,0,2,0,0,-1,0,0,0,1,0,0,-1,0,3,0,0,2,1,0,2,-1,0},
                new[]{0,1,0,2,0,3,0,4,1,5,1,6,1,7},Enumerable.Range(0,8).SelectMany(i=>new[]{0.0,0.0,1.0}).ToArray(),8,0,1,out error)) {
                using(var first=new Material(new MaterialIO(0,0,0.02,0.01,200000,0.3)))
                using(var second=new Material(new MaterialIO(0,0,0.04,0.02,200000,0.3))) {
                    Kernel.Material.ErodMaterialSetToLinkage(new[]{first.Handle,second.Handle},2,graph);
                }
                Expect(Math.Abs(Area(graph,1)-0.0002)<1e-8,"First joint material incorrect");
                Expect(Math.Abs(Area(graph,4)-0.0008)<1e-8,"Second joint material incorrect");
            }
        });
        Console.WriteLine("PASS "+passed+" managed interop test groups");
    }
}
