using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class Analysis
        {
            internal static double ErodXShellGetEnergy(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetEnergy(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetEnergy");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetEnergy")]
            private static extern double Native_ErodXShellGetEnergy(NativeHandle linkage);

            internal static double ErodXShellGetEnergyBend(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetEnergyBend(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetEnergyBend");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetEnergyBend")]
            private static extern double Native_ErodXShellGetEnergyBend(NativeHandle linkage);

            internal static double ErodXShellGetEnergyStretch(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetEnergyStretch(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetEnergyStretch");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetEnergyStretch")]
            private static extern double Native_ErodXShellGetEnergyStretch(NativeHandle linkage);

            internal static double ErodXShellGetEnergyTwist(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetEnergyTwist(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetEnergyTwist");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetEnergyTwist")]
            private static extern double Native_ErodXShellGetEnergyTwist(NativeHandle linkage);

            internal static double ErodXShellGetMaxStrain(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetMaxStrain(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetMaxStrain");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetMaxStrain")]
            private static extern double Native_ErodXShellGetMaxStrain(NativeHandle linkage);

            internal static double ErodXShellGetMaxRodEnergy(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetMaxRodEnergy(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetMaxRodEnergy");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetMaxRodEnergy")]
            private static extern double Native_ErodXShellGetMaxRodEnergy(NativeHandle linkage);

        }
    }
}
