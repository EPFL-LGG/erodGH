using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ErodDataLib.Types;
using ErodModelLib.Creators;
using ErodDataLib.Utils;
using Rhino.Geometry;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace ErodModelLib.Types
{
    public partial class RodLinkage
    {

        public void UpdateMesh()
        {
            double[] outCoords;
            int[] outQuads;
            GetMeshData(out outCoords, out outQuads);

            int vCount = outCoords.Length / 3;
            for (int i = 0; i < vCount; i++)
            {
                MeshVis.Vertices.SetVertex(i, outCoords[i * 3], outCoords[i * 3 + 1], outCoords[i * 3 + 2]);
            }
        }

        public double[] GetDoFs()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numDoFs;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellGetDoFs(Handle, out cPtr, out numDoFs);

            double[] outDoFs = new double[numDoFs];
            NativeBufferScope.Copy(cPtr, outDoFs, 0, numDoFs);
            Kernel.FreeNativeBuffer(cPtr);
            return outDoFs;

            }
        }

        public void SetDoFs(double[] dofs)
        {
            Kernel.RodLinkage.ErodXShellSetDoFs(Handle, dofs, dofs.Length);
        }

        public double[] GetScalarFieldSqrtBendingEnergies()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldSqrtBendingEnergies(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldMaxBendingStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldMaxBendingStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldMinBendingStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldMinBendingStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldTwistingStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldTwistingStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldStretchingStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldStretchingStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldVonMisesStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldVonMisesStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public override void SetMaterial(int sectionType, double E, double poisonRatio, double[] sectionParams, int axisType)
        {
            Kernel.RodLinkage.ErodXShellSetMaterial(Handle, sectionType, E, poisonRatio, sectionParams, sectionParams.Length, axisType);
        }

        public void SetCustomMaterial(double E, double poisonRatio, double[] inProfileCoords, int numVertices, double[] inHolesCoords, int numHoles, int axisType)
        {
            Kernel.RodLinkage.ErodXShellSetCustomMaterial(Handle, E, poisonRatio, inProfileCoords, numVertices, inHolesCoords, numHoles, axisType);
        }

        public void SetMaterial(int[] sectionType, double[] E, double[] poisonRatio, double[] sectionParams, int[] sectionParamsCount, int[] axisType)
        {
            Kernel.RodLinkage.ErodXShellSetJointMaterial(Handle, sectionType.Length, sectionType, E, poisonRatio, sectionParams, sectionParamsCount, axisType);
        }

        public void SetCustomMaterial(int[] sectionType, double[] E, double[] poisonRatio, double[] inProfileCoords, int[] profileParamsCount, double[] inHolesCoords, int[] holesCount, int[] axisType)
        {
            Kernel.RodLinkage.ErodXShellSetCustomJointMaterial(Handle, sectionType.Length, sectionType, E, poisonRatio, inProfileCoords, profileParamsCount, inHolesCoords, holesCount, axisType);
        }

        public int GetJointsCount()
        {
            return Kernel.RodLinkage.ErodXShellGetJointsCount(Handle);
        }

        public int GetNodesCount()
        {
            return Kernel.RodLinkage.ErodXShellGetCenterLinePositionsCount(Handle);
        }

        public int GetCentralJointIndex()
        {
            return Kernel.RodLinkage.ErodXShellGetCentralJointIndex(Handle);
        }

        public void AddStiffenRegion(Box[] boxes, double factor)
        {
            var coordinates=NativeChecks.BoxCoordinates(boxes);
            Kernel.RodLinkage.ErodXShellSetStiffenRegions(Handle,factor,coordinates,boxes.Length);
        }

        public override double[] GetForceVars(bool includeExternalForces= true, bool includeCables=false)
        {
            if(ModelIO.Forces.Count==0) return new double[0];

            double[] forceVars = new double[GetDoFCount()];

            foreach (ForceIO f in ModelIO.Forces)
            {
                if ((includeCables && f.ForceType == ForceIOType.Cable) ||
                    (includeExternalForces && f.ForceType == ForceIOType.External))
                {
                    int numPositions = f.NumPositions;
                    // Calculate forces using updated positions
                    Point3d[] pos = new Point3d[numPositions];
                    for (int i = 0; i < numPositions; i++)
                    {
                        int idx = f.Indices[i];
                        if (f.IsJoint[i]) pos[i] = Joints[idx].Position;
                        else pos[i] = Segments.Nodes[idx];
                    }
                    Vector3d[] forces = f.CalculateForce(pos);

                    // Set vars
                    for (int i = 0; i < numPositions; i++)
                    {
                        int[] indicesDoFs = f.IndicesDoFs[i];
                        for (int j = 0; j < indicesDoFs.Length; j++) forceVars[indicesDoFs[j]] += forces[i][j];
                    }
                }
            }

            return forceVars;
        }

        public int[] GetCentralSupportVars()
        {
            int jdo = GetCentralJointIndex();
            return new int[] { jdo, jdo + 1, jdo + 2, jdo + 3, jdo + 4, jdo + 5 };
        }

        public void RemoveInitialCurvatures()
        {
            Kernel.RodLinkage.ErodXShellRemoveRestCurvatures(Handle);
        }

        public void SetDesignParameters(bool use_restLength, bool use_restKappa, bool update_designParams_cache = true)
        {
            Kernel.RodLinkage.ErodXShellSetDesignParameterConfig(Handle, Convert.ToInt32(use_restLength), Convert.ToInt32(use_restKappa), Convert.ToInt32(update_designParams_cache));
        }

        public int GetRestKappaVarsCount()
        {
            return Kernel.RodLinkage.ErodXShellGetRestKappaVarsCount(Handle);
        }

        public double[] GetRestKappaVars()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numD;
            IntPtr ptrD;
            Kernel.RodLinkage.ErodXShellGetRestKappaVars(Handle, out ptrD, out numD);

            double[] data = new double[numD];
            NativeBufferScope.Copy(ptrD, data, 0, numD);
            Kernel.FreeNativeBuffer(ptrD);

            return data;

            }
        }

        public int GetHessianNNZ(bool variableDesignParameters)
        {
            return Kernel.RodLinkage.ErodXShellGetHessianNNZ(Handle, Convert.ToInt32(variableDesignParameters));
        }

        protected override void GetMeshData(out double[] outCoords, out int[] outQuads)
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numCoords, numQuads;
            IntPtr cPtr, qPtr;
            Kernel.RodLinkage.ErodXShellGetMeshData(Handle, out cPtr, out qPtr, out numCoords, out numQuads);

            outCoords = new double[numCoords];
            outQuads = new int[numQuads];
            NativeBufferScope.Copy(cPtr, outCoords, 0, numCoords);
            NativeBufferScope.Copy(qPtr, outQuads, 0, numQuads);
            Kernel.FreeNativeBuffer(cPtr);
            Kernel.FreeNativeBuffer(qPtr);

            }
        }

        public Mesh InferTargetSurface(int nsubdiv, int numExtensionLayers=1)
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numCoords, numTrias;
            IntPtr cPtr, qPtr;
            int error = Kernel.RodLinkage.ErodXShellInferTargetSurface(Handle, nsubdiv, numExtensionLayers,out cPtr, out qPtr, out numCoords, out numTrias, out Error);

            if (error==0)
            {
                double[] outCoords = new double[numCoords];
                int[] outTrias = new int[numTrias];
                NativeBufferScope.Copy(cPtr, outCoords, 0, numCoords);
                NativeBufferScope.Copy(qPtr, outTrias, 0, numTrias);
                Kernel.FreeNativeBuffer(cPtr);
                Kernel.FreeNativeBuffer(qPtr);

                Mesh m = Helpers.GetTriasMesh(outCoords, outTrias);

                return m;
            }
            else
            {
                string errorMsg = Marshal.PtrToStringAnsi(Error);
                throw new Exception(errorMsg);
            }

            }
        }

        public double[] GetDesignParameters()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numDP;
            IntPtr ptrDP;
            Kernel.RodLinkage.ErodXShellGetDesignParams(Handle, out ptrDP, out numDP);

            double[] designParams = new double[numDP];
            NativeBufferScope.Copy(ptrDP, designParams, 0, numDP);
            Kernel.FreeNativeBuffer(ptrDP);

            return designParams;

            }
        }

        public int GetDesignParametersNumber()
        {
            return Kernel.RodLinkage.ErodXShellGetDesignParametersNumber(Handle);
        }

        public double GetInitialMinRestLength()
        {
            return Kernel.RodLinkage.ErodXShellGetInitialMinRestLength(Handle);
        }

        public SparseMatrixData GetSegmentRestLenToEdgeRestLenMapTranspose()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr ptrAx, ptrAi, ptrAp;
            long M, N, NZ;
            int errorCode = Kernel.RodLinkage.ErodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(Handle, out ptrAx, out ptrAi, out ptrAp, out M, out N, out NZ, out Error);

            if (errorCode == 0)
            {
                double[] Ax = new double[NZ];
                long[] Ai = new long[NZ];
                long[] Ap = new long[N + 1];

                NativeBufferScope.Copy(ptrAx, Ax, 0, (int)NZ);
                NativeBufferScope.Copy(ptrAi, Ai, 0, (int)NZ);
                NativeBufferScope.Copy(ptrAp, Ap, 0, (int)N + 1);
                Kernel.FreeNativeBuffer(ptrAx);
                Kernel.FreeNativeBuffer(ptrAi);
                Kernel.FreeNativeBuffer(ptrAp);

                SparseMatrixData matrix = new SparseMatrixData(M, N, NZ);
                matrix.Ai = Ai.ToList();
                matrix.Ap = Ap.ToList();
                matrix.Ax = Ax.ToList();

                return matrix;
            }
            else
            {
                string errorMsg = Marshal.PtrToStringAnsi(Error);
                throw new Exception(errorMsg);
            }

            }
        }

        public void GetDesignParametersConfig(out bool restLength, out bool restKappa)
        {
            int outRestLength, outRestKappa;
            Kernel.RodLinkage.ErodXShellGetDesignParameterConfig(Handle, out outRestLength, out outRestKappa);
            restLength = Convert.ToBoolean(outRestLength);
            restKappa = Convert.ToBoolean(outRestKappa);
        }

        public double[] GetRestLenghtsSolveDoFs()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr ptrDoFs;
            int numDofs;
            Kernel.RodLinkage.ErodXShellGetRestLengthsSolveDoFs(Handle, out ptrDoFs, out numDofs);

            double[] dofs = new double[numDofs];

            NativeBufferScope.Copy(ptrDoFs, dofs, 0, (int) numDofs);
            Kernel.FreeNativeBuffer(ptrDoFs);

            return dofs;

            }
        }

        public void SetRestLenghtsSolveDoFs(double[] dofs)
        {
            Kernel.RodLinkage.ErodXShellSetRestLengthsSolveDoFs(Handle, dofs, dofs.Length);
        }

        public double[] GetPerSegmentRestLenghts()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr ptrLengths;
            int numLengths;
            Kernel.RodLinkage.ErodXShellGetPerSegmentRestLengths(Handle, out ptrLengths, out numLengths);

            double[] lengths = new double[numLengths];

            NativeBufferScope.Copy(ptrLengths, lengths, 0, (int)numLengths);
            Kernel.FreeNativeBuffer(ptrLengths);

            return lengths;

            }
        }

        public void SetPerSegmentRestLenghts(double[] lengths)
        {
            Kernel.RodLinkage.ErodXShellSetPerSegmentRestLengths(Handle, lengths, lengths.Length);
        }

        public double[] GetJointAngles()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr angPtr;
            int numAng;
            Kernel.RodLinkage.ErodXShellGetJointAngles(Handle, out angPtr, out numAng);

            double[] angles = new double[numAng];

            NativeBufferScope.Copy(angPtr, angles, 0, (int)numAng);
            Kernel.FreeNativeBuffer(angPtr);

            return angles;

            }
        }

        public LineCurve[] GetSegmentsAsLines()
        {
            int numSegments = Segments.Count;
            LineCurve[] edges = new LineCurve[numSegments];
            for (int i = 0; i < numSegments; i++)
            {
                int idx0 = Segments[i].GetStartJoint();
                int idx1 = Segments[i].GetEndJoint();

                double[] p0 = Joints[idx0].GetPosition();
                double[] p1 = Joints[idx1].GetPosition();

                Point3d pos0 = new Point3d(p0[0], p0[1], p0[2]);
                Point3d pos1 = new Point3d(p1[0], p1[1], p1[2]);

                edges[i] = new LineCurve(pos0, pos1);
            }

            return edges;
        }
    }
}
