using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class PeriodicRod
        {
            internal static NativeHandle ErodPeriodicElasticRodBuild(int numPoints, [In] double[] inCoords, int removeCurvature, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                if (numPoints < 0) throw new ArgumentOutOfRangeException(nameof(numPoints));
                NativeChecks.Array(inCoords, NativeChecks.Product(numPoints, 3), nameof(inCoords));
                NativeChecks.Finite(inCoords, nameof(inCoords));
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter())
                {
                    var result = Native_ErodPeriodicElasticRodBuild(numPoints, inCoords, removeCurvature, out errorMessage);
                    owner.Initialize(result, kind: 2);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodBuild");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodPeriodicElasticRodBuild", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodBuild")]
            private static extern IntPtr Native_ErodPeriodicElasticRodBuild(int numPoints, [In] double[] inCoords, int removeCurvature, out IntPtr errorMessage);

            internal static NativeHandle ErodPeriodicElasticRodCopy(NativeHandle rod, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodCopy(rod, out errorMessage);
                    owner.Initialize(result, kind: 2);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodCopy");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodPeriodicElasticRodCopy", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodCopy")]
            private static extern IntPtr Native_ErodPeriodicElasticRodCopy(NativeHandle rod, out IntPtr errorMessage);

            internal static void ErodPeriodicElasticRodGetMeshData(NativeHandle rod, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads)
            {
                outCoords = default(IntPtr);
                outQuads = default(IntPtr);
                numCoords = default(int);
                numQuads = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetMeshData(rod, out outCoords, out outQuads, out numCoords, out numQuads);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetMeshData");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetMeshData")]
            private static extern void Native_ErodPeriodicElasticRodGetMeshData(NativeHandle rod, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads);

            internal static void ErodPeriodicElasticRodSetMaterial(NativeHandle rod, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType)
            {
                if (numParams < 0) throw new ArgumentOutOfRangeException(nameof(numParams));
                NativeChecks.Array(sectionParam, NativeChecks.Product(numParams, 1), nameof(sectionParam));
                NativeChecks.Finite(sectionParam, nameof(sectionParam));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodSetMaterial(rod, sectionType, E, nu, sectionParam, numParams, axisType);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodSetMaterial");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodSetMaterial")]
            private static extern void Native_ErodPeriodicElasticRodSetMaterial(NativeHandle rod, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType);

            internal static int ErodPeriodicElasticRodGetDoFCount(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetDoFCount(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetDoFCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetDoFCount")]
            private static extern int Native_ErodPeriodicElasticRodGetDoFCount(NativeHandle rod);

            internal static int ErodPeriodicElasticRodGetEdgesCount(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetEdgesCount(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetEdgesCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetEdgesCount")]
            private static extern int Native_ErodPeriodicElasticRodGetEdgesCount(NativeHandle rod);

            internal static int ErodPeriodicElasticRodGetVerticesCount(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetVerticesCount(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetVerticesCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetVerticesCount")]
            private static extern int Native_ErodPeriodicElasticRodGetVerticesCount(NativeHandle rod);

            internal static void ErodPeriodicElasticRodGetRestLengths(NativeHandle rod, [In, Out] double[] restLengths, int numEdges)
            {
                if (numEdges < 0) throw new ArgumentOutOfRangeException(nameof(numEdges));
                NativeChecks.Array(restLengths, NativeChecks.Product(numEdges, 1), nameof(restLengths));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetRestLengths(rod, restLengths, numEdges);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetRestLengths");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetRestLengths")]
            private static extern void Native_ErodPeriodicElasticRodGetRestLengths(NativeHandle rod, [In, Out] double[] restLengths, int numEdges);

            internal static void ErodPeriodicElasticRodGetStretchingStresses(NativeHandle rod, [In, Out] double[] stresses, int numEdges)
            {
                if (numEdges < 0) throw new ArgumentOutOfRangeException(nameof(numEdges));
                NativeChecks.Array(stresses, NativeChecks.Product(numEdges, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetStretchingStresses(rod, stresses, numEdges);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetStretchingStresses");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetStretchingStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetStretchingStresses(NativeHandle rod, [In, Out] double[] stresses, int numEdges);

            internal static void ErodPeriodicElasticRodGetTwistingStresses(NativeHandle rod, [In, Out] double[] stresses, int numEdges)
            {
                if (numEdges < 0) throw new ArgumentOutOfRangeException(nameof(numEdges));
                NativeChecks.Array(stresses, NativeChecks.Product(numEdges, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetTwistingStresses(rod, stresses, numEdges);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetTwistingStresses");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetTwistingStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetTwistingStresses(NativeHandle rod, [In, Out] double[] stresses, int numEdges);

            internal static void ErodPeriodicElasticRodGetCenterLinePositions(NativeHandle rod, [In, Out] double[] outCoords, int numCoords)
            {
                if (numCoords < 0) throw new ArgumentOutOfRangeException(nameof(numCoords));
                NativeChecks.Array(outCoords, NativeChecks.Product(numCoords, 3), nameof(outCoords));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetCenterLinePositions(rod, outCoords, numCoords);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetCenterLinePositions");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetCenterLinePositions")]
            private static extern void Native_ErodPeriodicElasticRodGetCenterLinePositions(NativeHandle rod, [In, Out] double[] outCoords, int numCoords);

            internal static void ErodPeriodicElasticRodGetMaxBendingStresses(NativeHandle rod, [In, Out] double[] stresses, int numVertices)
            {
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                NativeChecks.Array(stresses, NativeChecks.Product(numVertices, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetMaxBendingStresses(rod, stresses, numVertices);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetMaxBendingStresses");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetMaxBendingStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetMaxBendingStresses(NativeHandle rod, [In, Out] double[] stresses, int numVertices);

            internal static void ErodPeriodicElasticRodGetMinBendingStresses(NativeHandle rod, [In, Out] double[] stresses, int numVertices)
            {
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                NativeChecks.Array(stresses, NativeChecks.Product(numVertices, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetMinBendingStresses(rod, stresses, numVertices);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetMinBendingStresses");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetMinBendingStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetMinBendingStresses(NativeHandle rod, [In, Out] double[] stresses, int numVertices);

            internal static void ErodPeriodicElasticRodGetSqrtBendingEnergies(NativeHandle rod, [In, Out] double[] stresses, int numVertices)
            {
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                NativeChecks.Array(stresses, NativeChecks.Product(numVertices, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetSqrtBendingEnergies(rod, stresses, numVertices);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetSqrtBendingEnergies");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetSqrtBendingEnergies")]
            private static extern void Native_ErodPeriodicElasticRodGetSqrtBendingEnergies(NativeHandle rod, [In, Out] double[] stresses, int numVertices);

            internal static void ErodPeriodicElasticRodGetDoFs(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetDoFs(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetDoFs");
                        NativeBufferScope.Track(outDoFs);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outDoFs); outDoFs = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetDoFs")]
            private static extern void Native_ErodPeriodicElasticRodGetDoFs(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodPeriodicElasticRodSetDoFs(NativeHandle rod, [In] double[] inDoFs, [In] int numDoFs)
            {
                if (numDoFs < 0) throw new ArgumentOutOfRangeException(nameof(numDoFs));
                NativeChecks.Array(inDoFs, NativeChecks.Product(numDoFs, 1), nameof(inDoFs));
                NativeChecks.Finite(inDoFs, nameof(inDoFs));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodSetDoFs(rod, inDoFs, numDoFs);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodSetDoFs");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodSetDoFs")]
            private static extern void Native_ErodPeriodicElasticRodSetDoFs(NativeHandle rod, [In] double[] inDoFs, [In] int numDoFs);

            internal static void ErodPeriodicElasticRodGetMaterialFrame(NativeHandle rod, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2)
            {
                outCoordsCount = default(int);
                outCoordsD1 = default(IntPtr);
                outCoordsD2 = default(IntPtr);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetMaterialFrame(rod, out outCoordsCount, out outCoordsD1, out outCoordsD2);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetMaterialFrame");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetMaterialFrame")]
            private static extern void Native_ErodPeriodicElasticRodGetMaterialFrame(NativeHandle rod, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2);

            internal static double ErodPeriodicElasticRodGetEnergy(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetEnergy(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetEnergy");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetEnergy")]
            private static extern double Native_ErodPeriodicElasticRodGetEnergy(NativeHandle rod);

            internal static double ErodPeriodicElasticRodGetEnergyBend(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetEnergyBend(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetEnergyBend");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetEnergyBend")]
            private static extern double Native_ErodPeriodicElasticRodGetEnergyBend(NativeHandle rod);

            internal static double ErodPeriodicElasticRodGetEnergyStretch(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetEnergyStretch(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetEnergyStretch");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetEnergyStretch")]
            private static extern double Native_ErodPeriodicElasticRodGetEnergyStretch(NativeHandle rod);

            internal static double ErodPeriodicElasticRodGetEnergyTwist(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetEnergyTwist(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetEnergyTwist");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetEnergyTwist")]
            private static extern double Native_ErodPeriodicElasticRodGetEnergyTwist(NativeHandle rod);

            internal static double ErodPeriodicElasticRodGetMaxStrain(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetMaxStrain(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetMaxStrain");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetMaxStrain")]
            private static extern double Native_ErodPeriodicElasticRodGetMaxStrain(NativeHandle rod);

            internal static void ErodPeriodicElasticRodGetBendingStiffness(NativeHandle rod, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2)
            {
                lambda1 = default(IntPtr);
                lambda2 = default(IntPtr);
                numLambda1 = default(int);
                numLambda2 = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetBendingStiffness(rod, out lambda1, out lambda2, out numLambda1, out numLambda2);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetBendingStiffness");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetBendingStiffness")]
            private static extern void Native_ErodPeriodicElasticRodGetBendingStiffness(NativeHandle rod, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2);

            internal static void ErodPeriodicElasticRodGetTwistingStiffness(NativeHandle rod, out IntPtr stiffness, out int numVertices)
            {
                stiffness = default(IntPtr);
                numVertices = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetTwistingStiffness(rod, out stiffness, out numVertices);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetTwistingStiffness");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetTwistingStiffness")]
            private static extern void Native_ErodPeriodicElasticRodGetTwistingStiffness(NativeHandle rod, out IntPtr stiffness, out int numVertices);

            internal static void ErodPeriodicElasticRodGetStretchingStiffness(NativeHandle rod, out IntPtr stiffness, out int numEdges)
            {
                stiffness = default(IntPtr);
                numEdges = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetStretchingStiffness(rod, out stiffness, out numEdges);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetStretchingStiffness");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetStretchingStiffness")]
            private static extern void Native_ErodPeriodicElasticRodGetStretchingStiffness(NativeHandle rod, out IntPtr stiffness, out int numEdges);

            internal static void ErodPeriodicElasticRodGetVonMisesStresses(NativeHandle rod, out IntPtr outData, out int numData)
            {
                outData = default(IntPtr);
                numData = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetVonMisesStresses(rod, out outData, out numData);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetVonMisesStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetVonMisesStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetVonMisesStresses(NativeHandle rod, out IntPtr outData, out int numData);

            internal static double ErodPeriodicElasticRodGetInitialMinRestLength(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetInitialMinRestLength(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetInitialMinRestLength");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetInitialMinRestLength")]
            private static extern double Native_ErodPeriodicElasticRodGetInitialMinRestLength(NativeHandle rod);

            internal static void ErodPeriodicElasticRodGetRestKappas(NativeHandle rod, out IntPtr restAngles, out int numAngles)
            {
                restAngles = default(IntPtr);
                numAngles = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetRestKappas(rod, out restAngles, out numAngles);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetRestKappas");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetRestKappas")]
            private static extern void Native_ErodPeriodicElasticRodGetRestKappas(NativeHandle rod, out IntPtr restAngles, out int numAngles);

            internal static void ErodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies");
                        NativeBufferScope.Track(outDoFs);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outDoFs); outDoFs = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies")]
            private static extern void Native_ErodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodPeriodicElasticRodGetScalarFieldMaxBendingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetScalarFieldMaxBendingStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetScalarFieldMaxBendingStresses");
                        NativeBufferScope.Track(outDoFs);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outDoFs); outDoFs = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetScalarFieldMaxBendingStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetScalarFieldMaxBendingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodPeriodicElasticRodGetScalarFieldMinBendingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetScalarFieldMinBendingStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetScalarFieldMinBendingStresses");
                        NativeBufferScope.Track(outDoFs);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outDoFs); outDoFs = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetScalarFieldMinBendingStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetScalarFieldMinBendingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodPeriodicElasticRodGetScalarFieldTwistingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetScalarFieldTwistingStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetScalarFieldTwistingStresses");
                        NativeBufferScope.Track(outDoFs);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outDoFs); outDoFs = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetScalarFieldTwistingStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetScalarFieldTwistingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodPeriodicElasticRodGetScalarFieldStretchingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetScalarFieldStretchingStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetScalarFieldStretchingStresses");
                        NativeBufferScope.Track(outDoFs);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outDoFs); outDoFs = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetScalarFieldStretchingStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetScalarFieldStretchingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodPeriodicElasticRodGetScalarFieldVonMisesStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodPeriodicElasticRodGetScalarFieldVonMisesStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetScalarFieldVonMisesStresses");
                        NativeBufferScope.Track(outDoFs);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outDoFs); outDoFs = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetScalarFieldVonMisesStresses")]
            private static extern void Native_ErodPeriodicElasticRodGetScalarFieldVonMisesStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static int ErodPeriodicElasticRodGetThetaOffset(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetThetaOffset(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetThetaOffset");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetThetaOffset")]
            private static extern int Native_ErodPeriodicElasticRodGetThetaOffset(NativeHandle rod);

            internal static int ErodPeriodicElasticRodGetRestLengthOffset(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetRestLengthOffset(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetRestLengthOffset");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetRestLengthOffset")]
            private static extern int Native_ErodPeriodicElasticRodGetRestLengthOffset(NativeHandle rod);

            internal static int ErodPeriodicElasticRodGetRestKappaOffset(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodPeriodicElasticRodGetRestKappaOffset(rod);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodGetRestKappaOffset");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodGetRestKappaOffset")]
            private static extern int Native_ErodPeriodicElasticRodGetRestKappaOffset(NativeHandle rod);
        }
    }
}
