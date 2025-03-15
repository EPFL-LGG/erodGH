using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class ElasticRod
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodBuild")]
            internal static extern IntPtr ErodElasticRodBuild(int numPoints, [In] double[] inCoords, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodCopy")]
            internal static extern IntPtr ErodElasticRodCopy(IntPtr rod, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetMeshData")]
            internal static extern void ErodElasticRodGetMeshData(IntPtr rod, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodSetMaterial")]
            internal static extern void ErodElasticRodSetMaterial(IntPtr rod, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetDoFCount")]
            internal static extern int ErodElasticRodGetDoFCount(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodRemoveRestCurvatures")]
            internal static extern void ErodElasticRodRemoveRestCurvatures(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetEdgesCount")]
            internal static extern int ErodElasticRodGetEdgesCount(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetVerticesCount")]
            internal static extern int ErodElasticRodGetVerticesCount(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetRestLengths")]
            internal static extern void ErodElasticRodGetRestLengths(IntPtr rod, [In, Out] double[] restLengths, int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetStretchingStresses")]
            internal static extern void ErodElasticRodGetStretchingStresses(IntPtr rod, [In, Out] double[] stresses, int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetTwistingStresses")]
            internal static extern void ErodElasticRodGetTwistingStresses(IntPtr rod, [In, Out] double[] stresses, int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetCenterLinePositions")]
            internal static extern void ErodElasticRodGetCenterLinePositions(IntPtr rod, [In, Out] double[] outCoords, int numCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetMaxBendingStresses")]
            internal static extern void ErodElasticRodGetMaxBendingStresses(IntPtr rod, [In, Out] double[] stresses, int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetMinBendingStresses")]
            internal static extern void ErodElasticRodGetMinBendingStresses(IntPtr rod, [In, Out] double[] stresses, int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetSqrtBendingEnergies")]
            internal static extern void ErodElasticRodGetSqrtBendingEnergies(IntPtr rod, [In, Out] double[] stresses, int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetDoFs")]
            internal static extern void ErodElasticRodGetDoFs(IntPtr rod, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodSetDoFs")]
            internal static extern void ErodElasticRodSetDoFs(IntPtr rod, [In] double[] inDoFs, [In] int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetMaterialFrame")]
            internal static extern void ErodElasticRodGetMaterialFrame(IntPtr rod, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetEnergy")]
            internal static extern double ErodElasticRodGetEnergy(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetEnergyBend")]
            internal static extern double ErodElasticRodGetEnergyBend(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetEnergyStretch")]
            internal static extern double ErodElasticRodGetEnergyStretch(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetEnergyTwist")]
            internal static extern double ErodElasticRodGetEnergyTwist(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetMaxStrain")]
            internal static extern double ErodElasticRodGetMaxStrain(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetBendingStiffness")]
            internal static extern void ErodElasticRodGetBendingStiffness(IntPtr rod, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetTwistingStiffness")]
            internal static extern void ErodElasticRodGetTwistingStiffness(IntPtr rod, out IntPtr stiffness, out int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetStretchingStiffness")]
            internal static extern void ErodElasticRodGetStretchingStiffness(IntPtr rod, out IntPtr stiffness, out int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetVonMisesStresses")]
            internal static extern void ErodElasticRodGetVonMisesStresses(IntPtr rod, out IntPtr outData, out int numData);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetInitialMinRestLength")]
            internal static extern double ErodElasticRodGetInitialMinRestLength(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetRestKappas")]
            internal static extern void ErodElasticRodGetRestKappas(IntPtr rod, out IntPtr restAngles, out int numAngles);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetScalarFieldSqrtBendingEnergies")]
            internal static extern void ErodElasticRodGetScalarFieldSqrtBendingEnergies(IntPtr rod, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetScalarFieldMaxBendingStresses")]
            internal static extern void ErodElasticRodGetScalarFieldMaxBendingStresses(IntPtr rod, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetScalarFieldMinBendingStresses")]
            internal static extern void ErodElasticRodGetScalarFieldMinBendingStresses(IntPtr rod, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetScalarFieldTwistingStresses")]
            internal static extern void ErodElasticRodGetScalarFieldTwistingStresses(IntPtr rod, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetScalarFieldStretchingStresses")]
            internal static extern void ErodElasticRodGetScalarFieldStretchingStresses(IntPtr rod, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetScalarFieldVonMisesStresses")]
            internal static extern void ErodElasticRodGetScalarFieldVonMisesStresses(IntPtr rod, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetThetaOffset")]
            internal static extern int ErodElasticRodGetThetaOffset(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetRestLengthOffset")]
            internal static extern int ErodElasticRodGetRestLengthOffset(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetRestKappaOffset")]
            internal static extern int ErodElasticRodGetRestKappaOffset(IntPtr rod);

        }
    }
}
