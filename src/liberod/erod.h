#ifndef EROD_H
#define EROD_H

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
    // AttractedLinkage
    EROD_API SurfaceAttractedLinkage *erodXShellAttractedSurfaceBuild(int numVertices, int numTrias, double *inCoords, int *inTrias, RodLinkage *linkage, double tgt_joint_weight, const char **errorMessage);

    EROD_API SurfaceAttractedLinkage *erodXShellAttractedSurfaceCopy(SurfaceAttractedLinkage *linkage, const char **errorMessage);

    // Target Surface
    EROD_API int erodXShellInferTargetSurface(RodLinkage *linkage, size_t nsubdiv, size_t numExtensionLayers, double **outCoords, int **outTrias, size_t *numCoords, size_t *numTrias, const char **errorMessage);

    // Linkage
    EROD_API RodLinkage *erodXShellCopy(RodLinkage *linkage, const char **errorMessage);

    EROD_API RodLinkage::RodSegment *erodXShellBuildRodSegment(int numVertices, double *inCoords);

    EROD_API RodLinkage *erodXShellBuildFromEdgeData(int numVertices, int numEdges, double *inCoords, int *inEdges, double *inNormals, int subdivision, int interleavingType, int initConsistentAngle, const char **errorMessage);

    EROD_API RodLinkage *erodXShellBuildFromJointData(int numVertices, int numJoints, int numEdges,
                                                      double *inRestLengths, int *inOffsetInteriorCoords, double *inInteriorCoords,
                                                      int *inStartJoints, int *inEndJoints,
                                                      double *inCoords, double *inNormals,
                                                      double *inEdgesA, double *inEdgesB,
                                                      int *inSegmentsA, int *inSegmentsB,
                                                      int *inIsStartA, int *inIsStartB,
                                                      int *inJointForVertex, int *inEdges, int inFirstJointVtx,
                                                      int interleavingType, int checkConsistentNormals, int initConsistentAngle, const char **errorMessage);

    EROD_API void erodXShellSetMaterial(RodLinkage *linkage, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType);

    EROD_API void erodXShellSetCustomMaterial(RodLinkage *linkage, double E, double nu, double *inCoords, int numVertices, double *inHolesCoords, int numHoles, int axisType);

    EROD_API void erodXShellSetJointMaterial(RodLinkage *linkage, size_t numMaterials, int *sectionType, double *E, double *nu, double *sectionParams, int *sectionParamsCount, int *axisType);

    EROD_API void erodXShellSetCustomJointMaterial(RodLinkage *linkage, size_t numMaterials, int *sectionType, double *E, double *nu, double *inCoords, int *inCoordsCount, double *inHolesCoords, int *inHolesCount, int *axisType);

    EROD_API void erodXShellSetDesignParameterConfig(RodLinkage *linkage, int use_restLen, int use_restKappa, int update_designParams_cache);

    EROD_API int erodXShellGetCentralJointIndex(RodLinkage *linkage);

    EROD_API void erodXShellGetDofOffsetForJoint(RodLinkage *linkage, int joint, int *DOF, int numDOF, int *outVars);

    EROD_API void erodXShellGetDofOffsetForCenterLinePos(RodLinkage *linkage, int index, int *DOF, int numDOF, int *outVars);

    EROD_API void erodXShellGetCenterLinePositions(RodLinkage *linkage, double** outCoords, size_t* numCoords);

    EROD_API double erodXShellGetAverageJointAngle(RodLinkage *linkage);

    EROD_API double erodXShellGetEnergy(RodLinkage *linkage);

    EROD_API double erodXShellGetEnergyBend(RodLinkage *linkage);

    EROD_API double erodXShellGetEnergyStretch(RodLinkage *linkage);

    EROD_API double erodXShellGetEnergyTwist(RodLinkage *linkage);

    EROD_API double erodXShellGetMaxStrain(RodLinkage *linkage);

    EROD_API size_t erodXShellGetCenterLinePositionsCount(RodLinkage *linkage); 

    EROD_API size_t erodXShellGetJointsCount(RodLinkage *linkage);

    EROD_API size_t erodXShellGetDoFCount(RodLinkage* linkage);

    EROD_API size_t erodXShellGetRodSegmentsCount(RodLinkage *linkage);

    EROD_API size_t erodXShellGetRestKappaVarsCount(RodLinkage *linkage);

    EROD_API void erodXShellGetRestKappaVars(RodLinkage *linkage, double **outData, size_t *numData);

    EROD_API void erodXShellGetMeshData(RodLinkage* linkage, double** outCoords, int** outQuads, size_t* numCoords, size_t* numQuads);

    EROD_API void erodXShellGetRodSegmentIndexesPerRod(RodLinkage *linkage, int index, int **segmentIndexes, size_t *numSeg, int *type);

    EROD_API size_t erodXShellGetRodTraceCount(RodLinkage *linkage, const char **errorMessage);

    EROD_API void erodXShellRemoveRestCurvatures(RodLinkage *linkage);

    EROD_API size_t erodXShellHessianNNZ(RodLinkage *linkage, int variableDesignParameters);

    EROD_API double erodXShellGetMinJointAngle(RodLinkage *linkage);

    EROD_API double erodXShellGetTotalRestLength(RodLinkage *linkage);

    EROD_API double erodXShellGetMaxRodEnergy(RodLinkage *linkage);

    EROD_API void erodXShellSetStiffenRegions(RodLinkage *linkage, double factor, double *coords, size_t numBoxes);

    EROD_API double erodXShellStripAutoDiff(double *edgeA, double *edgeB);

    EROD_API void erodXShellGetDesignParams(RodLinkage *linkage, double **outDesignParams, size_t *outNumDesignParameters);

    EROD_API int erodXShelNumberDesignParameters(RodLinkage *linkage);
    
    EROD_API void erodXShellGetDoFs(RodLinkage *linkage, double **outDoFs, size_t *numDoFs);

    EROD_API void erodXShellSetDoFs(RodLinkage *linkage, double *inDoFs, size_t numDoFs);

    EROD_API void erodXShellGetRestLengthsSolveDoFs(RodLinkage *linkage, double **outDoFs, size_t *numDoFs);

    EROD_API void erodXShellSetRestLengthsSolveDoFs(RodLinkage *linkage, double *outDoFs, size_t numDoFs);

    EROD_API void erodXShellScalarFieldSqrtBendingEnergies(RodLinkage *linkage, double **outField, size_t *numField);

    EROD_API void erodXShellScalarFieldMaxBendingStresses(RodLinkage *linkage, double **outField, size_t *numField);
    
    EROD_API void erodXShellScalarFieldVonMisesStresses(RodLinkage *linkage, double **outField, size_t *numField);

    EROD_API void erodXShellScalarFieldMinBendingStresses(RodLinkage *linkage, double **outField, size_t *numField);

    EROD_API void erodXShellScalarFieldTwistingStresses(RodLinkage *linkage, double **outField, size_t *numField);

    EROD_API void erodXShellScalarFieldStretchingStresses(RodLinkage *linkage, double **outField, size_t *numField);

    EROD_API double erodXShellGetInitialMinRestLength(RodLinkage *linkage);

    EROD_API void erodXShellGetPerSegmentRestLength(RodLinkage *linkage, double **outRestLengths, size_t *numRestLengths);

    EROD_API int erodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(RodLinkage *linkage, double **outAx, long **outAi, long **outAp, long *outM, long *outN, long *outNZ, const char **errorMessage);

    EROD_API void erodXShellGetDesignParameterConfig(RodLinkage *linkage, int *use_restLen, int *use_restKappa);

    EROD_API void erodXSHellGetJointAngles(RodLinkage *linkage, double **outAngles, size_t *numAngles);

    // Material
    EROD_API RodMaterial *erodMaterialBuild(int sectionType, double E, double nu, double *params, int numParams, int axisType);

    EROD_API RodMaterial *erodMaterialCustomBuild(double E, double nu, double *inCoords, int numVertices, double *inHolesCoords, int numHoles, int axisType);

    EROD_API void erodMaterialSetToLinkage(RodMaterial **materials, size_t numMaterials, RodLinkage *linkage);

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
    EROD_API const RodLinkage::Joint *erodJointBuild(RodLinkage *linkage, size_t index);

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

    EROD_API void erodJointGetNormalSigns(RodLinkage::Joint *joint, size_t **outSigns);

    EROD_API double erodJointGetSignB(RodLinkage::Joint *joint);

    EROD_API double erodJointGetLenA(RodLinkage::Joint *joint);

    EROD_API double erodJointGetLenB(RodLinkage::Joint *joint);

    EROD_API void erodJointGetSegmentsA(RodLinkage::Joint *joint, int **outSegmentsA);

    EROD_API void erodJointGetSegmentsB(RodLinkage::Joint *joint, int **outSegmentsB);

    EROD_API int erodJointGetType(RodLinkage::Joint *joint);

    // Segments
    EROD_API void erodRodSegmentMaterialFrame(RodLinkage::RodSegment *segment, size_t *outCoordsCount, double **outCoordsD1, double **outCoordsD2);

    EROD_API const RodLinkage::RodSegment *erodRodSegmentBuild(RodLinkage *linkage, size_t index);

    EROD_API size_t erodRodSegmentGetEdgesCount(RodLinkage::RodSegment *segment);

    EROD_API size_t erodRodSegmentGetVerticesCount(RodLinkage::RodSegment *segment);

    EROD_API size_t erodRodSegmentGetRestAnglesCount(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetRestLengths(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetRestKappas(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetStretchingStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetTwistingStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetCenterLinePositions(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetMaxBendingStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetMinBendingStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetSqrtBendingEnergies(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetMeshData(RodLinkage::RodSegment *segment, double **outCoords, int **outQuads, size_t *numCoords, size_t *numQuads);

    EROD_API int erodRodSegmentGetStartJointIndex(RodLinkage::RodSegment *segment);

    EROD_API int erodRodSegmentGetEndJointIndex(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetBendingStiffness(RodLinkage::RodSegment *segment, double **lambda1, double **lambda2, size_t *numLambda1, size_t *numLambda2);

    EROD_API void erodRodSegmentGetTwistingStiffness(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetStretchingStiffness(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API double erodRodSegmentGetEnergy(RodLinkage::RodSegment *segment);

    EROD_API double erodRodSegmentGetEnergyBend(RodLinkage::RodSegment *segment);

    EROD_API double erodRodSegmentGetEnergyStretch(RodLinkage::RodSegment *segment);

    EROD_API double erodRodSegmentGetEnergyTwist(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetRestPoints(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetRestDirectors(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API void erodRodSegmentGetRestTwists(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API int erodRodSegmentGetBendingEnergyType(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetDensities(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    EROD_API double erodRodSegmentGetInitialMinRestLength(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetEdgeMaterial(RodLinkage::RodSegment *segment, size_t idx, double **outMatData, double **outCoords, int **outEdges, size_t *numMatData, size_t *numCoords, size_t *numEdges);

    EROD_API int erodRodSegmentGetEdgeMaterialCount(RodLinkage::RodSegment *segment);

    EROD_API void erodRodSegmentGetDeformedState(RodLinkage::RodSegment *segment, double **outPtsCoords, double **outThetas, double **outTgtCoords, 
                                                double **outDirCoords, double **outSrcThetas, double **outSrcTwist, 
                                                size_t *outNumPtsCoords, size_t *outNumThetas, size_t *outNumTgtCoords,
                                                size_t *outNumDirCoords, size_t *outNumSrcThetas, size_t *outNumSrcTwist);
    
    EROD_API void erodRodSegmentGetVonMisesStresses(RodLinkage::RodSegment *segment, double **outData, size_t *numData);

    // ElasticRod
    EROD_API ElasticRod *erodElasticRodBuild(int numPoints, double *inCoords, const char **errorMessage);

    EROD_API ElasticRod *erodElasticRodCopy(ElasticRod *rod, const char **errorMessage);

    EROD_API void erodElasticRodRemoveRestCurvatures(ElasticRod *rod);

    EROD_API void erodElasticRodGetMeshData(ElasticRod *rod, double** outCoords, int** outQuads, size_t* numCoords, size_t* numQuads);

    EROD_API void erodElasticRodSetMaterial(ElasticRod *rod, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType);

    EROD_API size_t erodElasticRodGetDoFCount(ElasticRod *rod);

    EROD_API double erodElasticRodGetEnergy(ElasticRod *rod);

    EROD_API size_t erodElasticRodGetEdgesCount(ElasticRod *rod);

    EROD_API size_t erodElasticRodGetVerticesCount(ElasticRod *rod);

    EROD_API void erodElasticRodGetRestLengths(ElasticRod *rod, double *lengths, const size_t numEdges);

    EROD_API void erodElasticRodGetStretchingStresses(ElasticRod *rod, double *stresses, const size_t numEdges);

    EROD_API void erodElasticRodGetTwistingStresses(ElasticRod *rod, double *stresses, const size_t numVertices);

    EROD_API void erodElasticRodGetCenterLinePositions(ElasticRod *rod, double* outCoords, const size_t numCoords);

    EROD_API void erodElasticRodGetMaxBendingStresses(ElasticRod *rod, double* stresses, const size_t numVertices);

    EROD_API void erodElasticRodGetMinBendingStresses(ElasticRod *rod, double* stresses, const size_t numVertices);

    EROD_API void erodElasticRodGetSqrtBendingEnergies(ElasticRod *rod, double* stresses, const size_t numVertices);


    // ElasticRod Periodic
    EROD_API PeriodicRod *erodPeriodicElasticRodBuild(int numPoints, double *inCoords, int removeCurvature, const char **errorMessage);

    EROD_API PeriodicRod *erodPeriodicElasticRodCopy(PeriodicRod *pRod, const char **errorMessage);

    EROD_API void erodPeriodicElasticRodGetMeshData(PeriodicRod *pRod, double** outCoords, int** outQuads, size_t* numCoords, size_t* numQuads);

    EROD_API void erodPeriodicElasticRodSetMaterial(PeriodicRod *pRod, int sectionType, double E, double nu, double *sectionParams, int numParams, int axisType);

    EROD_API size_t erodPeriodicElasticRodGetDoFCount(PeriodicRod *pRod);

    EROD_API double erodPeriodicElasticRodGetEnergy(PeriodicRod *pRod);

    EROD_API size_t erodPeriodicElasticRodGetEdgesCount(PeriodicRod *pRod);

    EROD_API size_t erodPeriodicElasticRodGetVerticesCount(PeriodicRod *pRod);

    EROD_API void erodPeriodicElasticRodGetRestLengths(PeriodicRod *pRod, double *lengths, const size_t numEdges);

    EROD_API void erodPeriodicElasticRodGetStretchingStresses(PeriodicRod *pRod, double *stresses, const size_t numEdges);

    EROD_API void erodPeriodicElasticRodGetTwistingStresses(PeriodicRod *pRod, double *stresses, const size_t numVertices);

    EROD_API void erodPeriodicElasticRodGetCenterLinePositions(PeriodicRod *pRod, double* outCoords, const size_t numCoords);

    EROD_API void erodPeriodicElasticRodGetMaxBendingStresses(PeriodicRod *pRod, double* stresses, const size_t numVertices);

    EROD_API void erodPeriodicElasticRodGetMinBendingStresses(PeriodicRod *pRod, double* stresses, const size_t numVertices);

    EROD_API void erodPeriodicElasticRodGetSqrtBendingEnergies(PeriodicRod *pRod, double* stresses, const size_t numVertices);

}
#endif