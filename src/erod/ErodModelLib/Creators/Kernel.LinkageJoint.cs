using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
	public static partial class Kernel
	{
		public static class LinkageJoint
		{
            // Joints
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointBuild")]
            internal static extern IntPtr ErodJointBuild(IntPtr linkage, int index);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetPosition")]
            internal static extern void ErodJointGetPosition(IntPtr joint, out IntPtr outCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetNormal")]
            internal static extern void ErodJointGetNormal(IntPtr joint, out IntPtr outCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetEdgeVecA")]
            internal static extern void ErodJointGetEdgeVecA(IntPtr joint, out IntPtr outCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetEdgeVecB")]
            internal static extern void ErodJointGetEdgeVecB(IntPtr joint, out IntPtr outCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetConnectedSegments")]
            internal static extern void ErodJointGetConnectedSegments(IntPtr joint, out IntPtr outSegmentsA, out IntPtr outSegmentsB);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetIsStartA")]
            internal static extern void ErodJointGetIsStartA(IntPtr joint, out IntPtr outStartA);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetIsStartB")]
            internal static extern void ErodJointGetIsStartB(IntPtr joint, out IntPtr outStartB);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetOmega")]
            internal static extern void ErodJointGetOmega(IntPtr joint, out IntPtr outCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetSourceTangent")]
            internal static extern void ErodJointGetSourceTangent(IntPtr joint, out IntPtr outCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetSourceNormal")]
            internal static extern void ErodJointGetSourceNormal(IntPtr joint, out IntPtr outCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetAlpha")]
            internal static extern double ErodJointGetAlpha(IntPtr joint);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetNormalSigns")]
            internal static extern void ErodJointGetNormalSigns(IntPtr joint, out IntPtr outSigns);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetSignB")]
            internal static extern double ErodJointGetSignB(IntPtr joint);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetLenA")]
            internal static extern double ErodJointGetLenA(IntPtr joint);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetLenB")]
            internal static extern double ErodJointGetLenB(IntPtr joint);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetSegmentsA")]
            internal static extern void ErodJointGetSegmentsA(IntPtr joint, out IntPtr outSegmentsA);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetSegmentsB")]
            internal static extern void ErodJointGetSegmentsB(IntPtr joint, out IntPtr outSegmentsB);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodJointGetType")]
            internal static extern int ErodJointGetType(IntPtr joint);

        }
	}
}
