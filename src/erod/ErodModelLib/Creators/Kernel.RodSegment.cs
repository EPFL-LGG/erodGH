using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class RodSegment
        {
            // Segments
            internal static NativeHandle ErodRodSegmentBuild(NativeHandle linkage, int index)
            {
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodRodSegmentBuild(linkage, index);
                    owner.Initialize(result, linkage);
                    try
                    {
                        NativeApi.Check("erodRodSegmentBuild");
                    }
                    catch
                    {
                        throw;
                    }
                    if (owner.IsInvalid) throw new NativeInteropException("erodRodSegmentBuild", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentBuild")]
            private static extern IntPtr Native_ErodRodSegmentBuild(NativeHandle linkage, int index);

            internal static int ErodRodSegmentGetEdgesCount(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetEdgesCount(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetEdgesCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetEdgesCount")]
            private static extern int Native_ErodRodSegmentGetEdgesCount(NativeHandle segment);

            internal static int ErodRodSegmentGetVerticesCount(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetVerticesCount(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetVerticesCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetVerticesCount")]
            private static extern int Native_ErodRodSegmentGetVerticesCount(NativeHandle segment);

            internal static int ErodRodSegmentGetRestAnglesCount(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetRestAnglesCount(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetRestAnglesCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetRestAnglesCount")]
            private static extern int Native_ErodRodSegmentGetRestAnglesCount(NativeHandle segment);

            internal static void ErodRodSegmentGetRestLengths(NativeHandle segment, out IntPtr restLengths, out int numEdges)
            {
                restLengths = default(IntPtr);
                numEdges = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetRestLengths(segment, out restLengths, out numEdges);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetRestLengths");
                        NativeBufferScope.Track(restLengths);
                    }
                    catch
                    {
                        NativeBufferScope.Free(restLengths); restLengths = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetRestLengths")]
            private static extern void Native_ErodRodSegmentGetRestLengths(NativeHandle segment, out IntPtr restLengths, out int numEdges);

            internal static void ErodRodSegmentGetRestKappas(NativeHandle segment, out IntPtr restAngles, out int numAngles)
            {
                restAngles = default(IntPtr);
                numAngles = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetRestKappas(segment, out restAngles, out numAngles);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetRestKappas");
                        NativeBufferScope.Track(restAngles);
                    }
                    catch
                    {
                        NativeBufferScope.Free(restAngles); restAngles = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetRestKappas")]
            private static extern void Native_ErodRodSegmentGetRestKappas(NativeHandle segment, out IntPtr restAngles, out int numAngles);

            internal static void ErodRodSegmentGetStretchingStresses(NativeHandle segment, out IntPtr  stresses, out int numEdges)
            {
                stresses = default(IntPtr);
                numEdges = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetStretchingStresses(segment, out stresses, out numEdges);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetStretchingStresses");
                        NativeBufferScope.Track(stresses);
                    }
                    catch
                    {
                        NativeBufferScope.Free(stresses); stresses = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetStretchingStresses")]
            private static extern void Native_ErodRodSegmentGetStretchingStresses(NativeHandle segment, out IntPtr  stresses, out int numEdges);

            internal static void ErodRodSegmentGetTwistingStresses(NativeHandle segment, out IntPtr stresses, out int numEdges)
            {
                stresses = default(IntPtr);
                numEdges = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetTwistingStresses(segment, out stresses, out numEdges);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetTwistingStresses");
                        NativeBufferScope.Track(stresses);
                    }
                    catch
                    {
                        NativeBufferScope.Free(stresses); stresses = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetTwistingStresses")]
            private static extern void Native_ErodRodSegmentGetTwistingStresses(NativeHandle segment, out IntPtr stresses, out int numEdges);

            internal static void ErodRodSegmentGetCenterLinePositions(NativeHandle segment, out IntPtr outCoords, out int numCoords)
            {
                outCoords = default(IntPtr);
                numCoords = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetCenterLinePositions(segment, out outCoords, out numCoords);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetCenterLinePositions");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetCenterLinePositions")]
            private static extern void Native_ErodRodSegmentGetCenterLinePositions(NativeHandle segment, out IntPtr outCoords, out int numCoords);

            internal static void ErodRodSegmentGetMaxBendingStresses(NativeHandle segment, out IntPtr stresses, out int numVertices)
            {
                stresses = default(IntPtr);
                numVertices = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetMaxBendingStresses(segment, out stresses, out numVertices);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetMaxBendingStresses");
                        NativeBufferScope.Track(stresses);
                    }
                    catch
                    {
                        NativeBufferScope.Free(stresses); stresses = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetMaxBendingStresses")]
            private static extern void Native_ErodRodSegmentGetMaxBendingStresses(NativeHandle segment, out IntPtr stresses, out int numVertices);

            internal static void ErodRodSegmentGetMinBendingStresses(NativeHandle segment, out IntPtr stresses, out int numVertices)
            {
                stresses = default(IntPtr);
                numVertices = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetMinBendingStresses(segment, out stresses, out numVertices);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetMinBendingStresses");
                        NativeBufferScope.Track(stresses);
                    }
                    catch
                    {
                        NativeBufferScope.Free(stresses); stresses = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetMinBendingStresses")]
            private static extern void Native_ErodRodSegmentGetMinBendingStresses(NativeHandle segment, out IntPtr stresses, out int numVertices);

            internal static void ErodRodSegmentGetSqrtBendingEnergies(NativeHandle segment, out IntPtr stresses, out int numVertices)
            {
                stresses = default(IntPtr);
                numVertices = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetSqrtBendingEnergies(segment, out stresses, out numVertices);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetSqrtBendingEnergies");
                        NativeBufferScope.Track(stresses);
                    }
                    catch
                    {
                        NativeBufferScope.Free(stresses); stresses = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetSqrtBendingEnergies")]
            private static extern void Native_ErodRodSegmentGetSqrtBendingEnergies(NativeHandle segment, out IntPtr stresses, out int numVertices);

            internal static void ErodRodSegmentGetMeshData(NativeHandle segment, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads)
            {
                outCoords = default(IntPtr);
                outQuads = default(IntPtr);
                numCoords = default(int);
                numQuads = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetMeshData(segment, out outCoords, out outQuads, out numCoords, out numQuads);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetMeshData");
                        NativeBufferScope.Track(outCoords);
                        NativeBufferScope.Track(outQuads);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        NativeBufferScope.Free(outQuads); outQuads = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetMeshData")]
            private static extern void Native_ErodRodSegmentGetMeshData(NativeHandle segment, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads);

            internal static int ErodRodSegmentGetStartJointIndex(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetStartJointIndex(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetStartJointIndex");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetStartJointIndex")]
            private static extern int Native_ErodRodSegmentGetStartJointIndex(NativeHandle segment);

            internal static int ErodRodSegmentGetEndJointIndex(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetEndJointIndex(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetEndJointIndex");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetEndJointIndex")]
            private static extern int Native_ErodRodSegmentGetEndJointIndex(NativeHandle segment);

            internal static void ErodRodSegmentGetBendingStiffness(NativeHandle segment, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2)
            {
                lambda1 = default(IntPtr);
                lambda2 = default(IntPtr);
                numLambda1 = default(int);
                numLambda2 = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetBendingStiffness(segment, out lambda1, out lambda2, out numLambda1, out numLambda2);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetBendingStiffness");
                        NativeBufferScope.Track(lambda1);
                        NativeBufferScope.Track(lambda2);
                    }
                    catch
                    {
                        NativeBufferScope.Free(lambda1); lambda1 = IntPtr.Zero;
                        NativeBufferScope.Free(lambda2); lambda2 = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetBendingStiffness")]
            private static extern void Native_ErodRodSegmentGetBendingStiffness(NativeHandle segment, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2);

            internal static void ErodRodSegmentGetTwistingStiffness(NativeHandle segment, out IntPtr stiffness, out int numVertices)
            {
                stiffness = default(IntPtr);
                numVertices = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetTwistingStiffness(segment, out stiffness, out numVertices);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetTwistingStiffness");
                        NativeBufferScope.Track(stiffness);
                    }
                    catch
                    {
                        NativeBufferScope.Free(stiffness); stiffness = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetTwistingStiffness")]
            private static extern void Native_ErodRodSegmentGetTwistingStiffness(NativeHandle segment, out IntPtr stiffness, out int numVertices);

            internal static void ErodRodSegmentGetStretchingStiffness(NativeHandle segment, out IntPtr stiffness, out int numEdges)
            {
                stiffness = default(IntPtr);
                numEdges = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetStretchingStiffness(segment, out stiffness, out numEdges);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetStretchingStiffness");
                        NativeBufferScope.Track(stiffness);
                    }
                    catch
                    {
                        NativeBufferScope.Free(stiffness); stiffness = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetStretchingStiffness")]
            private static extern void Native_ErodRodSegmentGetStretchingStiffness(NativeHandle segment, out IntPtr stiffness, out int numEdges);

            internal static double ErodRodSegmentGetEnergy(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetEnergy(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetEnergy");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetEnergy")]
            private static extern double Native_ErodRodSegmentGetEnergy(NativeHandle segment);

            internal static double ErodRodSegmentGetEnergyBend(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetEnergyBend(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetEnergyBend");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetEnergyBend")]
            private static extern double Native_ErodRodSegmentGetEnergyBend(NativeHandle segment);

            internal static double ErodRodSegmentGetEnergyStretch(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetEnergyStretch(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetEnergyStretch");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetEnergyStretch")]
            private static extern double Native_ErodRodSegmentGetEnergyStretch(NativeHandle segment);

            internal static double ErodRodSegmentGetEnergyTwist(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetEnergyTwist(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetEnergyTwist");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetEnergyTwist")]
            private static extern double Native_ErodRodSegmentGetEnergyTwist(NativeHandle segment);

            internal static void ErodRodSegmentGetMaterialFrame(NativeHandle segment, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2)
            {
                outCoordsCount = default(int);
                outCoordsD1 = default(IntPtr);
                outCoordsD2 = default(IntPtr);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetMaterialFrame(segment, out outCoordsCount, out outCoordsD1, out outCoordsD2);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetMaterialFrame");
                        NativeBufferScope.Track(outCoordsD1);
                        NativeBufferScope.Track(outCoordsD2);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoordsD1); outCoordsD1 = IntPtr.Zero;
                        NativeBufferScope.Free(outCoordsD2); outCoordsD2 = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetMaterialFrame")]
            private static extern void Native_ErodRodSegmentGetMaterialFrame(NativeHandle segment, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2);

            internal static void ErodRodSegmentGetRestPoints(NativeHandle segment, out IntPtr outData, out int numData)
            {
                outData = default(IntPtr);
                numData = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetRestPoints(segment, out outData, out numData);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetRestPoints");
                        NativeBufferScope.Track(outData);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outData); outData = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetRestPoints")]
            private static extern void Native_ErodRodSegmentGetRestPoints(NativeHandle segment, out IntPtr outData, out int numData);

            internal static void ErodRodSegmentGetRestDirectors(NativeHandle segment, out IntPtr outData, out int numData)
            {
                outData = default(IntPtr);
                numData = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetRestDirectors(segment, out outData, out numData);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetRestDirectors");
                        NativeBufferScope.Track(outData);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outData); outData = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetRestDirectors")]
            private static extern void Native_ErodRodSegmentGetRestDirectors(NativeHandle segment, out IntPtr outData, out int numData);

            internal static void ErodRodSegmentGetRestTwists(NativeHandle segment, out IntPtr outData, out int numData)
            {
                outData = default(IntPtr);
                numData = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetRestTwists(segment, out outData, out numData);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetRestTwists");
                        NativeBufferScope.Track(outData);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outData); outData = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetRestTwists")]
            private static extern void Native_ErodRodSegmentGetRestTwists(NativeHandle segment, out IntPtr outData, out int numData);

            internal static int ErodRodSegmentGetBendingEnergyType(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetBendingEnergyType(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetBendingEnergyType");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetBendingEnergyType")]
            private static extern int Native_ErodRodSegmentGetBendingEnergyType(NativeHandle segment);

            internal static void ErodRodSegmentGetDensities(NativeHandle segment, out IntPtr outData, out int numData)
            {
                outData = default(IntPtr);
                numData = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetDensities(segment, out outData, out numData);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetDensities");
                        NativeBufferScope.Track(outData);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outData); outData = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetDensities")]
            private static extern void Native_ErodRodSegmentGetDensities(NativeHandle segment, out IntPtr outData, out int numData);

            internal static double ErodRodSegmentGetInitialMinRestLength(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetInitialMinRestLength(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetInitialMinRestLength");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetInitialMinRestLength")]
            private static extern double Native_ErodRodSegmentGetInitialMinRestLength(NativeHandle segment);

            internal static void ErodRodSegmentGetEdgeMaterial(NativeHandle segment, int idx, out IntPtr outMatData, out IntPtr outCoords, out IntPtr outEdges, out int numMatData, out int numCoords, out int numEdges)
            {
                outMatData = default(IntPtr);
                outCoords = default(IntPtr);
                outEdges = default(IntPtr);
                numMatData = default(int);
                numCoords = default(int);
                numEdges = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetEdgeMaterial(segment, idx, out outMatData, out outCoords, out outEdges, out numMatData, out numCoords, out numEdges);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetEdgeMaterial");
                        NativeBufferScope.Track(outMatData);
                        NativeBufferScope.Track(outCoords);
                        NativeBufferScope.Track(outEdges);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outMatData); outMatData = IntPtr.Zero;
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        NativeBufferScope.Free(outEdges); outEdges = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetEdgeMaterial")]
            private static extern void Native_ErodRodSegmentGetEdgeMaterial(NativeHandle segment, int idx, out IntPtr outMatData, out IntPtr outCoords, out IntPtr outEdges, out int numMatData, out int numCoords, out int numEdges);

            internal static int ErodRodSegmentGetEdgeMaterialCount(NativeHandle segment)
            {
                using (NativeCall.Enter(segment))
                {
                    var result = Native_ErodRodSegmentGetEdgeMaterialCount(segment);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetEdgeMaterialCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetEdgeMaterialCount")]
            private static extern int Native_ErodRodSegmentGetEdgeMaterialCount(NativeHandle segment);

            internal static void ErodRodSegmentGetDeformedState(NativeHandle segment, out IntPtr outPtsCoords, out IntPtr outThetas, out IntPtr outTgtCoords, out IntPtr outDirCoords, out IntPtr outSrcThetas, out IntPtr outSrcTwist, out int outNumPtsCoords, out int outNumThetas, out int outNumTgtCoords, out int outNumDirCoords, out int outNumSrcThetas, out int outNumSrcTwist)
            {
                outPtsCoords = default(IntPtr);
                outThetas = default(IntPtr);
                outTgtCoords = default(IntPtr);
                outDirCoords = default(IntPtr);
                outSrcThetas = default(IntPtr);
                outSrcTwist = default(IntPtr);
                outNumPtsCoords = default(int);
                outNumThetas = default(int);
                outNumTgtCoords = default(int);
                outNumDirCoords = default(int);
                outNumSrcThetas = default(int);
                outNumSrcTwist = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetDeformedState(segment, out outPtsCoords, out outThetas, out outTgtCoords, out outDirCoords, out outSrcThetas, out outSrcTwist, out outNumPtsCoords, out outNumThetas, out outNumTgtCoords, out outNumDirCoords, out outNumSrcThetas, out outNumSrcTwist);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetDeformedState");
                        NativeBufferScope.Track(outPtsCoords);
                        NativeBufferScope.Track(outThetas);
                        NativeBufferScope.Track(outTgtCoords);
                        NativeBufferScope.Track(outDirCoords);
                        NativeBufferScope.Track(outSrcThetas);
                        NativeBufferScope.Track(outSrcTwist);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outPtsCoords); outPtsCoords = IntPtr.Zero;
                        NativeBufferScope.Free(outThetas); outThetas = IntPtr.Zero;
                        NativeBufferScope.Free(outTgtCoords); outTgtCoords = IntPtr.Zero;
                        NativeBufferScope.Free(outDirCoords); outDirCoords = IntPtr.Zero;
                        NativeBufferScope.Free(outSrcThetas); outSrcThetas = IntPtr.Zero;
                        NativeBufferScope.Free(outSrcTwist); outSrcTwist = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetDeformedState")]
            private static extern void Native_ErodRodSegmentGetDeformedState(NativeHandle segment, out IntPtr outPtsCoords, out IntPtr outThetas, out IntPtr outTgtCoords, out IntPtr outDirCoords, out IntPtr outSrcThetas, out IntPtr outSrcTwist, out int outNumPtsCoords, out int outNumThetas, out int outNumTgtCoords, out int outNumDirCoords, out int outNumSrcThetas, out int outNumSrcTwist);
            internal static void ErodRodSegmentGetVonMisesStresses(NativeHandle segment, out IntPtr outData, out int numData)
            {
                outData = default(IntPtr);
                numData = default(int);
                using (NativeCall.Enter(segment))
                {
                    Native_ErodRodSegmentGetVonMisesStresses(segment, out outData, out numData);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetVonMisesStresses");
                        NativeBufferScope.Track(outData);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outData); outData = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetVonMisesStresses")]
            private static extern void Native_ErodRodSegmentGetVonMisesStresses(NativeHandle segment, out IntPtr outData, out int numData);

            internal static double ErodRodSegmentGetMaxStrain(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodRodSegmentGetMaxStrain(rod);
                    try
                    {
                        NativeApi.Check("erodRodSegmentGetMaxStrain");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodRodSegmentGetMaxStrain")]
            private static extern double Native_ErodRodSegmentGetMaxStrain(NativeHandle rod);
        }
    }
}
