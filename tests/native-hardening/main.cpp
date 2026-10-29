#include "ElasticRod.hh"
#include "RodLinkage.hh"
#include <MeshFEM/Parallelism.hh>
#include <iostream>
#include <functional>

void rejected(const char *name, const std::function<void()> &f) {
    try { f(); } catch (const std::runtime_error &e) {
        if (std::string(e.what()).find("Native precondition failed:") == std::string::npos) throw;
        std::cout << "PASS " << name << ": " << e.what() << std::endl;
        return;
    }
    throw std::runtime_error(std::string("Expected recoverable failure: ") + name);
}
int main() {
    try {
        ElasticRod rod(std::vector<ElasticRod::Pt3>{{0,0,0},{1,0,0},{2,0,0}});
        rejected("rest edge index", [&] { rod.restEdgeVector(rod.numEdges()); });
        rejected("density index", [&] { rod.density(rod.numEdges()); });
        rejected("bending stiffness index", [&] { rod.bendingStiffness(rod.numVertices()); });
        rejected("twisting stiffness index", [&] { rod.twistingStiffness(rod.numVertices()); });
        rejected("collapsed centerline", [&] {
            rod.setRestConfiguration(std::vector<ElasticRod::Pt3>{{0,0,0},{0,0,0},{2,0,0}});
        });
        rejected("antiparallel centerline", [&] {
            rod.setRestConfiguration(std::vector<ElasticRod::Pt3>{{0,0,0},{1,0,0},{0,0,0}});
        });
        rejected("nonfinite centerline", [&] {
            rod.setRestConfiguration(std::vector<ElasticRod::Pt3>{{0,0,0},{NAN,0,0},{2,0,0}});
        });
        int evaluated=0;
        rejected("condition evaluated once in Release", [&] { EROD_REQUIRE(++evaluated == 2); });
        if(evaluated != 1) throw std::runtime_error("Condition evaluated incorrectly");
#ifdef MESHFEM_WITH_TBB
        rejected("parallel exception propagation", [&] {
            parallel_for_range(64, [](size_t i) { EROD_REQUIRE(i != 17); });
        });
#endif
        // A failed read-only check must not prevent a subsequent valid operation.
        if(rod.restEdgeVector(0).norm() != 1) throw std::runtime_error("Valid operation failed after rejection");
        std::cout << "PASS valid operation after rejected checks" << std::endl;
        return 0;
    } catch(const std::exception &e) { std::cerr << e.what() << std::endl; return 1; }
}
