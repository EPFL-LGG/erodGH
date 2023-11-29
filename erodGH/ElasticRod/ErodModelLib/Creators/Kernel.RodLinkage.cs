using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        private const string erod_dylib = "liberod.dylib";

        public static class RodLinkage
        {
            // attracted target surface
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellAttractedSurfaceBuild")]
            internal static extern IntPtr ErodXShellAttractedSurfaceBuild(int numVertices, int numTias, [In] double[] inCoords, [In] int[] inTrias, IntPtr linkage, double weight, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellAttractedSurfaceCopy")]
            public static extern IntPtr ErodXShellAttractedSurfaceCopy(IntPtr linkage, out IntPtr errorMessage);

            // Infer target surface
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellInferTargetSurface")]
            internal static extern int ErodXShellInferTargetSurface(IntPtr linkage, int nsubdiv, int numExtensionLayers, out IntPtr outCoords, out IntPtr outTrias, out int numCoords, out int numTrias, out IntPtr errorMessage);

            // Linkage
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellBuildFromEdgeData")]
            public static extern IntPtr ErodXShellBuildFromEdgeData(int numVertices, int numEdges, [In] double[] inCoords, [In] int[] inEdges, [In] double[] inNormals, int subdivision, int interleavingType, bool initConsistentAngle, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellBuildFromJointData")]
            public static extern IntPtr ErodXShellBuildFromJointData(int numVertices, int numJoints, int numEdges,
                                                                     [In] double[] inRestLengths, [In] int[] inOffsetInteriorNodes, [In] double[] inInteriorNodes,
                                                                     [In] int[] inStartJoints, [In] int[] inEndJoints,
                                                                     [In] double[] inCoords, [In] double[] inNormals,
                                                                     [In] double[] inEdgesA, [In] double[] inEdgesB,
                                                                     [In] int[] inSegmentsA, [In] int[] inSegmentsB,
                                                                     [In] int[] inIsStartA, [In] int[] inIsStartB,
                                                                     [In] int[] inJointForVertex, [In] int[] inEdges, int inFirstJointVtx,
                                                                     int interleavingType, bool checkConsistentNormals, bool initConsistentAngle, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellCopy")]
            public static extern IntPtr ErodXShellCopy(IntPtr linkage, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellSetCustomMaterial")]
            internal static extern void ErodXShellSetCustomMaterial(IntPtr linkage, double E, double nu, [In] double[] inCoords, int numVertices, [In] double[] inHolesCoords, int numHoles, int axisType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellSetMaterial")]
            internal static extern void ErodXShellSetMaterial(IntPtr linkage, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellSetJointMaterial")]
            internal static extern void ErodXShellSetJointMaterial(IntPtr linkage, int numMaterials, [In] int[] sectionType, [In] double[] E, [In] double[] nu, [In] double[] sectionParams, [In] int[] sectionParamsCount, [In] int[] axisType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellSetCustomJointMaterial")]
            internal static extern void ErodXShellSetCustomJointMaterial(IntPtr linkage, int numMaterials, [In] int[] sectionType, [In] double[] E, [In] double[] nu, [In] double[] inCoords, [In] int[] inCoordsCount, [In] double[] inHolesCoords, [In] int[] inHolesCount, [In] int[] axisType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellSetDesignParameterConfig")]
            internal static extern void ErodXShellSetDesignParameterConfig(IntPtr linkage, bool use_restLen, bool use_restKappa, bool update_designParams_cache);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetCentralJointIndex")]
            internal static extern int ErodXShellGetCentralJointIndex(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetDofOffsetForJoint")]
            internal static extern void ErodXShellGetDofOffsetForJoint(IntPtr linkage, int joint, [In] int[] DOF, int numDOF, [In, Out] int[] outVars);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetDofOffsetForCenterLinePos")]
            internal static extern void ErodXShellGetDofOffsetForCenterLinePos(IntPtr linkage, int joint, [In] int[] DOF, int numDOF, [In, Out] int[] outVars);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetCenterLinePositions")]
            internal static extern void ErodXShellGetCenterLinePositions(IntPtr linkage, out IntPtr outCoords, out int numCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetDoFs")]
            internal static extern void ErodXShellGetDoFs(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetAverageJointAngle")]
            internal static extern double ErodXShellGetAverageJointAngle(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetCenterLinePositionsCount")]
            internal static extern int ErodXShellGetCenterLinePositionsCount(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetJointsCount")]
            internal static extern int ErodXShellGetJointsCount(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetDoFCount")]
            internal static extern int ErodXShellGetDoFCount(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetRodSegmentsCount")]
            internal static extern int ErodXShellRodGetSegmentsCount(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetMeshData")]
            internal static extern void ErodXShellGetMeshData(IntPtr linkage, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetRodSegmentIndexesPerRod")]
            internal static extern void ErodXShellGetRodSegmentIndexesPerRod(IntPtr linkage, int index, out IntPtr segmentIndexes, out int numSeg, out bool rodTypeA);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetRodTraceCount")]
            internal static extern int ErodXShellGetRodTraceCount(IntPtr linkage, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellRemoveRestCurvatures")]
            internal static extern double ErodXShellRemoveRestCurvatures(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellHessianNNZ")]
            internal static extern int ErodXShellGetHessianNNZ(IntPtr linkage, bool variableDesignParameters);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetRestKappaVarsCount")]
            internal static extern int ErodXShellGetRestKappaVarsCount(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetRestKappaVars")]
            internal static extern void ErodXShellGetRestKappaVars(IntPtr linkage, out IntPtr restAngles, out int numRestAngles);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetMinJointAngle")]
            internal static extern double ErodXShellGetMinJointAngle(IntPtr linkage);


            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetTotalRestLength")]
            internal static extern double ErodXShellGetTotalRestLength(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellSetStiffenRegions")]
            internal static extern void ErodXShellSetStiffenRegions(IntPtr linkage, double factor, [In, Out] double[] coords, int numBoxes);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellStripAutoDiff")]
            internal static extern double ErodXShellStripAutoDiff(double value);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetDesignParams")]
            internal static extern void ErodXShellGetDesignParams(IntPtr linkage, out IntPtr outDesignParams, out int outNumDesignParameters);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShelNumberDesignParameters")]
            internal static extern int ErodXShellGetDesignParametersNumber(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellScalarFieldSqrtBendingEnergies")]
            internal static extern void ErodXShellScalarFieldSqrtBendingEnergies(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellScalarFieldMaxBendingStresses")]
            internal static extern void ErodXShellScalarFieldMaxBendingStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellScalarFieldMinBendingStresses")]
            internal static extern void ErodXShellScalarFieldMinBendingStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellScalarFieldTwistingStresses")]
            internal static extern void ErodXShellScalarFieldTwistingStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellScalarFieldStretchingStresses")]
            internal static extern void ErodXShellScalarFieldStretchingStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetInitialMinRestLength")]
            internal static extern double ErodXShellGetInitialMinRestLength(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetPerSegmentRestLength")]
            internal static extern void ErodXShellGetPerSegmentRestLength(IntPtr linkage, out IntPtr outRestLengths, out int numRestLengths);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetSegmentRestLenToEdgeRestLenMapTranspose")]
            internal static extern int ErodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(IntPtr linkage, out IntPtr outAx, out IntPtr outAi, out IntPtr outAp, out long outM, out long outN, out long outNZ, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetDesignParameterConfig")]
            internal static extern void ErodXShellGetDesignParameterConfig(IntPtr linkage, out bool restLen, out bool restKappa);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetRestLengthsSolveDoFs")]
            internal static extern void ErodXShellGetRestLengthsSolveDoFs(IntPtr linkage, out IntPtr outDoFs, out long numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellSetRestLengthsSolveDoFs")]
            internal static extern void ErodXShellSetRestLengthsSolveDoFs(IntPtr linkage, double[] outDoFs, int numDoFs);
        }
    }
}

