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
            internal static extern IntPtr ErodPeriodicElasticRodBuild(int numPoints, [In] double[] inCoords, bool removeCurvature, out IntPtr errorMessage);

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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodGetEnergy")]
            internal static extern double ErodPeriodicElasticRodGetEnergy(IntPtr rod);

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
        }
    }
}
