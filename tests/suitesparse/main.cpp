// Exercise the actual MeshFEM wrappers against the bootstrapped SuiteSparse.
#include <MeshFEM/SparseMatrices.hh>
#include <cmath>
#include <iostream>
#include <type_traits>

static_assert(sizeof(SuiteSparse_long) == 8, "MeshFEM requires 64-bit sparse indices");
static_assert(std::is_same<SuiteSparse_long, int64_t>::value,
              "SuiteSparse 7 long API uses int64_t on both Windows and macOS");
using Matrix = TripletMatrix<Triplet<double>>;
void check(const std::vector<double> &x) {
    if (x.size() != 2 || std::abs(x[0]-1) > 1e-10 || std::abs(x[1]-2) > 1e-10)
        throw std::runtime_error("Incorrect sparse solve");
}
Matrix symmetric(double scale=1) {
    Matrix a(2,2);
    a.symmetry_mode=Matrix::SymmetryMode::UPPER_TRIANGLE;
    a.addNZ(0,0,4*scale); a.addNZ(0,1,scale); a.addNZ(1,1,3*scale);
    return a;
}
int main() {
    try {
        // Test both simplicial and BLAS-backed supernodal factorizations.
        for (bool supernodal : {false,true}) {
            auto a=symmetric(); CholmodFactorizer factor(a,supernodal,true,true);
            std::vector<double> x;
            factor.solve(std::vector<double>{6,7},x); check(x);
            auto updated=symmetric(2);
            factor.updateFactorization(updated);
            factor.solve(std::vector<double>{12,14},x); check(x);
        }
        bool rejected=false;
        try {
            Matrix a(2,2); a.addNZ(0,0,1); a.addNZ(1,1,-1);
            CholmodFactorizer factor(a,false,true,true); factor.factorize();
        } catch (const std::runtime_error &) { rejected=true; }
        if (!rejected) throw std::runtime_error("Indefinite LL factorization was accepted");
        // Nonsymmetric solve exercises the 64-bit UMFPACK API as well.
        for (int i=0;i<20;++i) {
            Matrix a(2,2); a.addNZ(0,0,3); a.addNZ(0,1,1);
            a.addNZ(1,0,2); a.addNZ(1,1,4);
            UmfpackFactorizer factor(a); std::vector<double> x;
            factor.solve(std::vector<double>{5,10},x); check(x);
        }
        std::cout << "PASS SuiteSparse " << SUITESPARSE_MAIN_VERSION << "."
                  << SUITESPARSE_SUB_VERSION << "." << SUITESPARSE_SUBSUB_VERSION
                  << ": CHOLMOD, supernodal BLAS, refactorization, failure handling, UMFPACK\n";
        return 0;
    } catch (const std::exception &e) { std::cerr << e.what() << '\n'; return 1; }
}
