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
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentBuild")]
            internal static extern IntPtr ErodRodSegmentBuild(IntPtr linkage, int index);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetEdgesCount")]
            internal static extern int ErodRodSegmentGetEdgesCount(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetVerticesCount")]
            internal static extern int ErodRodSegmentGetVerticesCount(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetRestAnglesCount")]
            internal static extern int ErodRodSegmentGetRestAnglesCount(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetRestLengths")]
            internal static extern void ErodRodSegmentGetRestLengths(IntPtr segment, out IntPtr restLengths, out int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetRestKappas")]
            internal static extern void ErodRodSegmentGetRestKappas(IntPtr segment, out IntPtr restAngles, out int numAngles);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetStretchingStresses")]
            internal static extern void ErodRodSegmentGetStretchingStresses(IntPtr segment, out IntPtr  stresses, out int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetTwistingStresses")]
            internal static extern void ErodRodSegmentGetTwistingStresses(IntPtr segment, out IntPtr stresses, out int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetCenterLinePositions")]
            internal static extern void ErodRodSegmentGetCenterLinePositions(IntPtr segment, out IntPtr outCoords, out int numCoords);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetMaxBendingStresses")]
            internal static extern void ErodRodSegmentGetMaxBendingStresses(IntPtr segment, out IntPtr stresses, out int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetMinBendingStresses")]
            internal static extern void ErodRodSegmentGetMinBendingStresses(IntPtr segment, out IntPtr stresses, out int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetSqrtBendingEnergies")]
            internal static extern void ErodRodSegmentGetSqrtBendingEnergies(IntPtr segment, out IntPtr stresses, out int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetMeshData")]
            internal static extern void ErodRodSegmentGetMeshData(IntPtr segment, out IntPtr outCoords, out IntPtr outQuads, out int numCoords, out int numQuads);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetStartJointIndex")]
            internal static extern int ErodRodSegmentGetStartJointIndex(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetEndJointIndex")]
            internal static extern int ErodRodSegmentGetEndJointIndex(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetBendingStiffness")]
            internal static extern void ErodRodSegmentGetBendingStiffness(IntPtr segment, out IntPtr lambda1, out IntPtr lambda2, out int numLambda1, out int numLambda2);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetTwistingStiffness")]
            internal static extern void ErodRodSegmentGetTwistingStiffness(IntPtr segment, out IntPtr stiffness, out int numVertices);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetStretchingStiffness")]
            internal static extern void ErodRodSegmentGetStretchingStiffness(IntPtr segment, out IntPtr stiffness, out int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetEnergy")]
            internal static extern double ErodRodSegmentGetEnergy(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetEnergyBend")]
            internal static extern double ErodRodSegmentGetEnergyBend(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetEnergyStretch")]
            internal static extern double ErodRodSegmentGetEnergyStretch(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetEnergyTwist")]
            internal static extern double ErodRodSegmentGetEnergyTwist(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentMaterialFrame")]
            internal static extern void ErodRodSegmentGetMaterialFrame(IntPtr segment, out int outCoordsCount, out IntPtr outCoordsD1, out IntPtr outCoordsD2);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetRestPoints")]
            internal static extern void ErodRodSegmentGetRestPoints(IntPtr segment, out IntPtr outData, out int numData);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetRestDirectors")]
            internal static extern void ErodRodSegmentGetRestDirectors(IntPtr segment, out IntPtr outData, out int numData);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetRestTwists")]
            internal static extern void ErodRodSegmentGetRestTwists(IntPtr segment, out IntPtr outData, out int numData);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetBendingEnergyType")]
            internal static extern int ErodRodSegmentGetBendingEnergyType(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetDensities")]
            internal static extern void ErodRodSegmentGetDensities(IntPtr segment, out IntPtr outData, out int numData);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetInitialMinRestLength")]
            internal static extern double ErodRodSegmentGetInitialMinRestLength(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetEdgeMaterial")]
            internal static extern void ErodRodSegmentGetEdgeMaterial(IntPtr segment, int idx, out IntPtr outMatData, out IntPtr outCoords, out IntPtr outEdges, out int numMatData, out int numCoords, out int numEdges);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetEdgeMaterialCount")]
            internal static extern int ErodRodSegmentGetEdgeMaterialCount(IntPtr segment);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetDeformedState")]
            internal static extern void ErodRodSegmentGetDeformedState(IntPtr segment, out IntPtr outPtsCoords, out IntPtr outThetas, out IntPtr outTgtCoords,
                                                out IntPtr outDirCoords, out IntPtr outSrcThetas, out IntPtr outSrcTwist,
                                                out int outNumPtsCoords, out int outNumThetas, out int outNumTgtCoords,
                                                out int outNumDirCoords, out int outNumSrcThetas, out int outNumSrcTwist);
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodRodSegmentGetVonMisesStresses")]
            internal static extern void ErodRodSegmentGetVonMisesStresses(IntPtr segment, out IntPtr outData, out int numData);
        }
    }
}
