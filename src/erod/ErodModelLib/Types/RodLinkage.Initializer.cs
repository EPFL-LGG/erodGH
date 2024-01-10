using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ErodDataLib.Types;
using ErodDataLib.Utils;
using ErodModelLib.Creators;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
    public partial class RodLinkage
    {
        public RodLinkage(RodLinkage model)
        {
            if (model.Model == IntPtr.Zero) throw new Exception("Invalid RodLinkage model");

            if (model.ModelType == ModelTypes.AttractedSurfaceRodLinkage) Model = Kernel.RodLinkage.ErodXShellAttractedSurfaceCopy(model.Model, out Error);
            else Model = Kernel.RodLinkage.ErodXShellCopy(model.Model, out Error);

            if (Model != IntPtr.Zero)
            {
                Init();
                Supports = (int[])model.Supports.Clone();
                TemporarySupports = (int[])model.TemporarySupports.Clone();

                _jointsCloud = new PointCloud(model._jointsCloud);
                _nodesCloud = new PointCloud(model._nodesCloud);
                Layout = new RodLinkageLayout(model.Layout);
                TargetAngle = model.TargetAngle;
                SupportVis = new List<Point3d>(model.SupportVis);
                TemporarySupportVis = new List<Point3d>(model.TemporarySupportVis);

                HomogenousMaterial = new MaterialData(model.HomogenousMaterial);
                RodMaterials = model.RodMaterials;
                ModelType = model.ModelType;

                if (model.Forces.Length != 0)
                {
                    Forces = new double[model.Forces.Length];
                    Array.Copy(model.Forces, Forces, model.Forces.Length);
                }
                InitMesh();
            }
            else
            {
                string errorMsg = Marshal.PtrToStringAnsi(Error);
                throw new Exception(errorMsg);
            }
        }

        public RodLinkage(RodLinkageData data, bool checkConsistentNormals, bool initConsistentAngle, bool edgeDataInitialization = false)
        {
            ///////////////////////////////////////////////////
            ///////////////////////////////////////////////////
            // Parse joint data
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////

            #region Joints
            int numJoints = data.Joints.Count;
            int[] jointForVertex = data.GetJointForVertexMaps();
            int numVertices = data.GetJointForVertexMaps().Length;
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
                JointData joint = data.Joints[i];

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
            #endregion

            ///////////////////////////////////////////////////
            ///////////////////////////////////////////////////
            // Parse edge data
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////
            #region Edges
            int numEdges = data.Segments.Count;
            List<double> curvePoints = new List<double>();
            int[] offsetCurvePoints = new int[numEdges];
            int[] startJoints = new int[numEdges];
            int[] endJoints = new int[numEdges];
            int[] subdivisions = new int[numEdges];
            int[] edges = new int[numEdges * 2];
            double[] restLengths = new double[numEdges];
            int offset = 0;

            for (int i = 0; i < numEdges; i++)
            {
                SegmentData edge = data.Segments[i];

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
                edges[i * 2] = edge.Indexes[0];
                edges[i * 2 + 1] = edge.Indexes[1];
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
                    Model = Kernel.RodLinkage.ErodXShellBuildFromJointData(numVertices, numJoints, numEdges,
                                                                            restLengths, offsetCurvePoints, curvePoints.ToArray(),
                                                                            startJoints, endJoints,
                                                                            coords, normals,
                                                                            edgesA, edgesB,
                                                                            segmentsA, segmentsB,
                                                                            isStartA, isStartB,
                                                                            jointForVertex, edges, data.GetFirstJointVertex(),
                                                                            data.Interleaving, Convert.ToInt32(checkConsistentNormals), Convert.ToInt32(initConsistentAngle), out Error);
                }
                else
                {
                    // For this constructor all rod segments needs to be subdivided equally
                    int subd = subdivisions.Sum() / subdivisions.Length;
                    Model = Kernel.RodLinkage.ErodXShellBuildFromEdgeData(numVertices, numEdges, coords, edges, normals, subd, data.Interleaving, Convert.ToInt32(initConsistentAngle), out Error);
                }

                if (Model != IntPtr.Zero)
                {
                    Init();
                    AddModelData(data);
                }
                else
                {
                    string errorMsg = Marshal.PtrToStringAuto(Error);
                    throw new Exception(errorMsg);
                }
            }
            #endregion
        }

        protected override void Init()
        {
            Supports = new int[0];
            TemporarySupports = new int[0];
            Forces = new double[0];
            ModelType = ModelTypes.RodLinkage;
            SupportVis = new List<Point3d>();
            TemporarySupportVis = new List<Point3d>();
            InitJoints();
            InitSegments();
            InitNodes();
        }

        private void AddModelData(RodLinkageData data)
        {
            AddMaterialData(data);
            AddSupportData(data);
            AddForceData(data);
            InitMesh();
            Layout = new RodLinkageLayout(data.Layout);
            if (data.TargetSurface != null) AddTargetSurface(data.TargetSurface);
        }

        public void AddTargetSurface(TargetSurfaceData data)
        {
            if (data == null || data == default) return;
            double[] inCoords = Helpers.FlattenDoubleArray(data.Vertices);
            int[] inTrias = Helpers.FlattenIntArray(data.Trias);
            Model = Kernel.RodLinkage.ErodXShellAttractedSurfaceBuild(data.Vertices.Length, data.Trias.Length, inCoords, inTrias, Model, data.TargetJointWeight, out Error);
            if (Model == IntPtr.Zero)
            {
                string errorMsg = Marshal.PtrToStringAnsi(Error);
                throw new Exception(errorMsg);
            }
            else
            {
                ModelType = ModelTypes.AttractedSurfaceRodLinkage;
            }
        }

        public override string ToString()
        {
            return ModelType.ToString();
        }
    }
}
