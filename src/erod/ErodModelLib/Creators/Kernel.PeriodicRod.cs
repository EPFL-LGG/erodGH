using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class PeriodicRod
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodBuild")]
            internal static extern IntPtr ErodPeriodicElasticRodBuild(int numPoints, [In] double[] inCoords, int removeCurvature, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodCopy")]
            internal static extern IntPtr ErodPeriodicElasticRodCopy(IntPtr rod, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetMeshData")]
            internal static extern void ErodPeriodicElasticRodGetMeshData(IntPtr rod, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodSetMaterial")]
            internal static extern void ErodPeriodicElasticRodSetMaterial(IntPtr rod, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetDoFCount")]
            internal static extern int ErodPeriodicElasticRodGetDoFCount(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetEdgesCount")]
            internal static extern int ErodPeriodicElasticRodGetEdgesCount(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetVerticesCount")]
            internal static extern int ErodPeriodicElasticRodGetVerticesCount(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetRestLengths")]
            internal static extern void ErodPeriodicElasticRodGetRestLengths(IntPtr rod, [In, Out] double[] restLengths, int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetStretchingStresses")]
            internal static extern void ErodPeriodicElasticRodGetStretchingStresses(IntPtr rod, [In, Out] double[] stresses, int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetTwistingStresses")]
            internal static extern void ErodPeriodicElasticRodGetTwistingStresses(IntPtr rod, [In, Out] double[] stresses, int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetCenterLinePositions")]
            internal static extern void ErodPeriodicElasticRodGetCenterLinePositions(IntPtr rod, [In, Out] double[] outCoords, int numCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetMaxBendingStresses")]
            internal static extern void ErodPeriodicElasticRodGetMaxBendingStresses(IntPtr rod, [In, Out] double[] stresses, int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetMinBendingStresses")]
            internal static extern void ErodPeriodicElasticRodGetMinBendingStresses(IntPtr rod, [In, Out] double[] stresses, int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetSqrtBendingEnergies")]
            internal static extern void ErodPeriodicElasticRodGetSqrtBendingEnergies(IntPtr rod, [In, Out] double[] stresses, int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetDoFs")]
            internal static extern void ErodPeriodicElasticRodGetDoFs(IntPtr rod, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodSetDoFs")]
            internal static extern void ErodPeriodicElasticRodSetDoFs(IntPtr rod, [In] double[] inDoFs, [In] int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetMaterialFrame")]
            internal static extern void ErodPeriodicElasticRodGetMaterialFrame(IntPtr rod, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetEnergy")]
            internal static extern double ErodPeriodicElasticRodGetEnergy(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetEnergyBend")]
            internal static extern double ErodPeriodicElasticRodGetEnergyBend(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetEnergyStretch")]
            internal static extern double ErodPeriodicElasticRodGetEnergyStretch(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetEnergyTwist")]
            internal static extern double ErodPeriodicElasticRodGetEnergyTwist(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetMaxStrain")]
            internal static extern double ErodPeriodicElasticRodGetMaxStrain(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetBendingStiffness")]
            internal static extern void ErodPeriodicElasticRodGetBendingStiffness(IntPtr rod, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetTwistingStiffness")]
            internal static extern void ErodPeriodicElasticRodGetTwistingStiffness(IntPtr rod, out IntPtr stiffness, out int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetStretchingStiffness")]
            internal static extern void ErodPeriodicElasticRodGetStretchingStiffness(IntPtr rod, out IntPtr stiffness, out int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetVonMisesStresses")]
            internal static extern void ErodPeriodicElasticRodGetVonMisesStresses(IntPtr rod, out IntPtr outData, out int numData);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetInitialMinRestLength")]
            internal static extern double ErodPeriodicElasticRodGetInitialMinRestLength(IntPtr rod);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetRestKappas")]
            internal static extern void ErodPeriodicElasticRodGetRestKappas(IntPtr rod, out IntPtr restAngles, out int numAngles);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies")]
            internal static extern void ErodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetScalarFieldMaxBendingStresses")]
            internal static extern void ErodPeriodicElasticRodGetScalarFieldMaxBendingStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetScalarFieldMinBendingStresses")]
            internal static extern void ErodPeriodicElasticRodGetScalarFieldMinBendingStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetScalarFieldTwistingStresses")]
            internal static extern void ErodPeriodicElasticRodGetScalarFieldTwistingStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetScalarFieldStretchingStresses")]
            internal static extern void ErodPeriodicElasticRodGetScalarFieldStretchingStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetScalarFieldVonMisesStresses")]
            internal static extern void ErodPeriodicElasticRodGetScalarFieldVonMisesStresses(IntPtr linkage, out IntPtr outDoFs, out int numDoFs);
        }
    }
}
