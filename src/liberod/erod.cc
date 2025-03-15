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

namespace ElasticRodsGH
{
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
        *outReport = static_cast<double *>(malloc(sizeReport));
        std::memcpy(*outReport, flatReport.data(), sizeReport);
    }

    // AttractedLinkage
    EROD_API SurfaceAttractedLinkage *erodXShellAttractedSurfaceBuild(int numVertices, int numTrias, double *inCoords, int *inTrias, RodLinkage *linkage, double tgt_joint_weight, const char **errorMessage)
    {
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
            auto attractedLinkage = new SurfaceAttractedLinkage(targetV, targetF, useCenterline, *linkage);
            attractedLinkage->set_attraction_tgt_joint_weight(tgt_joint_weight);
            return attractedLinkage;
        }
        catch(const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    EROD_API SurfaceAttractedLinkage *erodXShellAttractedSurfaceCopy(SurfaceAttractedLinkage *linkage, const char **errorMessage){
        try
        {
            *errorMessage = "Rod Linkage Copied";
            return new SurfaceAttractedLinkage(*linkage);
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    // Target Surface
    EROD_API int erodXShellInferTargetSurface(RodLinkage *linkage, size_t nsubdiv, size_t numExtensionLayers, double **outCoords, int **outTrias, size_t *numCoords, size_t *numTrias, const char **errorMessage)
    {
        try
        {
            std::vector<MeshIO::IOVertex> vertices;
            std::vector<MeshIO::IOElement> trias;
            infer_target_surface(*linkage, vertices, trias, nsubdiv, numExtensionLayers);

            *numCoords = vertices.size() * 3;
            auto sizeCoords = (*numCoords) * sizeof(double);
            *outCoords = static_cast<double *>(malloc(sizeCoords));
            std::vector<double> coords;
            for (size_t i = 0; i < vertices.size(); i++)
            {
                auto v = vertices[i].point;
                coords.push_back(v[0]);
                coords.push_back(v[1]);
                coords.push_back(v[2]);
            }
            std::memcpy(*outCoords, coords.data(), sizeCoords);

            *numTrias = trias.size() * 3;
            auto sizeTrias = (*numTrias) * sizeof(int);
            *outTrias = static_cast<int *>(malloc(sizeTrias));
            std::vector<int> triasIdx;
            for (size_t i = 0; i < trias.size(); i++)
            {
                auto q = trias[i];
                triasIdx.push_back(q[0]);
                triasIdx.push_back(q[1]);
                triasIdx.push_back(q[2]);
            }
            std::memcpy(*outTrias, triasIdx.data(), sizeTrias);

            *errorMessage = "";
            return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return 1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return 1;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return 1;
        }
    }

    // Linkage
    EROD_API RodLinkage *erodXShellBuildFromGraph(int numVertices, int numEdges, double *inCoords, int *inEdges, double *inNormals, int subdivision, int interleavingType, int initConsistentAngle, const char **errorMessage)
    {
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

            return new RodLinkage(vertices, edges, subdivision, consistentAngle, rod_interleaving_type, edge_callbacks, normals);
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    EROD_API RodLinkage *erodXShellCopy(RodLinkage *linkage, const char **errorMessage)
    {
        try
        {
            *errorMessage = "Rod Linkage Copied";
            return new RodLinkage(*linkage);
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    EROD_API RodLinkage *erodXShellBuild(int numVertices, int numEdges, double *inCoords, int *inEdges, double *inNormals,
                                                      double *inRestLengths, int *inOffsetInteriorCoords, double *inInteriorCoords,
                                                      int interleavingType, int initConsistentAngle, int initConsistentNormals, const char **errorMessage)
    {
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
            return new RodLinkage(vertices, normals, edges, rodPoints, restLengths, initConsistentAngle, initConsistentNormals, rod_interleaving_type);
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    EROD_API void erodXShellSetCustomMaterial(RodLinkage *linkage, double E, double nu, double *inCoords, int numVertices, double *inHolesCoords, int numHoles, int axisType)
    {
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
    }

    EROD_API void erodXShellSetMaterial(RodLinkage *linkage, int sectionType, double E, double nu, double *params, int numParams, int axisType)
    {
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
    }

    EROD_API void erodXShellSetJointMaterial(RodLinkage *linkage, size_t numMaterials, int *sectionType, double *E, double *nu, double *sectionParams, int *sectionParamsCount, int *axisType)
    {
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
    }

    EROD_API void erodXShellSetCustomJointMaterial(RodLinkage *linkage, size_t numMaterials, int *sectionType, double *E, double *nu, double *inCoords, int *inCoordsCount, double *inHolesCoords, int *inHolesCount, int *axisType)
    {
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
            idxV += numVertices;

            auto numHoles = inHolesCount[i];
            std::vector<Point2D> holesPts;
            holesPts.reserve(numHoles);
            for (int j = 0; j < numHoles; j++)
                holesPts.emplace_back(inHolesCoords[idxH + 2 * j], inHolesCoords[idxH + 2 * j + 1]);
            idxH += numHoles;

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
    }

    EROD_API void erodXShellSetDesignParameterConfig(RodLinkage *linkage, int use_restLen, int use_restKappa, int update_designParams_cache)
    {
        linkage->setDesignParameterConfig(use_restLen, use_restKappa, update_designParams_cache);
    }

    EROD_API int erodXShellGetCentralJointIndex(RodLinkage *linkage)
    {
        return linkage->centralJoint();
    }

    EROD_API void erodXShellGetDofOffsetForJoint(RodLinkage *linkage, int joint, int *DOF, int numDOF, int *outVars)
    {
        const size_t jdo = linkage->dofOffsetForJoint(joint);
        for (int i = 0; i < numDOF; i++)
            outVars[i] = jdo + DOF[i];
    }

    EROD_API void erodXShellGetDofOffsetForCenterLinePos(RodLinkage *linkage, int index, int *DOF, int numDOF, int *outVars)
    {
        const size_t jdo = linkage->dofOffsetForCenterlinePos(index);
        for (int i = 0; i < numDOF; i++)
            outVars[i] = jdo + DOF[i];
    }

    EROD_API void erodXShellGetCenterLinePositions(RodLinkage *linkage, double **outCoords, size_t *numCoords)
    {
        auto pos = linkage->centerLinePositions();
        *numCoords = pos.size();
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoords, pos.data(), sizeCoords);
    }

    EROD_API double erodXShellGetAverageJointAngle(RodLinkage *linkage)
    {
        return linkage->getAverageJointAngle();
    }

    EROD_API double erodXShellGetEnergy(RodLinkage *linkage)
    {
        return linkage->energy();
    }

    EROD_API void erodXShellGetDoFs(RodLinkage *linkage, double **outDoFs, size_t *numDoFs)
    {
        auto dofs = linkage->getDoFs();
        *numDoFs = dofs.size();
        auto sizeDoFs = (*numDoFs) * sizeof(double);
        *outDoFs = static_cast<double *>(malloc(sizeDoFs));
        std::memcpy(*outDoFs, dofs.data(), sizeDoFs);
    }

    EROD_API double erodXShellGetEnergyBend(RodLinkage *linkage)
    {
        return linkage->energyBend();
    }

    EROD_API double erodXShellGetEnergyStretch(RodLinkage *linkage)
    {
        return linkage->energyStretch();
    }

    EROD_API double erodXShellGetEnergyTwist(RodLinkage *linkage)
    {
        return linkage->energyTwist();
    }

    EROD_API double erodXShellGetMaxStrain(RodLinkage *linkage)

    {
        return linkage->maxStrain();
    }

    EROD_API size_t erodXShellGetCenterLinePositionsCount(RodLinkage *linkage)
    {
        return linkage->numCenterlinePos();
    }

    EROD_API size_t erodXShellGetJointsCount(RodLinkage *linkage)
    {
        return linkage->numJoints();
    }

    EROD_API size_t erodXShellGetDoFCount(RodLinkage *linkage)
    {
        return linkage->numDoF();
    }

    EROD_API size_t erodXShellGetRodSegmentsCount(RodLinkage *linkage)
    {
        return linkage->numSegments();
    }

    EROD_API void erodXShellGetMeshData(RodLinkage *linkage, double **outCoords, int **outQuads, size_t *numCoords, size_t *numQuads)
    {
        std::vector<MeshIO::IOVertex> vertices;
        std::vector<MeshIO::IOElement> quads;
        linkage->visualizationGeometry(vertices, quads, true, true);

        *numCoords = vertices.size() * 3;
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::vector<double> coords;
        for (size_t i = 0; i < vertices.size(); i++)
        {
            auto v = vertices[i].point;
            coords.push_back(v[0]);
            coords.push_back(v[1]);
            coords.push_back(v[2]);
        }
        std::memcpy(*outCoords, coords.data(), sizeCoords);

        *numQuads = quads.size() * 4;
        auto sizeQuads = (*numQuads) * sizeof(int);
        *outQuads = static_cast<int *>(malloc(sizeQuads));
        std::vector<int> quadsIdx;
        for (size_t i = 0; i < quads.size(); i++)
        {
            auto q = quads[i];
            quadsIdx.push_back(q[0]);
            quadsIdx.push_back(q[1]);
            quadsIdx.push_back(q[2]);
            quadsIdx.push_back(q[3]);
        }
        std::memcpy(*outQuads, quadsIdx.data(), sizeQuads);
    }

    EROD_API void erodXShellGetRodSegmentIndexesPerRod(RodLinkage *linkage, int index, int **segmentIndexes, size_t *numSeg, int *type)
    {
        const auto data = linkage->traceRods()[index];
        *type = std::get<0>(data);
        const auto vec = std::get<1>(data);

        *numSeg = vec.size();
        auto size = (*numSeg) * sizeof(int);
        *segmentIndexes = static_cast<int *>(malloc(size));
        std::memcpy(*segmentIndexes, vec.data(), size);
    }

    EROD_API size_t erodXShellGetRodTraceCount(RodLinkage *linkage, const char **errorMessage)
    {
        try
        {
            *errorMessage = "Trace Built";
            return linkage->traceRods().size();
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return -1;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return -1;
        }
    }

    EROD_API void erodXShellRemoveRestCurvatures(RodLinkage *linkage)
    {
        for (auto &seg : linkage->segments())
        {
            auto r = seg.rod;

            const auto nv = r.numVertices();
            ElasticRod::StdVectorVector2D restKappa;
            restKappa.assign(nv, ElasticRod::Vec2::Zero());

            r.setRestKappas(restKappa);
        }
    }

    EROD_API size_t erodXShellHessianNNZ(RodLinkage *linkage, int variableDesignParameters)
    {
        return linkage->hessianNNZ(variableDesignParameters);
    }

    EROD_API size_t erodXShellGetRestKappaVarsCount(RodLinkage *linkage)
    {
        return linkage->numRestKappaVars();
    }

    EROD_API void erodXShellGetRestLengthsSolveDoFs(RodLinkage *linkage, double **outDoFs, size_t *numDoFs){
        const auto data = linkage->getRestlenSolveDoF();
        
        *numDoFs = data.size();
        auto sizeData = (*numDoFs) * sizeof(double);
        *outDoFs = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outDoFs, data.data(), sizeData);
    }

    EROD_API void erodXShellGetPerSegmentRestLengths(RodLinkage *linkage, double **outLengths, size_t *numLengths){
        const auto data = linkage->getPerSegmentRestLength();
        
        *numLengths = data.size();
        auto sizeData = (*numLengths) * sizeof(double);
        *outLengths = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outLengths, data.data(), sizeData);
    }

    EROD_API void erodXShellSetRestLengthsSolveDoFs(RodLinkage *linkage, double *inDoFs, size_t numDoFs){
        Eigen::VectorXd dofs = Eigen::Map<Eigen::VectorXd>(inDoFs, numDoFs, 1);
        linkage->setRestlenSolveDoF(dofs);
    }

    EROD_API void erodXShellSetPerSegmentRestLengths(RodLinkage *linkage, double *inLengths, size_t numLengths){
        Eigen::VectorXd lengths = Eigen::Map<Eigen::VectorXd>(inLengths, numLengths, 1);
        linkage->setPerSegmentRestLength(lengths);
    }

    EROD_API void erodXShellSetDoFs(RodLinkage *linkage, double *inDoFs, size_t numDoFs){
        Eigen::VectorXd dofs = Eigen::Map<Eigen::VectorXd>(inDoFs, numDoFs, 1);
        linkage->setDoFs(dofs);
    }

    EROD_API void erodXShellGetRestKappaVars(RodLinkage *linkage, double **outData, size_t *numData)
    {
        const auto data = linkage->getRestKappaVars();

        *numData = data.size();
        auto sizeData = (*numData) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API double erodXShellGetMinJointAngle(RodLinkage *linkage)
    {
        return linkage->getMinJointAngle();
    }

    EROD_API double erodXShellGetTotalRestLength(RodLinkage *linkage)
    {
        return linkage->totalRestLength();
    }

    EROD_API double erodXShellGetMaxRodEnergy(RodLinkage *linkage)
    {
        return linkage->maxRodEnergy();
    }

    EROD_API void erodXShellSetStiffenRegions(RodLinkage *linkage, double factor, double *coords, size_t numBoxes)
    {
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
            start += 8;
        }

        RectangularBoxCollection bboxes(corners);
        linkage->stiffenRegions(bboxes, factor);
    }

    EROD_API double erodXShellStripAutoDiff(double *edgeA, double *edgeB)
    {
        RodLinkage::Vec3 tA(edgeA[0], edgeA[1], edgeA[2]),
            tB(edgeB[0], edgeB[1], edgeB[2]);
        return stripAutoDiff(tA.dot(tB));
    }

    EROD_API int erodXShelNumberDesignParameters(RodLinkage *linkage)
    {
        return linkage->numDesignParams();
    }

    EROD_API void erodXShellGetDesignParams(RodLinkage *linkage, double **outDesignParams, size_t *outNumDesignParameters)
    {
        const auto data = linkage->getDesignParameters();

        *outNumDesignParameters = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outDesignParams = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outDesignParams, data.data(), sizeData);
    }

    EROD_API void erodXShellScalarFieldSqrtBendingEnergies(RodLinkage *linkage, double **outField, size_t *numField)
    {
        ScalarField<Real> field(linkage->visualizationField(linkage->sqrtBendingEnergies()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodXShellScalarFieldMaxBendingStresses(RodLinkage *linkage, double **outField, size_t *numField)
    {
        ScalarField<Real> field(linkage->visualizationField(linkage->maxBendingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodXShellScalarFieldVonMisesStresses(RodLinkage *linkage, double **outField, size_t *numField)
    {
        ScalarField<Real> field(linkage->visualizationField(linkage->maxVonMisesStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodXShellScalarFieldMinBendingStresses(RodLinkage *linkage, double **outField, size_t *numField)
    {
        ScalarField<Real> field(linkage->visualizationField(linkage->minBendingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodXShellScalarFieldTwistingStresses(RodLinkage *linkage, double **outField, size_t *numField)
    {
        ScalarField<Real> field(linkage->visualizationField(linkage->twistingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodXShellScalarFieldStretchingStresses(RodLinkage *linkage, double **outField, size_t *numField)
    {
        ScalarField<Real> field(linkage->visualizationField(linkage->stretchingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API double erodXShellGetInitialMinRestLength(RodLinkage *linkage)
    {
        return linkage->initialMinRestLength();
    }

    EROD_API int erodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(RodLinkage *linkage, double **outAx, long **outAi, long **outAp, long *outM, long *outN, long *outNZ, const char **errorMessage)
    {
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
            *outAx = static_cast<double *>(malloc(sizeAx));
            std::memcpy(*outAx, Ax.data(), sizeAx);

            auto sizeAi = (Ai.size()) * sizeof(long);
            *outAi = static_cast<long *>(malloc(sizeAi));
            std::memcpy(*outAi, Ai.data(), sizeAi);

            auto sizeAp = (Ap.size()) * sizeof(long);
            *outAp = static_cast<long *>(malloc(sizeAp));
            std::memcpy(*outAp, Ap.data(), sizeAp);

            *errorMessage = "";
            return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return 1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return 1;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return 1;
        }
    }

    EROD_API void erodXShellGetDesignParameterConfig(RodLinkage *linkage, int *use_restLen, int *use_restKappa)
    {
        const auto dpc = linkage->getDesignParameterConfig();

        *use_restLen = dpc.restLen;
        *use_restKappa = dpc.restKappa;
    }

    EROD_API void erodXShellGetJointAngles(RodLinkage *linkage, double **outAngles, size_t *numAngles)
    {
        const auto joints = linkage->joints();
        std::vector<double> angles; 
        for (const auto &j : joints) angles.push_back(j.alpha());

        *numAngles = angles.size();
        auto sizeAngles = (*numAngles) * sizeof(double);
        *outAngles = static_cast<double *>(malloc(sizeAngles));
        std::memcpy(*outAngles, angles.data(), sizeAngles);
    }
    
    // Solver
    EROD_API int erodPeriodicElasticRodNewtonSolver(PeriodicRod *pRod, int numIterations, int numSupports, int numForces, int *supports, double *inForces,
                                                    double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                    int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
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
            *errorMessage = error.what();
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return -1;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return -1;
        }
    }

    EROD_API int erodElasticRodNewtonSolver(ElasticRod *rod, int numIterations, int numSupports, int numForces, int *supports, double *inForces,
                                            double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                            int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
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

                auto sizeReport = (numIterations * 5 + 2) * sizeof(double);
                *outReport = static_cast<double *>(malloc(sizeReport));
                std::memcpy(*outReport, flatReport.data(), sizeReport);
            }

            *errorMessage = "";

            if (report.success)
                return 1;
            else
                return 0;
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return -1;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return -1;
        }
    }

    EROD_API int erodXShellNewtonSolver(RodLinkage *linkage, int numIterations, double deployedAngle, int numSupports, int numForces, int *supports, double *inForces,
                                        double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                        int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
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
            *errorMessage = error.what();
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return -1;
        }
        catch (...)
        {
            *errorMessage = "Unknown Error from the Unmanaged Code";
            return -1;
        }
    }

    EROD_API int erodXShellAttractedLinkageNewtonSolver(SurfaceAttractedLinkage *linkage, int numIterations, double deployedAngle, int numSupports, int numForces, int *supports, double *inForces,
                                                        double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                        int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
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
            *errorMessage = error.what();
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return -1;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return -1;
        }
    }

    EROD_API int erodXShellLiveNewtonSolver(RodLinkage *linkage, int numIterations, double deployedAngle, int numSupports, int numForces, int *supports, double *inForces,
                                            double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                            int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage)
    {
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
            *errorMessage = error.what();
            return -1;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return -1;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return -1;
        }
    }

    // Material
    EROD_API RodMaterial *erodMaterialBuild(int sectionType, double E, double nu, double *params, int numParams, int axisType)
    {
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

        return new RodMaterial(type, E, nu, inParams, stiffAxis, keepCrossSectionMesh);
    }

    EROD_API RodMaterial *erodMaterialCustomBuild(double E, double nu, double *inCoords, int numVertices, double *inHolesCoords, int numHoles, int axisType)
    {
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

        auto mat = new RodMaterial();
        mat->setContourGH(E, nu, vertices, lines, holesPts, stiffAxis, keepCrossSectionMesh);
        return mat;
    }

    EROD_API void erodMaterialSetToLinkage(RodMaterial **materials, size_t numMaterials, RodLinkage *linkage)
    {
        if(numMaterials==1) linkage->setMaterial(*materials[0]);
        else{
            std::vector<RodMaterial> mat;
            mat.reserve(numMaterials);

            for (size_t i = 0; i < numMaterials; i++) mat.push_back(*materials[i]);

            linkage->setJointMaterials(mat);
        }
    }

    EROD_API double erodMaterialGetArea(RodMaterial *material)
    {
        return material->area;
    }

    EROD_API void erodMaterialGetMomentOfInertia(RodMaterial *material, double *lambda1, double *lambda2)
    {
        *lambda1 = material->momentOfInertia.lambda_1;
        *lambda2 = material->momentOfInertia.lambda_2;
    }

    EROD_API void erodMaterialGetBendingStiffness(RodMaterial *material, double *lambda1, double *lambda2)
    {
        *lambda1 = material->bendingStiffness.lambda_1;
        *lambda2 = material->bendingStiffness.lambda_2;
    }

    EROD_API double erodMaterialGetTwistingStiffness(RodMaterial *material)
    {
        return material->twistingStiffness;
    }

    EROD_API double erodMaterialGetStretchingStiffness(RodMaterial *material)
    {
        return material->stretchingStiffness;
    }

    EROD_API double erodMaterialGetSherModulus(RodMaterial *material)
    {
        return material->shearModulus;
    }

    EROD_API double erodMaterialGetCrossSectionHeight(RodMaterial *material)
    {
        return material->crossSectionHeight;
    }

    // Joints
    EROD_API const RodLinkage::Joint *erodJointBuild(RodLinkage *linkage, size_t index)
    {
        return &linkage->joint(index);
    }

    EROD_API void erodJointGetPosition(RodLinkage::Joint *joint, double **outCoords)
    {
        const auto coords = joint->pos();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoords, coords.data(), sizeCoords);
    }

    EROD_API void erodJointGetNormal(RodLinkage::Joint *joint, double **outCoords)
    {
        const auto coords = joint->normal();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoords, coords.data(), sizeCoords);
    }

    EROD_API void erodJointGetEdgeVecA(RodLinkage::Joint *joint, double **outCoords)
    {
        const auto coords = joint->source_t_A();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoords, coords.data(), sizeCoords);
    }

    EROD_API void erodJointGetEdgeVecB(RodLinkage::Joint *joint, double **outCoords)
    {
        const auto coords = joint->source_t_B();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoords, coords.data(), sizeCoords);
    }

    EROD_API void erodJointGetIsStartA(RodLinkage::Joint *joint, int **outStartA)
    {
        const auto s = joint->isStartA();

        std::vector<int> data;
        for (size_t i = 0; i < 2; i++)
        {
            data.push_back(s[i]);
        }

        auto sizeData = (data.size()) * sizeof(int);
        *outStartA = static_cast<int *>(malloc(sizeData));
        std::memcpy(*outStartA, data.data(), sizeData);
    }

    EROD_API void erodJointGetIsStartB(RodLinkage::Joint *joint, int **outStartB)
    {
        const auto s = joint->isStartB();

        std::vector<int> data;
        for (size_t i = 0; i < 2; i++)
        {
            data.push_back(s[i]);
        }

        auto sizeData = (data.size()) * sizeof(int);
        *outStartB = static_cast<int *>(malloc(sizeData));
        std::memcpy(*outStartB, data.data(), sizeData);
    }

    EROD_API void erodJointGetConnectedSegments(RodLinkage::Joint *joint, int **outSegmentsA, int **outSegmentsB)
    {
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
        *outSegmentsA = static_cast<int *>(malloc(sizeData));
        *outSegmentsB = static_cast<int *>(malloc(sizeData));
        std::memcpy(*outSegmentsA, ssA.data(), sizeData);
        std::memcpy(*outSegmentsB, ssB.data(), sizeData);
    }

    EROD_API void erodJointGetOmega(RodLinkage::Joint *joint, double **outCoords)
    {
        const auto coords = joint->omega();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoords, coords.data(), sizeCoords);
    }

    EROD_API void erodJointGetSourceTangent(RodLinkage::Joint *joint, double **outCoords)
    {
        const auto coords = joint->source_tangent();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoords, coords.data(), sizeCoords);
    }

    EROD_API void erodJointGetSourceNormal(RodLinkage::Joint *joint, double **outCoords)
    {
        const auto coords = joint->source_normal();

        auto sizeCoords = (coords.size()) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoords, coords.data(), sizeCoords);
    }

    EROD_API double erodJointGetAlpha(RodLinkage::Joint *joint)
    {
        return joint->alpha();
    }

    EROD_API void erodJointGetNormalSigns(RodLinkage::Joint *joint, size_t **outSigns)
    {
        const auto signs = joint->normal_signs();

        auto sizeSigns = (signs.size()) * sizeof(size_t);
        *outSigns = static_cast<size_t *>(malloc(sizeSigns));
        std::memcpy(*outSigns, signs.data(), sizeSigns);
    }

    EROD_API double erodJointGetSignB(RodLinkage::Joint *joint)
    {
        return joint->sign_B();
    }

    EROD_API double erodJointGetLenA(RodLinkage::Joint *joint)
    {
        return joint->len_A();
    }

    EROD_API double erodJointGetLenB(RodLinkage::Joint *joint)
    {
        return joint->len_B();
    }

    EROD_API int erodJointGetType(RodLinkage::Joint *joint)
    {
        return (int)joint->type;
    }

    EROD_API void erodJointGetSegmentsA(RodLinkage::Joint *joint, int **outSegmentsA)
    {
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
        *outSegmentsA = static_cast<int *>(malloc(sizeData));
        std::memcpy(*outSegmentsA, ss.data(), sizeData);
    }

    EROD_API void erodJointGetSegmentsB(RodLinkage::Joint *joint, int **outSegmentsB)
    {
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
        *outSegmentsB = static_cast<int *>(malloc(sizeData));
        std::memcpy(*outSegmentsB, ss.data(), sizeData);
    }

    // Segments
    EROD_API void erodRodSegmentGetMaterialFrame(RodLinkage::RodSegment *segment, size_t *outCoordsCount, double **outCoordsD1, double **outCoordsD2)
    {
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

        *outCoordsCount = coordsD1.size();
        auto sizeCoords = (*outCoordsCount) * sizeof(double);
        *outCoordsD1 = static_cast<double *>(malloc(sizeCoords));
        *outCoordsD2 = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoordsD1, coordsD1.data(), sizeCoords);
        std::memcpy(*outCoordsD2, coordsD2.data(), sizeCoords);
    }

    EROD_API const RodLinkage::RodSegment *erodRodSegmentBuild(RodLinkage *linkage, size_t index)
    {
        return &linkage->segment(index);
    }

    EROD_API size_t erodRodSegmentGetEdgesCount(RodLinkage::RodSegment *segment)
    {
        return segment->rod.numEdges();
    }

    EROD_API size_t erodRodSegmentGetVerticesCount(RodLinkage::RodSegment *segment)
    {
        return segment->rod.numVertices();
    }

    EROD_API size_t erodRodSegmentGetRestAnglesCount(RodLinkage::RodSegment *segment)
    {
        return segment->rod.numRestKappaVars();
    }

    EROD_API void erodRodSegmentGetRestKappas(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
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

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetRestLengths(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.restLengths();

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetStretchingStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.stretchingStresses();

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetTwistingStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.twistingStresses();

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetCenterLinePositions(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
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

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetMaxBendingStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.maxBendingStresses();

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetMinBendingStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.minBendingStresses();

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetSqrtBendingEnergies(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = stripAutoDiff(segment->rod.energyBendPerVertex()).array().sqrt().eval();

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetMeshData(RodLinkage::RodSegment *segment, double **outCoords, int **outQuads, size_t *numCoords, size_t *numQuads)
    {
        std::vector<MeshIO::IOVertex> vertices;
        std::vector<MeshIO::IOElement> quads;
        segment->rod.visualizationGeometry(vertices, quads, true, true);

        *numCoords = vertices.size() * 3;
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::vector<double> coords;
        for (size_t i = 0; i < vertices.size(); i++)
        {
            auto v = vertices[i].point;
            coords.push_back(v[0]);
            coords.push_back(v[1]);
            coords.push_back(v[2]);
        }
        std::memcpy(*outCoords, coords.data(), sizeCoords);

        *numQuads = quads.size() * 4;
        auto sizeQuads = (*numQuads) * sizeof(int);
        *outQuads = static_cast<int *>(malloc(sizeQuads));
        std::vector<int> quadsIdx;
        for (size_t i = 0; i < quads.size(); i++)
        {
            auto q = quads[i];
            quadsIdx.push_back(q[0]);
            quadsIdx.push_back(q[1]);
            quadsIdx.push_back(q[2]);
            quadsIdx.push_back(q[3]);
        }
        std::memcpy(*outQuads, quadsIdx.data(), sizeQuads);
    }

    EROD_API int erodRodSegmentGetStartJointIndex(RodLinkage::RodSegment *segment)
    {
        const auto idx = segment->startJoint;
        if (idx == RodLinkage::NONE)
            return -1;
        return idx;
    }

    EROD_API int erodRodSegmentGetEndJointIndex(RodLinkage::RodSegment *segment)
    {
        const auto idx = segment->endJoint;
        if (idx == RodLinkage::NONE)
            return -1;
        return idx;
    }

    EROD_API double erodRodSegmentGetEnergy(RodLinkage::RodSegment *segment)
    {
        return segment->rod.energy();
    }

    EROD_API double erodRodSegmentGetEnergyBend(RodLinkage::RodSegment *segment)
    {
        return segment->rod.energyBend();
    }

    EROD_API double erodRodSegmentGetEnergyStretch(RodLinkage::RodSegment *segment)
    {
        return segment->rod.energyStretch();
    }

    EROD_API double erodRodSegmentGetEnergyTwist(RodLinkage::RodSegment *segment)
    {
        return segment->rod.energyTwist();
    }

    EROD_API void erodRodSegmentGetBendingStiffness(RodLinkage::RodSegment *segment, double **lambda1, double **lambda2, size_t *numLambda1, size_t *numLambda2)
    {
        const auto s = segment->rod.bendingStiffnesses();
        size_t count = s.size();

        std::vector<double> lA, lB;
        for (size_t i = 0; i < count; i++)
        {
            lA.push_back(s[i].lambda_1);
            lB.push_back(s[i].lambda_2);
        }

        *numLambda1 = lA.size();
        auto sizeL1 = (lA.size()) * sizeof(double);
        *lambda1 = static_cast<double *>(malloc(sizeL1));
        std::memcpy(*lambda1, lA.data(), sizeL1);

        *numLambda2 = lB.size();
        auto sizeL2 = (lB.size()) * sizeof(double);
        *lambda2 = static_cast<double *>(malloc(sizeL2));
        std::memcpy(*lambda2, lB.data(), sizeL2);
    }

    EROD_API void erodRodSegmentGetTwistingStiffness(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.twistingStiffnesses();
        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetStretchingStiffness(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.stretchingStiffnesses();
        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }    

    EROD_API void erodRodSegmentGetRestPoints(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
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

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetRestDirectors(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
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

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodRodSegmentGetRestTwists(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.restTwists();

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API int erodRodSegmentGetBendingEnergyType(RodLinkage::RodSegment *segment)
    {
        const auto eType = segment->rod.bendingEnergyType();

        return eType == ElasticRod::BendingEnergyType::Bergou2008 ? 0 : 1;
    }

    EROD_API void erodRodSegmentGetDensities(RodLinkage::RodSegment *segment, double **outData, size_t *numData)
    {
        const auto data = segment->rod.densities();

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API double erodRodSegmentGetInitialMinRestLength(RodLinkage::RodSegment *segment)
    {
        return segment->rod.initialMinRestLength();
    }

    EROD_API void erodRodSegmentGetEdgeMaterial(RodLinkage::RodSegment *segment, size_t idx, double **outMatData, double **outCoords, int **outEdges, size_t *numMatData, size_t *numCoords, size_t *numEdges)
    {
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

        *numMatData = matData.size();
        auto sizeData = (matData.size()) * sizeof(double);
        *outMatData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outMatData, matData.data(), sizeData);

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

        *numCoords = pData.size();
        auto sizePData = (pData.size()) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizePData));
        std::memcpy(*outCoords, pData.data(), sizePData);

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

        *numEdges = eData.size();
        auto sizeEData = (eData.size()) * sizeof(int);
        *outEdges = static_cast<int *>(malloc(sizeEData));
        std::memcpy(*outEdges, eData.data(), sizeEData);
    }

    EROD_API int erodRodSegmentGetEdgeMaterialCount(RodLinkage::RodSegment *segment)
    {
        const auto eMat = segment->rod.edgeMaterials();
        return eMat.size();
    }

    EROD_API void erodRodSegmentGetDeformedState(RodLinkage::RodSegment *segment, double **outPtsCoords, double **outThetas, double **outTgtCoords,
                                                 double **outDirCoords, double **outSrcThetas, double **outSrcTwist,
                                                 size_t *outNumPtsCoords, size_t *outNumThetas, size_t *outNumTgtCoords,
                                                 size_t *outNumDirCoords, size_t *outNumSrcThetas, size_t *outNumSrcTwist)
    {
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

        *outNumPtsCoords = pData.size();
        auto sizePData = (pData.size()) * sizeof(double);
        *outPtsCoords = static_cast<double *>(malloc(sizePData));
        std::memcpy(*outPtsCoords, pData.data(), sizePData);

        // Thetas
        const auto thetas = dc.thetas();
        *outNumThetas = thetas.size();
        auto sizeThetas = (thetas.size()) * sizeof(double);
        *outThetas = static_cast<double *>(malloc(sizeThetas));
        std::memcpy(*outThetas, thetas.data(), sizeThetas);

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

        *outNumTgtCoords = tData.size();
        auto sizeTData = (tData.size()) * sizeof(double);
        *outTgtCoords = static_cast<double *>(malloc(sizeTData));
        std::memcpy(*outTgtCoords, tData.data(), sizeTData);

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

        *outNumDirCoords = dData.size();
        auto sizeDData = (dData.size()) * sizeof(double);
        *outDirCoords = static_cast<double *>(malloc(sizeDData));
        std::memcpy(*outDirCoords, dData.data(), sizeDData);

        // Source thetas
        const auto srcThetas = dc.sourceTheta;
        *outNumSrcThetas = srcThetas.size();
        auto sizeSrcThetas = (srcThetas.size()) * sizeof(double);
        *outSrcThetas = static_cast<double *>(malloc(sizeSrcThetas));
        std::memcpy(*outSrcThetas, srcThetas.data(), sizeSrcThetas);

        // Source twist
        const auto srcTwist = dc.sourceReferenceTwist;
        *outNumSrcTwist = srcTwist.size();
        auto sizeSrcTwist = (srcTwist.size()) * sizeof(double);
        *outSrcTwist = static_cast<double *>(malloc(sizeSrcTwist));
        std::memcpy(*outSrcTwist, srcTwist.data(), sizeSrcTwist);
    }
    
    EROD_API void erodRodSegmentGetVonMisesStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData){
        auto stresses = segment->rod.maxStresses(CrossSectionStressAnalysis::StressType::VonMises);
        // Stress Data
        *numData = stresses.size();
        auto sizeData = (*numData) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, stresses.data(), sizeData);
    }

    EROD_API double erodRodSegmentGetMaxStrain(RodLinkage::RodSegment *segment)
    {
        auto max_mag = 0, max_val = 0;
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
    }

    // ElasticRod
    EROD_API ElasticRod *erodElasticRodBuild(int numPoints, double *inCoords, const char **errorMessage)
    {
        try
        {
            std::vector<ElasticRod::Pt3> points;
            points.reserve(numPoints);
            for (int i = 0; i < numPoints; ++i)
            {
                points.emplace_back(inCoords[3 * i], inCoords[3 * i + 1], inCoords[3 * i + 2]);
            }

            *errorMessage = "Elastic Rod Built";

            return new ElasticRod(points);
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    EROD_API ElasticRod *erodElasticRodCopy(ElasticRod *rod, const char **errorMessage)
    {
        try
        {
            *errorMessage = "Elastic Rod Copied";
            return new ElasticRod(*rod);
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    EROD_API void erodElasticRodRemoveRestCurvatures(ElasticRod *rod)
    {
        const auto nv = rod->numVertices();
        ElasticRod::StdVectorVector2D restKappa;
        restKappa.assign(nv, ElasticRod::Vec2::Zero());
        rod->setRestKappas(restKappa);
    }

    EROD_API void erodElasticRodGetMeshData(ElasticRod *rod, double **outCoords, int **outQuads, size_t *numCoords, size_t *numQuads)
    {
        std::vector<MeshIO::IOVertex> vertices;
        std::vector<MeshIO::IOElement> quads;
        rod->visualizationGeometry(vertices, quads, true, true);

        *numCoords = vertices.size() * 3;
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::vector<double> coords;
        for (size_t i = 0; i < vertices.size(); i++)
        {
            auto v = vertices[i].point;
            coords.push_back(v[0]);
            coords.push_back(v[1]);
            coords.push_back(v[2]);
        }
        std::memcpy(*outCoords, coords.data(), sizeCoords);

        *numQuads = quads.size() * 4;
        auto sizeQuads = (*numQuads) * sizeof(int);
        *outQuads = static_cast<int *>(malloc(sizeQuads));
        std::vector<int> quadsIdx;
        for (size_t i = 0; i < quads.size(); i++)
        {
            auto q = quads[i];
            quadsIdx.push_back(q[0]);
            quadsIdx.push_back(q[1]);
            quadsIdx.push_back(q[2]);
            quadsIdx.push_back(q[3]);
        }
        std::memcpy(*outQuads, quadsIdx.data(), sizeQuads);
    }

    EROD_API void erodElasticRodSetMaterial(ElasticRod *rod, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType)
    {
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
    }

    EROD_API size_t erodElasticRodGetDoFCount(ElasticRod *rod)
    {
        return rod->numDoF();
    }

    EROD_API size_t erodElasticRodGetEdgesCount(ElasticRod *rod)
    {
        return rod->numEdges();
    }

    EROD_API size_t erodElasticRodGetVerticesCount(ElasticRod *rod)
    {
        return rod->numVertices();
    }

    EROD_API void erodElasticRodGetRestLengths(ElasticRod *rod, double *lengths, const size_t numEdges)
    {
        const auto r = rod->restLengths();
        for (size_t i = 0; i < numEdges; i++)
        {
            lengths[i] = r[i];
        }
    }

    EROD_API void erodElasticRodGetStretchingStresses(ElasticRod *rod, double *stresses, const size_t numEdges)
    {
        const auto s = rod->stretchingStresses();
        for (size_t i = 0; i < numEdges; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodElasticRodGetTwistingStresses(ElasticRod *rod, double *stresses, const size_t numVertices)
    {
        const auto s = rod->twistingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodElasticRodGetCenterLinePositions(ElasticRod *rod, double *outCoords, const size_t numCoords)
    {
        for (size_t i = 0; i < numCoords; i++)
        {
            const auto p = rod->deformedPoint(i);
            outCoords[i * 3] = p(0, 0);
            outCoords[i * 3 + 1] = p(1, 0);
            outCoords[i * 3 + 2] = p(2, 0);
        }
    }

    EROD_API void erodElasticRodGetMaxBendingStresses(ElasticRod *rod, double *stresses, const size_t numVertices)
    {
        const auto s = rod->maxBendingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodElasticRodGetMinBendingStresses(ElasticRod *rod, double *stresses, const size_t numVertices)
    {
        const auto s = rod->minBendingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodElasticRodGetSqrtBendingEnergies(ElasticRod *rod, double *stresses, const size_t numVertices)
    {
        const auto s = stripAutoDiff(rod->energyBendPerVertex()).array().sqrt().eval();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API double erodElasticRodGetEnergy(ElasticRod *rod)
    {
        return rod->energy();
    }

    EROD_API double erodElasticRodGetEnergyBend(ElasticRod *rod)
    {
        return rod->energyBend();
    }

    EROD_API double erodElasticRodGetEnergyStretch(ElasticRod *rod)
    {
        return rod->energyStretch();
    }

    EROD_API double erodElasticRodGetEnergyTwist(ElasticRod *rod)
    {
        return rod->energyTwist();
    }

    EROD_API void erodElasticRodGetDoFs(ElasticRod *rod, double **outDoFs, size_t *numDoFs){
        auto dofs = rod->getDoFs();
        *numDoFs = dofs.size();
        auto sizeDoFs = (*numDoFs) * sizeof(double);
        *outDoFs = static_cast<double *>(malloc(sizeDoFs));
        std::memcpy(*outDoFs, dofs.data(), sizeDoFs);
    }

    EROD_API void erodElasticRodSetDoFs(ElasticRod *rod, double *inDoFs, size_t numDoFs){
        Eigen::VectorXd dofs = Eigen::Map<Eigen::VectorXd>(inDoFs, numDoFs, 1);
        rod->setDoFs(dofs);
    }

    EROD_API void erodElasticRodGetMaterialFrame(ElasticRod *rod, size_t *outCoordsCount, double **outCoordsD1, double **outCoordsD2)
    {
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

        *outCoordsCount = coordsD1.size();
        auto sizeCoords = (*outCoordsCount) * sizeof(double);
        *outCoordsD1 = static_cast<double *>(malloc(sizeCoords));
        *outCoordsD2 = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoordsD1, coordsD1.data(), sizeCoords);
        std::memcpy(*outCoordsD2, coordsD2.data(), sizeCoords);
    }

    EROD_API double erodElasticRodGetMaxStrain(ElasticRod *rod)
    {
        auto max_mag = 0, max_val = 0;
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
    }
    
    EROD_API void erodElasticRodGetBendingStiffness(ElasticRod *rod, double **lambda1, double **lambda2, size_t *numLambda1, size_t *numLambda2)
    {
        const auto s = rod->bendingStiffnesses();
        size_t count = s.size();

        std::vector<double> lA, lB;
        for (size_t i = 0; i < count; i++)
        {
            lA.push_back(s[i].lambda_1);
            lB.push_back(s[i].lambda_2);
        }

        *numLambda1 = lA.size();
        auto sizeL1 = (lA.size()) * sizeof(double);
        *lambda1 = static_cast<double *>(malloc(sizeL1));
        std::memcpy(*lambda1, lA.data(), sizeL1);

        *numLambda2 = lB.size();
        auto sizeL2 = (lB.size()) * sizeof(double);
        *lambda2 = static_cast<double *>(malloc(sizeL2));
        std::memcpy(*lambda2, lB.data(), sizeL2);
    }

    EROD_API void erodElasticRodGetTwistingStiffness(ElasticRod *rod, double **outData, size_t *numData)
    {
        const auto data = rod->twistingStiffnesses();
        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodElasticRodGetStretchingStiffness(ElasticRod *rod, double **outData, size_t *numData)
    {
        const auto data = rod->stretchingStiffnesses();
        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }  
    
    EROD_API void erodElasticRodGetVonMisesStresses(ElasticRod *rod, double **outData, size_t *numData){
        auto stresses = rod->maxStresses(CrossSectionStressAnalysis::StressType::VonMises);
        // Stress Data
        *numData = stresses.size();
        auto sizeData = (*numData) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, stresses.data(), sizeData);
    }

    EROD_API double erodElasticRodGetInitialMinRestLength(ElasticRod *rod)
    {
        return rod->initialMinRestLength();
    }

    EROD_API void erodElasticRodGetRestKappas(ElasticRod *rod, double **outData, size_t *numData)
    {
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

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodElasticRodGetScalarFieldSqrtBendingEnergies(ElasticRod *rod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(rod->visualizationField(rod->sqrtBendingEnergies()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodElasticRodGetScalarFieldMaxBendingStresses(ElasticRod *rod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(rod->visualizationField(rod->maxBendingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodElasticRodGetScalarFieldVonMisesStresses(ElasticRod *rod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(rod->visualizationField(rod->maxVonMisesStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodElasticRodGetScalarFieldMinBendingStresses(ElasticRod *rod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(rod->visualizationField(rod->minBendingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodElasticRodGetScalarFieldTwistingStresses(ElasticRod *rod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(rod->visualizationField(rod->twistingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodElasticRodGetScalarFieldStretchingStresses(ElasticRod *rod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(rod->visualizationField(rod->stretchingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API int erodElasticRodGetThetaOffset(ElasticRod *rod){
        return rod->thetaOffset();
    }

    EROD_API int erodElasticRodGetRestLengthOffset(ElasticRod *rod){
        return rod->restLenOffset();
    }

    EROD_API int erodElasticRodGetRestKappaOffset(ElasticRod *rod){
        return rod->restKappaOffset();
    }

    // Periodic ElasticRod
    EROD_API PeriodicRod *erodPeriodicElasticRodBuild(int numPoints, double *inCoords, int removeCurvature, const char **errorMessage)
    {
        try
        {
            std::vector<PeriodicRod::Pt3> points;
            points.reserve(numPoints);
            for (int i = 0; i < numPoints; ++i)
            {
                points.emplace_back(inCoords[3 * i], inCoords[3 * i + 1], inCoords[3 * i + 2]);
            }

            *errorMessage = "Periodic Rod Built";
            return new PeriodicRod(points, removeCurvature);
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    EROD_API PeriodicRod *erodPeriodicElasticRodCopy(PeriodicRod *pRod, const char **errorMessage)
    {
        try
        {
            *errorMessage = "Periodic Rod Copied";
            return new PeriodicRod(*pRod);
        }
        catch (const std::runtime_error &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (const std::out_of_range &error)
        {
            *errorMessage = error.what();
            return nullptr;
        }
        catch (...)
        {
            *errorMessage = "Unknown error from the c++ library.";
            return nullptr;
        }
    }

    EROD_API void erodPeriodicElasticRodGetMeshData(PeriodicRod *pRod, double **outCoords, int **outQuads, size_t *numCoords, size_t *numQuads)
    {
        std::vector<MeshIO::IOVertex> vertices;
        std::vector<MeshIO::IOElement> quads;
        pRod->rod.visualizationGeometry(vertices, quads, true, true);

        *numCoords = vertices.size() * 3;
        auto sizeCoords = (*numCoords) * sizeof(double);
        *outCoords = static_cast<double *>(malloc(sizeCoords));
        std::vector<double> coords;
        for (size_t i = 0; i < vertices.size(); i++)
        {
            auto v = vertices[i].point;
            coords.push_back(v[0]);
            coords.push_back(v[1]);
            coords.push_back(v[2]);
        }
        std::memcpy(*outCoords, coords.data(), sizeCoords);

        *numQuads = quads.size() * 4;
        auto sizeQuads = (*numQuads) * sizeof(int);
        *outQuads = static_cast<int *>(malloc(sizeQuads));
        std::vector<int> quadsIdx;
        for (size_t i = 0; i < quads.size(); i++)
        {
            auto q = quads[i];
            quadsIdx.push_back(q[0]);
            quadsIdx.push_back(q[1]);
            quadsIdx.push_back(q[2]);
            quadsIdx.push_back(q[3]);
        }
        std::memcpy(*outQuads, quadsIdx.data(), sizeQuads);
    }

    EROD_API void erodPeriodicElasticRodSetMaterial(PeriodicRod *pRod, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType)
    {
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
    }

    EROD_API size_t erodPeriodicElasticRodGetDoFCount(PeriodicRod *pRod)
    {
        return pRod->numDoF();
    }

    EROD_API size_t erodPeriodicElasticRodGetEdgesCount(PeriodicRod *pRod)
    {
        return pRod->rod.numEdges();
    }

    EROD_API size_t erodPeriodicElasticRodGetVerticesCount(PeriodicRod *pRod)
    {
        return pRod->rod.numVertices();
    }

    EROD_API void erodPeriodicElasticRodGetRestLengths(PeriodicRod *pRod, double *lengths, const size_t numEdges)
    {
        const auto r = pRod->rod.restLengths();
        for (size_t i = 0; i < numEdges; i++)
        {
            lengths[i] = r[i];
        }
    }

    EROD_API void erodPeriodicElasticRodGetStretchingStresses(PeriodicRod *pRod, double *stresses, const size_t numEdges)
    {
        const auto s = pRod->rod.stretchingStresses();
        for (size_t i = 0; i < numEdges; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodPeriodicElasticRodGetTwistingStresses(PeriodicRod *pRod, double *stresses, const size_t numVertices)
    {
        const auto s = pRod->rod.twistingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodPeriodicElasticRodGetCenterLinePositions(PeriodicRod *pRod, double *outCoords, const size_t numCoords)
    {
        for (size_t i = 0; i < numCoords; i++)
        {
            const auto p = pRod->rod.deformedPoint(i);
            outCoords[i * 3] = p(0, 0);
            outCoords[i * 3 + 1] = p(1, 0);
            outCoords[i * 3 + 2] = p(2, 0);
        }
    }

    EROD_API void erodPeriodicElasticRodGetMaxBendingStresses(PeriodicRod *pRod, double *stresses, const size_t numVertices)
    {
        const auto s = pRod->rod.maxBendingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodPeriodicElasticRodGetMinBendingStresses(PeriodicRod *pRod, double *stresses, const size_t numVertices)
    {
        const auto s = pRod->rod.minBendingStresses();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodPeriodicElasticRodGetSqrtBendingEnergies(PeriodicRod *pRod, double *stresses, const size_t numVertices)
    {
        const auto s = stripAutoDiff(pRod->rod.energyBendPerVertex()).array().sqrt().eval();
        for (size_t i = 0; i < numVertices; i++)
        {
            stresses[i] = s(i);
        }
    }

    EROD_API void erodPeriodicElasticRodGetDoFs(PeriodicRod *pRod, double **outDoFs, size_t *numDoFs){
        auto dofs = pRod->getDoFs();
        *numDoFs = dofs.size();
        auto sizeDoFs = (*numDoFs) * sizeof(double);
        *outDoFs = static_cast<double *>(malloc(sizeDoFs));
        std::memcpy(*outDoFs, dofs.data(), sizeDoFs);
    }

    EROD_API void erodPeriodicElasticRodSetDoFs(PeriodicRod *pRod, double *inDoFs, size_t numDoFs){
        Eigen::VectorXd dofs = Eigen::Map<Eigen::VectorXd>(inDoFs, numDoFs, 1);
        pRod->setDoFs(dofs);
    }

    EROD_API void erodPeriodicElasticRodGetMaterialFrame(PeriodicRod *pRod, size_t *outCoordsCount, double **outCoordsD1, double **outCoordsD2)
    {
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

        *outCoordsCount = coordsD1.size();
        auto sizeCoords = (*outCoordsCount) * sizeof(double);
        *outCoordsD1 = static_cast<double *>(malloc(sizeCoords));
        *outCoordsD2 = static_cast<double *>(malloc(sizeCoords));
        std::memcpy(*outCoordsD1, coordsD1.data(), sizeCoords);
        std::memcpy(*outCoordsD2, coordsD2.data(), sizeCoords);
    }

    EROD_API double erodPeriodicElasticRodGetEnergy(PeriodicRod *pRod)
    {
        return pRod->rod.energy();
    }

    EROD_API double erodPeriodicElasticRodGetEnergyBend(PeriodicRod *pRod)
    {
        return pRod->rod.energyBend();
    }

    EROD_API double erodPeriodicElasticRodGetEnergyStretch(PeriodicRod *pRod)
    {
        return pRod->rod.energyStretch();
    }

    EROD_API double erodPeriodicElasticRodGetEnergyTwist(PeriodicRod *pRod)
    {
        return pRod->rod.energyTwist();
    }

    EROD_API double erodPeriodicElasticRodGetMaxStrain(PeriodicRod *pRod)
    {
        auto max_mag = 0, max_val = 0;
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
    }

    EROD_API void erodPeriodicElasticRodGetBendingStiffness(PeriodicRod *pRod, double **lambda1, double **lambda2, size_t *numLambda1, size_t *numLambda2)
    {
        const auto s = pRod->rod.bendingStiffnesses();
        size_t count = s.size();

        std::vector<double> lA, lB;
        for (size_t i = 0; i < count; i++)
        {
            lA.push_back(s[i].lambda_1);
            lB.push_back(s[i].lambda_2);
        }

        *numLambda1 = lA.size();
        auto sizeL1 = (lA.size()) * sizeof(double);
        *lambda1 = static_cast<double *>(malloc(sizeL1));
        std::memcpy(*lambda1, lA.data(), sizeL1);

        *numLambda2 = lB.size();
        auto sizeL2 = (lB.size()) * sizeof(double);
        *lambda2 = static_cast<double *>(malloc(sizeL2));
        std::memcpy(*lambda2, lB.data(), sizeL2);
    }

    EROD_API void erodPeriodicElasticRodGetTwistingStiffness(PeriodicRod *pRod, double **outData, size_t *numData)
    {
        const auto data = pRod->rod.twistingStiffnesses();
        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodPeriodicElasticRodGetStretchingStiffness(PeriodicRod *pRod, double **outData, size_t *numData)
    {
        const auto data = pRod->rod.stretchingStiffnesses();
        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }  

    EROD_API void erodPeriodicElasticRodGetVonMisesStresses(PeriodicRod *pRod, double **outData, size_t *numData){
        auto stresses = pRod->rod.maxStresses(CrossSectionStressAnalysis::StressType::VonMises);
        // Stress Data
        *numData = stresses.size();
        auto sizeData = (*numData) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, stresses.data(), sizeData);
    }

    EROD_API double erodPeriodicElasticRodGetInitialMinRestLength(PeriodicRod *pRod)
    {
        return pRod->rod.initialMinRestLength();
    }

    EROD_API void erodPeriodicElasticRodGetRestKappas(PeriodicRod *pRod, double **outData, size_t *numData)
    {
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

        *numData = data.size();
        auto sizeData = (data.size()) * sizeof(double);
        *outData = static_cast<double *>(malloc(sizeData));
        std::memcpy(*outData, data.data(), sizeData);
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(PeriodicRod *pRod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.sqrtBendingEnergies()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldMaxBendingStresses(PeriodicRod *pRod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.maxBendingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldVonMisesStresses(PeriodicRod *pRod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.maxVonMisesStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldMinBendingStresses(PeriodicRod *pRod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.minBendingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldTwistingStresses(PeriodicRod *pRod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.twistingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API void erodPeriodicElasticRodGetScalarFieldStretchingStresses(PeriodicRod *pRod, double **outField, size_t *numField)
    {
        ScalarField<Real> field(pRod->rod.visualizationField(pRod->rod.stretchingStresses()));

        *numField = field.size();
        auto sizeField = (*numField) * sizeof(double);
        *outField = static_cast<double *>(malloc(sizeField));
        std::memcpy(*outField, field.data(), sizeField);
    }

    EROD_API int erodPeriodicElasticRodGetThetaOffset(PeriodicRod *pRod){
        return pRod->rod.thetaOffset();
    }

    EROD_API int erodPeriodicElasticRodGetRestLengthOffset(PeriodicRod *pRod){
        return pRod->rod.restLenOffset();
    }

    EROD_API int erodPeriodicElasticRodGetRestKappaOffset(PeriodicRod *pRod){
        return pRod->rod.restKappaOffset();
    }
}
