#include <MeshFEM/newton_optimizer/ProgressLog.hh>
#include "RodLinkage.hh"
#include "restlen_solve.hh"
#include "compute_equilibrium.hh"
#include "open_linkage.hh"
#include "ElasticRod.hh"
#include "PeriodicRod.hh"
#include "infer_target_surface.hh"
#include "MeshFEM/Fields.hh"
#include "RodMaterial.hh"
#include "SurfaceAttractedLinkage.hh"
#include "ElasticRod.hh"
#include "CrossSectionStressAnalysis.hh"
#include "python_bindings/visualization.hh"

extern "C"
{
#include "erod.h"
}

#include "interop_support.hh"

namespace ElasticRodsGH
{
    EROD_API void erodSetSolverLogCallback(void (*callback)(const char *, void *), void *context) { erod_progress::setCallback(callback, context); }
    EROD_API int erodGetApiVersion() { return 2; }

    EROD_API void erodFree(void *buffer) { interop::release_buffer(buffer); }
    EROD_API const char *erodGetLastError() { return interop::error_storage(); }
    EROD_API void erodDestroyObject(void *object) { interop::destroy(object); }
    EROD_API uint64_t erodGetBufferSize(const void *buffer) {
        auto &r=interop::registry(); std::lock_guard<std::mutex> lock(r.mutex);
        auto found=r.buffers.find(buffer); return found==r.buffers.end() ? 0 : found->second;
    }
    EROD_API uint64_t erodGetOutstandingObjectCount() {
        auto &r=interop::registry(); std::lock_guard<std::mutex> lock(r.mutex);
        uint64_t count=0; for (const auto &entry:r.objects) if (entry.second.destroy) ++count; return count;
    }
    EROD_API uint64_t erodGetOutstandingBufferCount() {
        auto &r=interop::registry(); std::lock_guard<std::mutex> lock(r.mutex); return r.buffers.size();
    }
    EROD_API RodMaterial *erodMaterialCopy(RodMaterial *material) {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(material);

            auto result=[&]() -> RodMaterial * { return interop::own(new RodMaterial(*material));
            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return nullptr;
        }
    }


    void getConvergenceReport(ConvergenceReport report, double **outReport)
    {
        std::vector<double> flatReport;
        flatReport.push_back(static_cast<double>(report.success));
        flatReport.push_back(static_cast<double>(report.backtracking_failure));
        flatReport.insert(flatReport.end(), report.energy.begin(), report.energy.end());
        flatReport.insert(flatReport.end(), report.gradientNorm.begin(), report.gradientNorm.end());
        flatReport.insert(flatReport.end(), report.freeGradientNorm.begin(), report.freeGradientNorm.end());
        flatReport.insert(flatReport.end(), report.stepLength.begin(), report.stepLength.end());
        flatReport.insert(flatReport.end(), report.indefinite.begin(), report.indefinite.end());

        auto sizeReport = flatReport.size() * sizeof(double);//(numIterations * 5 + 2) * sizeof(double);
        *outReport = static_cast<double *>(interop::allocate(sizeReport));
        interop::copy(*outReport, flatReport.data(), sizeReport);
    }

    // AttractedLinkage
    EROD_API SurfaceAttractedLinkage *erodXShellAttractedSurfaceBuild(int numVertices, int numTrias, double *inCoords, int *inTrias, RodLinkage *linkage, double tgt_joint_weight, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numTrias<0) throw std::invalid_argument("Negative count: numTrias");
            interop::require(linkage);
            if (!std::isfinite(tgt_joint_weight)) throw std::invalid_argument("Non-finite parameter: tgt_joint_weight");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numVertices>0 && !inCoords) throw std::invalid_argument("Null input buffer: inCoords");
            if (numTrias>0 && !inTrias) throw std::invalid_argument("Null input buffer: inTrias");
            auto result=[&]() -> SurfaceAttractedLinkage * {
        try
        {
            Eigen::MatrixXd targetV(numVertices,3);
            for (int i = 0; i < numVertices; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    targetV(i,j) = inCoords[ 3*i+j ];
                }
            }

            Eigen::MatrixXi targetF(numTrias,3);
            for (int i = 0; i < numTrias; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    targetF(i,j) = inTrias[ 3*i+j ];
                }
            }

            const bool useCenterline = true;
            *errorMessage = "Rod Linkage Built";
            std::unique_ptr<SurfaceAttractedLinkage> attractedLinkage(new SurfaceAttractedLinkage(targetV, targetF, useCenterline, *linkage));
            attractedLinkage->set_attraction_tgt_joint_weight(tgt_joint_weight);
            return interop::own(attractedLinkage.release());
        }
        catch(const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    EROD_API SurfaceAttractedLinkage *erodXShellAttractedSurfaceCopy(SurfaceAttractedLinkage *linkage, const char **errorMessage){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            auto result=[&]() -> SurfaceAttractedLinkage * {
        try
        {
            *errorMessage = "Rod Linkage Copied";
            return interop::own(new SurfaceAttractedLinkage(*linkage));
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    // Target Surface
    EROD_API int erodXShellInferTargetSurface(RodLinkage *linkage, int nsubdiv, int numExtensionLayers, double **outCoords, int **outTrias, int *numCoords, int *numTrias, const char **errorMessage)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        if (outTrias) *outTrias = {};
        if (numCoords) *numCoords = {};
        if (numTrias) *numTrias = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (nsubdiv<0) throw std::invalid_argument("Negative count: nsubdiv");
            if (numExtensionLayers<0) throw std::invalid_argument("Negative count: numExtensionLayers");
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            if (!outTrias) throw std::invalid_argument("Missing output: outTrias");
            if (!numCoords) throw std::invalid_argument("Missing output: numCoords");
            if (!numTrias) throw std::invalid_argument("Missing output: numTrias");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            auto result=[&]() -> int {
        try
        {
            std::vector<MeshIO::IOVertex> vertices;
            std::vector<MeshIO::IOElement> trias;
            infer_target_surface(*linkage, vertices, trias, nsubdiv, numExtensionLayers);

            *numCoords = interop::checked_count(vertices.size() * 3);
            auto sizeCoords = (*numCoords) * sizeof(double);
            *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
            std::vector<double> coords;
            for (size_t i = 0; i < vertices.size(); i++)
            {
                auto v = vertices[i].point;
                coords.push_back(v[0]);
                coords.push_back(v[1]);
                coords.push_back(v[2]);
            }
            interop::copy(*outCoords, coords.data(), sizeCoords);

            *numTrias = interop::checked_count(trias.size() * 3);
            auto sizeTrias = (*numTrias) * sizeof(int);
            *outTrias = static_cast<int *>(interop::allocate(sizeTrias));
            std::vector<int> triasIdx;
            for (size_t i = 0; i < trias.size(); i++)
            {
                auto q = trias[i];
                triasIdx.push_back(q[0]);
                triasIdx.push_back(q[1]);
                triasIdx.push_back(q[2]);
            }
            interop::copy(*outTrias, triasIdx.data(), sizeTrias);

            *errorMessage = "";
            return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return 1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return 1;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return 1;
        }

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
        if (outTrias) *outTrias = {};
        if (numCoords) *numCoords = {};
        if (numTrias) *numTrias = {};
            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};
        if (outTrias) *outTrias = {};
        if (numCoords) *numCoords = {};
        if (numTrias) *numTrias = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};
        if (outTrias) *outTrias = {};
        if (numCoords) *numCoords = {};
        if (numTrias) *numTrias = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        }
    }

    // Linkage
    EROD_API RodLinkage *erodXShellBuildFromGraph(int numVertices, int numEdges, double *inCoords, int *inEdges, double *inNormals, int subdivision, int interleavingType, int initConsistentAngle, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numEdges<0) throw std::invalid_argument("Negative count: numEdges");
            if (subdivision<0) throw std::invalid_argument("Negative count: subdivision");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numVertices>0 && !inNormals) throw std::invalid_argument("Null input buffer: inNormals");
            if (numVertices>0 && !inCoords) throw std::invalid_argument("Null input buffer: inCoords");
            if (numEdges>0 && !inEdges) throw std::invalid_argument("Null input buffer: inEdges");
            if(subdivision<2) throw std::invalid_argument("A segment needs at least two edges");
            auto result=[&]() -> RodLinkage * {
        try
        {
            // Vertices
            std::vector<MeshIO::IOVertex> vertices;
            std::vector<RodLinkage::Vec3> normals;
            for (int i = 0; i < numVertices; ++i)
            {
                vertices.emplace_back(inCoords[3 * i], inCoords[3 * i + 1], inCoords[3 * i + 2]);
                normals.emplace_back(inNormals[3 * i], inNormals[3 * i + 1], inNormals[3 * i + 2]);
            }

            // Edges
            std::vector<MeshIO::IOElement> edges;
            for (int i = 0; i < numEdges; ++i)
            {
                edges.emplace_back(inEdges[2 * i], inEdges[2 * i + 1]);
            }

            InterleavingType rod_interleaving_type;
            bool consistentAngle = initConsistentAngle == 1;
            switch (interleavingType)
            {
            case 0:
                rod_interleaving_type = InterleavingType::xshell;
                break;
            case 1:
                rod_interleaving_type = InterleavingType::weaving;
                consistentAngle = false;
                break;
            case 2:
                rod_interleaving_type = InterleavingType::noOffset;
                break;
            case 3:
                rod_interleaving_type = InterleavingType::triaxialWeave;
                consistentAngle = false;
                break;
            default:
                rod_interleaving_type = InterleavingType::noOffset;
            }

            *errorMessage = "Rod Linkage Built";

            // Edge callbacks
            std::vector<std::function<RodLinkage::Pt3(Real, bool)>> edge_callbacks = {};

            return interop::own(new RodLinkage(vertices, edges, subdivision, consistentAngle, rod_interleaving_type, edge_callbacks, normals));
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    EROD_API RodLinkage *erodXShellCopy(RodLinkage *linkage, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            auto result=[&]() -> RodLinkage * {
        try
        {
            *errorMessage = "Rod Linkage Copied";
            return interop::own(new RodLinkage(*linkage));
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    EROD_API RodLinkage *erodXShellBuild(int numVertices, int numEdges, double *inCoords, int *inEdges, double *inNormals,
                                                      double *inRestLengths, int *inOffsetInteriorCoords, double *inInteriorCoords,
                                                      int interleavingType, int initConsistentAngle, int initConsistentNormals, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numEdges<0) throw std::invalid_argument("Negative count: numEdges");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numVertices>0 && !inNormals) throw std::invalid_argument("Null input buffer: inNormals");
            if (numVertices>0 && !inCoords) throw std::invalid_argument("Null input buffer: inCoords");
            if (numEdges>0 && !inEdges) throw std::invalid_argument("Null input buffer: inEdges");
            if (numEdges>0 && !inOffsetInteriorCoords) throw std::invalid_argument("Null input buffer: inOffsetInteriorCoords");
            if (numEdges>0 && !inRestLengths) throw std::invalid_argument("Null input buffer: inRestLengths");
            if(numEdges>0 && inOffsetInteriorCoords[numEdges-1]>0 && !inInteriorCoords) throw std::invalid_argument("Null interior coordinates");
            for(int i=0, previous=0;i<numEdges;previous=inOffsetInteriorCoords[i++]) if(inOffsetInteriorCoords[i]<previous || (inOffsetInteriorCoords[i]-previous)%3!=0) throw std::invalid_argument("Invalid interior offsets");
            auto result=[&]() -> RodLinkage * {
        try
        {
            // Vertices
            std::vector<MeshIO::IOVertex> vertices;
            std::vector<RodLinkage::Vec3> normals;
            for (int i = 0; i < numVertices; ++i)
            {
                vertices.emplace_back(inCoords[3 * i], inCoords[3 * i + 1], inCoords[3 * i + 2]);
                normals.emplace_back(inNormals[3 * i], inNormals[3 * i + 1], inNormals[3 * i + 2]);
            }

            InterleavingType rod_interleaving_type;
            bool consistentAngle = initConsistentAngle == 1;
            switch (interleavingType)
            {
            case 0:
                rod_interleaving_type = InterleavingType::xshell;
                break;
            case 1:
                rod_interleaving_type = InterleavingType::weaving;
                consistentAngle = false;
                break;
            case 2:
                rod_interleaving_type = InterleavingType::noOffset;
                break;
            case 3:
                rod_interleaving_type = InterleavingType::triaxialWeave;
                consistentAngle = false;
                break;
            default:
                rod_interleaving_type = InterleavingType::noOffset;
            }

            // Edges
            RodLinkage::VecX restLengths(numEdges);
            std::vector<MeshIO::IOElement> edges;
            std::vector<std::vector<RodLinkage::Pt3>> rodPoints(numEdges);
            int startOff = 0;

            for (int i = 0; i < numEdges; i++)
            {
                restLengths[i] = inRestLengths[i];
                edges.emplace_back(inEdges[2 * i], inEdges[2 * i + 1]);

                std::vector<RodLinkage::Pt3> pts;
                int endOff = inOffsetInteriorCoords[i];
                int numInteriorPoints = (endOff - startOff) / 3;

                for (int j = 0; j < numInteriorPoints; j++) pts.emplace_back(inInteriorCoords[startOff + 3 * j], inInteriorCoords[startOff + 3 * j + 1], inInteriorCoords[startOff + 3 * j + 2]);
                rodPoints[i] = pts;

                startOff = endOff;
            }

            *errorMessage = "Rod Linkage Built";
            return interop::own(new RodLinkage(vertices, normals, edges, rodPoints, restLengths, initConsistentAngle, initConsistentNormals, rod_interleaving_type));
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    EROD_API void erodXShellSetCustomMaterial(RodLinkage *linkage, double E, double nu, double *inCoords, int numVertices, double *inHolesCoords, int numHoles, int axisType)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numHoles<0) throw std::invalid_argument("Negative count: numHoles");
            if (!std::isfinite(E)) throw std::invalid_argument("Non-finite parameter: E");
            if (!std::isfinite(nu)) throw std::invalid_argument("Non-finite parameter: nu");
            if (numVertices>0 && !inCoords) throw std::invalid_argument("Null input buffer: inCoords");
            if (numHoles>0 && !inHolesCoords) throw std::invalid_argument("Null input buffer: inHolesCoords");
            if(numVertices<3) throw std::invalid_argument("A section needs at least three vertices");
            [&]() -> void {
        std::vector<MeshIO::IOVertex> vertices;
        vertices.reserve(numVertices);
        std::vector<MeshIO::IOElement> lines;
        lines.reserve(numVertices);

        for (int i = 0; i < numVertices; i++)
        {
            vertices.emplace_back(inCoords[3 * i], inCoords[3 * i + 1], inCoords[3 * i + 2]);
            int next = i + 1;
            if (i == numVertices - 1)
                next = 0;
            lines.emplace_back(i, next);
        }

        std::vector<Point2D> holesPts;
        holesPts.reserve(numHoles);
        for (int i = 0; i < numHoles; i++) holesPts.emplace_back(inHolesCoords[2 * i], inHolesCoords[2 * i + 1]);

        RodMaterial::StiffAxis stiffAxis;
        switch (axisType)
        {
            case 0:
                stiffAxis = RodMaterial::StiffAxis::D1;
                break;
            case 1:
                stiffAxis = RodMaterial::StiffAxis::D2;
                break;
            default:
                stiffAxis = RodMaterial::StiffAxis::D1;
        }
        bool keepCrossSectionMesh = true;

        RodMaterial mat;
        mat.setContourGH(E, nu, vertices, lines, holesPts, stiffAxis, keepCrossSectionMesh);

        linkage->setMaterial(mat);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellSetMaterial(RodLinkage *linkage, int sectionType, double E, double nu, double *params, int numParams, int axisType)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numParams<0) throw std::invalid_argument("Negative count: numParams");
            if (!std::isfinite(E)) throw std::invalid_argument("Non-finite parameter: E");
            if (!std::isfinite(nu)) throw std::invalid_argument("Non-finite parameter: nu");
            if (numParams>0 && !params) throw std::invalid_argument("Null input buffer: params");
            [&]() -> void {
        std::string type;
        switch (sectionType)
        {
        case 0:
            type = "RECTANGLE";
            break;
        case 1:
            type = "ELLIPSE";
            break;
        case 2:
            type = "I";
            break;
        case 3:
            type = "L";
            break;
        case 4:
            type = "+";
            break;
        default:
            type = "RECTANGLE";
        }

        std::vector<Real> inParams;
        inParams.reserve(numParams);
        for (int i = 0; i < numParams; i++) inParams.push_back(params[i]);

        RodMaterial::StiffAxis stiffAxis;
        switch (axisType)
        {
        case 0:
            stiffAxis = RodMaterial::StiffAxis::D1;
            break;
        case 1:
            stiffAxis = RodMaterial::StiffAxis::D2;
            break;
        default:
            stiffAxis = RodMaterial::StiffAxis::D1;
        }
        bool keepCrossSectionMesh = true;

        const RodMaterial mat(type, E, nu, inParams, stiffAxis, keepCrossSectionMesh);
        linkage->setMaterial(mat);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellSetJointMaterial(RodLinkage *linkage, int numMaterials, int *sectionType, double *E, double *nu, double *sectionParams, int *sectionParamsCount, int *axisType)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numMaterials<0) throw std::invalid_argument("Negative count: numMaterials");
            if (numMaterials>0 && !sectionType) throw std::invalid_argument("Null input buffer: sectionType");
            if (numMaterials>0 && !sectionParamsCount) throw std::invalid_argument("Null input buffer: sectionParamsCount");
            if (numMaterials>0 && !axisType) throw std::invalid_argument("Null input buffer: axisType");
            if (numMaterials>0 && !nu) throw std::invalid_argument("Null input buffer: nu");
            if (numMaterials>0 && !sectionParams) throw std::invalid_argument("Null input buffer: sectionParams");
            if (numMaterials>0 && !E) throw std::invalid_argument("Null input buffer: E");
            [&]() -> void {
        std::vector<RodMaterial> materials;
        materials.reserve(numMaterials);

        size_t idx = 0;
        for (size_t i = 0; i < numMaterials; i++)
        {
            std::string type;
            switch (sectionType[i])
            {
                case 0:
                    type = "RECTANGLE";
                    break;
                case 1:
                    type = "ELLIPSE";
                    break;
                case 2:
                    type = "I";
                    break;
                case 3:
                    type = "L";
                    break;
                case 4:
                    type = "+";
                    break;
                default:
                    type = "RECTANGLE";
            }

            auto paramCount = sectionParamsCount[i];
            std::vector<Real> params;
            params.reserve(paramCount);
            for (int j = 0; j < paramCount; j++) params.push_back(sectionParams[idx + j]);
            idx += paramCount;

            RodMaterial::StiffAxis stiffAxis;
            switch (axisType[i])
            {
                case 0:
                    stiffAxis = RodMaterial::StiffAxis::D1;
                    break;
                case 1:
                    stiffAxis = RodMaterial::StiffAxis::D2;
                    break;
                default:
                    stiffAxis = RodMaterial::StiffAxis::D1;
            }
            bool keepCrossSectionMesh = true;

            RodMaterial mat(type, E[i], nu[i], params, stiffAxis, keepCrossSectionMesh);
            materials.push_back(mat);
        }

        linkage->setJointMaterials(materials);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellSetCustomJointMaterial(RodLinkage *linkage, int numMaterials, int *sectionType, double *E, double *nu, double *inCoords, int *inCoordsCount, double *inHolesCoords, int *inHolesCount, int *axisType)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numMaterials<0) throw std::invalid_argument("Negative count: numMaterials");
            if (numMaterials>0 && !sectionType) throw std::invalid_argument("Null input buffer: sectionType");
            if (numMaterials>0 && !axisType) throw std::invalid_argument("Null input buffer: axisType");
            if (numMaterials>0 && !inCoordsCount) throw std::invalid_argument("Null input buffer: inCoordsCount");
            if (numMaterials>0 && !nu) throw std::invalid_argument("Null input buffer: nu");
            if (numMaterials>0 && !inHolesCount) throw std::invalid_argument("Null input buffer: inHolesCount");
            if (numMaterials>0 && !inCoords) throw std::invalid_argument("Null input buffer: inCoords");
            if (numMaterials>0 && !E) throw std::invalid_argument("Null input buffer: E");
            for (int i=0;i<numMaterials;++i) if(inHolesCount[i]>0 && !inHolesCoords) throw std::invalid_argument("Null hole coordinates");
            [&]() -> void {
        std::vector<RodMaterial> materials;
        materials.reserve(numMaterials);

        size_t idxV = 0, idxH = 0;
        for (size_t i = 0; i < numMaterials; i++)
        {
            std::string type;
            switch (sectionType[i])
            {
            case 0:
                type = "RECTANGLE";
                break;
            case 1:
                type = "ELLIPSE";
                break;
            case 2:
                type = "I";
                break;
            case 3:
                type = "L";
                break;
            case 4:
                type = "+";
                break;
            default:
                type = "RECTANGLE";
            }

            auto numVertices = inCoordsCount[i];
            std::vector<MeshIO::IOVertex> vertices;
            vertices.reserve(numVertices);
            std::vector<MeshIO::IOElement> lines;
            lines.reserve(numVertices);
            for (int j = 0; j < numVertices; j++)
            {
                vertices.emplace_back(inCoords[idxV + (j * 3)], inCoords[idxV + j * 3 + 1], inCoords[idxV + j * 3 + 2]);
                int next = j + 1;
                if (j == numVertices - 1)
                    next = 0;
                lines.emplace_back(j, next);
            }
            idxV += 3 * numVertices;

            auto numHoles = inHolesCount[i];
            std::vector<Point2D> holesPts;
            holesPts.reserve(numHoles);
            for (int j = 0; j < numHoles; j++)
                holesPts.emplace_back(inHolesCoords[idxH + 2 * j], inHolesCoords[idxH + 2 * j + 1]);
            idxH += 2 * numHoles;

            RodMaterial::StiffAxis stiffAxis;
            switch (axisType[i])
            {
            case 0:
                stiffAxis = RodMaterial::StiffAxis::D1;
                break;
            case 1:
                stiffAxis = RodMaterial::StiffAxis::D2;
                break;
            default:
                stiffAxis = RodMaterial::StiffAxis::D1;
            }
            bool keepCrossSectionMesh = true;

            RodMaterial mat;
            mat.setContourGH(E[i], nu[i], vertices, lines, holesPts, stiffAxis, keepCrossSectionMesh);
            materials.push_back(mat);
        }

        linkage->setJointMaterials(materials);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellSetDesignParameterConfig(RodLinkage *linkage, int use_restLen, int use_restKappa, int update_designParams_cache)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            [&]() -> void {
        linkage->setDesignParameterConfig(use_restLen, use_restKappa, update_designParams_cache);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API int erodXShellGetCentralJointIndex(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> int {
        return linkage->centralJoint();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodXShellGetDofOffsetForJoint(RodLinkage *linkage, int joint, int *DOF, int numDOF, int *outVars)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numDOF<0) throw std::invalid_argument("Negative count: numDOF");
            if (numDOF>0 && !outVars) throw std::invalid_argument("Null input buffer: outVars");
            if (numDOF>0 && !DOF) throw std::invalid_argument("Null input buffer: DOF");
            interop::index(joint,linkage->numJoints());
            [&]() -> void {
        const size_t jdo = linkage->dofOffsetForJoint(joint);
        for (int i = 0; i < numDOF; i++)
            outVars[i] = jdo + DOF[i];

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellGetDofOffsetForCenterLinePos(RodLinkage *linkage, int index, int *DOF, int numDOF, int *outVars)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numDOF<0) throw std::invalid_argument("Negative count: numDOF");
            if (numDOF>0 && !outVars) throw std::invalid_argument("Null input buffer: outVars");
            if (numDOF>0 && !DOF) throw std::invalid_argument("Null input buffer: DOF");
            interop::index(index,linkage->numCenterlinePos());
            [&]() -> void {
        const size_t jdo = linkage->dofOffsetForCenterlinePos(index);
        for (int i = 0; i < numDOF; i++)
            outVars[i] = jdo + DOF[i];

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellGetCenterLinePositions(RodLinkage *linkage, double **outCoords, int *numCoords)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        if (numCoords) *numCoords = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            if (!numCoords) throw std::invalid_argument("Missing output: numCoords");
            [&]() -> void {
        auto pos = linkage->centerLinePositions();
        *numCoords = interop::checked_count(pos.size());
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoords, pos.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
        if (numCoords) *numCoords = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};
        if (numCoords) *numCoords = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};
        if (numCoords) *numCoords = {};

            return;
        }
    }

    EROD_API double erodXShellGetAverageJointAngle(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->getAverageJointAngle();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodXShellGetEnergy(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->energy();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodXShellGetDoFs(RodLinkage *linkage, double **outDoFs, int *numDoFs)
    {
        interop::clear_error();
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outDoFs) throw std::invalid_argument("Missing output: outDoFs");
            if (!numDoFs) throw std::invalid_argument("Missing output: numDoFs");
            [&]() -> void {
        auto dofs = linkage->getDoFs();
        *numDoFs = interop::checked_count(dofs.size());
        auto sizeDoFs = (*numDoFs) * sizeof(double);
        *outDoFs = static_cast<double *>(interop::allocate(sizeDoFs));
        interop::copy(*outDoFs, dofs.data(), sizeDoFs);

            }();
            if (interop::has_error()) {
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};

            return;
        }
    }

    EROD_API double erodXShellGetEnergyBend(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->energyBend();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodXShellGetEnergyStretch(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->energyStretch();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodXShellGetEnergyTwist(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->energyTwist();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodXShellGetMaxStrain(RodLinkage *linkage)

    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->maxStrain();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API int erodXShellGetCenterLinePositionsCount(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> int {
        return linkage->numCenterlinePos();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodXShellGetJointsCount(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> int {
        return linkage->numJoints();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodXShellGetDoFCount(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> int {
        return linkage->numDoF();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodXShellGetRodSegmentsCount(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> int {
        return linkage->numSegments();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodXShellGetMeshData(RodLinkage *linkage, double **outCoords, int **outQuads, int *numCoords, int *numQuads)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            if (!outQuads) throw std::invalid_argument("Missing output: outQuads");
            if (!numCoords) throw std::invalid_argument("Missing output: numCoords");
            if (!numQuads) throw std::invalid_argument("Missing output: numQuads");
            [&]() -> void {
        std::vector<MeshIO::IOVertex> vertices;
        std::vector<MeshIO::IOElement> quads;
        linkage->visualizationGeometry(vertices, quads, true, true);

        *numCoords = interop::checked_count(vertices.size() * 3);
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        std::vector<double> coords;
        for (size_t i = 0; i < vertices.size(); i++)
        {
            auto v = vertices[i].point;
            coords.push_back(v[0]);
            coords.push_back(v[1]);
            coords.push_back(v[2]);
        }
        interop::copy(*outCoords, coords.data(), sizeCoords);

        *numQuads = interop::checked_count(quads.size() * 4);
        auto sizeQuads = (*numQuads) * sizeof(int);
        *outQuads = static_cast<int *>(interop::allocate(sizeQuads));
        std::vector<int> quadsIdx;
        for (size_t i = 0; i < quads.size(); i++)
        {
            auto q = quads[i];
            quadsIdx.push_back(q[0]);
            quadsIdx.push_back(q[1]);
            quadsIdx.push_back(q[2]);
            quadsIdx.push_back(q[3]);
        }
        interop::copy(*outQuads, quadsIdx.data(), sizeQuads);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};

            return;
        }
    }

    EROD_API void erodXShellGetRodSegmentIndexesPerRod(RodLinkage *linkage, int index, int **segmentIndexes, int *numSeg, int *type)
    {
        interop::clear_error();
        if (segmentIndexes) *segmentIndexes = {};
        if (numSeg) *numSeg = {};
        if (type) *type = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!segmentIndexes) throw std::invalid_argument("Missing output: segmentIndexes");
            if (!numSeg) throw std::invalid_argument("Missing output: numSeg");
            if (!type) throw std::invalid_argument("Missing output: type");
            interop::index(index,linkage->traceRods().size());
            [&]() -> void {
        const auto data = linkage->traceRods()[index];
        *type = std::get<0>(data);
        const auto vec = std::get<1>(data);

        *numSeg = interop::checked_count(vec.size());
        auto size = (*numSeg) * sizeof(int);
        *segmentIndexes = static_cast<int *>(interop::allocate(size));
        interop::copy(*segmentIndexes, vec.data(), size);

            }();
            if (interop::has_error()) {
        if (segmentIndexes) *segmentIndexes = {};
        if (numSeg) *numSeg = {};
        if (type) *type = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (segmentIndexes) *segmentIndexes = {};
        if (numSeg) *numSeg = {};
        if (type) *type = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (segmentIndexes) *segmentIndexes = {};
        if (numSeg) *numSeg = {};
        if (type) *type = {};

            return;
        }
    }

    EROD_API int erodXShellGetRodTraceCount(RodLinkage *linkage, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            auto result=[&]() -> int {
        try
        {
            *errorMessage = "Trace Built";
            return linkage->traceRods().size();
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        }
    }

    EROD_API void erodXShellRemoveRestCurvatures(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            [&]() -> void {
        for (auto &seg : linkage->segments())
        {
            auto r = seg.rod;

            const auto nv = r.numVertices();
            ElasticRod::StdVectorVector2D restKappa;
            restKappa.assign(nv, ElasticRod::Vec2::Zero());

            r.setRestKappas(restKappa);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API int erodXShellHessianNNZ(RodLinkage *linkage, int variableDesignParameters)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> int {
        return linkage->hessianNNZ(variableDesignParameters);

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodXShellGetRestKappaVarsCount(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> int {
        return linkage->numRestKappaVars();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodXShellGetRestLengthsSolveDoFs(RodLinkage *linkage, double **outDoFs, int *numDoFs){
        interop::clear_error();
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outDoFs) throw std::invalid_argument("Missing output: outDoFs");
            if (!numDoFs) throw std::invalid_argument("Missing output: numDoFs");
            [&]() -> void {
        const auto data = linkage->getRestlenSolveDoF();

        *numDoFs = interop::checked_count(data.size());
        auto sizeData = (*numDoFs) * sizeof(double);
        *outDoFs = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outDoFs, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};

            return;
        }
    }

    EROD_API void erodXShellGetPerSegmentRestLengths(RodLinkage *linkage, double **outLengths, int *numLengths){
        interop::clear_error();
        if (outLengths) *outLengths = {};
        if (numLengths) *numLengths = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outLengths) throw std::invalid_argument("Missing output: outLengths");
            if (!numLengths) throw std::invalid_argument("Missing output: numLengths");
            [&]() -> void {
        const auto data = linkage->getPerSegmentRestLength();

        *numLengths = interop::checked_count(data.size());
        auto sizeData = (*numLengths) * sizeof(double);
        *outLengths = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outLengths, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outLengths) *outLengths = {};
        if (numLengths) *numLengths = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outLengths) *outLengths = {};
        if (numLengths) *numLengths = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outLengths) *outLengths = {};
        if (numLengths) *numLengths = {};

            return;
        }
    }

    EROD_API void erodXShellSetRestLengthsSolveDoFs(RodLinkage *linkage, double *inDoFs, int numDoFs){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numDoFs<0) throw std::invalid_argument("Negative count: numDoFs");
            if (numDoFs>0 && !inDoFs) throw std::invalid_argument("Null input buffer: inDoFs");
            [&]() -> void {
        Eigen::VectorXd dofs = Eigen::Map<Eigen::VectorXd>(inDoFs, numDoFs, 1);
        linkage->setRestlenSolveDoF(dofs);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellSetPerSegmentRestLengths(RodLinkage *linkage, double *inLengths, int numLengths){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numLengths<0) throw std::invalid_argument("Negative count: numLengths");
            if (numLengths>0 && !inLengths) throw std::invalid_argument("Null input buffer: inLengths");
            [&]() -> void {
        Eigen::VectorXd lengths = Eigen::Map<Eigen::VectorXd>(inLengths, numLengths, 1);
        linkage->setPerSegmentRestLength(lengths);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellSetDoFs(RodLinkage *linkage, double *inDoFs, int numDoFs){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numDoFs<0) throw std::invalid_argument("Negative count: numDoFs");
            if (numDoFs>0 && !inDoFs) throw std::invalid_argument("Null input buffer: inDoFs");
            [&]() -> void {
        Eigen::VectorXd dofs = Eigen::Map<Eigen::VectorXd>(inDoFs, numDoFs, 1);
        linkage->setDoFs(dofs);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodXShellGetRestKappaVars(RodLinkage *linkage, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = linkage->getRestKappaVars();

        *numData = interop::checked_count(data.size());
        auto sizeData = (*numData) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API double erodXShellGetMinJointAngle(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->getMinJointAngle();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodXShellGetTotalRestLength(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->totalRestLength();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodXShellGetMaxRodEnergy(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->maxRodEnergy();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodXShellSetStiffenRegions(RodLinkage *linkage, double factor, double *coords, int numBoxes)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numBoxes<0) throw std::invalid_argument("Negative count: numBoxes");
            if (!std::isfinite(factor)) throw std::invalid_argument("Non-finite parameter: factor");
            if (numBoxes>0 && !coords) throw std::invalid_argument("Null input buffer: coords");
            [&]() -> void {
        std::vector<Eigen::Matrix<double, 8, 3>> corners;
        corners.reserve(numBoxes);

        size_t start = 0;
        for (size_t i = 0; i < numBoxes; i++)
        {
            Eigen::Matrix<double, 8, 3> box;
            for (size_t j = 0; j < 8; j++)
            {
                box.row(j) = Eigen::Vector3d(coords[start + j * 3], coords[start + j * 3 + 1], coords[start + j * 3 + 2]);
            }
            corners.push_back(box);
            start += 24;
        }

        RectangularBoxCollection bboxes(corners);
        linkage->stiffenRegions(bboxes, factor);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API double erodXShellStripAutoDiff(double *edgeA, double *edgeB)
    {
        interop::clear_error();

        try {
            interop::Call allocations;

            if(!edgeA || !edgeB) throw std::invalid_argument("Missing edge vectors");
            auto result=[&]() -> double {
        RodLinkage::Vec3 tA(edgeA[0], edgeA[1], edgeA[2]),
            tB(edgeB[0], edgeB[1], edgeB[2]);
        return stripAutoDiff(tA.dot(tB));

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API int erodXShelNumberDesignParameters(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> int {
        return linkage->numDesignParams();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodXShellGetDesignParams(RodLinkage *linkage, double **outDesignParams, int *outNumDesignParameters)
    {
        interop::clear_error();
        if (outDesignParams) *outDesignParams = {};
        if (outNumDesignParameters) *outNumDesignParameters = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outDesignParams) throw std::invalid_argument("Missing output: outDesignParams");
            if (!outNumDesignParameters) throw std::invalid_argument("Missing output: outNumDesignParameters");
            [&]() -> void {
        const auto data = linkage->getDesignParameters();

        *outNumDesignParameters = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outDesignParams = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outDesignParams, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outDesignParams) *outDesignParams = {};
        if (outNumDesignParameters) *outNumDesignParameters = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outDesignParams) *outDesignParams = {};
        if (outNumDesignParameters) *outNumDesignParameters = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outDesignParams) *outDesignParams = {};
        if (outNumDesignParameters) *outNumDesignParameters = {};

            return;
        }
    }

    EROD_API void erodXShellScalarFieldSqrtBendingEnergies(RodLinkage *linkage, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(linkage->visualizationField(linkage->sqrtBendingEnergies()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodXShellScalarFieldMaxBendingStresses(RodLinkage *linkage, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(linkage->visualizationField(linkage->maxBendingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodXShellScalarFieldVonMisesStresses(RodLinkage *linkage, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(linkage->visualizationField(linkage->maxVonMisesStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodXShellScalarFieldMinBendingStresses(RodLinkage *linkage, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(linkage->visualizationField(linkage->minBendingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodXShellScalarFieldTwistingStresses(RodLinkage *linkage, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(linkage->visualizationField(linkage->twistingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodXShellScalarFieldStretchingStresses(RodLinkage *linkage, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(linkage->visualizationField(linkage->stretchingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API double erodXShellGetInitialMinRestLength(RodLinkage *linkage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);

            auto result=[&]() -> double {
        return linkage->initialMinRestLength();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API int erodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(RodLinkage *linkage, double **outAx, int64_t **outAi, int64_t **outAp, int64_t *outM, int64_t *outN, int64_t *outNZ, const char **errorMessage)
    {
        interop::clear_error();
        if (outAx) *outAx = {};
        if (outAi) *outAi = {};
        if (outAp) *outAp = {};
        if (outM) *outM = {};
        if (outN) *outN = {};
        if (outNZ) *outNZ = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outAx) throw std::invalid_argument("Missing output: outAx");
            if (!outAi) throw std::invalid_argument("Missing output: outAi");
            if (!outAp) throw std::invalid_argument("Missing output: outAp");
            if (!outM) throw std::invalid_argument("Missing output: outM");
            if (!outN) throw std::invalid_argument("Missing output: outN");
            if (!outNZ) throw std::invalid_argument("Missing output: outNZ");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            auto result=[&]() -> int {
        try
        {
            const auto matrix = linkage->segmentRestLenToEdgeRestLenMapTranspose();

            const auto Ax = matrix.Ax;
            const auto Ai = matrix.Ai;
            const auto Ap = matrix.Ap;

            *outM = matrix.m;
            *outN = matrix.n;
            *outNZ = matrix.nz;

            auto sizeAx = (Ax.size()) * sizeof(double);
            *outAx = static_cast<double *>(interop::allocate(sizeAx));
            interop::copy(*outAx, Ax.data(), sizeAx);

            auto sizeAi = (Ai.size()) * sizeof(int64_t);
            *outAi = static_cast<int64_t *>(interop::allocate(sizeAi));
            std::copy(Ai.begin(), Ai.end(), *outAi);

            auto sizeAp = (Ap.size()) * sizeof(int64_t);
            *outAp = static_cast<int64_t *>(interop::allocate(sizeAp));
            std::copy(Ap.begin(), Ap.end(), *outAp);

            *errorMessage = "";
            return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return 1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return 1;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return 1;
        }

            }();
            if (interop::has_error()) {
        if (outAx) *outAx = {};
        if (outAi) *outAi = {};
        if (outAp) *outAp = {};
        if (outM) *outM = {};
        if (outN) *outN = {};
        if (outNZ) *outNZ = {};
            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outAx) *outAx = {};
        if (outAi) *outAi = {};
        if (outAp) *outAp = {};
        if (outM) *outM = {};
        if (outN) *outN = {};
        if (outNZ) *outNZ = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outAx) *outAx = {};
        if (outAi) *outAi = {};
        if (outAp) *outAp = {};
        if (outM) *outM = {};
        if (outN) *outN = {};
        if (outNZ) *outNZ = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        }
    }

    EROD_API void erodXShellGetDesignParameterConfig(RodLinkage *linkage, int *use_restLen, int *use_restKappa)
    {
        interop::clear_error();
        if (use_restLen) *use_restLen = {};
        if (use_restKappa) *use_restKappa = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!use_restLen) throw std::invalid_argument("Missing output: use_restLen");
            if (!use_restKappa) throw std::invalid_argument("Missing output: use_restKappa");
            [&]() -> void {
        const auto dpc = linkage->getDesignParameterConfig();

        *use_restLen = dpc.restLen;
        *use_restKappa = dpc.restKappa;

            }();
            if (interop::has_error()) {
        if (use_restLen) *use_restLen = {};
        if (use_restKappa) *use_restKappa = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (use_restLen) *use_restLen = {};
        if (use_restKappa) *use_restKappa = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (use_restLen) *use_restLen = {};
        if (use_restKappa) *use_restKappa = {};

            return;
        }
    }

    EROD_API void erodXShellGetJointAngles(RodLinkage *linkage, double **outAngles, int *numAngles)
    {
        interop::clear_error();
        if (outAngles) *outAngles = {};
        if (numAngles) *numAngles = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (!outAngles) throw std::invalid_argument("Missing output: outAngles");
            if (!numAngles) throw std::invalid_argument("Missing output: numAngles");
            [&]() -> void {
        const auto joints = linkage->joints();
        std::vector<double> angles;
        for (const auto &j : joints) angles.push_back(j.alpha());

        *numAngles = interop::checked_count(angles.size());
        auto sizeAngles = (*numAngles) * sizeof(double);
        *outAngles = static_cast<double *>(interop::allocate(sizeAngles));
        interop::copy(*outAngles, angles.data(), sizeAngles);

            }();
            if (interop::has_error()) {
        if (outAngles) *outAngles = {};
        if (numAngles) *numAngles = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outAngles) *outAngles = {};
        if (numAngles) *numAngles = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outAngles) *outAngles = {};
        if (numAngles) *numAngles = {};

            return;
        }
    }

    // Solver
    EROD_API int erodPeriodicElasticRodNewtonSolver(PeriodicRod *pRod, int numIterations, int numSupports, int numForces, int *supports, double *inForces,
                                                    double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                    int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
        interop::clear_error();
        if (outReport) *outReport = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numIterations<0) throw std::invalid_argument("Negative count: numIterations");
            if (numSupports<0) throw std::invalid_argument("Negative count: numSupports");
            if (numForces<0) throw std::invalid_argument("Negative count: numForces");
            if (!std::isfinite(gradTol)) throw std::invalid_argument("Non-finite parameter: gradTol");
            if (!std::isfinite(beta)) throw std::invalid_argument("Non-finite parameter: beta");
            if (!outReport) throw std::invalid_argument("Missing output: outReport");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numSupports>0 && !supports) throw std::invalid_argument("Null input buffer: supports");
            if (numForces>0 && !inForces) throw std::invalid_argument("Null input buffer: inForces");
            for(int i=0;i<numSupports;++i) interop::index(supports[i],pRod->numDoF());
            if(numForces!=0 && static_cast<size_t>(numForces)!=pRod->numDoF()) throw std::invalid_argument("Force vector has incorrect length");
            auto result=[&]() -> int {
        try
        {
            std::vector<size_t> fixedVars;
            for (int i = 0; i < numSupports; i++)
            {
                fixedVars.push_back(supports[i]);
            }

            NewtonOptimizerOptions options;
            options.gradTol = gradTol;
            options.niter = numIterations;
            options.beta = beta;
            options.useIdentityMetric = useIdentityMetric;
            options.useNegativeCurvatureDirection = useNegativeCurvatureDirection;
            options.feasibilitySolve = feasibilitySolve;
            options.verboseNonPosDef = verboseNonPosDef;
            options.verbose = verbose;

            auto problem = equilibrium_problem(*pRod, fixedVars);

            if (includeForces && numForces > 0)
            {
                Eigen::VectorXd externalForces(numForces);
                for (int i = 0; i < numForces; i++)
                {
                    externalForces(i) = inForces[i];
                }
                problem->external_forces = externalForces;
            }

            NewtonOptimizer solver(std::move(problem));
            solver.options = options;
            const auto report = solver.optimize();

            if (writeReport) getConvergenceReport(report, outReport);

            *errorMessage = "";

            if (report.success)
                return 1;
            else
                return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }

            }();
            if (interop::has_error()) {
        if (outReport) *outReport = {};
            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        }
    }

    EROD_API int erodElasticRodNewtonSolver(ElasticRod *rod, int numIterations, int numSupports, int numForces, int *supports, double *inForces,
                                            double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                            int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
        interop::clear_error();
        if (outReport) *outReport = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (numIterations<0) throw std::invalid_argument("Negative count: numIterations");
            if (numSupports<0) throw std::invalid_argument("Negative count: numSupports");
            if (numForces<0) throw std::invalid_argument("Negative count: numForces");
            if (!std::isfinite(gradTol)) throw std::invalid_argument("Non-finite parameter: gradTol");
            if (!std::isfinite(beta)) throw std::invalid_argument("Non-finite parameter: beta");
            if (!outReport) throw std::invalid_argument("Missing output: outReport");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numSupports>0 && !supports) throw std::invalid_argument("Null input buffer: supports");
            if (numForces>0 && !inForces) throw std::invalid_argument("Null input buffer: inForces");
            for(int i=0;i<numSupports;++i) interop::index(supports[i],rod->numDoF());
            if(numForces!=0 && static_cast<size_t>(numForces)!=rod->numDoF()) throw std::invalid_argument("Force vector has incorrect length");
            auto result=[&]() -> int {
        try
        {
            std::vector<size_t> fixedVars;
            for (int i = 0; i < numSupports; i++)
            {
                fixedVars.push_back(supports[i]);
            }

            NewtonOptimizerOptions options;
            options.gradTol = gradTol;
            options.niter = numIterations;
            options.beta = beta;
            options.useIdentityMetric = useIdentityMetric;
            options.useNegativeCurvatureDirection = useNegativeCurvatureDirection;
            options.feasibilitySolve = feasibilitySolve;
            options.verboseNonPosDef = verboseNonPosDef;
            options.verbose = verbose;

            auto problem = equilibrium_problem(*rod, fixedVars);

            if (includeForces && numForces > 0)
            {
                Eigen::VectorXd externalForces(numForces);
                for (int i = 0; i < numForces; i++)
                {
                    externalForces(i) = inForces[i];
                }
                problem->external_forces = externalForces;
            }

            NewtonOptimizer solver(std::move(problem));
            solver.options = options;
            const auto report = solver.optimize();

            if (writeReport)
            {
                std::vector<double> flatReport;
                flatReport.push_back(report.success);
                flatReport.push_back(report.backtracking_failure);
                flatReport.insert(flatReport.end(), report.energy.begin(), report.energy.end());
                flatReport.insert(flatReport.end(), report.gradientNorm.begin(), report.gradientNorm.end());
                flatReport.insert(flatReport.end(), report.freeGradientNorm.begin(), report.freeGradientNorm.end());
                flatReport.insert(flatReport.end(), report.stepLength.begin(), report.stepLength.end());
                flatReport.insert(flatReport.end(), report.indefinite.begin(), report.indefinite.end());

                auto sizeReport = flatReport.size() * sizeof(double);
                *outReport = static_cast<double *>(interop::allocate(sizeReport));
                interop::copy(*outReport, flatReport.data(), sizeReport);
            }

            *errorMessage = "";

            if (report.success)
                return 1;
            else
                return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }

            }();
            if (interop::has_error()) {
        if (outReport) *outReport = {};
            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        }
    }

    EROD_API int erodXShellNewtonSolver(RodLinkage *linkage, int numIterations, double deployedAngle, int numSupports, int numForces, int *supports, double *inForces,
                                        double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                        int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
        interop::clear_error();
        if (outReport) *outReport = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numIterations<0) throw std::invalid_argument("Negative count: numIterations");
            if (numSupports<0) throw std::invalid_argument("Negative count: numSupports");
            if (numForces<0) throw std::invalid_argument("Negative count: numForces");
            if (!std::isfinite(deployedAngle)) throw std::invalid_argument("Non-finite parameter: deployedAngle");
            if (!std::isfinite(gradTol)) throw std::invalid_argument("Non-finite parameter: gradTol");
            if (!std::isfinite(beta)) throw std::invalid_argument("Non-finite parameter: beta");
            if (!outReport) throw std::invalid_argument("Missing output: outReport");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numSupports>0 && !supports) throw std::invalid_argument("Null input buffer: supports");
            if (numForces>0 && !inForces) throw std::invalid_argument("Null input buffer: inForces");
            for(int i=0;i<numSupports;++i) interop::index(supports[i],linkage->numDoF());
            if(numForces!=0 && static_cast<size_t>(numForces)!=linkage->numDoF()) throw std::invalid_argument("Force vector has incorrect length");
            auto result=[&]() -> int {
        try
        {
            std::vector<size_t> fixedVars;
            for (int i = 0; i < numSupports; i++)
            {
                fixedVars.push_back(supports[i]);
            }

            NewtonOptimizerOptions options;
            options.gradTol = gradTol;
            options.niter = numIterations;
            options.beta = beta;
            options.useIdentityMetric = useIdentityMetric;
            options.useNegativeCurvatureDirection = useNegativeCurvatureDirection;
            options.feasibilitySolve = feasibilitySolve;
            options.verboseNonPosDef = verboseNonPosDef;
            options.verbose = verbose;

            std::unique_ptr<EquilibriumProblem<RodLinkage>> problem;
            if (deployedAngle == 0)
                problem = equilibrium_problem(*linkage, fixedVars);
            else
                problem = equilibrium_problem(*linkage, deployedAngle, fixedVars);

            if (includeForces && numForces > 0)
            {
                Eigen::VectorXd externalForces(numForces);
                for (int i = 0; i < numForces; i++)
                {
                    externalForces(i) = inForces[i];
                }
                problem->external_forces = externalForces;
            }

            NewtonOptimizer solver(std::move(problem));
            solver.options = options;
            const auto report = solver.optimize();

            if (writeReport) getConvergenceReport(report, outReport);

            *errorMessage = "";
            if (report.success) return 1;
            else return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }

            }();
            if (interop::has_error()) {
        if (outReport) *outReport = {};
            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        }
    }

    EROD_API int erodXShellAttractedLinkageNewtonSolver(SurfaceAttractedLinkage *linkage, int numIterations, double deployedAngle, int numSupports, int numForces, int *supports, double *inForces,
                                                        double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                        int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
        interop::clear_error();
        if (outReport) *outReport = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numIterations<0) throw std::invalid_argument("Negative count: numIterations");
            if (numSupports<0) throw std::invalid_argument("Negative count: numSupports");
            if (numForces<0) throw std::invalid_argument("Negative count: numForces");
            if (!std::isfinite(deployedAngle)) throw std::invalid_argument("Non-finite parameter: deployedAngle");
            if (!std::isfinite(gradTol)) throw std::invalid_argument("Non-finite parameter: gradTol");
            if (!std::isfinite(beta)) throw std::invalid_argument("Non-finite parameter: beta");
            if (!outReport) throw std::invalid_argument("Missing output: outReport");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numSupports>0 && !supports) throw std::invalid_argument("Null input buffer: supports");
            if (numForces>0 && !inForces) throw std::invalid_argument("Null input buffer: inForces");
            for(int i=0;i<numSupports;++i) interop::index(supports[i],linkage->numDoF());
            if(numForces!=0 && static_cast<size_t>(numForces)!=linkage->numDoF()) throw std::invalid_argument("Force vector has incorrect length");
            auto result=[&]() -> int {
        try
        {
            std::vector<size_t> fixedVars;
            for (int i = 0; i < numSupports; i++)
            {
                fixedVars.push_back(supports[i]);
            }

            NewtonOptimizerOptions options;
            options.gradTol = gradTol;
            options.niter = numIterations;
            options.beta = beta;
            options.useIdentityMetric = useIdentityMetric;
            options.useNegativeCurvatureDirection = useNegativeCurvatureDirection;
            options.feasibilitySolve = feasibilitySolve;
            options.verboseNonPosDef = verboseNonPosDef;
            options.verbose = verbose;

            std::unique_ptr<EquilibriumProblem<SurfaceAttractedLinkage>> problem;
            if (deployedAngle == 0)
                problem = equilibrium_problem(*linkage, fixedVars);
            else
                problem = equilibrium_problem(*linkage, deployedAngle, fixedVars);

            if (includeForces && numForces > 0)
            {
                Eigen::VectorXd externalForces(numForces);
                for (int i = 0; i < numForces; i++)
                {
                    externalForces(i) = inForces[i];
                }
                problem->external_forces = externalForces;
            }

            NewtonOptimizer solver(std::move(problem));
            solver.options = options;
            const auto report = solver.optimize();

            if (writeReport) getConvergenceReport(report, outReport);

            *errorMessage = "";

            if (report.success)
                return 1;
            else
                return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }

            }();
            if (interop::has_error()) {
        if (outReport) *outReport = {};
            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        }
    }

    EROD_API int erodXShellLiveNewtonSolver(RodLinkage *linkage, int numIterations, double deployedAngle, int numSupports, int numForces, int *supports, double *inForces,
                                            double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                            int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
        interop::clear_error();
        if (outReport) *outReport = {};
        try {
            interop::Call allocations;
            interop::require(linkage);
            if (numIterations<0) throw std::invalid_argument("Negative count: numIterations");
            if (numSupports<0) throw std::invalid_argument("Negative count: numSupports");
            if (numForces<0) throw std::invalid_argument("Negative count: numForces");
            if (!std::isfinite(deployedAngle)) throw std::invalid_argument("Non-finite parameter: deployedAngle");
            if (!std::isfinite(gradTol)) throw std::invalid_argument("Non-finite parameter: gradTol");
            if (!std::isfinite(beta)) throw std::invalid_argument("Non-finite parameter: beta");
            if (!outReport) throw std::invalid_argument("Missing output: outReport");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numSupports>0 && !supports) throw std::invalid_argument("Null input buffer: supports");
            if (numForces>0 && !inForces) throw std::invalid_argument("Null input buffer: inForces");
            for(int i=0;i<numSupports;++i) interop::index(supports[i],linkage->numDoF());
            if(numForces!=0 && static_cast<size_t>(numForces)!=linkage->numDoF()) throw std::invalid_argument("Force vector has incorrect length");
            auto result=[&]() -> int {
        try
        {
            std::vector<size_t> fixedVars;
            for (int i = 0; i < numSupports; i++)
            {
                fixedVars.push_back(supports[i]);
            }

            NewtonOptimizerOptions options;
            options.gradTol = gradTol;
            options.niter = numIterations;
            options.beta = beta;
            options.useIdentityMetric = useIdentityMetric;
            options.useNegativeCurvatureDirection = useNegativeCurvatureDirection;
            options.feasibilitySolve = feasibilitySolve;
            options.verboseNonPosDef = verboseNonPosDef;
            options.verbose = verbose;

            std::unique_ptr<EquilibriumProblem<RodLinkage>> problem;
            if (deployedAngle == 0)
                problem = equilibrium_problem(*linkage, fixedVars);
            else
                problem = equilibrium_problem(*linkage, deployedAngle, fixedVars);

            if (includeForces && numForces > 0)
            {
                Eigen::VectorXd externalForces(numForces);
                for (int i = 0; i < numForces; i++)
                {
                    externalForces(i) = inForces[i];
                }
                problem->external_forces = externalForces;
            }

            NewtonOptimizer solver(std::move(problem));
            solver.options = options;
            const auto report = solver.optimize();

            if (writeReport) getConvergenceReport(report, outReport);

            *errorMessage = "";

            if (report.success)
                return 1;
            else
                return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return -1;
        }

            }();
            if (interop::has_error()) {
        if (outReport) *outReport = {};
            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outReport) *outReport = {};
            if (errorMessage) *errorMessage=interop::error_storage();
            return -1;
        }
    }

    // Material
    EROD_API RodMaterial *erodMaterialBuild(int sectionType, double E, double nu, double *params, int numParams, int axisType)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            if (numParams<0) throw std::invalid_argument("Negative count: numParams");
            if (!std::isfinite(E)) throw std::invalid_argument("Non-finite parameter: E");
            if (!std::isfinite(nu)) throw std::invalid_argument("Non-finite parameter: nu");
            if (numParams>0 && !params) throw std::invalid_argument("Null input buffer: params");
            auto result=[&]() -> RodMaterial * {
        std::string type;
        switch (sectionType)
        {
        case 0:
            type = "RECTANGLE";
            break;
        case 1:
            type = "ELLIPSE";
            break;
        case 2:
            type = "I";
            break;
        case 3:
            type = "L";
            break;
        case 4:
            type = "+";
            break;
        default:
            type = "RECTANGLE";
        }

        std::vector<Real> inParams;
        inParams.reserve(numParams);
        for (int i = 0; i < numParams; i++) inParams.push_back(params[i]);

        RodMaterial::StiffAxis stiffAxis;
        switch (axisType)
        {
        case 0:
            stiffAxis = RodMaterial::StiffAxis::D1;
            break;
        case 1:
            stiffAxis = RodMaterial::StiffAxis::D2;
            break;
        default:
            stiffAxis = RodMaterial::StiffAxis::D1;
        }
        bool keepCrossSectionMesh = true;

        return interop::own(new RodMaterial(type, E, nu, inParams, stiffAxis, keepCrossSectionMesh));

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return nullptr;
        }
    }

    EROD_API RodMaterial *erodMaterialCustomBuild(double E, double nu, double *inCoords, int numVertices, double *inHolesCoords, int numHoles, int axisType)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numHoles<0) throw std::invalid_argument("Negative count: numHoles");
            if (!std::isfinite(E)) throw std::invalid_argument("Non-finite parameter: E");
            if (!std::isfinite(nu)) throw std::invalid_argument("Non-finite parameter: nu");
            if (numVertices>0 && !inCoords) throw std::invalid_argument("Null input buffer: inCoords");
            if (numHoles>0 && !inHolesCoords) throw std::invalid_argument("Null input buffer: inHolesCoords");
            if(numVertices<3) throw std::invalid_argument("A section needs at least three vertices");
            auto result=[&]() -> RodMaterial * {
        std::vector<MeshIO::IOVertex> vertices;
        vertices.reserve(numVertices);
        std::vector<MeshIO::IOElement> lines;
        lines.reserve(numVertices);

        for (int i = 0; i < numVertices; i++)
        {
            vertices.emplace_back(inCoords[3 * i], inCoords[3 * i + 1], inCoords[3 * i + 2]);
            int next = i + 1;
            if (i == numVertices - 1)
                next = 0;
            lines.emplace_back(i, next);
        }

        std::vector<Point2D> holesPts;
        holesPts.reserve(numHoles);
        for (int i = 0; i < numHoles; i++) holesPts.emplace_back(inHolesCoords[2 * i], inHolesCoords[2 * i + 1]);

        RodMaterial::StiffAxis stiffAxis;
        switch (axisType)
        {
            case 0:
                stiffAxis = RodMaterial::StiffAxis::D1;
                break;
            case 1:
                stiffAxis = RodMaterial::StiffAxis::D2;
                break;
            default:
                stiffAxis = RodMaterial::StiffAxis::D1;
        }
        bool keepCrossSectionMesh = true;

        std::unique_ptr<RodMaterial> mat(new RodMaterial());
        mat->setContourGH(E, nu, vertices, lines, holesPts, stiffAxis, keepCrossSectionMesh);
        return interop::own(mat.release());

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return nullptr;
        }
    }

    EROD_API void erodMaterialSetToLinkage(RodMaterial *const *materials, int numMaterials, RodLinkage *linkage)
    {
        interop::clear_error();
        try {
            interop::Call allocations;
            if (numMaterials<0) throw std::invalid_argument("Negative count: numMaterials");
            interop::require(linkage);
            if (!materials || numMaterials<1) throw std::invalid_argument("No materials supplied");
            if (numMaterials != 1 && static_cast<size_t>(numMaterials) != linkage->numJoints()) throw std::invalid_argument("Joint material count mismatch");
            for (int i=0;i<numMaterials;++i) interop::require(materials[i]);
            [&]() -> void {
        if(numMaterials==1) linkage->setMaterial(*materials[0]);
        else{
            std::vector<RodMaterial> mat;
            mat.reserve(numMaterials);

            for (size_t i = 0; i < numMaterials; i++) mat.push_back(*materials[i]);

            linkage->setJointMaterials(mat);
        }

            }();
            if (interop::has_error()) {
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");

            return;
        }
    }

    EROD_API double erodMaterialGetArea(RodMaterial *material)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(material);

            auto result=[&]() -> double {
        return material->area;

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodMaterialGetMomentOfInertia(RodMaterial *material, double *lambda1, double *lambda2)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(material);

            [&]() -> void {
        *lambda1 = material->momentOfInertia.lambda_1;
        *lambda2 = material->momentOfInertia.lambda_2;

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodMaterialGetBendingStiffness(RodMaterial *material, double *lambda1, double *lambda2)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(material);

            [&]() -> void {
        *lambda1 = material->bendingStiffness.lambda_1;
        *lambda2 = material->bendingStiffness.lambda_2;

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API double erodMaterialGetTwistingStiffness(RodMaterial *material)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(material);

            auto result=[&]() -> double {
        return material->twistingStiffness;

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodMaterialGetStretchingStiffness(RodMaterial *material)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(material);

            auto result=[&]() -> double {
        return material->stretchingStiffness;

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodMaterialGetSherModulus(RodMaterial *material)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(material);

            auto result=[&]() -> double {
        return material->shearModulus;

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodMaterialGetCrossSectionHeight(RodMaterial *material)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(material);

            auto result=[&]() -> double {
        return material->crossSectionHeight;

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    // Joints
    EROD_API const RodLinkage::Joint *erodJointBuild(RodLinkage *linkage, int index)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            interop::index(index,linkage->numJoints());

            auto result=[&]() -> const RodLinkage::Joint * {
        return interop::borrow(&linkage->joint(index),linkage);

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return nullptr;
        }
    }

    EROD_API void erodJointGetPosition(RodLinkage::Joint *joint, double **outCoords)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            [&]() -> void {
        const auto coords = joint->pos();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoords, coords.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};

            return;
        }
    }

    EROD_API void erodJointGetNormal(RodLinkage::Joint *joint, double **outCoords)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            [&]() -> void {
        const auto coords = joint->normal();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoords, coords.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};

            return;
        }
    }

    EROD_API void erodJointGetEdgeVecA(RodLinkage::Joint *joint, double **outCoords)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            [&]() -> void {
        const auto coords = joint->source_t_A();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoords, coords.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};

            return;
        }
    }

    EROD_API void erodJointGetEdgeVecB(RodLinkage::Joint *joint, double **outCoords)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            [&]() -> void {
        const auto coords = joint->source_t_B();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoords, coords.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};

            return;
        }
    }

    EROD_API void erodJointGetIsStartA(RodLinkage::Joint *joint, int **outStartA)
    {
        interop::clear_error();
        if (outStartA) *outStartA = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outStartA) throw std::invalid_argument("Missing output: outStartA");
            [&]() -> void {
        const auto s = joint->isStartA();

        std::vector<int> data;
        for (size_t i = 0; i < 2; i++)
        {
            data.push_back(s[i]);
        }

        auto sizeData = (data.size()) * sizeof(int);
        *outStartA = static_cast<int *>(interop::allocate(sizeData));
        interop::copy(*outStartA, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outStartA) *outStartA = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outStartA) *outStartA = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outStartA) *outStartA = {};

            return;
        }
    }

    EROD_API void erodJointGetIsStartB(RodLinkage::Joint *joint, int **outStartB)
    {
        interop::clear_error();
        if (outStartB) *outStartB = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outStartB) throw std::invalid_argument("Missing output: outStartB");
            [&]() -> void {
        const auto s = joint->isStartB();

        std::vector<int> data;
        for (size_t i = 0; i < 2; i++)
        {
            data.push_back(s[i]);
        }

        auto sizeData = (data.size()) * sizeof(int);
        *outStartB = static_cast<int *>(interop::allocate(sizeData));
        interop::copy(*outStartB, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outStartB) *outStartB = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outStartB) *outStartB = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outStartB) *outStartB = {};

            return;
        }
    }

    EROD_API void erodJointGetConnectedSegments(RodLinkage::Joint *joint, int **outSegmentsA, int **outSegmentsB)
    {
        interop::clear_error();
        if (outSegmentsA) *outSegmentsA = {};
        if (outSegmentsB) *outSegmentsB = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outSegmentsA) throw std::invalid_argument("Missing output: outSegmentsA");
            if (!outSegmentsB) throw std::invalid_argument("Missing output: outSegmentsB");
            [&]() -> void {
        const auto sA = joint->segmentsA();
        const auto sB = joint->segmentsB();

        std::vector<int> ssA, ssB;
        for (int i = 0; i < 2; i++)
        {
            if (sA[i] == RodLinkage::NONE)
                ssA.push_back(-1);
            else
                ssA.push_back(static_cast<int>(sA[i]));

            if (sB[i] == RodLinkage::NONE)
                ssB.push_back(-1);
            else
                ssB.push_back(static_cast<int>(sB[i]));
        }

        auto sizeData = 2 * sizeof(int);
        *outSegmentsA = static_cast<int *>(interop::allocate(sizeData));
        *outSegmentsB = static_cast<int *>(interop::allocate(sizeData));
        interop::copy(*outSegmentsA, ssA.data(), sizeData);
        interop::copy(*outSegmentsB, ssB.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outSegmentsA) *outSegmentsA = {};
        if (outSegmentsB) *outSegmentsB = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outSegmentsA) *outSegmentsA = {};
        if (outSegmentsB) *outSegmentsB = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outSegmentsA) *outSegmentsA = {};
        if (outSegmentsB) *outSegmentsB = {};

            return;
        }
    }

    EROD_API void erodJointGetOmega(RodLinkage::Joint *joint, double **outCoords)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            [&]() -> void {
        const auto coords = joint->omega();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoords, coords.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};

            return;
        }
    }

    EROD_API void erodJointGetSourceTangent(RodLinkage::Joint *joint, double **outCoords)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            [&]() -> void {
        const auto coords = joint->source_tangent();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoords, coords.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};

            return;
        }
    }

    EROD_API void erodJointGetSourceNormal(RodLinkage::Joint *joint, double **outCoords)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            [&]() -> void {
        const auto coords = joint->source_normal();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoords, coords.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};

            return;
        }
    }

    EROD_API double erodJointGetAlpha(RodLinkage::Joint *joint)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(joint);

            auto result=[&]() -> double {
        return joint->alpha();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodJointGetNormalSigns(RodLinkage::Joint *joint, int **outSigns)
    {
        interop::clear_error();
        if (outSigns) *outSigns = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outSigns) throw std::invalid_argument("Missing output: outSigns");
            [&]() -> void {
        const auto signs = joint->normal_signs();

        auto sizeSigns = (signs.size()) * sizeof(size_t);
        *outSigns = static_cast<int *>(interop::allocate(sizeSigns));
        interop::copy(*outSigns, signs.data(), sizeSigns);

            }();
            if (interop::has_error()) {
        if (outSigns) *outSigns = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outSigns) *outSigns = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outSigns) *outSigns = {};

            return;
        }
    }

    EROD_API double erodJointGetSignB(RodLinkage::Joint *joint)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(joint);

            auto result=[&]() -> double {
        return joint->sign_B();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodJointGetLenA(RodLinkage::Joint *joint)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(joint);

            auto result=[&]() -> double {
        return joint->len_A();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodJointGetLenB(RodLinkage::Joint *joint)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(joint);

            auto result=[&]() -> double {
        return joint->len_B();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API int erodJointGetType(RodLinkage::Joint *joint)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(joint);

            auto result=[&]() -> int {
        return (int)joint->type;

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodJointGetSegmentsA(RodLinkage::Joint *joint, int **outSegmentsA)
    {
        interop::clear_error();
        if (outSegmentsA) *outSegmentsA = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outSegmentsA) throw std::invalid_argument("Missing output: outSegmentsA");
            [&]() -> void {
        const auto s = joint->segmentsA();
        std::vector<int> ss;
        for (int i = 0; i < 2; i++)
        {
            if (s[i] == RodLinkage::NONE)
                ss.push_back(-1);
            else
                ss.push_back(static_cast<int>(s[i]));
        }

        auto sizeData = ss.size() * sizeof(int);
        *outSegmentsA = static_cast<int *>(interop::allocate(sizeData));
        interop::copy(*outSegmentsA, ss.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outSegmentsA) *outSegmentsA = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outSegmentsA) *outSegmentsA = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outSegmentsA) *outSegmentsA = {};

            return;
        }
    }

    EROD_API void erodJointGetSegmentsB(RodLinkage::Joint *joint, int **outSegmentsB)
    {
        interop::clear_error();
        if (outSegmentsB) *outSegmentsB = {};
        try {
            interop::Call allocations;
            interop::require(joint);
            if (!outSegmentsB) throw std::invalid_argument("Missing output: outSegmentsB");
            [&]() -> void {
        const auto s = joint->segmentsB();
        std::vector<int> ss;
        for (int i = 0; i < 2; i++)
        {
            if (s[i] == RodLinkage::NONE)
                ss.push_back(-1);
            else
                ss.push_back(static_cast<int>(s[i]));
        }

        auto sizeData = ss.size() * sizeof(int);
        *outSegmentsB = static_cast<int *>(interop::allocate(sizeData));
        interop::copy(*outSegmentsB, ss.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outSegmentsB) *outSegmentsB = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outSegmentsB) *outSegmentsB = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outSegmentsB) *outSegmentsB = {};

            return;
        }
    }

    // Segments
    EROD_API void erodRodSegmentGetMaterialFrame(RodLinkage::RodSegment *segment, int *outCoordsCount, double **outCoordsD1, double **outCoordsD2)
    {
        interop::clear_error();
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outCoordsCount) throw std::invalid_argument("Missing output: outCoordsCount");
            if (!outCoordsD1) throw std::invalid_argument("Missing output: outCoordsD1");
            if (!outCoordsD2) throw std::invalid_argument("Missing output: outCoordsD2");
            [&]() -> void {
        const auto rod = segment->rod;

        std::vector<double> coordsD1, coordsD2;
        coordsD1.reserve(rod.numEdges() * 3);
        coordsD2.reserve(rod.numEdges() * 3);
        for (size_t i = 0; i < rod.numEdges(); i++)
        {
            const auto d1 = rod.deformedMaterialFrameD1(i);
            coordsD1.push_back(d1.x());
            coordsD1.push_back(d1.y());
            coordsD1.push_back(d1.z());

            const auto d2 = rod.deformedMaterialFrameD2(i);
            coordsD2.push_back(d2.x());
            coordsD2.push_back(d2.y());
            coordsD2.push_back(d2.z());
        }

        *outCoordsCount = interop::checked_count(coordsD1.size());
        auto sizeCoords = (*outCoordsCount) * sizeof(double);
        *outCoordsD1 = static_cast<double *>(interop::allocate(sizeCoords));
        *outCoordsD2 = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoordsD1, coordsD1.data(), sizeCoords);
        interop::copy(*outCoordsD2, coordsD2.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};

            return;
        }
    }

    EROD_API const RodLinkage::RodSegment *erodRodSegmentBuild(RodLinkage *linkage, int index)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(linkage);
            interop::index(index,linkage->numSegments());

            auto result=[&]() -> const RodLinkage::RodSegment * {
        return interop::borrow(&linkage->segment(index),linkage);

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return nullptr;
        }
    }

    EROD_API int erodRodSegmentGetEdgesCount(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> int {
        return segment->rod.numEdges();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodRodSegmentGetVerticesCount(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> int {
        return segment->rod.numVertices();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodRodSegmentGetRestAnglesCount(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> int {
        return segment->rod.numRestKappaVars();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodRodSegmentGetRestKappas(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto kappas = segment->rod.restKappas();
        size_t count = kappas.size();

        std::vector<double> data;
        data.reserve(count * 2);

        for (size_t i = 0; i < count; i++)
        {
            const auto p = kappas[i];
            data.push_back(p(0, 0));
            data.push_back(p(1, 0));
        }

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetRestLengths(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.restLengths();

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetStretchingStresses(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.stretchingStresses();

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetTwistingStresses(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.twistingStresses();

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetCenterLinePositions(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto pts = segment->rod.deformedPoints();
        size_t count = pts.size();

        std::vector<double> data;
        data.reserve(count);
        for (size_t i = 0; i < count; i++)
        {
            const auto p = pts[i];
            data.push_back(p(0, 0));
            data.push_back(p(1, 0));
            data.push_back(p(2, 0));
        }

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetMaxBendingStresses(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.maxBendingStresses();

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetMinBendingStresses(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.minBendingStresses();

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetSqrtBendingEnergies(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = stripAutoDiff(segment->rod.energyBendPerVertex()).array().sqrt().eval();

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetMeshData(RodLinkage::RodSegment *segment, double **outCoords, int **outQuads, int *numCoords, int *numQuads)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            if (!outQuads) throw std::invalid_argument("Missing output: outQuads");
            if (!numCoords) throw std::invalid_argument("Missing output: numCoords");
            if (!numQuads) throw std::invalid_argument("Missing output: numQuads");
            [&]() -> void {
        std::vector<MeshIO::IOVertex> vertices;
        std::vector<MeshIO::IOElement> quads;
        segment->rod.visualizationGeometry(vertices, quads, true, true);

        *numCoords = interop::checked_count(vertices.size() * 3);
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        std::vector<double> coords;
        for (size_t i = 0; i < vertices.size(); i++)
        {
            auto v = vertices[i].point;
            coords.push_back(v[0]);
            coords.push_back(v[1]);
            coords.push_back(v[2]);
        }
        interop::copy(*outCoords, coords.data(), sizeCoords);

        *numQuads = interop::checked_count(quads.size() * 4);
        auto sizeQuads = (*numQuads) * sizeof(int);
        *outQuads = static_cast<int *>(interop::allocate(sizeQuads));
        std::vector<int> quadsIdx;
        for (size_t i = 0; i < quads.size(); i++)
        {
            auto q = quads[i];
            quadsIdx.push_back(q[0]);
            quadsIdx.push_back(q[1]);
            quadsIdx.push_back(q[2]);
            quadsIdx.push_back(q[3]);
        }
        interop::copy(*outQuads, quadsIdx.data(), sizeQuads);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};

            return;
        }
    }

    EROD_API int erodRodSegmentGetStartJointIndex(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> int {
        const auto idx = segment->startJoint;
        if (idx == RodLinkage::NONE)
            return -1;
        return idx;

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodRodSegmentGetEndJointIndex(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> int {
        const auto idx = segment->endJoint;
        if (idx == RodLinkage::NONE)
            return -1;
        return idx;

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API double erodRodSegmentGetEnergy(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> double {
        return segment->rod.energy();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodRodSegmentGetEnergyBend(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> double {
        return segment->rod.energyBend();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodRodSegmentGetEnergyStretch(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> double {
        return segment->rod.energyStretch();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodRodSegmentGetEnergyTwist(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> double {
        return segment->rod.energyTwist();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodRodSegmentGetBendingStiffness(RodLinkage::RodSegment *segment, double **lambda1, double **lambda2, int *numLambda1, int *numLambda2)
    {
        interop::clear_error();
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!lambda1) throw std::invalid_argument("Missing output: lambda1");
            if (!lambda2) throw std::invalid_argument("Missing output: lambda2");
            if (!numLambda1) throw std::invalid_argument("Missing output: numLambda1");
            if (!numLambda2) throw std::invalid_argument("Missing output: numLambda2");
            [&]() -> void {
        const auto s = segment->rod.bendingStiffnesses();
        size_t count = s.size();

        std::vector<double> lA, lB;
        for (size_t i = 0; i < count; i++)
        {
            lA.push_back(s[i].lambda_1);
            lB.push_back(s[i].lambda_2);
        }

        *numLambda1 = interop::checked_count(lA.size());
        auto sizeL1 = (lA.size()) * sizeof(double);
        *lambda1 = static_cast<double *>(interop::allocate(sizeL1));
        interop::copy(*lambda1, lA.data(), sizeL1);

        *numLambda2 = interop::checked_count(lB.size());
        auto sizeL2 = (lB.size()) * sizeof(double);
        *lambda2 = static_cast<double *>(interop::allocate(sizeL2));
        interop::copy(*lambda2, lB.data(), sizeL2);

            }();
            if (interop::has_error()) {
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetTwistingStiffness(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.twistingStiffnesses();
        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetStretchingStiffness(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.stretchingStiffnesses();
        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetRestPoints(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto pts = segment->rod.restPoints();
        size_t count = pts.size();

        std::vector<double> data;
        data.reserve(count * 3);

        for (size_t i = 0; i < count; i++)
        {
            const auto p = pts[i];
            data.push_back(p(0, 0));
            data.push_back(p(1, 0));
            data.push_back(p(2, 0));
        }

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetRestDirectors(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto dir = segment->rod.restDirectors();
        size_t count = dir.size();

        std::vector<double> data;
        data.reserve(count * 6);

        for (size_t i = 0; i < count; i++)
        {
            const auto d1 = dir[i].d1;
            data.push_back(d1(0, 0));
            data.push_back(d1(1, 0));
            data.push_back(d1(2, 0));

            const auto d2 = dir[i].d2;
            data.push_back(d2(0, 0));
            data.push_back(d2(1, 0));
            data.push_back(d2(2, 0));
        }

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetRestTwists(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.restTwists();

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API int erodRodSegmentGetBendingEnergyType(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> int {
        const auto eType = segment->rod.bendingEnergyType();

        return eType == ElasticRod::BendingEnergyType::Bergou2008 ? 0 : 1;

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodRodSegmentGetDensities(RodLinkage::RodSegment *segment, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = segment->rod.densities();

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API double erodRodSegmentGetInitialMinRestLength(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> double {
        return segment->rod.initialMinRestLength();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodRodSegmentGetEdgeMaterial(RodLinkage::RodSegment *segment, int idx, double **outMatData, double **outCoords, int **outEdges, int *numMatData, int *numCoords, int *numEdges)
    {
        interop::clear_error();
        if (outMatData) *outMatData = {};
        if (outCoords) *outCoords = {};
        if (outEdges) *outEdges = {};
        if (numMatData) *numMatData = {};
        if (numCoords) *numCoords = {};
        if (numEdges) *numEdges = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            interop::index(idx,segment->rod.edgeMaterials().size());
            if (!outMatData) throw std::invalid_argument("Missing output: outMatData");
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            if (!outEdges) throw std::invalid_argument("Missing output: outEdges");
            if (!numMatData) throw std::invalid_argument("Missing output: numMatData");
            if (!numCoords) throw std::invalid_argument("Missing output: numCoords");
            if (!numEdges) throw std::invalid_argument("Missing output: numEdges");
            [&]() -> void {
        const auto eMat = segment->rod.edgeMaterials();

        // Material Data (11): Area, StretchingStiffness, TwistingStiffness, BendingStiffness, MomentOfInertia, TorsionStressCoefficient
        //                     YoungModulus, ShearModulus, CrossSectionHeight, CrossSectionBoundaryPts, CrossSectionBoundaryEdges
        std::vector<double> matData;
        matData.reserve(11);

        const auto m = eMat[idx];
        // Area
        matData.push_back(m.area);
        // StretchingStiffness
        matData.push_back(m.stretchingStiffness);
        // TwistingStiffness
        matData.push_back(m.twistingStiffness);
        // BendingStiffness (Diagonalized Tensor)
        matData.push_back(m.bendingStiffness.lambda_1);
        matData.push_back(m.bendingStiffness.lambda_2);
        // MomentOfInertia (Diagonalized Tensor)
        matData.push_back(m.momentOfInertia.lambda_1);
        matData.push_back(m.momentOfInertia.lambda_2);
        // TorsionStressCoefficient
        matData.push_back(m.torsionStressCoefficient);
        // YoungModulus
        matData.push_back(m.youngModulus);
        // ShearModulus
        matData.push_back(m.shearModulus);
        // CrossSectionHeight
        matData.push_back(m.crossSectionHeight);

        *numMatData = interop::checked_count(matData.size());
        auto sizeData = (matData.size()) * sizeof(double);
        *outMatData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outMatData, matData.data(), sizeData);

        // CrossSectionBoundaryPts
        const auto pts = m.crossSectionBoundaryPts;
        size_t pCount = pts.size();

        std::vector<double> pData;
        pData.reserve(pCount * 2);

        for (size_t i = 0; i < pCount; i++)
        {
            const auto p = pts[i];
            pData.push_back(p(0, 0));
            pData.push_back(p(1, 0));
        }

        *numCoords = interop::checked_count(pData.size());
        auto sizePData = (pData.size()) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizePData));
        interop::copy(*outCoords, pData.data(), sizePData);

        // CrossSectionBoundaryEdges
        const auto edges = m.crossSectionBoundaryEdges;
        size_t eCount = edges.size();

        std::vector<int> eData;
        eData.reserve(eCount * 2);

        for (size_t i = 0; i < eCount; i++)
        {
            const auto e = edges[i];
            eData.push_back(static_cast<int>(e.first));
            eData.push_back(static_cast<int>(e.second));
        }

        *numEdges = interop::checked_count(eData.size());
        auto sizeEData = (eData.size()) * sizeof(int);
        *outEdges = static_cast<int *>(interop::allocate(sizeEData));
        interop::copy(*outEdges, eData.data(), sizeEData);

            }();
            if (interop::has_error()) {
        if (outMatData) *outMatData = {};
        if (outCoords) *outCoords = {};
        if (outEdges) *outEdges = {};
        if (numMatData) *numMatData = {};
        if (numCoords) *numCoords = {};
        if (numEdges) *numEdges = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outMatData) *outMatData = {};
        if (outCoords) *outCoords = {};
        if (outEdges) *outEdges = {};
        if (numMatData) *numMatData = {};
        if (numCoords) *numCoords = {};
        if (numEdges) *numEdges = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outMatData) *outMatData = {};
        if (outCoords) *outCoords = {};
        if (outEdges) *outEdges = {};
        if (numMatData) *numMatData = {};
        if (numCoords) *numCoords = {};
        if (numEdges) *numEdges = {};

            return;
        }
    }

    EROD_API int erodRodSegmentGetEdgeMaterialCount(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> int {
        const auto eMat = segment->rod.edgeMaterials();
        return eMat.size();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodRodSegmentGetDeformedState(RodLinkage::RodSegment *segment, double **outPtsCoords, double **outThetas, double **outTgtCoords,
                                                 double **outDirCoords, double **outSrcThetas, double **outSrcTwist,
                                                 int *outNumPtsCoords, int *outNumThetas, int *outNumTgtCoords,
                                                 int *outNumDirCoords, int *outNumSrcThetas, int *outNumSrcTwist)
    {
        interop::clear_error();
        if (outPtsCoords) *outPtsCoords = {};
        if (outThetas) *outThetas = {};
        if (outTgtCoords) *outTgtCoords = {};
        if (outDirCoords) *outDirCoords = {};
        if (outSrcThetas) *outSrcThetas = {};
        if (outSrcTwist) *outSrcTwist = {};
        if (outNumPtsCoords) *outNumPtsCoords = {};
        if (outNumThetas) *outNumThetas = {};
        if (outNumTgtCoords) *outNumTgtCoords = {};
        if (outNumDirCoords) *outNumDirCoords = {};
        if (outNumSrcThetas) *outNumSrcThetas = {};
        if (outNumSrcTwist) *outNumSrcTwist = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outPtsCoords) throw std::invalid_argument("Missing output: outPtsCoords");
            if (!outThetas) throw std::invalid_argument("Missing output: outThetas");
            if (!outTgtCoords) throw std::invalid_argument("Missing output: outTgtCoords");
            if (!outDirCoords) throw std::invalid_argument("Missing output: outDirCoords");
            if (!outSrcThetas) throw std::invalid_argument("Missing output: outSrcThetas");
            if (!outSrcTwist) throw std::invalid_argument("Missing output: outSrcTwist");
            if (!outNumPtsCoords) throw std::invalid_argument("Missing output: outNumPtsCoords");
            if (!outNumThetas) throw std::invalid_argument("Missing output: outNumThetas");
            if (!outNumTgtCoords) throw std::invalid_argument("Missing output: outNumTgtCoords");
            if (!outNumDirCoords) throw std::invalid_argument("Missing output: outNumDirCoords");
            if (!outNumSrcThetas) throw std::invalid_argument("Missing output: outNumSrcThetas");
            if (!outNumSrcTwist) throw std::invalid_argument("Missing output: outNumSrcTwist");
            [&]() -> void {
        const auto dc = segment->rod.deformedConfiguration();

        // Points
        const auto pts = dc.points();
        size_t count = pts.size();

        std::vector<double> pData;
        pData.reserve(count * 3);

        for (size_t i = 0; i < count; i++)
        {
            const auto p = pts[i];
            pData.push_back(p(0, 0));
            pData.push_back(p(1, 0));
            pData.push_back(p(2, 0));
        }

        *outNumPtsCoords = interop::checked_count(pData.size());
        auto sizePData = (pData.size()) * sizeof(double);
        *outPtsCoords = static_cast<double *>(interop::allocate(sizePData));
        interop::copy(*outPtsCoords, pData.data(), sizePData);

        // Thetas
        const auto thetas = dc.thetas();
        *outNumThetas = interop::checked_count(thetas.size());
        auto sizeThetas = (thetas.size()) * sizeof(double);
        *outThetas = static_cast<double *>(interop::allocate(sizeThetas));
        interop::copy(*outThetas, thetas.data(), sizeThetas);

        // Source tangent
        const auto tangents = dc.sourceTangent;
        count = tangents.size();

        std::vector<double> tData;
        tData.reserve(count * 3);

        for (size_t i = 0; i < count; i++)
        {
            const auto p = tangents[i];
            tData.push_back(p(0, 0));
            tData.push_back(p(1, 0));
            tData.push_back(p(2, 0));
        }

        *outNumTgtCoords = interop::checked_count(tData.size());
        auto sizeTData = (tData.size()) * sizeof(double);
        *outTgtCoords = static_cast<double *>(interop::allocate(sizeTData));
        interop::copy(*outTgtCoords, tData.data(), sizeTData);

        // Source Directors
        const auto dir = dc.sourceReferenceDirectors;
        count = dir.size();

        std::vector<double> dData;
        dData.reserve(count * 6);

        for (size_t i = 0; i < count; i++)
        {
            const auto d1 = dir[i].d1;
            dData.push_back(d1(0, 0));
            dData.push_back(d1(1, 0));
            dData.push_back(d1(2, 0));

            const auto d2 = dir[i].d2;
            dData.push_back(d2(0, 0));
            dData.push_back(d2(1, 0));
            dData.push_back(d2(2, 0));
        }

        *outNumDirCoords = interop::checked_count(dData.size());
        auto sizeDData = (dData.size()) * sizeof(double);
        *outDirCoords = static_cast<double *>(interop::allocate(sizeDData));
        interop::copy(*outDirCoords, dData.data(), sizeDData);

        // Source thetas
        const auto srcThetas = dc.sourceTheta;
        *outNumSrcThetas = interop::checked_count(srcThetas.size());
        auto sizeSrcThetas = (srcThetas.size()) * sizeof(double);
        *outSrcThetas = static_cast<double *>(interop::allocate(sizeSrcThetas));
        interop::copy(*outSrcThetas, srcThetas.data(), sizeSrcThetas);

        // Source twist
        const auto srcTwist = dc.sourceReferenceTwist;
        *outNumSrcTwist = interop::checked_count(srcTwist.size());
        auto sizeSrcTwist = (srcTwist.size()) * sizeof(double);
        *outSrcTwist = static_cast<double *>(interop::allocate(sizeSrcTwist));
        interop::copy(*outSrcTwist, srcTwist.data(), sizeSrcTwist);

            }();
            if (interop::has_error()) {
        if (outPtsCoords) *outPtsCoords = {};
        if (outThetas) *outThetas = {};
        if (outTgtCoords) *outTgtCoords = {};
        if (outDirCoords) *outDirCoords = {};
        if (outSrcThetas) *outSrcThetas = {};
        if (outSrcTwist) *outSrcTwist = {};
        if (outNumPtsCoords) *outNumPtsCoords = {};
        if (outNumThetas) *outNumThetas = {};
        if (outNumTgtCoords) *outNumTgtCoords = {};
        if (outNumDirCoords) *outNumDirCoords = {};
        if (outNumSrcThetas) *outNumSrcThetas = {};
        if (outNumSrcTwist) *outNumSrcTwist = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outPtsCoords) *outPtsCoords = {};
        if (outThetas) *outThetas = {};
        if (outTgtCoords) *outTgtCoords = {};
        if (outDirCoords) *outDirCoords = {};
        if (outSrcThetas) *outSrcThetas = {};
        if (outSrcTwist) *outSrcTwist = {};
        if (outNumPtsCoords) *outNumPtsCoords = {};
        if (outNumThetas) *outNumThetas = {};
        if (outNumTgtCoords) *outNumTgtCoords = {};
        if (outNumDirCoords) *outNumDirCoords = {};
        if (outNumSrcThetas) *outNumSrcThetas = {};
        if (outNumSrcTwist) *outNumSrcTwist = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outPtsCoords) *outPtsCoords = {};
        if (outThetas) *outThetas = {};
        if (outTgtCoords) *outTgtCoords = {};
        if (outDirCoords) *outDirCoords = {};
        if (outSrcThetas) *outSrcThetas = {};
        if (outSrcTwist) *outSrcTwist = {};
        if (outNumPtsCoords) *outNumPtsCoords = {};
        if (outNumThetas) *outNumThetas = {};
        if (outNumTgtCoords) *outNumTgtCoords = {};
        if (outNumDirCoords) *outNumDirCoords = {};
        if (outNumSrcThetas) *outNumSrcThetas = {};
        if (outNumSrcTwist) *outNumSrcTwist = {};

            return;
        }
    }

    EROD_API void erodRodSegmentGetVonMisesStresses(RodLinkage::RodSegment *segment, double **outData, int *numData){
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(segment);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        auto stresses = segment->rod.maxStresses(CrossSectionStressAnalysis::StressType::VonMises);
        // Stress Data
        *numData = interop::checked_count(stresses.size());
        auto sizeData = (*numData) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, stresses.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API double erodRodSegmentGetMaxStrain(RodLinkage::RodSegment *segment)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(segment);

            auto result=[&]() -> double {
        double max_mag = 0.0, max_val = 0.0;
        const auto &r = segment->rod;
        const size_t ne = r.numEdges();
        const auto &dc = r.deformedConfiguration();
        const auto &len = dc.len;
        const auto &restLen = r.restLengths();

        for (size_t j = 0; j < ne; ++j) {
            auto val = len[j] / restLen[j] - 1.0;
            if (std::abs(stripAutoDiff(val)) > max_mag) {
                max_mag = std::abs(stripAutoDiff(val));
                max_val = val;
            }
        }

        return max_val;

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    // ElasticRod
    EROD_API ElasticRod *erodElasticRodBuild(int numPoints, double *inCoords, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            if (numPoints<0) throw std::invalid_argument("Negative count: numPoints");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numPoints>0 && !inCoords) throw std::invalid_argument("Null input buffer: inCoords");
            if(numPoints<3) throw std::invalid_argument("A rod needs at least three points");
            auto result=[&]() -> ElasticRod * {
        try
        {
            std::vector<ElasticRod::Pt3> points;
            points.reserve(numPoints);
            for (int i = 0; i < numPoints; ++i)
            {
                points.emplace_back(inCoords[3 * i], inCoords[3 * i + 1], inCoords[3 * i + 2]);
            }

            *errorMessage = "Elastic Rod Built";

            return interop::own(new ElasticRod(points));
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    EROD_API ElasticRod *erodElasticRodCopy(ElasticRod *rod, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            auto result=[&]() -> ElasticRod * {
        try
        {
            *errorMessage = "Elastic Rod Copied";
            return interop::own(new ElasticRod(*rod));
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    EROD_API void erodElasticRodRemoveRestCurvatures(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            [&]() -> void {
        const auto nv = rod->numVertices();
        ElasticRod::StdVectorVector2D restKappa;
        restKappa.assign(nv, ElasticRod::Vec2::Zero());
        rod->setRestKappas(restKappa);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodElasticRodGetMeshData(ElasticRod *rod, double **outCoords, int **outQuads, int *numCoords, int *numQuads)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            if (!outQuads) throw std::invalid_argument("Missing output: outQuads");
            if (!numCoords) throw std::invalid_argument("Missing output: numCoords");
            if (!numQuads) throw std::invalid_argument("Missing output: numQuads");
            [&]() -> void {
        std::vector<MeshIO::IOVertex> vertices;
        std::vector<MeshIO::IOElement> quads;
        rod->visualizationGeometry(vertices, quads, true, true);

        *numCoords = interop::checked_count(vertices.size() * 3);
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        std::vector<double> coords;
        for (size_t i = 0; i < vertices.size(); i++)
        {
            auto v = vertices[i].point;
            coords.push_back(v[0]);
            coords.push_back(v[1]);
            coords.push_back(v[2]);
        }
        interop::copy(*outCoords, coords.data(), sizeCoords);

        *numQuads = interop::checked_count(quads.size() * 4);
        auto sizeQuads = (*numQuads) * sizeof(int);
        *outQuads = static_cast<int *>(interop::allocate(sizeQuads));
        std::vector<int> quadsIdx;
        for (size_t i = 0; i < quads.size(); i++)
        {
            auto q = quads[i];
            quadsIdx.push_back(q[0]);
            quadsIdx.push_back(q[1]);
            quadsIdx.push_back(q[2]);
            quadsIdx.push_back(q[3]);
        }
        interop::copy(*outQuads, quadsIdx.data(), sizeQuads);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};

            return;
        }
    }

    EROD_API void erodElasticRodSetMaterial(ElasticRod *rod, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numParams<0) throw std::invalid_argument("Negative count: numParams");
            if (!std::isfinite(E)) throw std::invalid_argument("Non-finite parameter: E");
            if (!std::isfinite(nu)) throw std::invalid_argument("Non-finite parameter: nu");
            if (numParams>0 && !sectionParams) throw std::invalid_argument("Null input buffer: sectionParams");
            [&]() -> void {
        std::string type;
        switch (sectionType)
        {
        case 0:
            type = "RECTANGLE";
            break;
        case 1:
            type = "ELLIPSE";
            break;
        case 2:
            type = "I";
            break;
        case 3:
            type = "L";
            break;
        case 4:
            type = "+";
            break;
        default:
            type = "RECTANGLE";
        }

        std::vector<Real> inParams;
        inParams.reserve(numParams);
        for (int i = 0; i < numParams; i++)
            inParams.push_back(sectionParams[i]);

        RodMaterial::StiffAxis stiffAxis;
        switch (axisType)
        {
        case 0:
            stiffAxis = RodMaterial::StiffAxis::D1;
            break;
        case 1:
            stiffAxis = RodMaterial::StiffAxis::D2;
            break;
        default:
            stiffAxis = RodMaterial::StiffAxis::D1;
        }
        bool keepCrossSectionMesh = true;

        rod->setMaterial(RodMaterial(type, E, nu, inParams, stiffAxis, keepCrossSectionMesh));

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API int erodElasticRodGetDoFCount(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> int {
        return rod->numDoF();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodElasticRodGetEdgesCount(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> int {
        return rod->numEdges();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodElasticRodGetVerticesCount(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> int {
        return rod->numVertices();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodElasticRodGetRestLengths(ElasticRod *rod, double *lengths, const int numEdges)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numEdges<0) throw std::invalid_argument("Negative count: numEdges");
            if (numEdges>0 && !lengths) throw std::invalid_argument("Null input buffer: lengths");
            [&]() -> void {
        const auto r = rod->restLengths();
        for (size_t i = 0; i < numEdges; i++)
        {
            lengths[i] = r[i];
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodElasticRodGetStretchingStresses(ElasticRod *rod, double *stresses, const int numEdges)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numEdges<0) throw std::invalid_argument("Negative count: numEdges");
            if (numEdges>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = rod->stretchingStresses();
        for (size_t i = 0; i < numEdges; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodElasticRodGetTwistingStresses(ElasticRod *rod, double *stresses, const int numVertices)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numVertices>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = rod->twistingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodElasticRodGetCenterLinePositions(ElasticRod *rod, double *outCoords, const int numCoords)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numCoords<0) throw std::invalid_argument("Negative count: numCoords");
            if (numCoords>0 && !outCoords) throw std::invalid_argument("Null input buffer: outCoords");
            [&]() -> void {
        for (size_t i = 0; i < numCoords; i++)
        {
            const auto p = rod->deformedPoint(i);
            outCoords[i * 3] = p(0, 0);
            outCoords[i * 3 + 1] = p(1, 0);
            outCoords[i * 3 + 2] = p(2, 0);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodElasticRodGetMaxBendingStresses(ElasticRod *rod, double *stresses, const int numVertices)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numVertices>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = rod->maxBendingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodElasticRodGetMinBendingStresses(ElasticRod *rod, double *stresses, const int numVertices)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numVertices>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = rod->minBendingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodElasticRodGetSqrtBendingEnergies(ElasticRod *rod, double *stresses, const int numVertices)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numVertices>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = stripAutoDiff(rod->energyBendPerVertex()).array().sqrt().eval();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API double erodElasticRodGetEnergy(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> double {
        return rod->energy();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodElasticRodGetEnergyBend(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> double {
        return rod->energyBend();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodElasticRodGetEnergyStretch(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> double {
        return rod->energyStretch();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodElasticRodGetEnergyTwist(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> double {
        return rod->energyTwist();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodElasticRodGetDoFs(ElasticRod *rod, double **outDoFs, int *numDoFs){
        interop::clear_error();
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outDoFs) throw std::invalid_argument("Missing output: outDoFs");
            if (!numDoFs) throw std::invalid_argument("Missing output: numDoFs");
            [&]() -> void {
        auto dofs = rod->getDoFs();
        *numDoFs = interop::checked_count(dofs.size());
        auto sizeDoFs = (*numDoFs) * sizeof(double);
        *outDoFs = static_cast<double *>(interop::allocate(sizeDoFs));
        interop::copy(*outDoFs, dofs.data(), sizeDoFs);

            }();
            if (interop::has_error()) {
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};

            return;
        }
    }

    EROD_API void erodElasticRodSetDoFs(ElasticRod *rod, double *inDoFs, int numDoFs){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);
            if (numDoFs<0) throw std::invalid_argument("Negative count: numDoFs");
            if (numDoFs>0 && !inDoFs) throw std::invalid_argument("Null input buffer: inDoFs");
            [&]() -> void {
        Eigen::VectorXd dofs = Eigen::Map<Eigen::VectorXd>(inDoFs, numDoFs, 1);
        rod->setDoFs(dofs);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodElasticRodGetMaterialFrame(ElasticRod *rod, int *outCoordsCount, double **outCoordsD1, double **outCoordsD2)
    {
        interop::clear_error();
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outCoordsCount) throw std::invalid_argument("Missing output: outCoordsCount");
            if (!outCoordsD1) throw std::invalid_argument("Missing output: outCoordsD1");
            if (!outCoordsD2) throw std::invalid_argument("Missing output: outCoordsD2");
            [&]() -> void {
        const auto numEdges = rod->numEdges();
        std::vector<double> coordsD1, coordsD2;
        coordsD1.reserve(numEdges * 3);
        coordsD2.reserve(numEdges* 3);
        for (size_t i = 0; i < numEdges; i++)
        {
            const auto d1 = rod->deformedMaterialFrameD1(i);
            coordsD1.push_back(d1.x());
            coordsD1.push_back(d1.y());
            coordsD1.push_back(d1.z());

            const auto d2 = rod->deformedMaterialFrameD2(i);
            coordsD2.push_back(d2.x());
            coordsD2.push_back(d2.y());
            coordsD2.push_back(d2.z());
        }

        *outCoordsCount = interop::checked_count(coordsD1.size());
        auto sizeCoords = (*outCoordsCount) * sizeof(double);
        *outCoordsD1 = static_cast<double *>(interop::allocate(sizeCoords));
        *outCoordsD2 = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoordsD1, coordsD1.data(), sizeCoords);
        interop::copy(*outCoordsD2, coordsD2.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};

            return;
        }
    }

    EROD_API double erodElasticRodGetMaxStrain(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> double {
        double max_mag = 0.0, max_val = 0.0;
        const size_t ne = rod->numEdges();
        const auto &dc = rod->deformedConfiguration();
        const auto &len = dc.len;
        const auto &restLen = rod->restLengths();

        for (size_t j = 0; j < ne; ++j) {
            auto val = len[j] / restLen[j] - 1.0;
            if (std::abs(stripAutoDiff(val)) > max_mag) {
                max_mag = std::abs(stripAutoDiff(val));
                max_val = val;
            }
        }

        return max_val;

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodElasticRodGetBendingStiffness(ElasticRod *rod, double **lambda1, double **lambda2, int *numLambda1, int *numLambda2)
    {
        interop::clear_error();
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!lambda1) throw std::invalid_argument("Missing output: lambda1");
            if (!lambda2) throw std::invalid_argument("Missing output: lambda2");
            if (!numLambda1) throw std::invalid_argument("Missing output: numLambda1");
            if (!numLambda2) throw std::invalid_argument("Missing output: numLambda2");
            [&]() -> void {
        const auto s = rod->bendingStiffnesses();
        size_t count = s.size();

        std::vector<double> lA, lB;
        for (size_t i = 0; i < count; i++)
        {
            lA.push_back(s[i].lambda_1);
            lB.push_back(s[i].lambda_2);
        }

        *numLambda1 = interop::checked_count(lA.size());
        auto sizeL1 = (lA.size()) * sizeof(double);
        *lambda1 = static_cast<double *>(interop::allocate(sizeL1));
        interop::copy(*lambda1, lA.data(), sizeL1);

        *numLambda2 = interop::checked_count(lB.size());
        auto sizeL2 = (lB.size()) * sizeof(double);
        *lambda2 = static_cast<double *>(interop::allocate(sizeL2));
        interop::copy(*lambda2, lB.data(), sizeL2);

            }();
            if (interop::has_error()) {
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetTwistingStiffness(ElasticRod *rod, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = rod->twistingStiffnesses();
        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetStretchingStiffness(ElasticRod *rod, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = rod->stretchingStiffnesses();
        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetVonMisesStresses(ElasticRod *rod, double **outData, int *numData){
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        auto stresses = rod->maxStresses(CrossSectionStressAnalysis::StressType::VonMises);
        // Stress Data
        *numData = interop::checked_count(stresses.size());
        auto sizeData = (*numData) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, stresses.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API double erodElasticRodGetInitialMinRestLength(ElasticRod *rod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> double {
        return rod->initialMinRestLength();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodElasticRodGetRestKappas(ElasticRod *rod, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto kappas = rod->restKappas();
        size_t count = kappas.size();

        std::vector<double> data;
        data.reserve(count * 2);

        for (size_t i = 0; i < count; i++)
        {
            const auto p = kappas[i];
            data.push_back(p(0, 0));
            data.push_back(p(1, 0));
        }

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetScalarFieldSqrtBendingEnergies(ElasticRod *rod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(rod->visualizationField(rod->sqrtBendingEnergies()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetScalarFieldMaxBendingStresses(ElasticRod *rod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(rod->visualizationField(rod->maxBendingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetScalarFieldVonMisesStresses(ElasticRod *rod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(rod->visualizationField(rod->maxVonMisesStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetScalarFieldMinBendingStresses(ElasticRod *rod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(rod->visualizationField(rod->minBendingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetScalarFieldTwistingStresses(ElasticRod *rod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(rod->visualizationField(rod->twistingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodElasticRodGetScalarFieldStretchingStresses(ElasticRod *rod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(rod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(rod->visualizationField(rod->stretchingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API int erodElasticRodGetThetaOffset(ElasticRod *rod){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> int {
        return rod->thetaOffset();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodElasticRodGetRestLengthOffset(ElasticRod *rod){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> int {
        return rod->restLenOffset();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodElasticRodGetRestKappaOffset(ElasticRod *rod){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(rod);

            auto result=[&]() -> int {
        return rod->restKappaOffset();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    // Periodic ElasticRod
    EROD_API PeriodicRod *erodPeriodicElasticRodBuild(int numPoints, double *inCoords, int removeCurvature, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            if (numPoints<0) throw std::invalid_argument("Negative count: numPoints");
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            if (numPoints>0 && !inCoords) throw std::invalid_argument("Null input buffer: inCoords");
            if(numPoints<3) throw std::invalid_argument("A rod needs at least three points");
            auto result=[&]() -> PeriodicRod * {
        try
        {
            std::vector<PeriodicRod::Pt3> points;
            points.reserve(numPoints);
            for (int i = 0; i < numPoints; ++i)
            {
                points.emplace_back(inCoords[3 * i], inCoords[3 * i + 1], inCoords[3 * i + 2]);
            }

            *errorMessage = "Periodic Rod Built";
            return interop::own(new PeriodicRod(points, removeCurvature));
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    EROD_API PeriodicRod *erodPeriodicElasticRodCopy(PeriodicRod *pRod, const char **errorMessage)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!errorMessage) throw std::invalid_argument("Missing output: errorMessage");
            auto result=[&]() -> PeriodicRod * {
        try
        {
            *errorMessage = "Periodic Rod Copied";
            return interop::own(new PeriodicRod(*pRod));
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }
        catch (const std::exception &error)
        {
            *errorMessage = interop::record_error(error.what());
            return nullptr;
        }

            }();
            if (interop::has_error()) {

            return nullptr;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        } catch (...) {
            interop::record_error("Unknown native exception");

            if (errorMessage) *errorMessage=interop::error_storage();
            return nullptr;
        }
    }

    EROD_API void erodPeriodicElasticRodGetMeshData(PeriodicRod *pRod, double **outCoords, int **outQuads, int *numCoords, int *numQuads)
    {
        interop::clear_error();
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outCoords) throw std::invalid_argument("Missing output: outCoords");
            if (!outQuads) throw std::invalid_argument("Missing output: outQuads");
            if (!numCoords) throw std::invalid_argument("Missing output: numCoords");
            if (!numQuads) throw std::invalid_argument("Missing output: numQuads");
            [&]() -> void {
        std::vector<MeshIO::IOVertex> vertices;
        std::vector<MeshIO::IOElement> quads;
        pRod->rod.visualizationGeometry(vertices, quads, true, true);

        *numCoords = interop::checked_count(vertices.size() * 3);
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(interop::allocate(sizeCoords));
        std::vector<double> coords;
        for (size_t i = 0; i < vertices.size(); i++)
        {
            auto v = vertices[i].point;
            coords.push_back(v[0]);
            coords.push_back(v[1]);
            coords.push_back(v[2]);
        }
        interop::copy(*outCoords, coords.data(), sizeCoords);

        *numQuads = interop::checked_count(quads.size() * 4);
        auto sizeQuads = (*numQuads) * sizeof(int);
        *outQuads = static_cast<int *>(interop::allocate(sizeQuads));
        std::vector<int> quadsIdx;
        for (size_t i = 0; i < quads.size(); i++)
        {
            auto q = quads[i];
            quadsIdx.push_back(q[0]);
            quadsIdx.push_back(q[1]);
            quadsIdx.push_back(q[2]);
            quadsIdx.push_back(q[3]);
        }
        interop::copy(*outQuads, quadsIdx.data(), sizeQuads);

            }();
            if (interop::has_error()) {
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoords) *outCoords = {};
        if (outQuads) *outQuads = {};
        if (numCoords) *numCoords = {};
        if (numQuads) *numQuads = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodSetMaterial(PeriodicRod *pRod, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numParams<0) throw std::invalid_argument("Negative count: numParams");
            if (!std::isfinite(E)) throw std::invalid_argument("Non-finite parameter: E");
            if (!std::isfinite(nu)) throw std::invalid_argument("Non-finite parameter: nu");
            if (numParams>0 && !sectionParams) throw std::invalid_argument("Null input buffer: sectionParams");
            [&]() -> void {
        std::string type;
        switch (sectionType)
        {
        case 0:
            type = "RECTANGLE";
            break;
        case 1:
            type = "ELLIPSE";
            break;
        case 2:
            type = "I";
            break;
        case 3:
            type = "L";
            break;
        case 4:
            type = "+";
            break;
        default:
            type = "RECTANGLE";
        }

        std::vector<Real> inParams;
        inParams.reserve(numParams);
        for (int i = 0; i < numParams; i++)
            inParams.push_back(sectionParams[i]);

        RodMaterial::StiffAxis stiffAxis;
        switch (axisType)
        {
        case 0:
            stiffAxis = RodMaterial::StiffAxis::D1;
            break;
        case 1:
            stiffAxis = RodMaterial::StiffAxis::D2;
            break;
        default:
            stiffAxis = RodMaterial::StiffAxis::D1;
        }
        bool keepCrossSectionMesh = true;

        pRod->setMaterial(RodMaterial(type, E, nu, inParams, stiffAxis, keepCrossSectionMesh));

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API int erodPeriodicElasticRodGetDoFCount(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> int {
        return pRod->numDoF();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodPeriodicElasticRodGetEdgesCount(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> int {
        return pRod->rod.numEdges();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodPeriodicElasticRodGetVerticesCount(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> int {
        return pRod->rod.numVertices();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API void erodPeriodicElasticRodGetRestLengths(PeriodicRod *pRod, double *lengths, const int numEdges)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numEdges<0) throw std::invalid_argument("Negative count: numEdges");
            if (numEdges>0 && !lengths) throw std::invalid_argument("Null input buffer: lengths");
            [&]() -> void {
        const auto r = pRod->rod.restLengths();
        for (size_t i = 0; i < numEdges; i++)
        {
            lengths[i] = r[i];
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetStretchingStresses(PeriodicRod *pRod, double *stresses, const int numEdges)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numEdges<0) throw std::invalid_argument("Negative count: numEdges");
            if (numEdges>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = pRod->rod.stretchingStresses();
        for (size_t i = 0; i < numEdges; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetTwistingStresses(PeriodicRod *pRod, double *stresses, const int numVertices)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numVertices>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = pRod->rod.twistingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetCenterLinePositions(PeriodicRod *pRod, double *outCoords, const int numCoords)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numCoords<0) throw std::invalid_argument("Negative count: numCoords");
            if (numCoords>0 && !outCoords) throw std::invalid_argument("Null input buffer: outCoords");
            [&]() -> void {
        for (size_t i = 0; i < numCoords; i++)
        {
            const auto p = pRod->rod.deformedPoint(i);
            outCoords[i * 3] = p(0, 0);
            outCoords[i * 3 + 1] = p(1, 0);
            outCoords[i * 3 + 2] = p(2, 0);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetMaxBendingStresses(PeriodicRod *pRod, double *stresses, const int numVertices)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numVertices>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = pRod->rod.maxBendingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetMinBendingStresses(PeriodicRod *pRod, double *stresses, const int numVertices)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numVertices>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = pRod->rod.minBendingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetSqrtBendingEnergies(PeriodicRod *pRod, double *stresses, const int numVertices)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numVertices<0) throw std::invalid_argument("Negative count: numVertices");
            if (numVertices>0 && !stresses) throw std::invalid_argument("Null input buffer: stresses");
            [&]() -> void {
        const auto s = stripAutoDiff(pRod->rod.energyBendPerVertex()).array().sqrt().eval();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetDoFs(PeriodicRod *pRod, double **outDoFs, int *numDoFs){
        interop::clear_error();
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outDoFs) throw std::invalid_argument("Missing output: outDoFs");
            if (!numDoFs) throw std::invalid_argument("Missing output: numDoFs");
            [&]() -> void {
        auto dofs = pRod->getDoFs();
        *numDoFs = interop::checked_count(dofs.size());
        auto sizeDoFs = (*numDoFs) * sizeof(double);
        *outDoFs = static_cast<double *>(interop::allocate(sizeDoFs));
        interop::copy(*outDoFs, dofs.data(), sizeDoFs);

            }();
            if (interop::has_error()) {
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outDoFs) *outDoFs = {};
        if (numDoFs) *numDoFs = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodSetDoFs(PeriodicRod *pRod, double *inDoFs, int numDoFs){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);
            if (numDoFs<0) throw std::invalid_argument("Negative count: numDoFs");
            if (numDoFs>0 && !inDoFs) throw std::invalid_argument("Null input buffer: inDoFs");
            [&]() -> void {
        Eigen::VectorXd dofs = Eigen::Map<Eigen::VectorXd>(inDoFs, numDoFs, 1);
        pRod->setDoFs(dofs);

            }();
            if (interop::has_error()) {

            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetMaterialFrame(PeriodicRod *pRod, int *outCoordsCount, double **outCoordsD1, double **outCoordsD2)
    {
        interop::clear_error();
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outCoordsCount) throw std::invalid_argument("Missing output: outCoordsCount");
            if (!outCoordsD1) throw std::invalid_argument("Missing output: outCoordsD1");
            if (!outCoordsD2) throw std::invalid_argument("Missing output: outCoordsD2");
            [&]() -> void {
        const auto numEdges = pRod->rod.numEdges();
        std::vector<double> coordsD1, coordsD2;
        coordsD1.reserve(numEdges * 3);
        coordsD2.reserve(numEdges * 3);
        for (size_t i = 0; i < numEdges; i++)
        {
            const auto d1 =pRod->rod.deformedMaterialFrameD1(i);
            coordsD1.push_back(d1.x());
            coordsD1.push_back(d1.y());
            coordsD1.push_back(d1.z());

            const auto d2 = pRod->rod.deformedMaterialFrameD2(i);
            coordsD2.push_back(d2.x());
            coordsD2.push_back(d2.y());
            coordsD2.push_back(d2.z());
        }

        *outCoordsCount = interop::checked_count(coordsD1.size());
        auto sizeCoords = (*outCoordsCount) * sizeof(double);
        *outCoordsD1 = static_cast<double *>(interop::allocate(sizeCoords));
        *outCoordsD2 = static_cast<double *>(interop::allocate(sizeCoords));
        interop::copy(*outCoordsD1, coordsD1.data(), sizeCoords);
        interop::copy(*outCoordsD2, coordsD2.data(), sizeCoords);

            }();
            if (interop::has_error()) {
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outCoordsCount) *outCoordsCount = {};
        if (outCoordsD1) *outCoordsD1 = {};
        if (outCoordsD2) *outCoordsD2 = {};

            return;
        }
    }

    EROD_API double erodPeriodicElasticRodGetEnergy(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> double {
        return pRod->rod.energy();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodPeriodicElasticRodGetEnergyBend(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> double {
        return pRod->rod.energyBend();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodPeriodicElasticRodGetEnergyStretch(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> double {
        return pRod->rod.energyStretch();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodPeriodicElasticRodGetEnergyTwist(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> double {
        return pRod->rod.energyTwist();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API double erodPeriodicElasticRodGetMaxStrain(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> double {
        double max_mag = 0.0, max_val = 0.0;
        const auto &r = pRod->rod;
        const size_t ne = r.numEdges();
        const auto &dc = r.deformedConfiguration();
        const auto &len = dc.len;
        const auto &restLen = r.restLengths();

        for (size_t j = 0; j < ne; ++j) {
            auto val = len[j] / restLen[j] - 1.0;
            if (std::abs(stripAutoDiff(val)) > max_mag) {
                max_mag = std::abs(stripAutoDiff(val));
                max_val = val;
            }
        }

        return max_val;

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodPeriodicElasticRodGetBendingStiffness(PeriodicRod *pRod, double **lambda1, double **lambda2, int *numLambda1, int *numLambda2)
    {
        interop::clear_error();
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!lambda1) throw std::invalid_argument("Missing output: lambda1");
            if (!lambda2) throw std::invalid_argument("Missing output: lambda2");
            if (!numLambda1) throw std::invalid_argument("Missing output: numLambda1");
            if (!numLambda2) throw std::invalid_argument("Missing output: numLambda2");
            [&]() -> void {
        const auto s = pRod->rod.bendingStiffnesses();
        size_t count = s.size();

        std::vector<double> lA, lB;
        for (size_t i = 0; i < count; i++)
        {
            lA.push_back(s[i].lambda_1);
            lB.push_back(s[i].lambda_2);
        }

        *numLambda1 = interop::checked_count(lA.size());
        auto sizeL1 = (lA.size()) * sizeof(double);
        *lambda1 = static_cast<double *>(interop::allocate(sizeL1));
        interop::copy(*lambda1, lA.data(), sizeL1);

        *numLambda2 = interop::checked_count(lB.size());
        auto sizeL2 = (lB.size()) * sizeof(double);
        *lambda2 = static_cast<double *>(interop::allocate(sizeL2));
        interop::copy(*lambda2, lB.data(), sizeL2);

            }();
            if (interop::has_error()) {
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (lambda1) *lambda1 = {};
        if (lambda2) *lambda2 = {};
        if (numLambda1) *numLambda1 = {};
        if (numLambda2) *numLambda2 = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetTwistingStiffness(PeriodicRod *pRod, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = pRod->rod.twistingStiffnesses();
        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetStretchingStiffness(PeriodicRod *pRod, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto data = pRod->rod.stretchingStiffnesses();
        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetVonMisesStresses(PeriodicRod *pRod, double **outData, int *numData){
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        auto stresses = pRod->rod.maxStresses(CrossSectionStressAnalysis::StressType::VonMises);
        // Stress Data
        *numData = interop::checked_count(stresses.size());
        auto sizeData = (*numData) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, stresses.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API double erodPeriodicElasticRodGetInitialMinRestLength(PeriodicRod *pRod)
    {
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> double {
        return pRod->rod.initialMinRestLength();

            }();
            if (interop::has_error()) {

            return 0;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return 0;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return 0;
        }
    }

    EROD_API void erodPeriodicElasticRodGetRestKappas(PeriodicRod *pRod, double **outData, int *numData)
    {
        interop::clear_error();
        if (outData) *outData = {};
        if (numData) *numData = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outData) throw std::invalid_argument("Missing output: outData");
            if (!numData) throw std::invalid_argument("Missing output: numData");
            [&]() -> void {
        const auto kappas = pRod->rod.restKappas();
        size_t count = kappas.size();

        std::vector<double> data;
        data.reserve(count * 2);

        for (size_t i = 0; i < count; i++)
        {
            const auto p = kappas[i];
            data.push_back(p(0, 0));
            data.push_back(p(1, 0));
        }

        *numData = interop::checked_count(data.size());
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(interop::allocate(sizeData));
        interop::copy(*outData, data.data(), sizeData);

            }();
            if (interop::has_error()) {
        if (outData) *outData = {};
        if (numData) *numData = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outData) *outData = {};
        if (numData) *numData = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(PeriodicRod *pRod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.sqrtBendingEnergies()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldMaxBendingStresses(PeriodicRod *pRod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.maxBendingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldVonMisesStresses(PeriodicRod *pRod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.maxVonMisesStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldMinBendingStresses(PeriodicRod *pRod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.minBendingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldTwistingStresses(PeriodicRod *pRod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.twistingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldStretchingStresses(PeriodicRod *pRod, double **outField, int *numField)
    {
        interop::clear_error();
        if (outField) *outField = {};
        if (numField) *numField = {};
        try {
            interop::Call allocations;
            interop::require(pRod);
            if (!outField) throw std::invalid_argument("Missing output: outField");
            if (!numField) throw std::invalid_argument("Missing output: numField");
            [&]() -> void {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.stretchingStresses()));

        *numField = interop::checked_count(field.size());
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(interop::allocate(sizeField));
        interop::copy(*outField, field.data(), sizeField);

            }();
            if (interop::has_error()) {
        if (outField) *outField = {};
        if (numField) *numField = {};
            return;
            }
            allocations.commit();
            return;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        } catch (...) {
            interop::record_error("Unknown native exception");
        if (outField) *outField = {};
        if (numField) *numField = {};

            return;
        }
    }

    EROD_API int erodPeriodicElasticRodGetThetaOffset(PeriodicRod *pRod){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> int {
        return pRod->rod.thetaOffset();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodPeriodicElasticRodGetRestLengthOffset(PeriodicRod *pRod){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> int {
        return pRod->rod.restLenOffset();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }

    EROD_API int erodPeriodicElasticRodGetRestKappaOffset(PeriodicRod *pRod){
        interop::clear_error();

        try {
            interop::Call allocations;
            interop::require(pRod);

            auto result=[&]() -> int {
        return pRod->rod.restKappaOffset();

            }();
            if (interop::has_error()) {

            return -1;
            }
            allocations.commit();
            return result;
        } catch (const std::exception &exception) {
            interop::record_error(exception.what());


            return -1;
        } catch (...) {
            interop::record_error("Unknown native exception");


            return -1;
        }
    }
}
