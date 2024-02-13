using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class DebuggerGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public DebuggerGH()
          : base("Debugger", "Debugger",
                "Debug a RodLinkage model.",
                "Erod", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("RestLengths", "RestLengths", "RestLengths.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("OffsetInteriorCoords", "OffsetInteriorCoords", "OffsetInteriorCoords.", GH_ParamAccess.list);
            pManager.AddNumberParameter("InteriorCoords", "InteriorCoords", "InteriorCoords.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Coords", "Coords", "Coords.", GH_ParamAccess.list);
            pManager.AddNumberParameter("EdgesA", "EdgesA", "EdgesA.", GH_ParamAccess.list);
            pManager.AddNumberParameter("EdgesB", "EdgesB", "EdgesB.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("StartJoints", "StartJoints", "StartJoints.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("EndJoints", "EndJoints", "EndJoints.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("SegmentsA", "SegmentsA", "SegmentsA.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("SegmentsB", "SegmentsB", "SegmentsB.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("IsStartA", "IsStartA", "IsStartA.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("IsStartB", "IsStartB", "IsStartB.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("JointForVertex", "JointForVertex", "JointForVertex.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Edges", "Edges", "Edges.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Normals", "Normals", "Normals.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("NumJoints", "NumJoints", "NumJoints.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumVertices", "NumVertices", "NumVertices.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumEdges", "NumEdges", "NumEdges.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkageData data = null;
            DA.GetData(0, ref data);

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


            DA.SetDataList(0, restLengths);
            DA.SetDataList(1, offsetCurvePoints);
            DA.SetDataList(2, curvePoints);
            DA.SetDataList(3, coords);
            DA.SetDataList(4, edgesA);
            DA.SetDataList(5, edgesB);
            DA.SetDataList(6, startJoints);
            DA.SetDataList(7, endJoints);
            DA.SetDataList(8, segmentsA);
            DA.SetDataList(9, segmentsB);
            DA.SetDataList(10, isStartA);
            DA.SetDataList(11, isStartB);
            DA.SetDataList(12, jointForVertex);
            DA.SetDataList(13, edges);
            DA.SetDataList(14, normals);
            DA.SetData(15, numJoints);
            DA.SetData(16, numVertices);
            DA.SetData(17, numEdges);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("831aa7ac-5e42-40d9-90bf-04a5b2fa04c4"); }
        }
    }
}
