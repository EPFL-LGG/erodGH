using System.Linq;
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
            internal static NativeHandle ErodJointBuild(NativeHandle linkage, int index)
            {
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodJointBuild(linkage, index);
                    owner.Initialize(result, linkage);
                    try
                    {
                        NativeApi.Check("erodJointBuild");
                    }
                    catch
                    {
                        throw;
                    }
                    if (owner.IsInvalid) throw new NativeInteropException("erodJointBuild", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointBuild")]
            private static extern IntPtr Native_ErodJointBuild(NativeHandle linkage, int index);

            internal static void ErodJointGetPosition(NativeHandle joint, out IntPtr outCoords)
            {
                outCoords = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetPosition(joint, out outCoords);
                    try
                    {
                        NativeApi.Check("erodJointGetPosition");
                        NativeBufferScope.Track(outCoords);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetPosition")]
            private static extern void Native_ErodJointGetPosition(NativeHandle joint, out IntPtr outCoords);

            internal static void ErodJointGetNormal(NativeHandle joint, out IntPtr outCoords)
            {
                outCoords = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetNormal(joint, out outCoords);
                    try
                    {
                        NativeApi.Check("erodJointGetNormal");
                        NativeBufferScope.Track(outCoords);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetNormal")]
            private static extern void Native_ErodJointGetNormal(NativeHandle joint, out IntPtr outCoords);

            internal static void ErodJointGetEdgeVecA(NativeHandle joint, out IntPtr outCoords)
            {
                outCoords = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetEdgeVecA(joint, out outCoords);
                    try
                    {
                        NativeApi.Check("erodJointGetEdgeVecA");
                        NativeBufferScope.Track(outCoords);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetEdgeVecA")]
            private static extern void Native_ErodJointGetEdgeVecA(NativeHandle joint, out IntPtr outCoords);

            internal static void ErodJointGetEdgeVecB(NativeHandle joint, out IntPtr outCoords)
            {
                outCoords = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetEdgeVecB(joint, out outCoords);
                    try
                    {
                        NativeApi.Check("erodJointGetEdgeVecB");
                        NativeBufferScope.Track(outCoords);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetEdgeVecB")]
            private static extern void Native_ErodJointGetEdgeVecB(NativeHandle joint, out IntPtr outCoords);

            internal static void ErodJointGetConnectedSegments(NativeHandle joint, out IntPtr outSegmentsA, out IntPtr outSegmentsB)
            {
                outSegmentsA = default(IntPtr);
                outSegmentsB = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetConnectedSegments(joint, out outSegmentsA, out outSegmentsB);
                    try
                    {
                        NativeApi.Check("erodJointGetConnectedSegments");
                        NativeBufferScope.Track(outSegmentsA);
                        NativeBufferScope.Track(outSegmentsB);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outSegmentsA); outSegmentsA = IntPtr.Zero;
                        NativeBufferScope.Free(outSegmentsB); outSegmentsB = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetConnectedSegments")]
            private static extern void Native_ErodJointGetConnectedSegments(NativeHandle joint, out IntPtr outSegmentsA, out IntPtr outSegmentsB);

            internal static void ErodJointGetIsStartA(NativeHandle joint, out IntPtr outStartA)
            {
                outStartA = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetIsStartA(joint, out outStartA);
                    try
                    {
                        NativeApi.Check("erodJointGetIsStartA");
                        NativeBufferScope.Track(outStartA);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outStartA); outStartA = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetIsStartA")]
            private static extern void Native_ErodJointGetIsStartA(NativeHandle joint, out IntPtr outStartA);

            internal static void ErodJointGetIsStartB(NativeHandle joint, out IntPtr outStartB)
            {
                outStartB = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetIsStartB(joint, out outStartB);
                    try
                    {
                        NativeApi.Check("erodJointGetIsStartB");
                        NativeBufferScope.Track(outStartB);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outStartB); outStartB = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetIsStartB")]
            private static extern void Native_ErodJointGetIsStartB(NativeHandle joint, out IntPtr outStartB);

            internal static void ErodJointGetOmega(NativeHandle joint, out IntPtr outCoords)
            {
                outCoords = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetOmega(joint, out outCoords);
                    try
                    {
                        NativeApi.Check("erodJointGetOmega");
                        NativeBufferScope.Track(outCoords);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetOmega")]
            private static extern void Native_ErodJointGetOmega(NativeHandle joint, out IntPtr outCoords);

            internal static void ErodJointGetSourceTangent(NativeHandle joint, out IntPtr outCoords)
            {
                outCoords = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetSourceTangent(joint, out outCoords);
                    try
                    {
                        NativeApi.Check("erodJointGetSourceTangent");
                        NativeBufferScope.Track(outCoords);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetSourceTangent")]
            private static extern void Native_ErodJointGetSourceTangent(NativeHandle joint, out IntPtr outCoords);

            internal static void ErodJointGetSourceNormal(NativeHandle joint, out IntPtr outCoords)
            {
                outCoords = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetSourceNormal(joint, out outCoords);
                    try
                    {
                        NativeApi.Check("erodJointGetSourceNormal");
                        NativeBufferScope.Track(outCoords);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetSourceNormal")]
            private static extern void Native_ErodJointGetSourceNormal(NativeHandle joint, out IntPtr outCoords);

            internal static double ErodJointGetAlpha(NativeHandle joint)
            {
                using (NativeCall.Enter(joint))
                {
                    var result = Native_ErodJointGetAlpha(joint);
                    try
                    {
                        NativeApi.Check("erodJointGetAlpha");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetAlpha")]
            private static extern double Native_ErodJointGetAlpha(NativeHandle joint);

            internal static void ErodJointGetNormalSigns(NativeHandle joint, out IntPtr outSigns)
            {
                outSigns = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetNormalSigns(joint, out outSigns);
                    try
                    {
                        NativeApi.Check("erodJointGetNormalSigns");
                        NativeBufferScope.Track(outSigns);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outSigns); outSigns = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetNormalSigns")]
            private static extern void Native_ErodJointGetNormalSigns(NativeHandle joint, out IntPtr outSigns);

            internal static double ErodJointGetSignB(NativeHandle joint)
            {
                using (NativeCall.Enter(joint))
                {
                    var result = Native_ErodJointGetSignB(joint);
                    try
                    {
                        NativeApi.Check("erodJointGetSignB");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetSignB")]
            private static extern double Native_ErodJointGetSignB(NativeHandle joint);

            internal static double ErodJointGetLenA(NativeHandle joint)
            {
                using (NativeCall.Enter(joint))
                {
                    var result = Native_ErodJointGetLenA(joint);
                    try
                    {
                        NativeApi.Check("erodJointGetLenA");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetLenA")]
            private static extern double Native_ErodJointGetLenA(NativeHandle joint);

            internal static double ErodJointGetLenB(NativeHandle joint)
            {
                using (NativeCall.Enter(joint))
                {
                    var result = Native_ErodJointGetLenB(joint);
                    try
                    {
                        NativeApi.Check("erodJointGetLenB");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetLenB")]
            private static extern double Native_ErodJointGetLenB(NativeHandle joint);

            internal static void ErodJointGetSegmentsA(NativeHandle joint, out IntPtr outSegmentsA)
            {
                outSegmentsA = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetSegmentsA(joint, out outSegmentsA);
                    try
                    {
                        NativeApi.Check("erodJointGetSegmentsA");
                        NativeBufferScope.Track(outSegmentsA);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outSegmentsA); outSegmentsA = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetSegmentsA")]
            private static extern void Native_ErodJointGetSegmentsA(NativeHandle joint, out IntPtr outSegmentsA);

            internal static void ErodJointGetSegmentsB(NativeHandle joint, out IntPtr outSegmentsB)
            {
                outSegmentsB = default(IntPtr);
                using (NativeCall.Enter(joint))
                {
                    Native_ErodJointGetSegmentsB(joint, out outSegmentsB);
                    try
                    {
                        NativeApi.Check("erodJointGetSegmentsB");
                        NativeBufferScope.Track(outSegmentsB);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outSegmentsB); outSegmentsB = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetSegmentsB")]
            private static extern void Native_ErodJointGetSegmentsB(NativeHandle joint, out IntPtr outSegmentsB);

            internal static int ErodJointGetType(NativeHandle joint)
            {
                using (NativeCall.Enter(joint))
                {
                    var result = Native_ErodJointGetType(joint);
                    try
                    {
                        NativeApi.Check("erodJointGetType");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodJointGetType")]
            private static extern int Native_ErodJointGetType(NativeHandle joint);

        }
	}
}
