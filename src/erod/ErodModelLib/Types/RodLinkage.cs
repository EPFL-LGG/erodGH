using System;
using ErodModelLib.Creators;
using Rhino.Geometry;
using ErodDataLib.Utils;
using ErodDataLib.Types;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.IO;
using Grasshopper.Kernel;

namespace ErodModelLib.Types
{
    public partial class RodLinkage : ElasticModel
    {
        public LinkageIO ModelIO => (LinkageIO)_modelIO;
        public RodSegmentCollection Segments { get; private set; }
        public JointCollection Joints { get; private set; }

        public RodLinkage(RodLinkage model) : base((LinkageIO) model.ModelIO.Clone())
        {
            if (model.ModelIO.ModelType == ElasticModelType.AttractedSurfaceRodLinkage) Model = Kernel.RodLinkage.ErodXShellAttractedSurfaceCopy(model.Model, out Error);
            else Model = Kernel.RodLinkage.ErodXShellCopy(model.Model, out Error);

            if (Model != IntPtr.Zero) Init();
            else throw new Exception(Marshal.PtrToStringAnsi(Error));
        }

        public override object Clone()
        {
            return new RodLinkage(this);
        }

        public RodLinkage(LinkageIO modelIO, bool checkConsistentNormals, bool initConsistentAngle, bool edgeDataInitialization = false) : base(modelIO)
        {
            ///////////////////////////////////////////////////
            ///////////////////////////////////////////////////
            // Parse joint data
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////

            #region Joints
            int numJoints = modelIO.Joints.Count;
            int[] jointForVertex = modelIO.NodeToJointMaps;
            int numVertices = modelIO.NodeToJointMaps.Length;
            int[] segmentsA = new int[numJoints * 2];
            int[] segmentsB = new int[numJoints * 2];
            int[] isStartA = new int[numJoints * 2];
            int[] isStartB = new int[numJoints * 2];
            double[] coords = new double[numJoints * 3];
            double[] normals = new double[numJoints * 3];
            double[] edgesA = new double[numJoints * 3];
            double[] edgesB = new double[numJoints * 3];


            for (int i = 0; i < numJoints; i++)
            {
                JointIO joint = modelIO.Joints[i];

                coords[i * 3] = joint.Position.X;
                coords[i * 3 + 1] = joint.Position.Y;
                coords[i * 3 + 2] = joint.Position.Z;

                normals[i * 3] = joint.Normal.X;
                normals[i * 3 + 1] = joint.Normal.Y;
                normals[i * 3 + 2] = joint.Normal.Z;

                edgesA[i * 3] = joint.EdgeA.X;
                edgesA[i * 3 + 1] = joint.EdgeA.Y;
                edgesA[i * 3 + 2] = joint.EdgeA.Z;
                edgesB[i * 3] = joint.EdgeB.X;
                edgesB[i * 3 + 1] = joint.EdgeB.Y;
                edgesB[i * 3 + 2] = joint.EdgeB.Z;

                segmentsA[i * 2] = joint.SegmentsA[0];
                segmentsA[i * 2 + 1] = joint.SegmentsA[1];
                segmentsB[i * 2] = joint.SegmentsB[0];
                segmentsB[i * 2 + 1] = joint.SegmentsB[1];

                isStartA[i * 2] = Convert.ToInt32(joint.IsStartA[0]);
                isStartA[i * 2 + 1] = Convert.ToInt32(joint.IsStartA[1]);
                isStartB[i * 2] = Convert.ToInt32(joint.IsStartB[0]);
                isStartB[i * 2 + 1] = Convert.ToInt32(joint.IsStartB[1]);
            }

            double[] coordsNodes = new double[numVertices * 3];
            double[] normalsNodes = new double[numVertices * 3];
            for (int i = 0; i < numVertices; i++)
            {
                Point3d node = modelIO.Graph.Nodes[i];
                Vector3d normal = modelIO.Graph.Normals[i];

                coordsNodes[i * 3] = node.X;
                coordsNodes[i * 3 + 1] = node.Y;
                coordsNodes[i * 3 + 2] = node.Z;

                normalsNodes[i * 3] = normal.X;
                normalsNodes[i * 3 + 1] = normal.Y;
                normalsNodes[i * 3 + 2] = normal.Z;
            }
            #endregion

            ///////////////////////////////////////////////////
            ///////////////////////////////////////////////////
            // Parse edge data
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////
            #region Edges
            int numEdges = modelIO.Segments.Count;
            List<double> curvePoints = new List<double>();
            int[] offsetCurvePoints = new int[numEdges];
            int[] startJoints = new int[numEdges];
            int[] endJoints = new int[numEdges];
            int[] subdivisions = new int[numEdges];
            int[] edges = new int[numEdges * 2];
            int[] isCurvedEdge = new int[numEdges];
            double[] restLengths = new double[numEdges];
            int offset = 0;

            for (int i = 0; i < numEdges; i++)
            {
                SegmentIO edge = modelIO.Segments[i];

                isCurvedEdge[i] = edge.IsCurvedEdge ? 1 : 0;

                // Rest lengths
                restLengths[i] = edge.RestLength;

                // Subdivisions
                subdivisions[i] = edge.Subdivision;

                // Curve points
                for (int j = 0; j < edge.CurvePoints.Length; j++)
                {
                    Point3d p = edge.CurvePoints[j];
                    curvePoints.AddRange(new double[] { p.X, p.Y, p.Z });
                    offset += 3;
                }
                offsetCurvePoints[i] = offset;
                startJoints[i] = edge.StartJoint;
                endJoints[i] = edge.EndJoint;

                // Edges
                edges[i * 2] = edge.Indices[0];
                edges[i * 2 + 1] = edge.Indices[1];
            }
            #endregion

            ///////////////////////////////////////////////////
            ///////////////////////////////////////////////////
            // Build RodLinkage
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////

            #region Linkage
            if (coords.Length == 0 || coords == null || edgesA.Length == 0 || edgesA == null
                || edgesB.Length == 0 || edgesB == null)
            {
                throw new MissingFieldException("Missing joint vertex data.");
            }
            else if (curvePoints.Count == 0 || curvePoints == null)
            {
                throw new MissingFieldException("Missing edge data.");
            }
            else
            {
                if (!edgeDataInitialization)
                {
                    Model = Kernel.RodLinkage.ErodXShellBuild(numVertices, numEdges, coordsNodes, edges, normalsNodes, restLengths, offsetCurvePoints, curvePoints.ToArray(), modelIO.Interleaving, Convert.ToInt32(initConsistentAngle), Convert.ToInt32(initConsistentAngle), out Error);
                }
                else
                {
                    numVertices = modelIO.Graph.NumNodes;
                    numEdges = modelIO.Graph.NumEdges;
                    modelIO.Graph.GetFlattenGraphData(out coords, out normals, out edges);
                    // For this constructor all rod segments needs to be subdivided equally
                    int subd = subdivisions.Sum() / subdivisions.Length;
                    Model = Kernel.RodLinkage.ErodXShellBuildFromGraph(numVertices, numEdges, coords, edges, normals, subd, modelIO.Interleaving, Convert.ToInt32(initConsistentAngle), out Error);
                }

                if (ModelIO.ContainsTargetSurface()) AddTargetSurface(ModelIO.TargetSurface);

                if (Model != IntPtr.Zero) Init();
                else throw new Exception(Marshal.PtrToStringAuto(Error));
            }
            #endregion
        }

        public void AddTargetSurface(TargetSurfaceIO data)
        {
            if (data == null || data == default) return;
            double[] inCoords = Helpers.FlattenDoubleArray(data.Vertices);
            int[] inTrias = Helpers.FlattenIntArray(data.Trias);
            Model = Kernel.RodLinkage.ErodXShellAttractedSurfaceBuild(data.Vertices.Length, data.Trias.Length, inCoords, inTrias, Model, data.TargetJointWeight, out Error);
            if (Model == IntPtr.Zero) throw new Exception(Marshal.PtrToStringAnsi(Error));
        }

        private void InitMaterials()
        {
            int numMaterials = ModelIO.Materials.Count;
            int numJoints = Joints.Count;
            Material[] _materials = new Material[numMaterials];

            if (numMaterials == 0) throw new Exception("No material has been assigned to the linkage.");

            if (numMaterials == 1) _materials[0] = new Material(ModelIO.Materials[0]);
            else
            {
                _materials = new Material[numJoints];

                // First: Assign the first material to all the joints
                for (int i = 0; i < numJoints; i++) _materials[i] = new Material(ModelIO.Materials[0]);

                // Second: Re-assign materials to specific joints
                for (int i = 0; i < numMaterials; i++)
                {
                    var mat = ModelIO.Materials[i];
                    int idx = Joints.ClosestJoint(mat.ReferencePosition);
                    _materials[idx] = new Material(mat);
                }
            }
            Kernel.Material.ErodMaterialSetToLinkage(_materials.Select(m => m.Model).ToArray(), _materials.Length, Model);
        }

        public void InitSupports()
        {
            if (ModelIO.Supports.Count == 0) ModelIO.AddCentralSupport();
            if (ModelIO.Supports.GetNumberFixSupport() == 0)
            {
                var sp = ModelIO.Supports[0];
                sp.IsTemporary = false;
                ModelIO.Supports[0] = sp;
            }

            for(int i= 0; i < ModelIO.Supports.Count; i++)
            {
                var sp = ModelIO.Supports[i];
                if (sp.IndexMap == -1)
                {
                    Point3d p = sp.ReferencePosition;

                    int[] dof = new int[] { 0, 1, 2 };
                    int[] indicesDoFs = new int[dof.Length];

                    int idxJ = Joints.ClosestJoint(p);
                    int idxN = Segments.ClosestNode(p);

                    if (p.DistanceTo(Joints[idxJ].Position) < p.DistanceTo(Segments.GetNode(idxN)))
                    {
                        Kernel.RodLinkage.ErodXShellGetDofOffsetForJoint(Model, idxJ, dof, dof.Length, indicesDoFs);
                        sp.IndexMap = idxJ;
                        sp.IsJointSupport = true;
                    }
                    else
                    {
                        Kernel.RodLinkage.ErodXShellGetDofOffsetForCenterLinePos(Model, idxN, dof, dof.Length, indicesDoFs);
                        sp.IndexMap = idxN;
                        sp.IsJointSupport = false;
                    }

                    for (int j = 0; j < sp.LockedDoFs.Length; j++) if (!sp.LockedDoFs[j]) indicesDoFs[j] = -1;
                    sp.SetIndicesDoFs(indicesDoFs);
                }
                sp.UpdateReferencePosition(sp.IsJointSupport ? Joints[sp.IndexMap].Position : Segments.GetNode(sp.IndexMap));
                ModelIO.Supports[i] = sp;
            }
        }

        public void InitForces()
        {
            var allForces = ModelIO.Forces;

            // Check for forces with unset reference positions. These will be global forces that are applied to all joints 
            var tempForces = allForces.Where(f => f.Positions[0] == Point3d.Unset);
            List<ForceIO> forces = new List<ForceIO>();
            if (tempForces.Count() != 0) {
                foreach (var f in tempForces)
                {
                    if (f is ForceExternalIO)
                    {
                        ForceExternalIO extForce = (ForceExternalIO)f;

                        for (int i = 0; i < Joints.Count; i++)
                        {
                            var jt = Joints[i];
                            ForceExternalIO newForce = new ForceExternalIO(jt.Position, extForce.Force);

                            int[] dof = new int[] { 0, 1, 2 };
                            int[] indicesDoFs = new int[dof.Length];
                            Kernel.RodLinkage.ErodXShellGetDofOffsetForJoint(Model, i, dof, dof.Length, indicesDoFs);
                            newForce.SetIndexMap(0, i, true, indicesDoFs);
                            forces.Add(newForce);
                        }
                    }
                }
            }

            // Check forces with reference positions
            tempForces = allForces.Where(f => f.Positions[0] != Point3d.Unset);
            foreach (var f in tempForces)
            {
                for (int i = 0; i < f.Positions.Length; i++)
                {
                    Point3d p = f.Positions[i];

                    int[] dof = new int[] { 0, 1, 2 };
                    int[] indicesDoFs = new int[dof.Length];

                    int idxJ = Joints.ClosestJoint(p);
                    int idxN = Segments.ClosestNode(p);

                    if (p.DistanceTo(Joints[idxJ].Position) < p.DistanceTo(Segments.GetNode(idxN)))
                    {
                        Kernel.RodLinkage.ErodXShellGetDofOffsetForJoint(Model, idxJ, dof, dof.Length, indicesDoFs);
                        f.SetIndexMap(i, idxJ, true, indicesDoFs);
                    }
                    else
                    {
                        Kernel.RodLinkage.ErodXShellGetDofOffsetForCenterLinePos(Model, idxN, dof, dof.Length, indicesDoFs);
                        f.SetIndexMap(i, idxN, false, indicesDoFs);
                    }
                }
                forces.Add((ForceIO)f.Clone());
            }

            ModelIO.SetForces(forces);
        }

        protected override void Init()
        {
            InitJoints();
            InitSegments();
            InitMaterials();
            InitSupports();
            InitForces();
            InitMesh();
        }

        protected void InitSegments()
        {
            int count = Kernel.RodLinkage.ErodXShellRodGetSegmentsCount(Model);
            Segments = new RodSegmentCollection(Model);
            for (int i = 0; i < count; i++) Segments.Add(new RodSegment(Model, i));
            Segments.UpdateNodePositions();
        }

        protected void InitJoints()
        {
            int count = Kernel.RodLinkage.ErodXShellGetJointsCount(Model);
            Joints = new JointCollection();
            for (int i = 0; i < count; i++) Joints.Add(new Joint(Model, i));
        }

        public override void InitMesh()
        {
            double[] outCoords;
            int[] outQuads;
            GetMeshData(out outCoords, out outQuads);

            MeshVis = Helpers.GetQuadMesh(outCoords, outQuads);
        }

        public override void Update()
        {
            Joints.UpdateJointPositions();
            Segments.UpdateNodePositions();
            UpdateMesh();
        }

        public override string ToString()
        {
            return ModelIO.ModelType.ToString();
        }

        public void WriteJsonFile(string path, string filename)
        {
            // Serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(@path + filename + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }

        public override int[] GetFixedVars(int numDeploymentSteps, int deploymentStep, double step=1.0)
        {
            if (step < 0) step = 0.0;
            if (step > 1) step = 1.0;

            double[] dofs = GetDoFs();
            // Update positions of supports
            for(int i = 0; i < ModelIO.Supports.Count; i++)
            {
                var sp = ModelIO.Supports[i];

                if (!sp.ContainsTarget) continue;
                if (sp.IsTemporary && deploymentStep >= (int)Math.Floor(sp.ReleaseCoefficient * (numDeploymentSteps - 1))) continue;

                // Compute linear interpolation between initial position and target position
                Line ln = new Line(sp.ReferencePosition, sp.TargetPosition);
                var pos = ln.PointAt(step);
                sp.VisualizationPosition = pos;

                // Only update dofs linked with the position
                int[] indicesDoFs = sp.IndicesDoFs;
                for (int j = 0; j < 3; j++) dofs[indicesDoFs[j]] = pos[j];
                ModelIO.Supports[i] = sp;
            }
            SetDoFs(dofs);

            return ModelIO.Supports.GetSupportsDoFsIndices(numDeploymentSteps, deploymentStep);
        }

        public override int[] GetFixedVars(bool includeTemporarySupports, double step = 1.0)
        {
            if (step < 0) step = 0.0;
            if (step > 1) step = 1.0;

            double[] dofs = GetDoFs();
            // Update positions of supports
            for (int i = 0; i < ModelIO.Supports.Count; i++)
            {
                var sp = ModelIO.Supports[i];
                if (!sp.ContainsTarget) continue;

                // Compute linear interpolation between initial position and target position
                Line ln = new Line(sp.ReferencePosition, sp.TargetPosition);
                var pos = ln.PointAt(step);
                sp.VisualizationPosition = pos;

                // Only update dofs linked with the position
                int[] indicesDoFs = sp.IndicesDoFs;
                for (int j = 0; j < 3; j++) dofs[indicesDoFs[j]] = pos[j];
                ModelIO.Supports[i] = sp;
            }
            SetDoFs(dofs);

            return ModelIO.Supports.GetSupportsDoFsIndices(includeTemporarySupports);
        }

        public override Line[] GetCablesAsLines()
        {
            {
                if (_modelIO.Forces.Count == 0) return new Line[0];

                return _modelIO.Forces.Where(f => f.ForceType == ForceIOType.Cable).Select(f => new Line(f.IsJoint[0] ? Joints[f.Indices[0]].Position : Segments.GetNode(f.Indices[0]), f.IsJoint[1] ? Joints[f.Indices[1]].Position : Segments.GetNode(f.Indices[1]))).ToArray();
            }
        }
    }
}
