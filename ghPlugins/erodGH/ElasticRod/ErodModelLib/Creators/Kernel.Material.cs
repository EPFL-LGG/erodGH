using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class Material
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialBuild")]
            internal static extern IntPtr ErodMaterialBuild(int sectionType, double E, double nu, [In] double[] parameters, int numParams, int axisType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialCustomBuild")]
            internal static extern IntPtr ErodMaterialCustomBuild(double E, double nu, double[] inCoords, int numVertices, double[] inHolesCoords, int numHoles, int axisType);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialSetToLinkage")]
            internal static extern void ErodMaterialSetToLinkage(IntPtr[] materials, int numMaterials, IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialGetArea")]
            internal static extern double ErodMaterialGetArea(IntPtr material);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialGetMomentOfInertia")]
            internal static extern void ErodMaterialGetMomentOfInertia(IntPtr material, out double lambda1, out double lambda2);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialGetBendingStiffness")]
            internal static extern void ErodMaterialGetBendingStiffness(IntPtr material, out double lambda1, out double lambda2);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialGetTwistingStiffness")]
            internal static extern double ErodMaterialGetTwistingStiffness(IntPtr material);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialGetStretchingStiffness")]
            internal static extern double ErodMaterialGetStretchingStiffness(IntPtr material);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialGetSherModulus")]
            internal static extern double ErodMaterialGetSherModulus(IntPtr material);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodMaterialGetCrossSectionHeight")]
            internal static extern double ErodMaterialGetCrossSectionHeight(IntPtr material);
        }
    }
}
