#ifndef EROD_H
#define EROD_H
#include <cstdint>

#include "RodLinkage.hh"
#include "restlen_solve.hh"
#include "compute_equilibrium.hh"
#include "open_linkage.hh"
#include "ElasticRod.hh"
#include "PeriodicRod.hh"
#include "infer_target_surface.hh"
#include "RodMaterial.hh"
#include "SurfaceAttractedLinkage.hh"

#define EROD_API_VERSION "erod"

#if defined(EROD_DLL)
#if defined(EROD_DLL_EXPORT)
#define EROD_API __declspec(dllexport)
#else
#define EROD_API __declspec(dllimport)
#endif
#else
#define EROD_API __attribute__((visibility("default")))
#endif

namespace ElasticRodsGH
{
    EROD_API void erodSetSolverLogCallback(void (*callback)(const char *, void *), void *context);
    EROD_API int erodGetApiVersion();
    EROD_API void erodFree(void *buffer);
    // ABI 2: errors are borrowed until the next operation on this thread;
    // returned array buffers belong to this module and must use erodFree.
    EROD_API const char *erodGetLastError();
    EROD_API uint64_t erodGetBufferSize(const void *buffer);
    EROD_API void erodDestroyObject(void *object);
    EROD_API uint64_t erodGetOutstandingObjectCount();
    EROD_API uint64_t erodGetOutstandingBufferCount();
    EROD_API RodMaterial *erodMaterialCopy(RodMaterial *material);


    // AttractedLinkage
    EROD_API SurfaceAttractedLinkage *erodXShellAttractedSurfaceBuild(int numVertices, int numTrias, double *inCoords, int *inTrias, RodLinkage *linkage, double tgt_joint_weight, const char **errorMessage);

    EROD_API SurfaceAttractedLinkage *erodXShellAttractedSurfaceCopy(SurfaceAttractedLinkage *linkage, const char **errorMessage);

    // Target Surface
    EROD_API int erodXShellInferTargetSurface(RodLinkage *linkage, int nsubdiv, int numExtensionLayers, double **outCoords, int **outTrias, int *numCoords, int *numTrias, const char **errorMessage);

    // Linkage
    EROD_API RodLinkage *erodXShellBuild(int numVertices, int numEdges, double *inCoords, int *inEdges, double *inNormals,
                                                      double *inRestLengths, int *inOffsetInteriorCoords, double *inInteriorCoords,
                                                      int interleavingType, int initConsistentAngle, int initConsistentNormals, const char **errorMessage);

    EROD_API RodLinkage *erodXShellBuildFromGraph(int numVertices, int numEdges, double *inCoords, int *inEdges, double *inNormals, int subdivision, int interleavingType, int initConsistentAngle, const char **errorMessage);

    EROD_API RodLinkage *erodXShellCopy(RodLinkage *linkage, const char **errorMessage);

    EROD_API RodLinkage::RodSegment *erodXShellBuildRodSegment(int numVertices, double *inCoords);

    EROD_API void erodXShellSetMaterial(RodLinkage *linkage, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType);

    EROD_API void erodXShellSetCustomMaterial(RodLinkage *linkage, double E, double nu, double *inCoords, int numVertices, double *inHolesCoords, int numHoles, int axisType);

    EROD_API void erodXShellSetJointMaterial(RodLinkage *linkage, int numMaterials, int *sectionType, double *E, double *nu, double *sectionParams, int *sectionParamsCount, int *axisType);

    EROD_API void erodXShellSetCustomJointMaterial(RodLinkage *linkage, int numMaterials, int *sectionType, double *E, double *nu, double *inCoords, int *inCoordsCount, double *inHolesCoords, int *inHolesCount, int *axisType);

    EROD_API void erodXShellSetDesignParameterConfig(RodLinkage *linkage, int use_restLen, int use_restKappa, int update_designParams_cache);

    EROD_API int erodXShellGetCentralJointIndex(RodLinkage *linkage);

    EROD_API void erodXShellGetDofOffsetForJoint(RodLinkage *linkage, int joint, int *DOF, int numDOF, int *outVars);

    EROD_API void erodXShellGetDofOffsetForCenterLinePos(RodLinkage *linkage, int index, int *DOF, int numDOF, int *outVars);

    EROD_API void erodXShellGetCenterLinePositions(RodLinkage *linkage, double** outCoords, int * numCoords);

    EROD_API double erodXShellGetAverageJointAngle(RodLinkage *linkage);

    EROD_API double erodXShellGetEnergy(RodLinkage *linkage);

    EROD_API double erodXShellGetEnergyBend(RodLinkage *linkage);

    EROD_API double erodXShellGetEnergyStretch(RodLinkage *linkage);

    EROD_API double erodXShellGetEnergyTwist(RodLinkage *linkage);

    EROD_API double erodXShellGetMaxStrain(RodLinkage *linkage);

    EROD_API int erodXShellGetCenterLinePositionsCount(RodLinkage *linkage);

    EROD_API int erodXShellGetJointsCount(RodLinkage *linkage);

    EROD_API int erodXShellGetDoFCount(RodLinkage* linkage);

    EROD_API int erodXShellGetRodSegmentsCount(RodLinkage *linkage);

    EROD_API int erodXShellGetRestKappaVarsCount(RodLinkage *linkage);

    EROD_API void erodXShellGetRestKappaVars(RodLinkage *linkage, double **outData, int *numData);

    EROD_API void erodXShellGetMeshData(RodLinkage* linkage, double** outCoords, int** outQuads, int * numCoords, int * numQuads);

    EROD_API void erodXShellGetRodSegmentIndexesPerRod(RodLinkage *linkage, int index, int **segmentIndexes, int *numSeg, int *type);

    EROD_API int erodXShellGetRodTraceCount(RodLinkage *linkage, const char **errorMessage);

    EROD_API void erodXShellRemoveRestCurvatures(RodLinkage *linkage);

    EROD_API int erodXShellHessianNNZ(RodLinkage *linkage, int variableDesignParameters);

    EROD_API double erodXShellGetMinJointAngle(RodLinkage *linkage);

    EROD_API double erodXShellGetTotalRestLength(RodLinkage *linkage);

    EROD_API double erodXShellGetMaxRodEnergy(RodLinkage *linkage);

    EROD_API void erodXShellSetStiffenRegions(RodLinkage *linkage, double factor, double *coords, int numBoxes);

    EROD_API double erodXShellStripAutoDiff(double *edgeA, double *edgeB);

    EROD_API void erodXShellGetDesignParams(RodLinkage *linkage, double **outDesignParams, int *outNumDesignParameters);

    EROD_API int erodXShelNumberDesignParameters(RodLinkage *linkage);
    
    EROD_API void erodXShellGetDoFs(RodLinkage *linkage, double **outDoFs, int *numDoFs);

    EROD_API void erodXShellSetDoFs(RodLinkage *linkage, double *inDoFs, int numDoFs);

    EROD_API void erodXShellGetRestLengthsSolveDoFs(RodLinkage *linkage, double **outDoFs, int *numDoFs);

    EROD_API void erodXShellGetPerSegmentRestLengths(RodLinkage *linkage, double **outLengths, int *numLengths);

    EROD_API void erodXShellSetRestLengthsSolveDoFs(RodLinkage *linkage, double *outDoFs, int numDoFs);

    EROD_API void erodXShellSetPerSegmentRestLengths(RodLinkage *linkage, double *inLengths, int numLengths);

    EROD_API void erodXShellScalarFieldSqrtBendingEnergies(RodLinkage *linkage, double **outField, int *numField);

    EROD_API void erodXShellScalarFieldMaxBendingStresses(RodLinkage *linkage, double **outField, int *numField);
    
    EROD_API void erodXShellScalarFieldVonMisesStresses(RodLinkage *linkage, double **outField, int *numField);

    EROD_API void erodXShellScalarFieldMinBendingStresses(RodLinkage *linkage, double **outField, int *numField);

    EROD_API void erodXShellScalarFieldTwistingStresses(RodLinkage *linkage, double **outField, int *numField);

    EROD_API void erodXShellScalarFieldStretchingStresses(RodLinkage *linkage, double **outField, int *numField);

    EROD_API double erodXShellGetInitialMinRestLength(RodLinkage *linkage);

    EROD_API int erodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(RodLinkage *linkage, double **outAx, int64_t **outAi, int64_t **outAp, int64_t *outM, int64_t *outN, int64_t *outNZ, const char **errorMessage);

    EROD_API void erodXShellGetDesignParameterConfig(RodLinkage *linkage, int *use_restLen, int *use_restKappa);

    EROD_API void erodXShellGetJointAngles(RodLinkage *linkage, double **outAngles, int *numAngles);

    // Material
    EROD_API RodMaterial *erodMaterialBuild(int sectionType, double E, double nu, double *params, int numParams, int axisType);

    EROD_API RodMaterial *erodMaterialCustomBuild(double E, double nu, double *inCoords, int numVertices, double *inHolesCoords, int numHoles, int axisType);

    EROD_API void erodMaterialSetToLinkage(RodMaterial *const *materials, int numMaterials, RodLinkage *linkage);

    EROD_API double erodMaterialGetArea(RodMaterial *material);

    EROD_API void erodMaterialGetMomentOfInertia(RodMaterial *material, double *lambda1, double *lambda2);

    EROD_API void erodMaterialGetBendingStiffness(RodMaterial *material, double *lambda1, double *lambda2);

    EROD_API double erodMaterialGetTwistingStiffness(RodMaterial *material);

    EROD_API double erodMaterialGetStretchingStiffness(RodMaterial *material);

    EROD_API double erodMaterialGetSherModulus(RodMaterial *material);

    EROD_API double erodMaterialGetCrossSectionHeight(RodMaterial *material);

    // Solver
    EROD_API int erodPeriodicElasticRodNewtonSolver(PeriodicRod *rod, int numIterations, int numSupports, int numForces, int *supports, double *inForces,
                                                    double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                    int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage);

    EROD_API int erodElasticRodNewtonSolver(ElasticRod *rod, int numIterations, int numSupports, int numForces, int *supports, double *inForces,
                                            double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                            int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage);

    EROD_API int erodXShellNewtonSolver(RodLinkage *linkage, int numIterations, double deployedAngle, int numSupports, int numForces, int *supports, double* inForces, 
                                              double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, 
                                              int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage);

    EROD_API int erodXShellAttractedLinkageNewtonSolver(SurfaceAttractedLinkage *linkage, int numIterations, double deployedAngle, int numSupports, int numForces, int *supports, double *inForces,
                                                        double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                        int feasibilitySolve, int verboseNonPosDef, int writeReport, double **outReport, const char **errorMessage);

    // Joints 
    EROD_API const RodLinkage::Joint *erodJointBuild(RodLinkage *linkage, int index);

    EROD_API void erodJointGetPosition(RodLinkage::Joint *joint, double **outCoords); 

    EROD_API void erodJointGetNormal(RodLinkage::Joint *joint, double **outCoords);

    EROD_API void erodJointGetEdgeVecA(RodLinkage::Joint *joint, double **outCoords);

    EROD_API void erodJointGetEdgeVecB(RodLinkage::Joint *joint, double **outCoords);

    EROD_API void erodJointGetIsStartA(RodLinkage::Joint *joint, int **outStartA);

    EROD_API void erodJointGetIsStartB(RodLinkage::Joint *joint, int **outStartB);

    EROD_API void erodJointGetConnectedSegments(RodLinkage::Joint *joint, int **outSegmentsA, int **outSegmentsB);

    EROD_API void erodJointGetOmega(RodLinkage::Joint *joint, double **outCoords);

    EROD_API void erodJointGetSourceTangent(RodLinkage::Joint *joint, double **outCoords);

    EROD_API void erodJointGetSourceNormal(RodLinkage::Joint *joint, double **outCoords);

    EROD_API double erodJointGetAlpha(RodLinkage::Joint *joint);

    EROD_API void erodJointGetNormalSigns(RodLinkage::Joint *joint, int **outSigns);

    EROD_API double erodJointGetSignB(RodLinkage::Joint *joint);

    EROD_API double erodJointGetLenA(RodLinkage::Joint *joint);

    EROD_API double erodJointGetLenB(RodLinkage::Joint *joint);

    EROD_API void erodJointGetSegmentsA(RodLinkage::Joint *joint, int **outSegmentsA);

    EROD_API void erodJointGetSegmentsB(RodLinkage::Joint *joint, int **outSegmentsB);

    EROD_API int erodJointGetType(RodLinkage::Joint *joint);

    // Segments
    EROD_API void erodRodSegmentGetMaterialFrame(RodLinkage::RodSegment *segment, int *outCoordsCount, double **outCoordsD1, double **outCoordsD2);

    EROD_API const RodLinkage::RodSegment *erodRodSegmentBuild(RodLinkage *linkage, int index);

    EROD_API int erodRodSegmentGetEdgesCount(RodLinkage::RodSegment *segment);

    EROD_API int erodRodSegmentGetVerticesCount(RodLinkage::RodSegment *segment);

    EROD_API int erodRodSegmentGetRestAnglesCount(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetRestLengths(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetRestKappas(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetStretchingStresses(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetTwistingStresses(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetCenterLinePositions(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetMaxBendingStresses(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetMinBendingStresses(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetSqrtBendingEnergies(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetMeshData(RodLinkage::RodSegment *segment, double **outCoords, int **outQuads, int *numCoords, int *numQuads);

    EROD_API int erodRodSegmentGetStartJointIndex(RodLinkage::RodSegment *segment);

    EROD_API int erodRodSegmentGetEndJointIndex(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetBendingStiffness(RodLinkage::RodSegment *segment, double **lambda1, double **lambda2, int *numLambda1, int *numLambda2);

    EROD_API void erodRodSegmentGetTwistingStiffness(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetStretchingStiffness(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API double erodRodSegmentGetEnergy(RodLinkage::RodSegment *segment);

    EROD_API double erodRodSegmentGetEnergyBend(RodLinkage::RodSegment *segment);

    EROD_API double erodRodSegmentGetEnergyStretch(RodLinkage::RodSegment *segment);

    EROD_API double erodRodSegmentGetEnergyTwist(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetRestPoints(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetRestDirectors(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API void erodRodSegmentGetRestTwists(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API int erodRodSegmentGetBendingEnergyType(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetDensities(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API double erodRodSegmentGetInitialMinRestLength(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetEdgeMaterial(RodLinkage::RodSegment *segment, int idx, double **outMatData, double **outCoords, int **outEdges, int *numMatData, int *numCoords, int *numEdges);

    EROD_API int erodRodSegmentGetEdgeMaterialCount(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetDeformedState(RodLinkage::RodSegment *segment, double **outPtsCoords, double **outThetas, double **outTgtCoords, 
                                                double **outDirCoords, double **outSrcThetas, double **outSrcTwist, 
                                                int *outNumPtsCoords, int *outNumThetas, int *outNumTgtCoords,
                                                int *outNumDirCoords, int *outNumSrcThetas, int *outNumSrcTwist);
    
    EROD_API void erodRodSegmentGetVonMisesStresses(RodLinkage::RodSegment *segment, double **outData, int *numData);

    EROD_API double erodRodSegmentGetMaxStrain(RodLinkage::RodSegment *segment);

    // ElasticRod
    EROD_API ElasticRod *erodElasticRodBuild(int numPoints, double *inCoords, const char **errorMessage);

    EROD_API ElasticRod *erodElasticRodCopy(ElasticRod *rod, const char **errorMessage);

    EROD_API void erodElasticRodRemoveRestCurvatures(ElasticRod *rod);

    EROD_API void erodElasticRodGetMeshData(ElasticRod *rod, double** outCoords, int** outQuads, int * numCoords, int * numQuads);

    EROD_API void erodElasticRodSetMaterial(ElasticRod *rod, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType);

    EROD_API int erodElasticRodGetDoFCount(ElasticRod *rod);

    EROD_API int erodElasticRodGetEdgesCount(ElasticRod *rod);

    EROD_API int erodElasticRodGetVerticesCount(ElasticRod *rod);

    EROD_API void erodElasticRodGetRestLengths(ElasticRod *rod, double *lengths, const int numEdges);

    EROD_API void erodElasticRodGetStretchingStresses(ElasticRod *rod, double *stresses, const int numEdges);

    EROD_API void erodElasticRodGetTwistingStresses(ElasticRod *rod, double *stresses, const int numVertices);

    EROD_API void erodElasticRodGetCenterLinePositions(ElasticRod *rod, double* outCoords, const int numCoords);

    EROD_API void erodElasticRodGetMaxBendingStresses(ElasticRod *rod, double* stresses, const int numVertices);

    EROD_API void erodElasticRodGetMinBendingStresses(ElasticRod *rod, double* stresses, const int numVertices);

    EROD_API void erodElasticRodGetSqrtBendingEnergies(ElasticRod *rod, double* stresses, const int numVertices);

    EROD_API void erodElasticRodGetBendingEnergies(ElasticRod *rod, double* energies, const int numVertices);

    EROD_API void erodElasticRodGetDoFs(ElasticRod *rod, double **outDoFs, int *numDoFs);

    EROD_API void erodElasticRodSetDoFs(ElasticRod *rod, double *inDoFs, int numDoFs);

    EROD_API void erodElasticRodGetMaterialFrame(ElasticRod *rod, int *outCoordsCount, double **outCoordsD1, double **outCoordsD2);

    EROD_API double erodElasticRodGetEnergy(ElasticRod *rod);

    EROD_API double erodElasticRodGetEnergyBend(ElasticRod *rod);

    EROD_API double erodElasticRodGetEnergyStretch(ElasticRod *rod);

    EROD_API double erodElasticRodGetEnergyTwist(ElasticRod *rod);

    EROD_API double erodElasticRodGetMaxStrain(ElasticRod *rod);

    EROD_API void erodElasticRodGetBendingStiffness(ElasticRod *rod, double **lambda1, double **lambda2, int *numLambda1, int *numLambda2);

    EROD_API void erodElasticRodGetTwistingStiffness(ElasticRod *rod, double **outData, int *numData);

    EROD_API void erodElasticRodGetStretchingStiffness(ElasticRod *rod, double **outData, int *numData);

    EROD_API void erodElasticRodGetVonMisesStresses(ElasticRod *rod, double **outData, int *numData);

    EROD_API double erodElasticRodGetInitialMinRestLength(ElasticRod *rod);

    EROD_API void erodElasticRodGetRestKappas(ElasticRod *rod, double **outData, int *numData);

    EROD_API void erodElasticRodGetScalarFieldSqrtBendingEnergies(ElasticRod *rod, double **outField, int *numField);

    EROD_API void erodElasticRodGetScalarFieldMaxBendingStresses(ElasticRod *rod, double **outField, int *numField);
    
    EROD_API void erodElasticRodGetScalarFieldVonMisesStresses(ElasticRod *rod, double **outField, int *numField);

    EROD_API void erodElasticRodGetScalarFieldMinBendingStresses(ElasticRod *rod, double **outField, int *numField);

    EROD_API void erodElasticRodGetScalarFieldTwistingStresses(ElasticRod *rod, double **outField, int *numField);

    EROD_API void erodElasticRodGetScalarFieldStretchingStresses(ElasticRod *rod, double **outField, int *numField);

    EROD_API int erodElasticRodGetThetaOffset(ElasticRod *rod);

    EROD_API int erodElasticRodGetRestLengthOffset(ElasticRod *rod);

    EROD_API int erodElasticRodGetRestKappaOffset(ElasticRod *rod);

    // ElasticRod Periodic
    EROD_API PeriodicRod *erodPeriodicElasticRodBuild(int numPoints, double *inCoords, int removeCurvature, const char **errorMessage);

    EROD_API PeriodicRod *erodPeriodicElasticRodCopy(PeriodicRod *pRod, const char **errorMessage);

    EROD_API void erodPeriodicElasticRodGetMeshData(PeriodicRod *pRod, double** outCoords, int** outQuads, int * numCoords, int * numQuads);

    EROD_API void erodPeriodicElasticRodSetMaterial(PeriodicRod *pRod, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType);

    EROD_API int erodPeriodicElasticRodGetDoFCount(PeriodicRod *pRod);

    EROD_API int erodPeriodicElasticRodGetEdgesCount(PeriodicRod *pRod);

    EROD_API int erodPeriodicElasticRodGetVerticesCount(PeriodicRod *pRod);

    EROD_API void erodPeriodicElasticRodGetRestLengths(PeriodicRod *pRod, double *lengths, const int numEdges);

    EROD_API void erodPeriodicElasticRodGetStretchingStresses(PeriodicRod *pRod, double *stresses, const int numEdges);

    EROD_API void erodPeriodicElasticRodGetTwistingStresses(PeriodicRod *pRod, double *stresses, const int numVertices);

    EROD_API void erodPeriodicElasticRodGetCenterLinePositions(PeriodicRod *pRod, double* outCoords, const int numCoords);

    EROD_API void erodPeriodicElasticRodGetMaxBendingStresses(PeriodicRod *pRod, double* stresses, const int numVertices);

    EROD_API void erodPeriodicElasticRodGetMinBendingStresses(PeriodicRod *pRod, double* stresses, const int numVertices);

    EROD_API void erodPeriodicElasticRodGetSqrtBendingEnergies(PeriodicRod *pRod, double* stresses, const int numVertices);

    EROD_API void erodPeriodicElasticRodGetBendingEnergies(ElasticRod *pRod, double* energies, const int numVertices);

    EROD_API void erodPeriodicElasticRodGetDoFs(PeriodicRod *pRod, double **outDoFs, int *numDoFs);

    EROD_API void erodPeriodicElasticRodSetDoFs(PeriodicRod *pRod, double *inDoFs, int numDoFs);

    EROD_API void erodPeriodicElasticRodGetMaterialFrame(PeriodicRod *pRod, int *outCoordsCount, double **outCoordsD1, double **outCoordsD2);

    EROD_API double erodPeriodicElasticRodGetEnergy(PeriodicRod *pRod);

    EROD_API double erodPeriodicElasticRodGetEnergyBend(PeriodicRod *pRod);

    EROD_API double erodPeriodicElasticRodGetEnergyStretch(PeriodicRod *pRod);

    EROD_API double erodPeriodicElasticRodGetEnergyTwist(PeriodicRod *pRod);

    EROD_API double erodPeriodicElasticRodGetMaxStrain(PeriodicRod *pRod);

    EROD_API void erodPeriodicElasticRodGetBendingStiffness(PeriodicRod *pRod, double **lambda1, double **lambda2, int *numLambda1, int *numLambda2);

    EROD_API void erodPeriodicElasticRodGetTwistingStiffness(PeriodicRod *pRod, double **outData, int *numData);

    EROD_API void erodPeriodicElasticRodGetStretchingStiffness(PeriodicRod *pRod, double **outData, int *numData);

    EROD_API void erodPeriodicElasticRodGetVonMisesStresses(PeriodicRod *pRod, double **outData, int *numData);

    EROD_API double erodPeriodicElasticRodGetInitialMinRestLength(PeriodicRod *pRod);

    EROD_API void erodPeriodicElasticRodGetRestKappas(PeriodicRod *pRod, double **outData, int *numData);

    EROD_API void erodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(PeriodicRod *pRod, double **outField, int *numField);

    EROD_API void erodPeriodicElasticRodGetScalarFieldMaxBendingStresses(PeriodicRod *pRod, double **outField, int *numField);
    
    EROD_API void erodPeriodicElasticRodGetScalarFieldVonMisesStresses(PeriodicRod *pRod, double **outField, int *numField);

    EROD_API void erodPeriodicElasticRodGetScalarFieldMinBendingStresses(PeriodicRod *pRod, double **outField, int *numField);

    EROD_API void erodPeriodicElasticRodGetScalarFieldTwistingStresses(PeriodicRod *pRod, double **outField, int *numField);

    EROD_API void erodPeriodicElasticRodGetScalarFieldStretchingStresses(PeriodicRod *pRod, double **outField, int *numField);

    EROD_API int erodPeriodicElasticRodGetThetaOffset(PeriodicRod *pRod);

    EROD_API int erodPeriodicElasticRodGetRestLengthOffset(PeriodicRod *pRod);

    EROD_API int erodPeriodicElasticRodGetRestKappaOffset(PeriodicRod *pRod);
}
#endif