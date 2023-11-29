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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodGetEnergy")]
            internal static extern double ErodElasticRodGetEnergy(IntPtr rod);

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

        }
    }
}
