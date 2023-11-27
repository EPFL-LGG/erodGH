using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class Analysis
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetEnergy")]
            internal static extern double ErodXShellGetEnergy(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetEnergyBend")]
            internal static extern double ErodXShellGetEnergyBend(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetEnergyStretch")]
            internal static extern double ErodXShellGetEnergyStretch(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetEnergyTwist")]
            internal static extern double ErodXShellGetEnergyTwist(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetMaxStrain")]
            internal static extern double ErodXShellGetMaxStrain(IntPtr linkage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellGetMaxRodEnergy")]
            internal static extern double ErodXShellGetMaxRodEnergy(IntPtr linkage);

        }
    }
}
