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
            int numDoFs;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellGetDoFs(Model, out cPtr, out numDoFs);

            double[] outDoFs = new double[numDoFs];
            Marshal.Copy(cPtr, outDoFs, 0, numDoFs);
            Marshal.FreeCoTaskMem(cPtr);
            return outDoFs;
        }

        public void SetDoFs(double[] dofs)
        {
            Kernel.RodLinkage.ErodXShellSetDoFs(Model, dofs, dofs.Length);
        }

        public double[] GetScalarFieldSqrtBendingEnergies()
        {
            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldSqrtBendingEnergies(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldMaxBendingStresses()
        {
            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldMaxBendingStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldMinBendingStresses()
        {
            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldMinBendingStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldTwistingStresses()
        {
            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldTwistingStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldStretchingStresses()
        {
            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldStretchingStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldVonMisesStresses()
        {
            int numField;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellScalarFieldVonMisesStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public override void SetMaterial(int sectionType, double E, double poisonRatio, double[] sectionParams, int axisType)
        {
            Kernel.RodLinkage.ErodXShellSetMaterial(Model, sectionType, E, poisonRatio, sectionParams, sectionParams.Length, axisType);
        }

        public void SetCustomMaterial(double E, double poisonRatio, double[] inProfileCoords, int numVertices, double[] inHolesCoords, int numHoles, int axisType)
        {
            Kernel.RodLinkage.ErodXShellSetCustomMaterial(Model, E, poisonRatio, inProfileCoords, numVertices, inHolesCoords, numHoles, axisType);
        }

        public void SetMaterial(int[] sectionType, double[] E, double[] poisonRatio, double[] sectionParams, int[] sectionParamsCount, int[] axisType)
        {
            Kernel.RodLinkage.ErodXShellSetJointMaterial(Model, sectionType.Length, sectionType, E, poisonRatio, sectionParams, sectionParamsCount, axisType);
        }

        public void SetCustomMaterial(int[] sectionType, double[] E, double[] poisonRatio, double[] inProfileCoords, int[] profileParamsCount, double[] inHolesCoords, int[] holesCount, int[] axisType)
        {
            Kernel.RodLinkage.ErodXShellSetCustomJointMaterial(Model, sectionType.Length, sectionType, E, poisonRatio, inProfileCoords, profileParamsCount, inHolesCoords, holesCount, axisType);
        }

        public int GetJointsCount()
        {
            return Kernel.RodLinkage.ErodXShellGetJointsCount(Model);
        }

        public int GetNodesCount()
        {
            return Kernel.RodLinkage.ErodXShellGetCenterLinePositionsCount(Model);
        }

        public int GetCentralJointIndex()
        {
            return Kernel.RodLinkage.ErodXShellGetCentralJointIndex(Model);
        }

        public void AddStiffenRegion(Box[] boxes, double factor)
        {
            int numBoxes = boxes.Length;
            double[] coords = new double[numBoxes * 8 * 3];
            int idx = 0;
            for (int i = 0; i < numBoxes; i++)
            {
                Box b = boxes[i];
                foreach (Point3d pt in b.GetCorners())
                {
                    coords[idx] = pt.X;
                    coords[idx + 1] = pt.X;
                    coords[idx + 2] = pt.X;
                    idx += 3;
                }
            }

            Kernel.RodLinkage.ErodXShellSetStiffenRegions(Model, factor, coords, numBoxes);
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
            Kernel.RodLinkage.ErodXShellRemoveRestCurvatures(Model);
        }

        public void SetDesignParameters(bool use_restLength, bool use_restKappa, bool update_designParams_cache = true)
        {
            Kernel.RodLinkage.ErodXShellSetDesignParameterConfig(Model, Convert.ToInt32(use_restLength), Convert.ToInt32(use_restKappa), Convert.ToInt32(update_designParams_cache));
        }

        public int GetRestKappaVarsCount()
        {
            return Kernel.RodLinkage.ErodXShellGetRestKappaVarsCount(Model);
        }

        public double[] GetRestKappaVars()
        {
            int numD;
            IntPtr ptrD;
            Kernel.RodLinkage.ErodXShellGetRestKappaVars(Model, out ptrD, out numD);

            double[] data = new double[numD];
            Marshal.Copy(ptrD, data, 0, numD);
            Marshal.FreeCoTaskMem(ptrD);

            return data;
        }

        public int GetHessianNNZ(bool variableDesignParameters)
        {
            return Kernel.RodLinkage.ErodXShellGetHessianNNZ(Model, Convert.ToInt32(variableDesignParameters));
        }

        protected override void GetMeshData(out double[] outCoords, out int[] outQuads)
        {
            int numCoords, numQuads;
            IntPtr cPtr, qPtr;
            Kernel.RodLinkage.ErodXShellGetMeshData(Model, out cPtr, out qPtr, out numCoords, out numQuads);

            outCoords = new double[numCoords];
            outQuads = new int[numQuads];
            Marshal.Copy(cPtr, outCoords, 0, numCoords);
            Marshal.Copy(qPtr, outQuads, 0, numQuads);
            Marshal.FreeCoTaskMem(cPtr);
            Marshal.FreeCoTaskMem(qPtr);
        }

        public Mesh InferTargetSurface(int nsubdiv, int numExtensionLayers=1)
        {
            int numCoords, numTrias;
            IntPtr cPtr, qPtr;
            int error = Kernel.RodLinkage.ErodXShellInferTargetSurface(Model, nsubdiv, numExtensionLayers,out cPtr, out qPtr, out numCoords, out numTrias, out Error);

            if (error==0)
            {
                double[] outCoords = new double[numCoords];
                int[] outTrias = new int[numTrias];
                Marshal.Copy(cPtr, outCoords, 0, numCoords);
                Marshal.Copy(qPtr, outTrias, 0, numTrias);
                Marshal.FreeCoTaskMem(cPtr);
                Marshal.FreeCoTaskMem(qPtr);

                Mesh m = Helpers.GetTriasMesh(outCoords, outTrias);

                return m;
            }
            else
            {
                string errorMsg = Marshal.PtrToStringAnsi(Error);
                throw new Exception(errorMsg);
            }
        }

        public double[] GetDesignParameters()
        {
            int numDP;
            IntPtr ptrDP;
            Kernel.RodLinkage.ErodXShellGetDesignParams(Model, out ptrDP, out numDP);

            double[] designParams = new double[numDP];
            Marshal.Copy(ptrDP, designParams, 0, numDP);
            Marshal.FreeCoTaskMem(ptrDP);

            return designParams;
        }

        public int GetDesignParametersNumber()
        {
            return Kernel.RodLinkage.ErodXShellGetDesignParametersNumber(Model);
        }

        public double GetInitialMinRestLength()
        {
            return Kernel.RodLinkage.ErodXShellGetInitialMinRestLength(Model);
        }

        public SparseMatrixData GetSegmentRestLenToEdgeRestLenMapTranspose()
        {
            IntPtr ptrAx, ptrAi, ptrAp;
            long M, N, NZ;
            int errorCode = Kernel.RodLinkage.ErodXShellGetSegmentRestLenToEdgeRestLenMapTranspose(Model, out ptrAx, out ptrAi, out ptrAp, out M, out N, out NZ, out Error);

            if (errorCode == 0)
            {
                double[] Ax = new double[NZ];
                long[] Ai = new long[NZ];
                long[] Ap = new long[N + 1];

                Marshal.Copy(ptrAx, Ax, 0, (int)NZ);
                Marshal.Copy(ptrAi, Ai, 0, (int)NZ);
                Marshal.Copy(ptrAp, Ap, 0, (int)N + 1);
                Marshal.FreeCoTaskMem(ptrAx);
                Marshal.FreeCoTaskMem(ptrAi);
                Marshal.FreeCoTaskMem(ptrAp);

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

        public void GetDesignParametersConfig(out bool restLength, out bool restKappa)
        {
            int outRestLength, outRestKappa;
            Kernel.RodLinkage.ErodXShellGetDesignParameterConfig(Model, out outRestLength, out outRestKappa);
            restLength = Convert.ToBoolean(outRestLength);
            restKappa = Convert.ToBoolean(outRestKappa);
        }

        public double[] GetRestLenghtsSolveDoFs()
        {
            IntPtr ptrDoFs;
            long numDofs;
            Kernel.RodLinkage.ErodXShellGetRestLengthsSolveDoFs(Model, out ptrDoFs, out numDofs);

            double[] dofs = new double[numDofs];

            Marshal.Copy(ptrDoFs, dofs, 0, (int) numDofs);
            Marshal.FreeCoTaskMem(ptrDoFs);

            return dofs;
        }

        public void SetRestLenghtsSolveDoFs(double[] dofs)
        {
            Kernel.RodLinkage.ErodXShellSetRestLengthsSolveDoFs(Model, dofs, dofs.Length);
        }

        public double[] GetPerSegmentRestLenghts()
        {
            IntPtr ptrLengths;
            long numLengths;
            Kernel.RodLinkage.ErodXShellGetPerSegmentRestLengths(Model, out ptrLengths, out numLengths);

            double[] lengths = new double[numLengths];

            Marshal.Copy(ptrLengths, lengths, 0, (int)numLengths);
            Marshal.FreeCoTaskMem(ptrLengths);

            return lengths;
        }

        public void SetPerSegmentRestLenghts(double[] lengths)
        {
            Kernel.RodLinkage.ErodXShellSetPerSegmentRestLengths(Model, lengths, lengths.Length);
        }

        public double[] GetJointAngles()
        {
            IntPtr angPtr;
            long numAng;
            Kernel.RodLinkage.ErodXShellGetJointAngles(Model, out angPtr, out numAng);

            double[] angles = new double[numAng];

            Marshal.Copy(angPtr, angles, 0, (int)numAng);
            Marshal.FreeCoTaskMem(angPtr);

            return angles;
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
