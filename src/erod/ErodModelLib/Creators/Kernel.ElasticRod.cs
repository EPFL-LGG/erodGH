using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class ElasticRod
        {
            internal static NativeHandle ErodElasticRodBuild(int numPoints, [In] double[] inCoords, out IntPtr errorMessage)
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
                    var result = Native_ErodElasticRodBuild(numPoints, inCoords, out errorMessage);
                    owner.Initialize(result, kind: 1);
                    try
                    {
                        NativeApi.Check("erodElasticRodBuild");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodElasticRodBuild", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodBuild")]
            private static extern IntPtr Native_ErodElasticRodBuild(int numPoints, [In] double[] inCoords, out IntPtr errorMessage);

            internal static NativeHandle ErodElasticRodCopy(NativeHandle rod, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodCopy(rod, out errorMessage);
                    owner.Initialize(result, kind: 1);
                    try
                    {
                        NativeApi.Check("erodElasticRodCopy");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodElasticRodCopy", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodCopy")]
            private static extern IntPtr Native_ErodElasticRodCopy(NativeHandle rod, out IntPtr errorMessage);

            internal static void ErodElasticRodGetMeshData(NativeHandle rod, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads)
            {
                outCoords = default(IntPtr);
                outQuads = default(IntPtr);
                numCoords = default(int);
                numQuads = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetMeshData(rod, out outCoords, out outQuads, out numCoords, out numQuads);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetMeshData");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetMeshData")]
            private static extern void Native_ErodElasticRodGetMeshData(NativeHandle rod, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads);

            internal static void ErodElasticRodSetMaterial(NativeHandle rod, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType)
            {
                if (numParams < 0) throw new ArgumentOutOfRangeException(nameof(numParams));
                NativeChecks.Array(sectionParam, NativeChecks.Product(numParams, 1), nameof(sectionParam));
                NativeChecks.Finite(sectionParam, nameof(sectionParam));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodSetMaterial(rod, sectionType, E, nu, sectionParam, numParams, axisType);
                    try
                    {
                        NativeApi.Check("erodElasticRodSetMaterial");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodSetMaterial")]
            private static extern void Native_ErodElasticRodSetMaterial(NativeHandle rod, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType);

            internal static int ErodElasticRodGetDoFCount(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetDoFCount(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetDoFCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetDoFCount")]
            private static extern int Native_ErodElasticRodGetDoFCount(NativeHandle rod);

            internal static void ErodElasticRodRemoveRestCurvatures(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodRemoveRestCurvatures(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodRemoveRestCurvatures");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodRemoveRestCurvatures")]
            private static extern void Native_ErodElasticRodRemoveRestCurvatures(NativeHandle rod);

            internal static int ErodElasticRodGetEdgesCount(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetEdgesCount(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetEdgesCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetEdgesCount")]
            private static extern int Native_ErodElasticRodGetEdgesCount(NativeHandle rod);

            internal static int ErodElasticRodGetVerticesCount(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetVerticesCount(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetVerticesCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetVerticesCount")]
            private static extern int Native_ErodElasticRodGetVerticesCount(NativeHandle rod);

            internal static void ErodElasticRodGetRestLengths(NativeHandle rod, [In, Out] double[] restLengths, int numEdges)
            {
                if (numEdges < 0) throw new ArgumentOutOfRangeException(nameof(numEdges));
                NativeChecks.Array(restLengths, NativeChecks.Product(numEdges, 1), nameof(restLengths));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetRestLengths(rod, restLengths, numEdges);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetRestLengths");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetRestLengths")]
            private static extern void Native_ErodElasticRodGetRestLengths(NativeHandle rod, [In, Out] double[] restLengths, int numEdges);

            internal static void ErodElasticRodGetStretchingStresses(NativeHandle rod, [In, Out] double[] stresses, int numEdges)
            {
                if (numEdges < 0) throw new ArgumentOutOfRangeException(nameof(numEdges));
                NativeChecks.Array(stresses, NativeChecks.Product(numEdges, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetStretchingStresses(rod, stresses, numEdges);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetStretchingStresses");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetStretchingStresses")]
            private static extern void Native_ErodElasticRodGetStretchingStresses(NativeHandle rod, [In, Out] double[] stresses, int numEdges);

            internal static void ErodElasticRodGetTwistingStresses(NativeHandle rod, [In, Out] double[] stresses, int numEdges)
            {
                if (numEdges < 0) throw new ArgumentOutOfRangeException(nameof(numEdges));
                NativeChecks.Array(stresses, NativeChecks.Product(numEdges, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetTwistingStresses(rod, stresses, numEdges);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetTwistingStresses");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetTwistingStresses")]
            private static extern void Native_ErodElasticRodGetTwistingStresses(NativeHandle rod, [In, Out] double[] stresses, int numEdges);

            internal static void ErodElasticRodGetCenterLinePositions(NativeHandle rod, [In, Out] double[] outCoords, int numCoords)
            {
                if (numCoords < 0) throw new ArgumentOutOfRangeException(nameof(numCoords));
                NativeChecks.Array(outCoords, NativeChecks.Product(numCoords, 3), nameof(outCoords));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetCenterLinePositions(rod, outCoords, numCoords);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetCenterLinePositions");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetCenterLinePositions")]
            private static extern void Native_ErodElasticRodGetCenterLinePositions(NativeHandle rod, [In, Out] double[] outCoords, int numCoords);

            internal static void ErodElasticRodGetMaxBendingStresses(NativeHandle rod, [In, Out] double[] stresses, int numVertices)
            {
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                NativeChecks.Array(stresses, NativeChecks.Product(numVertices, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetMaxBendingStresses(rod, stresses, numVertices);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetMaxBendingStresses");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetMaxBendingStresses")]
            private static extern void Native_ErodElasticRodGetMaxBendingStresses(NativeHandle rod, [In, Out] double[] stresses, int numVertices);

            internal static void ErodElasticRodGetMinBendingStresses(NativeHandle rod, [In, Out] double[] stresses, int numVertices)
            {
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                NativeChecks.Array(stresses, NativeChecks.Product(numVertices, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetMinBendingStresses(rod, stresses, numVertices);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetMinBendingStresses");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetMinBendingStresses")]
            private static extern void Native_ErodElasticRodGetMinBendingStresses(NativeHandle rod, [In, Out] double[] stresses, int numVertices);

            internal static void ErodElasticRodGetSqrtBendingEnergies(NativeHandle rod, [In, Out] double[] stresses, int numVertices)
            {
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                NativeChecks.Array(stresses, NativeChecks.Product(numVertices, 1), nameof(stresses));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetSqrtBendingEnergies(rod, stresses, numVertices);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetSqrtBendingEnergies");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetSqrtBendingEnergies")]
            private static extern void Native_ErodElasticRodGetSqrtBendingEnergies(NativeHandle rod, [In, Out] double[] stresses, int numVertices);

            internal static void ErodElasticRodGetDoFs(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetDoFs(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetDoFs");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetDoFs")]
            private static extern void Native_ErodElasticRodGetDoFs(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodElasticRodSetDoFs(NativeHandle rod, [In] double[] inDoFs, [In] int numDoFs)
            {
                if (numDoFs < 0) throw new ArgumentOutOfRangeException(nameof(numDoFs));
                NativeChecks.Array(inDoFs, NativeChecks.Product(numDoFs, 1), nameof(inDoFs));
                NativeChecks.Finite(inDoFs, nameof(inDoFs));
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodSetDoFs(rod, inDoFs, numDoFs);
                    try
                    {
                        NativeApi.Check("erodElasticRodSetDoFs");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodSetDoFs")]
            private static extern void Native_ErodElasticRodSetDoFs(NativeHandle rod, [In] double[] inDoFs, [In] int numDoFs);

            internal static void ErodElasticRodGetMaterialFrame(NativeHandle rod, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2)
            {
                outCoordsCount = default(int);
                outCoordsD1 = default(IntPtr);
                outCoordsD2 = default(IntPtr);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetMaterialFrame(rod, out outCoordsCount, out outCoordsD1, out outCoordsD2);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetMaterialFrame");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetMaterialFrame")]
            private static extern void Native_ErodElasticRodGetMaterialFrame(NativeHandle rod, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2);

            internal static double ErodElasticRodGetEnergy(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetEnergy(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetEnergy");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetEnergy")]
            private static extern double Native_ErodElasticRodGetEnergy(NativeHandle rod);

            internal static double ErodElasticRodGetEnergyBend(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetEnergyBend(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetEnergyBend");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetEnergyBend")]
            private static extern double Native_ErodElasticRodGetEnergyBend(NativeHandle rod);

            internal static double ErodElasticRodGetEnergyStretch(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetEnergyStretch(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetEnergyStretch");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetEnergyStretch")]
            private static extern double Native_ErodElasticRodGetEnergyStretch(NativeHandle rod);

            internal static double ErodElasticRodGetEnergyTwist(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetEnergyTwist(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetEnergyTwist");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetEnergyTwist")]
            private static extern double Native_ErodElasticRodGetEnergyTwist(NativeHandle rod);

            internal static double ErodElasticRodGetMaxStrain(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetMaxStrain(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetMaxStrain");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetMaxStrain")]
            private static extern double Native_ErodElasticRodGetMaxStrain(NativeHandle rod);

            internal static void ErodElasticRodGetBendingStiffness(NativeHandle rod, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2)
            {
                lambda1 = default(IntPtr);
                lambda2 = default(IntPtr);
                numLambda1 = default(int);
                numLambda2 = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetBendingStiffness(rod, out lambda1, out lambda2, out numLambda1, out numLambda2);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetBendingStiffness");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetBendingStiffness")]
            private static extern void Native_ErodElasticRodGetBendingStiffness(NativeHandle rod, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2);

            internal static void ErodElasticRodGetTwistingStiffness(NativeHandle rod, out IntPtr stiffness, out int numVertices)
            {
                stiffness = default(IntPtr);
                numVertices = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetTwistingStiffness(rod, out stiffness, out numVertices);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetTwistingStiffness");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetTwistingStiffness")]
            private static extern void Native_ErodElasticRodGetTwistingStiffness(NativeHandle rod, out IntPtr stiffness, out int numVertices);

            internal static void ErodElasticRodGetStretchingStiffness(NativeHandle rod, out IntPtr stiffness, out int numEdges)
            {
                stiffness = default(IntPtr);
                numEdges = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetStretchingStiffness(rod, out stiffness, out numEdges);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetStretchingStiffness");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetStretchingStiffness")]
            private static extern void Native_ErodElasticRodGetStretchingStiffness(NativeHandle rod, out IntPtr stiffness, out int numEdges);

            internal static void ErodElasticRodGetVonMisesStresses(NativeHandle rod, out IntPtr outData, out int numData)
            {
                outData = default(IntPtr);
                numData = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetVonMisesStresses(rod, out outData, out numData);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetVonMisesStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetVonMisesStresses")]
            private static extern void Native_ErodElasticRodGetVonMisesStresses(NativeHandle rod, out IntPtr outData, out int numData);

            internal static double ErodElasticRodGetInitialMinRestLength(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetInitialMinRestLength(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetInitialMinRestLength");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetInitialMinRestLength")]
            private static extern double Native_ErodElasticRodGetInitialMinRestLength(NativeHandle rod);

            internal static void ErodElasticRodGetRestKappas(NativeHandle rod, out IntPtr restAngles, out int numAngles)
            {
                restAngles = default(IntPtr);
                numAngles = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetRestKappas(rod, out restAngles, out numAngles);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetRestKappas");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetRestKappas")]
            private static extern void Native_ErodElasticRodGetRestKappas(NativeHandle rod, out IntPtr restAngles, out int numAngles);

            internal static void ErodElasticRodGetScalarFieldSqrtBendingEnergies(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetScalarFieldSqrtBendingEnergies(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetScalarFieldSqrtBendingEnergies");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetScalarFieldSqrtBendingEnergies")]
            private static extern void Native_ErodElasticRodGetScalarFieldSqrtBendingEnergies(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodElasticRodGetScalarFieldMaxBendingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetScalarFieldMaxBendingStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetScalarFieldMaxBendingStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetScalarFieldMaxBendingStresses")]
            private static extern void Native_ErodElasticRodGetScalarFieldMaxBendingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodElasticRodGetScalarFieldMinBendingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetScalarFieldMinBendingStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetScalarFieldMinBendingStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetScalarFieldMinBendingStresses")]
            private static extern void Native_ErodElasticRodGetScalarFieldMinBendingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodElasticRodGetScalarFieldTwistingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetScalarFieldTwistingStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetScalarFieldTwistingStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetScalarFieldTwistingStresses")]
            private static extern void Native_ErodElasticRodGetScalarFieldTwistingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodElasticRodGetScalarFieldStretchingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetScalarFieldStretchingStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetScalarFieldStretchingStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetScalarFieldStretchingStresses")]
            private static extern void Native_ErodElasticRodGetScalarFieldStretchingStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodElasticRodGetScalarFieldVonMisesStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(rod))
                {
                    Native_ErodElasticRodGetScalarFieldVonMisesStresses(rod, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetScalarFieldVonMisesStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetScalarFieldVonMisesStresses")]
            private static extern void Native_ErodElasticRodGetScalarFieldVonMisesStresses(NativeHandle rod, out IntPtr outDoFs, out int numDoFs);

            internal static int ErodElasticRodGetThetaOffset(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetThetaOffset(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetThetaOffset");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetThetaOffset")]
            private static extern int Native_ErodElasticRodGetThetaOffset(NativeHandle rod);

            internal static int ErodElasticRodGetRestLengthOffset(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetRestLengthOffset(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetRestLengthOffset");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetRestLengthOffset")]
            private static extern int Native_ErodElasticRodGetRestLengthOffset(NativeHandle rod);

            internal static int ErodElasticRodGetRestKappaOffset(NativeHandle rod)
            {
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodGetRestKappaOffset(rod);
                    try
                    {
                        NativeApi.Check("erodElasticRodGetRestKappaOffset");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodGetRestKappaOffset")]
            private static extern int Native_ErodElasticRodGetRestKappaOffset(NativeHandle rod);

        }
    }
}
