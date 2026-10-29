using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class Material
        {
            internal static NativeHandle ErodMaterialCopy(NativeHandle material)
            {
                var owner=new NativeHandle();
                try { using(NativeCall.Enter(material)) { owner.Initialize(Native_ErodMaterialCopy(material), kind: 5); NativeApi.Check("erodMaterialCopy"); return owner; } }
                catch { owner.Dispose(); throw; }
            }
            [DllImport(erod_dylib, CallingConvention=CallingConvention.Cdecl, EntryPoint="erodMaterialCopy")]
            private static extern IntPtr Native_ErodMaterialCopy(NativeHandle material);

            internal static NativeHandle ErodMaterialBuild(int sectionType, double E, double nu, [In] double[] parameters, int numParams, int axisType)
            {
                if (numParams < 0) throw new ArgumentOutOfRangeException(nameof(numParams));
                NativeChecks.Array(parameters, NativeChecks.Product(numParams, 1), nameof(parameters));
                NativeChecks.Finite(parameters, nameof(parameters));
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter())
                {
                    var result = Native_ErodMaterialBuild(sectionType, E, nu, parameters, numParams, axisType);
                    owner.Initialize(result, kind: 5);
                    try
                    {
                        NativeApi.Check("erodMaterialBuild");
                    }
                    catch
                    {
                        throw;
                    }
                    if (owner.IsInvalid) throw new NativeInteropException("erodMaterialBuild", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialBuild")]
            private static extern IntPtr Native_ErodMaterialBuild(int sectionType, double E, double nu, [In] double[] parameters, int numParams, int axisType);

            internal static NativeHandle ErodMaterialCustomBuild(double E, double nu, double[] inCoords, int numVertices, double[] inHolesCoords, int numHoles, int axisType)
            {
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                if (numHoles < 0) throw new ArgumentOutOfRangeException(nameof(numHoles));
                NativeChecks.Array(inCoords, NativeChecks.Product(numVertices, 3), nameof(inCoords));
                NativeChecks.Array(inHolesCoords, NativeChecks.Product(numHoles, 2), nameof(inHolesCoords));
                NativeChecks.Finite(inCoords, nameof(inCoords));
                NativeChecks.Finite(inHolesCoords, nameof(inHolesCoords));
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter())
                {
                    var result = Native_ErodMaterialCustomBuild(E, nu, inCoords, numVertices, inHolesCoords, numHoles, axisType);
                    owner.Initialize(result, kind: 5);
                    try
                    {
                        NativeApi.Check("erodMaterialCustomBuild");
                    }
                    catch
                    {
                        throw;
                    }
                    if (owner.IsInvalid) throw new NativeInteropException("erodMaterialCustomBuild", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialCustomBuild")]
            private static extern IntPtr Native_ErodMaterialCustomBuild(double E, double nu, double[] inCoords, int numVertices, double[] inHolesCoords, int numHoles, int axisType);

            internal static void ErodMaterialSetToLinkage(NativeHandle[] materials, int numMaterials, NativeHandle linkage)
            {
                if (numMaterials < 0) throw new ArgumentOutOfRangeException(nameof(numMaterials));
                NativeChecks.Array(materials, NativeChecks.Product(numMaterials, 1), nameof(materials));
                using (NativeCall.Enter(materials.Concat(new[] { linkage }).ToArray()))
                {
                    Native_ErodMaterialSetToLinkage(materials.Select(h => h.DangerousGetHandle()).ToArray(), numMaterials, linkage);
                    try
                    {
                        NativeApi.Check("erodMaterialSetToLinkage");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialSetToLinkage")]
            private static extern void Native_ErodMaterialSetToLinkage([In] IntPtr[] materials, int numMaterials, NativeHandle linkage);

            internal static double ErodMaterialGetArea(NativeHandle material)
            {
                using (NativeCall.Enter(material))
                {
                    var result = Native_ErodMaterialGetArea(material);
                    try
                    {
                        NativeApi.Check("erodMaterialGetArea");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialGetArea")]
            private static extern double Native_ErodMaterialGetArea(NativeHandle material);

            internal static void ErodMaterialGetMomentOfInertia(NativeHandle material, out double lambda1, out double lambda2)
            {
                lambda1 = default(double);
                lambda2 = default(double);
                using (NativeCall.Enter(material))
                {
                    Native_ErodMaterialGetMomentOfInertia(material, out lambda1, out lambda2);
                    try
                    {
                        NativeApi.Check("erodMaterialGetMomentOfInertia");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialGetMomentOfInertia")]
            private static extern void Native_ErodMaterialGetMomentOfInertia(NativeHandle material, out double lambda1, out double lambda2);

            internal static void ErodMaterialGetBendingStiffness(NativeHandle material, out double lambda1, out double lambda2)
            {
                lambda1 = default(double);
                lambda2 = default(double);
                using (NativeCall.Enter(material))
                {
                    Native_ErodMaterialGetBendingStiffness(material, out lambda1, out lambda2);
                    try
                    {
                        NativeApi.Check("erodMaterialGetBendingStiffness");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialGetBendingStiffness")]
            private static extern void Native_ErodMaterialGetBendingStiffness(NativeHandle material, out double lambda1, out double lambda2);

            internal static double ErodMaterialGetTwistingStiffness(NativeHandle material)
            {
                using (NativeCall.Enter(material))
                {
                    var result = Native_ErodMaterialGetTwistingStiffness(material);
                    try
                    {
                        NativeApi.Check("erodMaterialGetTwistingStiffness");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialGetTwistingStiffness")]
            private static extern double Native_ErodMaterialGetTwistingStiffness(NativeHandle material);

            internal static double ErodMaterialGetStretchingStiffness(NativeHandle material)
            {
                using (NativeCall.Enter(material))
                {
                    var result = Native_ErodMaterialGetStretchingStiffness(material);
                    try
                    {
                        NativeApi.Check("erodMaterialGetStretchingStiffness");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialGetStretchingStiffness")]
            private static extern double Native_ErodMaterialGetStretchingStiffness(NativeHandle material);

            internal static double ErodMaterialGetSherModulus(NativeHandle material)
            {
                using (NativeCall.Enter(material))
                {
                    var result = Native_ErodMaterialGetSherModulus(material);
                    try
                    {
                        NativeApi.Check("erodMaterialGetSherModulus");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialGetSherModulus")]
            private static extern double Native_ErodMaterialGetSherModulus(NativeHandle material);

            internal static double ErodMaterialGetCrossSectionHeight(NativeHandle material)
            {
                using (NativeCall.Enter(material))
                {
                    var result = Native_ErodMaterialGetCrossSectionHeight(material);
                    try
                    {
                        NativeApi.Check("erodMaterialGetCrossSectionHeight");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodMaterialGetCrossSectionHeight")]
            private static extern double Native_ErodMaterialGetCrossSectionHeight(NativeHandle material);
        }
    }
}
