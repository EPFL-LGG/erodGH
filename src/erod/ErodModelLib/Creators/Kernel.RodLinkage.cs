using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        #if EROD_WINDOWS
        private const string erod_dylib = "erod.dll";
#else
        private const string erod_dylib = "liberod.dylib";
#endif
        public static void FreeNativeBuffer(IntPtr buffer) { NativeBufferScope.Free(buffer); }

        internal const string NativeLibrary = erod_dylib;

        public static class RodLinkage
        {
            // attracted target surface
            internal static NativeHandle ErodXShellAttractedSurfaceBuild(int numVertices, int numTias, [In] double[] inCoords, [In] int[] inTrias, NativeHandle linkage, double weight, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                if (numTias < 0) throw new ArgumentOutOfRangeException(nameof(numTias));
                NativeChecks.Array(inCoords, NativeChecks.Product(numVertices, 3), nameof(inCoords));
                NativeChecks.Array(inTrias, NativeChecks.Product(numTias, 3), nameof(inTrias));
                NativeChecks.Finite(inCoords, nameof(inCoords));
                NativeChecks.Indices(inTrias, numVertices, nameof(inTrias));
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellAttractedSurfaceBuild(numVertices, numTias, inCoords, inTrias, linkage, weight, out errorMessage);
                    owner.Initialize(result, kind: 4);
                    try
                    {
                        NativeApi.Check("erodXShellAttractedSurfaceBuild");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodXShellAttractedSurfaceBuild", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellAttractedSurfaceBuild")]
            private static extern IntPtr Native_ErodXShellAttractedSurfaceBuild(int numVertices, int numTias, [In] double[] inCoords, [In] int[] inTrias, NativeHandle linkage, double weight, out IntPtr errorMessage);

            public static NativeHandle ErodXShellAttractedSurfaceCopy(NativeHandle linkage, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellAttractedSurfaceCopy(linkage, out errorMessage);
                    owner.Initialize(result, kind: 4);
                    try
                    {
                        NativeApi.Check("erodXShellAttractedSurfaceCopy");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodXShellAttractedSurfaceCopy", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellAttractedSurfaceCopy")]
            private static extern IntPtr Native_ErodXShellAttractedSurfaceCopy(NativeHandle linkage, out IntPtr errorMessage);

            // Infer target surface
            internal static int ErodXShellInferTargetSurface(NativeHandle linkage, int nsubdiv, int numExtensionLayers, out IntPtr outCoords, out IntPtr outTrias, out int numCoords, out int numTrias, out IntPtr errorMessage)
            {
                outCoords = default(IntPtr);
                outTrias = default(IntPtr);
                numCoords = default(int);
                numTrias = default(int);
                errorMessage = default(IntPtr);
                if (nsubdiv < 0) throw new ArgumentOutOfRangeException(nameof(nsubdiv));
                if (numExtensionLayers < 0) throw new ArgumentOutOfRangeException(nameof(numExtensionLayers));
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellInferTargetSurface(linkage, nsubdiv, numExtensionLayers, out outCoords, out outTrias, out numCoords, out numTrias, out errorMessage);
                    try
                    {
                        NativeApi.Check("erodXShellInferTargetSurface");
                        NativeBufferScope.Track(outCoords);
                        NativeBufferScope.Track(outTrias);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outCoords); outCoords = IntPtr.Zero;
                        NativeBufferScope.Free(outTrias); outTrias = IntPtr.Zero;
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellInferTargetSurface")]
            private static extern int Native_ErodXShellInferTargetSurface(NativeHandle linkage, int nsubdiv, int numExtensionLayers, out IntPtr outCoords, out IntPtr outTrias, out int numCoords, out int numTrias, out IntPtr errorMessage);

            // Linkage
            public static NativeHandle ErodXShellBuildFromGraph(int numVertices, int numEdges, [In] double[] inCoords, [In] int[] inEdges, [In] double[] inNormals, int subdivision, int interleavingType, int initConsistentAngle, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                if (numEdges < 0) throw new ArgumentOutOfRangeException(nameof(numEdges));
                if (subdivision < 0) throw new ArgumentOutOfRangeException(nameof(subdivision));
                NativeChecks.Array(inCoords, NativeChecks.Product(numVertices, 3), nameof(inCoords));
                NativeChecks.Array(inEdges, NativeChecks.Product(numEdges, 2), nameof(inEdges));
                NativeChecks.Array(inNormals, NativeChecks.Product(numVertices, 3), nameof(inNormals));
                NativeChecks.Finite(inCoords, nameof(inCoords));
                NativeChecks.Finite(inNormals, nameof(inNormals));
                NativeChecks.Indices(inEdges, numVertices, nameof(inEdges));
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter())
                {
                    var result = Native_ErodXShellBuildFromGraph(numVertices, numEdges, inCoords, inEdges, inNormals, subdivision, interleavingType, initConsistentAngle, out errorMessage);
                    owner.Initialize(result, kind: 3);
                    try
                    {
                        NativeApi.Check("erodXShellBuildFromGraph");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodXShellBuildFromGraph", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellBuildFromGraph")]
            private static extern IntPtr Native_ErodXShellBuildFromGraph(int numVertices, int numEdges, [In] double[] inCoords, [In] int[] inEdges, [In] double[] inNormals, int subdivision, int interleavingType, int initConsistentAngle, out IntPtr errorMessage);

            public static NativeHandle ErodXShellBuild(int numVertices, int numEdges, [In] double[] inCoords, [In] int[] inEdges, [In] double[] inNormals, [In] double[] inRestLengths, [In] int[] inOffsetInteriorCoords, [In] double[] inInteriorCoords, int interleavingType, int initConsistentAngle, int initConsistentNormals, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                if (numEdges < 0) throw new ArgumentOutOfRangeException(nameof(numEdges));
                NativeChecks.Array(inCoords, NativeChecks.Product(numVertices, 3), nameof(inCoords));
                NativeChecks.Array(inEdges, NativeChecks.Product(numEdges, 2), nameof(inEdges));
                NativeChecks.Array(inNormals, NativeChecks.Product(numVertices, 3), nameof(inNormals));
                NativeChecks.Array(inRestLengths, 0, nameof(inRestLengths));
                NativeChecks.Array(inOffsetInteriorCoords, numEdges, nameof(inOffsetInteriorCoords));
                NativeChecks.Array(inInteriorCoords, 0, nameof(inInteriorCoords));
                NativeChecks.Finite(inCoords, nameof(inCoords));
                NativeChecks.Finite(inNormals, nameof(inNormals));
                NativeChecks.Finite(inRestLengths, nameof(inRestLengths));
                NativeChecks.Finite(inInteriorCoords, nameof(inInteriorCoords));
                NativeChecks.Indices(inEdges, numVertices, nameof(inEdges));
                NativeChecks.Array(inInteriorCoords, numEdges == 0 ? 0 : inOffsetInteriorCoords[numEdges-1], nameof(inInteriorCoords));
                for (int i = 0, previous = 0; i < numEdges; previous = inOffsetInteriorCoords[i++]) if(inOffsetInteriorCoords[i]<previous || (inOffsetInteriorCoords[i]-previous)%3!=0) throw new ArgumentException("Invalid interior-coordinate offsets");
                NativeChecks.Array(inRestLengths, numEdges, nameof(inRestLengths));
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter())
                {
                    var result = Native_ErodXShellBuild(numVertices, numEdges, inCoords, inEdges, inNormals, inRestLengths, inOffsetInteriorCoords, inInteriorCoords, interleavingType, initConsistentAngle, initConsistentNormals, out errorMessage);
                    owner.Initialize(result, kind: 3);
                    try
                    {
                        NativeApi.Check("erodXShellBuild");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodXShellBuild", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellBuild")]
            private static extern IntPtr Native_ErodXShellBuild(int numVertices, int numEdges, [In] double[] inCoords, [In] int[] inEdges, [In] double[] inNormals, [In] double[] inRestLengths, [In] int[] inOffsetInteriorCoords, [In] double[] inInteriorCoords, int interleavingType, int initConsistentAngle, int initConsistentNormals, out IntPtr errorMessage);

            public static NativeHandle ErodXShellCopy(NativeHandle linkage, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                var owner = new NativeHandle();
                try
                {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellCopy(linkage, out errorMessage);
                    owner.Initialize(result, kind: 3);
                    try
                    {
                        NativeApi.Check("erodXShellCopy");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    if (owner.IsInvalid) throw new NativeInteropException("erodXShellCopy", "Native factory returned no object");
                    return owner;
                }
                }
                catch { owner.Dispose(); throw; }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellCopy")]
            private static extern IntPtr Native_ErodXShellCopy(NativeHandle linkage, out IntPtr errorMessage);

            internal static void ErodXShellSetCustomMaterial(NativeHandle linkage, double E, double nu, [In] double[] inCoords, int numVertices, [In] double[] inHolesCoords, int numHoles, int axisType)
            {
                if (numVertices < 0) throw new ArgumentOutOfRangeException(nameof(numVertices));
                if (numHoles < 0) throw new ArgumentOutOfRangeException(nameof(numHoles));
                NativeChecks.Array(inCoords, NativeChecks.Product(numVertices, 3), nameof(inCoords));
                NativeChecks.Array(inHolesCoords, NativeChecks.Product(numHoles, 2), nameof(inHolesCoords));
                NativeChecks.Finite(inCoords, nameof(inCoords));
                NativeChecks.Finite(inHolesCoords, nameof(inHolesCoords));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetCustomMaterial(linkage, E, nu, inCoords, numVertices, inHolesCoords, numHoles, axisType);
                    try
                    {
                        NativeApi.Check("erodXShellSetCustomMaterial");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetCustomMaterial")]
            private static extern void Native_ErodXShellSetCustomMaterial(NativeHandle linkage, double E, double nu, [In] double[] inCoords, int numVertices, [In] double[] inHolesCoords, int numHoles, int axisType);

            internal static void ErodXShellSetMaterial(NativeHandle linkage, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType)
            {
                if (numParams < 0) throw new ArgumentOutOfRangeException(nameof(numParams));
                NativeChecks.Array(sectionParam, NativeChecks.Product(numParams, 1), nameof(sectionParam));
                NativeChecks.Finite(sectionParam, nameof(sectionParam));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetMaterial(linkage, sectionType, E, nu, sectionParam, numParams, axisType);
                    try
                    {
                        NativeApi.Check("erodXShellSetMaterial");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetMaterial")]
            private static extern void Native_ErodXShellSetMaterial(NativeHandle linkage, int sectionType, double E, double nu, double[] sectionParam, int numParams, int axisType);

            internal static void ErodXShellSetJointMaterial(NativeHandle linkage, int numMaterials, [In] int[] sectionType, [In] double[] E, [In] double[] nu, [In] double[] sectionParams, [In] int[] sectionParamsCount, [In] int[] axisType)
            {
                if (numMaterials < 0) throw new ArgumentOutOfRangeException(nameof(numMaterials));
                NativeChecks.Array(sectionType, NativeChecks.Product(numMaterials, 1), nameof(sectionType));
                NativeChecks.Array(E, NativeChecks.Product(numMaterials, 1), nameof(E));
                NativeChecks.Array(nu, NativeChecks.Product(numMaterials, 1), nameof(nu));
                NativeChecks.Array(sectionParams, 0, nameof(sectionParams));
                NativeChecks.Array(sectionParamsCount, NativeChecks.Product(numMaterials, 1), nameof(sectionParamsCount));
                NativeChecks.Array(axisType, NativeChecks.Product(numMaterials, 1), nameof(axisType));
                NativeChecks.Array(sectionParams, NativeChecks.Sum(sectionParamsCount), nameof(sectionParams));
                NativeChecks.Finite(E, nameof(E));
                NativeChecks.Finite(nu, nameof(nu));
                NativeChecks.Finite(sectionParams, nameof(sectionParams));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetJointMaterial(linkage, numMaterials, sectionType, E, nu, sectionParams, sectionParamsCount, axisType);
                    try
                    {
                        NativeApi.Check("erodXShellSetJointMaterial");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetJointMaterial")]
            private static extern void Native_ErodXShellSetJointMaterial(NativeHandle linkage, int numMaterials, [In] int[] sectionType, [In] double[] E, [In] double[] nu, [In] double[] sectionParams, [In] int[] sectionParamsCount, [In] int[] axisType);

            internal static void ErodXShellSetCustomJointMaterial(NativeHandle linkage, int numMaterials, [In] int[] sectionType, [In] double[] E, [In] double[] nu, [In] double[] inCoords, [In] int[] inCoordsCount, [In] double[] inHolesCoords, [In] int[] inHolesCount, [In] int[] axisType)
            {
                if (numMaterials < 0) throw new ArgumentOutOfRangeException(nameof(numMaterials));
                NativeChecks.Array(sectionType, NativeChecks.Product(numMaterials, 1), nameof(sectionType));
                NativeChecks.Array(E, NativeChecks.Product(numMaterials, 1), nameof(E));
                NativeChecks.Array(nu, NativeChecks.Product(numMaterials, 1), nameof(nu));
                NativeChecks.Array(inCoords, 0, nameof(inCoords));
                NativeChecks.Array(inCoordsCount, NativeChecks.Product(numMaterials, 1), nameof(inCoordsCount));
                NativeChecks.Array(inHolesCoords, 0, nameof(inHolesCoords));
                NativeChecks.Array(inHolesCount, NativeChecks.Product(numMaterials, 1), nameof(inHolesCount));
                NativeChecks.Array(axisType, NativeChecks.Product(numMaterials, 1), nameof(axisType));
                NativeChecks.Array(inCoords, NativeChecks.Product(NativeChecks.Sum(inCoordsCount), 3), nameof(inCoords));
                NativeChecks.Array(inHolesCoords, NativeChecks.Product(NativeChecks.Sum(inHolesCount), 2), nameof(inHolesCoords));
                NativeChecks.Finite(E, nameof(E));
                NativeChecks.Finite(nu, nameof(nu));
                NativeChecks.Finite(inCoords, nameof(inCoords));
                NativeChecks.Finite(inHolesCoords, nameof(inHolesCoords));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetCustomJointMaterial(linkage, numMaterials, sectionType, E, nu, inCoords, inCoordsCount, inHolesCoords, inHolesCount, axisType);
                    try
                    {
                        NativeApi.Check("erodXShellSetCustomJointMaterial");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetCustomJointMaterial")]
            private static extern void Native_ErodXShellSetCustomJointMaterial(NativeHandle linkage, int numMaterials, [In] int[] sectionType, [In] double[] E, [In] double[] nu, [In] double[] inCoords, [In] int[] inCoordsCount, [In] double[] inHolesCoords, [In] int[] inHolesCount, [In] int[] axisType);

            internal static void ErodXShellSetDesignParameterConfig(NativeHandle linkage, int use_restLen, int use_restKappa, int update_designParams_cache)
            {
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetDesignParameterConfig(linkage, use_restLen, use_restKappa, update_designParams_cache);
                    try
                    {
                        NativeApi.Check("erodXShellSetDesignParameterConfig");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetDesignParameterConfig")]
            private static extern void Native_ErodXShellSetDesignParameterConfig(NativeHandle linkage, int use_restLen, int use_restKappa, int update_designParams_cache);

            internal static int ErodXShellGetCentralJointIndex(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetCentralJointIndex(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetCentralJointIndex");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetCentralJointIndex")]
            private static extern int Native_ErodXShellGetCentralJointIndex(NativeHandle linkage);

            internal static void ErodXShellGetDofOffsetForJoint(NativeHandle linkage, int joint, [In] int[] DOF, int numDOF, [In, Out] int[] outVars)
            {
                if (numDOF < 0) throw new ArgumentOutOfRangeException(nameof(numDOF));
                NativeChecks.Array(DOF, NativeChecks.Product(numDOF, 1), nameof(DOF));
                NativeChecks.Array(outVars, NativeChecks.Product(numDOF, 1), nameof(outVars));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetDofOffsetForJoint(linkage, joint, DOF, numDOF, outVars);
                    try
                    {
                        NativeApi.Check("erodXShellGetDofOffsetForJoint");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetDofOffsetForJoint")]
            private static extern void Native_ErodXShellGetDofOffsetForJoint(NativeHandle linkage, int joint, [In] int[] DOF, int numDOF, [In, Out] int[] outVars);

            internal static void ErodXShellGetDofOffsetForCenterLinePos(NativeHandle linkage, int joint, [In] int[] DOF, int numDOF, [In, Out] int[] outVars)
            {
                if (numDOF < 0) throw new ArgumentOutOfRangeException(nameof(numDOF));
                NativeChecks.Array(DOF, NativeChecks.Product(numDOF, 1), nameof(DOF));
                NativeChecks.Array(outVars, NativeChecks.Product(numDOF, 1), nameof(outVars));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetDofOffsetForCenterLinePos(linkage, joint, DOF, numDOF, outVars);
                    try
                    {
                        NativeApi.Check("erodXShellGetDofOffsetForCenterLinePos");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetDofOffsetForCenterLinePos")]
            private static extern void Native_ErodXShellGetDofOffsetForCenterLinePos(NativeHandle linkage, int joint, [In] int[] DOF, int numDOF, [In, Out] int[] outVars);

            internal static void ErodXShellGetCenterLinePositions(NativeHandle linkage, out IntPtr outCoords, out int numCoords)
            {
                outCoords = default(IntPtr);
                numCoords = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetCenterLinePositions(linkage, out outCoords, out numCoords);
                    try
                    {
                        NativeApi.Check("erodXShellGetCenterLinePositions");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetCenterLinePositions")]
            private static extern void Native_ErodXShellGetCenterLinePositions(NativeHandle linkage, out IntPtr outCoords, out int numCoords);

            internal static void ErodXShellGetDoFs(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetDoFs(linkage, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellGetDoFs");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetDoFs")]
            private static extern void Native_ErodXShellGetDoFs(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodXShellSetDoFs(NativeHandle linkage, [In] double[] inDoFs, [In] int numDoFs)
            {
                if (numDoFs < 0) throw new ArgumentOutOfRangeException(nameof(numDoFs));
                NativeChecks.Array(inDoFs, NativeChecks.Product(numDoFs, 1), nameof(inDoFs));
                NativeChecks.Finite(inDoFs, nameof(inDoFs));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetDoFs(linkage, inDoFs, numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellSetDoFs");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetDoFs")]
            private static extern void Native_ErodXShellSetDoFs(NativeHandle linkage, [In] double[] inDoFs, [In] int numDoFs);

            internal static double ErodXShellGetAverageJointAngle(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetAverageJointAngle(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetAverageJointAngle");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetAverageJointAngle")]
            private static extern double Native_ErodXShellGetAverageJointAngle(NativeHandle linkage);

            internal static int ErodXShellGetCenterLinePositionsCount(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetCenterLinePositionsCount(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetCenterLinePositionsCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetCenterLinePositionsCount")]
            private static extern int Native_ErodXShellGetCenterLinePositionsCount(NativeHandle linkage);

            internal static int ErodXShellGetJointsCount(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetJointsCount(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetJointsCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetJointsCount")]
            private static extern int Native_ErodXShellGetJointsCount(NativeHandle linkage);

            internal static int ErodXShellGetDoFCount(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetDoFCount(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetDoFCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetDoFCount")]
            private static extern int Native_ErodXShellGetDoFCount(NativeHandle linkage);

            internal static int ErodXShellRodGetSegmentsCount(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellRodGetSegmentsCount(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetRodSegmentsCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetRodSegmentsCount")]
            private static extern int Native_ErodXShellRodGetSegmentsCount(NativeHandle linkage);

            internal static void ErodXShellGetMeshData(NativeHandle linkage, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads)
            {
                outCoords = default(IntPtr);
                outQuads = default(IntPtr);
                numCoords = default(int);
                numQuads = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetMeshData(linkage, out outCoords, out outQuads, out numCoords, out numQuads);
                    try
                    {
                        NativeApi.Check("erodXShellGetMeshData");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetMeshData")]
            private static extern void Native_ErodXShellGetMeshData(NativeHandle linkage, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads);

            internal static void ErodXShellGetRodSegmentIndexesPerRod(NativeHandle linkage, int index, out IntPtr segmentIndexes, out int numSeg, out int rodTypeA)
            {
                segmentIndexes = default(IntPtr);
                numSeg = default(int);
                rodTypeA = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetRodSegmentIndexesPerRod(linkage, index, out segmentIndexes, out numSeg, out rodTypeA);
                    try
                    {
                        NativeApi.Check("erodXShellGetRodSegmentIndexesPerRod");
                        NativeBufferScope.Track(segmentIndexes);
                    }
                    catch
                    {
                        NativeBufferScope.Free(segmentIndexes); segmentIndexes = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetRodSegmentIndexesPerRod")]
            private static extern void Native_ErodXShellGetRodSegmentIndexesPerRod(NativeHandle linkage, int index, out IntPtr segmentIndexes, out int numSeg, out int rodTypeA);

            internal static int ErodXShellGetRodTraceCount(NativeHandle linkage, out IntPtr errorMessage)
            {
                errorMessage = default(IntPtr);
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetRodTraceCount(linkage, out errorMessage);
                    try
                    {
                        NativeApi.Check("erodXShellGetRodTraceCount");
                    }
                    catch
                    {
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetRodTraceCount")]
            private static extern int Native_ErodXShellGetRodTraceCount(NativeHandle linkage, out IntPtr errorMessage);

            internal static void ErodXShellRemoveRestCurvatures(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellRemoveRestCurvatures(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellRemoveRestCurvatures");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellRemoveRestCurvatures")]
            private static extern void Native_ErodXShellRemoveRestCurvatures(NativeHandle linkage);

            internal static int ErodXShellGetHessianNNZ(NativeHandle linkage, int variableDesignParameters)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetHessianNNZ(linkage, variableDesignParameters);
                    try
                    {
                        NativeApi.Check("erodXShellHessianNNZ");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellHessianNNZ")]
            private static extern int Native_ErodXShellGetHessianNNZ(NativeHandle linkage, int variableDesignParameters);

            internal static int ErodXShellGetRestKappaVarsCount(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetRestKappaVarsCount(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetRestKappaVarsCount");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetRestKappaVarsCount")]
            private static extern int Native_ErodXShellGetRestKappaVarsCount(NativeHandle linkage);

            internal static void ErodXShellGetRestKappaVars(NativeHandle linkage, out IntPtr restAngles, out int numRestAngles)
            {
                restAngles = default(IntPtr);
                numRestAngles = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetRestKappaVars(linkage, out restAngles, out numRestAngles);
                    try
                    {
                        NativeApi.Check("erodXShellGetRestKappaVars");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetRestKappaVars")]
            private static extern void Native_ErodXShellGetRestKappaVars(NativeHandle linkage, out IntPtr restAngles, out int numRestAngles);

            internal static double ErodXShellGetMinJointAngle(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetMinJointAngle(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetMinJointAngle");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetMinJointAngle")]
            private static extern double Native_ErodXShellGetMinJointAngle(NativeHandle linkage);


            internal static double ErodXShellGetTotalRestLength(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetTotalRestLength(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetTotalRestLength");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetTotalRestLength")]
            private static extern double Native_ErodXShellGetTotalRestLength(NativeHandle linkage);

            internal static void ErodXShellSetStiffenRegions(NativeHandle linkage, double factor, [In, Out] double[] coords, int numBoxes)
            {
                if (numBoxes < 0) throw new ArgumentOutOfRangeException(nameof(numBoxes));
                NativeChecks.Array(coords, NativeChecks.Product(numBoxes, 24), nameof(coords));
                NativeChecks.Finite(coords, nameof(coords));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetStiffenRegions(linkage, factor, coords, numBoxes);
                    try
                    {
                        NativeApi.Check("erodXShellSetStiffenRegions");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetStiffenRegions")]
            private static extern void Native_ErodXShellSetStiffenRegions(NativeHandle linkage, double factor, [In, Out] double[] coords, int numBoxes);

            internal static double ErodXShellStripAutoDiff([In] double[] edgeA, [In] double[] edgeB)
            {
                NativeChecks.Array(edgeA, 3, nameof(edgeA));
                NativeChecks.Array(edgeB, 3, nameof(edgeB));
                NativeChecks.Finite(edgeA, nameof(edgeA));
                NativeChecks.Finite(edgeB, nameof(edgeB));
                using (NativeCall.Enter())
                {
                    var result = Native_ErodXShellStripAutoDiff(edgeA, edgeB);
                    try
                    {
                        NativeApi.Check("erodXShellStripAutoDiff");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellStripAutoDiff")]
            private static extern double Native_ErodXShellStripAutoDiff([In] double[] edgeA, [In] double[] edgeB);

            internal static void ErodXShellGetDesignParams(NativeHandle linkage, out IntPtr outDesignParams, out int outNumDesignParameters)
            {
                outDesignParams = default(IntPtr);
                outNumDesignParameters = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetDesignParams(linkage, out outDesignParams, out outNumDesignParameters);
                    try
                    {
                        NativeApi.Check("erodXShellGetDesignParams");
                        NativeBufferScope.Track(outDesignParams);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outDesignParams); outDesignParams = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetDesignParams")]
            private static extern void Native_ErodXShellGetDesignParams(NativeHandle linkage, out IntPtr outDesignParams, out int outNumDesignParameters);

            internal static int ErodXShellGetDesignParametersNumber(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetDesignParametersNumber(linkage);
                    try
                    {
                        NativeApi.Check("erodXShelNumberDesignParameters");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShelNumberDesignParameters")]
            private static extern int Native_ErodXShellGetDesignParametersNumber(NativeHandle linkage);

            internal static void ErodXShellScalarFieldSqrtBendingEnergies(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellScalarFieldSqrtBendingEnergies(linkage, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellScalarFieldSqrtBendingEnergies");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellScalarFieldSqrtBendingEnergies")]
            private static extern void Native_ErodXShellScalarFieldSqrtBendingEnergies(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodXShellScalarFieldMaxBendingStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellScalarFieldMaxBendingStresses(linkage, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellScalarFieldMaxBendingStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellScalarFieldMaxBendingStresses")]
            private static extern void Native_ErodXShellScalarFieldMaxBendingStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodXShellScalarFieldMinBendingStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellScalarFieldMinBendingStresses(linkage, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellScalarFieldMinBendingStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellScalarFieldMinBendingStresses")]
            private static extern void Native_ErodXShellScalarFieldMinBendingStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodXShellScalarFieldTwistingStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellScalarFieldTwistingStresses(linkage, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellScalarFieldTwistingStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellScalarFieldTwistingStresses")]
            private static extern void Native_ErodXShellScalarFieldTwistingStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodXShellScalarFieldStretchingStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellScalarFieldStretchingStresses(linkage, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellScalarFieldStretchingStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellScalarFieldStretchingStresses")]
            private static extern void Native_ErodXShellScalarFieldStretchingStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodXShellScalarFieldVonMisesStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellScalarFieldVonMisesStresses(linkage, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellScalarFieldVonMisesStresses");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellScalarFieldVonMisesStresses")]
            private static extern void Native_ErodXShellScalarFieldVonMisesStresses(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs);

            internal static double ErodXShellGetInitialMinRestLength(NativeHandle linkage)
            {
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetInitialMinRestLength(linkage);
                    try
                    {
                        NativeApi.Check("erodXShellGetInitialMinRestLength");
                    }
                    catch
                    {
                        throw;
                    }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetInitialMinRestLength")]
            private static extern double Native_ErodXShellGetInitialMinRestLength(NativeHandle linkage);

            internal static int ErodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(NativeHandle linkage, out IntPtr outAx, out IntPtr outAi, out IntPtr outAp, out long outM, out long outN, out long outNZ, out IntPtr errorMessage)
            {
                outAx = default(IntPtr);
                outAi = default(IntPtr);
                outAp = default(IntPtr);
                outM = default(long);
                outN = default(long);
                outNZ = default(long);
                errorMessage = default(IntPtr);
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(linkage, out outAx, out outAi, out outAp, out outM, out outN, out outNZ, out errorMessage);
                    try
                    {
                        NativeApi.Check("erodXShellGetSegmentRestLenToEdgeRestLenMapTranspose");
                        NativeBufferScope.Track(outAx);
                        NativeBufferScope.Track(outAi);
                        NativeBufferScope.Track(outAp);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outAx); outAx = IntPtr.Zero;
                        NativeBufferScope.Free(outAi); outAi = IntPtr.Zero;
                        NativeBufferScope.Free(outAp); outAp = IntPtr.Zero;
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetSegmentRestLenToEdgeRestLenMapTranspose")]
            private static extern int Native_ErodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(NativeHandle linkage, out IntPtr outAx, out IntPtr outAi, out IntPtr outAp, out long outM, out long outN, out long outNZ, out IntPtr errorMessage);

            internal static void ErodXShellGetDesignParameterConfig(NativeHandle linkage, out int restLen, out int restKappa)
            {
                restLen = default(int);
                restKappa = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetDesignParameterConfig(linkage, out restLen, out restKappa);
                    try
                    {
                        NativeApi.Check("erodXShellGetDesignParameterConfig");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetDesignParameterConfig")]
            private static extern void Native_ErodXShellGetDesignParameterConfig(NativeHandle linkage, out int restLen, out int restKappa);

            internal static void ErodXShellGetRestLengthsSolveDoFs(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs)
            {
                outDoFs = default(IntPtr);
                numDoFs = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetRestLengthsSolveDoFs(linkage, out outDoFs, out numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellGetRestLengthsSolveDoFs");
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
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetRestLengthsSolveDoFs")]
            private static extern void Native_ErodXShellGetRestLengthsSolveDoFs(NativeHandle linkage, out IntPtr outDoFs, out int numDoFs);

            internal static void ErodXShellSetRestLengthsSolveDoFs(NativeHandle linkage, double[] outDoFs, int numDoFs)
            {
                if (numDoFs < 0) throw new ArgumentOutOfRangeException(nameof(numDoFs));
                NativeChecks.Array(outDoFs, NativeChecks.Product(numDoFs, 1), nameof(outDoFs));
                NativeChecks.Finite(outDoFs, nameof(outDoFs));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetRestLengthsSolveDoFs(linkage, outDoFs, numDoFs);
                    try
                    {
                        NativeApi.Check("erodXShellSetRestLengthsSolveDoFs");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetRestLengthsSolveDoFs")]
            private static extern void Native_ErodXShellSetRestLengthsSolveDoFs(NativeHandle linkage, double[] outDoFs, int numDoFs);

            internal static void ErodXShellGetPerSegmentRestLengths(NativeHandle linkage, out IntPtr outLengths, out int numLengths)
            {
                outLengths = default(IntPtr);
                numLengths = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetPerSegmentRestLengths(linkage, out outLengths, out numLengths);
                    try
                    {
                        NativeApi.Check("erodXShellGetPerSegmentRestLengths");
                        NativeBufferScope.Track(outLengths);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outLengths); outLengths = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetPerSegmentRestLengths")]
            private static extern void Native_ErodXShellGetPerSegmentRestLengths(NativeHandle linkage, out IntPtr outLengths, out int numLengths);

            internal static void ErodXShellSetPerSegmentRestLengths(NativeHandle linkage, double[] inLengths, int numLengths)
            {
                if (numLengths < 0) throw new ArgumentOutOfRangeException(nameof(numLengths));
                NativeChecks.Array(inLengths, NativeChecks.Product(numLengths, 1), nameof(inLengths));
                NativeChecks.Finite(inLengths, nameof(inLengths));
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellSetPerSegmentRestLengths(linkage, inLengths, numLengths);
                    try
                    {
                        NativeApi.Check("erodXShellSetPerSegmentRestLengths");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellSetPerSegmentRestLengths")]
            private static extern void Native_ErodXShellSetPerSegmentRestLengths(NativeHandle linkage, double[] inLengths, int numLengths);

            internal static void ErodXShellGetJointAngles(NativeHandle linkage, out IntPtr outAngles, out int numAngles)
            {
                outAngles = default(IntPtr);
                numAngles = default(int);
                using (NativeCall.Enter(linkage))
                {
                    Native_ErodXShellGetJointAngles(linkage, out outAngles, out numAngles);
                    try
                    {
                        NativeApi.Check("erodXShellGetJointAngles");
                        NativeBufferScope.Track(outAngles);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outAngles); outAngles = IntPtr.Zero;
                        throw;
                    }
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellGetJointAngles")]
            private static extern void Native_ErodXShellGetJointAngles(NativeHandle linkage, out IntPtr outAngles, out int numAngles);
        }
    }
}

