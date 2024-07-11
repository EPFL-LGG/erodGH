using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class StiffnessesGH : GH_Component
    {
        private PointCloud vertices, edgeMidPts;
        List<double> twisting, lambda1, lambda2, stretching;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StiffnessesGH()
          : base("Stiffnesses", "Stiffnesses",
            "Computes the stiffness of an elastic rod or linkage",
            "Erod", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Input the model to compute energies. The model should be either an elastic rod, a rod segment of a linkage or an elastic linkage.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes", "Nodes", "Sample points where twisting and bending stiffness are measured.", GH_ParamAccess.list);
            pManager.AddPointParameter("EdgeNodes", "EdgeNodes", "Sample points where stretching stiffness is measured.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stretching", "Stretching", "Stretching stiffnesses [per edge].", GH_ParamAccess.list);
            pManager.AddNumberParameter("Twisting", "Twisting", "Twisting stiffnesses [per node].", GH_ParamAccess.list);
            pManager.AddNumberParameter("EIx", "EIx", "Bending stiffness about the major axis [per node].", GH_ParamAccess.list);
            pManager.AddNumberParameter("EIy", "EIy", "Bending stiffness about the minor axis [per node].", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object obj = null;
            DA.GetData(0, ref obj);

            vertices = new PointCloud();
            edgeMidPts = new PointCloud();
            twisting = new List<double>();
            lambda1 = new List<double>();
            lambda2 = new List<double>();
            stretching = new List<double>();

            if (obj is RodLinkage)
            {
                RodLinkage model = null;

                int numRodSegments = model.Segments.Count;

                for (int i = 0; i < numRodSegments; i++)
                {
                    RodSegment seg = model.Segments[i];
                    double[] coords = seg.GetCenterLineCoordinates();
                    int numVertices = seg.VerticesCount;

                    double[] tempTwisting = seg.GetTwistingStiffnesses();
                    double[] tempLambda1, tempLambda2;
                    seg.GetBendingStiffnesses(out tempLambda1, out tempLambda2);
                    double[] tempStretching = seg.GetStretchingStiffnesses();

                    CollectStiffnesses(numVertices, coords, tempTwisting, tempLambda1, tempLambda2, tempStretching);
                }
            }
            else if (obj is RodSegment)
            {
                RodSegment seg = (RodSegment)obj;
                double[] coords = seg.GetCenterLineCoordinates();
                int numVertices = seg.VerticesCount;

                double[] tempTwisting = seg.GetTwistingStiffnesses();
                double[] tempLambda1, tempLambda2;
                seg.GetBendingStiffnesses(out tempLambda1, out tempLambda2);
                double[] tempStretching = seg.GetStretchingStiffnesses();

                CollectStiffnesses(numVertices, coords, tempTwisting, tempLambda1, tempLambda2, tempStretching);
            }
            else if (obj is ElasticRod)
            {
                ElasticRod seg = (ElasticRod) obj;
                double[] coords = seg.GetCenterLineCoordinates();
                int numVertices = coords.Length / 3;

                double[] tempTwisting = seg.GetTwistingStiffnesses();
                double[] tempLambda1, tempLambda2;
                seg.GetBendingStiffnesses(out tempLambda1, out tempLambda2);
                double[] tempStretching = seg.GetStretchingStiffnesses();

                CollectStiffnesses(numVertices, coords, tempTwisting, tempLambda1, tempLambda2, tempStretching);
            }
            else throw new Exception("Invalid input type. The type should be an elastic rod, a rod segment of an elastic linkage or an elastic linkage.");

            DA.SetDataList(0, vertices.GetPoints());
            DA.SetDataList(1, edgeMidPts.GetPoints());
            DA.SetDataList(2, stretching);
            DA.SetDataList(3, twisting);
            DA.SetDataList(4, lambda1);
            DA.SetDataList(5, lambda2);
        }

        private void CollectStiffnesses(int numVertices, double[] coords, double[] tempTwisting, double[] tempLambda1, double[] tempLambda2, double[] tempStretching)
        {
            // Stresses per vertex
            for (int j = 0; j < numVertices; j++)
            {
                Point3d p0 = new Point3d(coords[j * 3], coords[j * 3 + 1], coords[j * 3 + 2]);
                int idx = vertices.ClosestPoint(p0);
                if (idx == -1 || p0.DistanceTo(vertices[idx].Location) > 0.01)
                {
                    vertices.Add(p0);
                    twisting.Add(tempTwisting[j]);
                    lambda1.Add(tempLambda1[j]);
                    lambda2.Add(tempLambda2[j]);
                }

                // Stresses per edge
                if (j < numVertices - 1)
                {
                    Point3d p1 = new Point3d(coords[(j + 1) * 3], coords[(j + 1) * 3 + 1], coords[(j + 1) * 3 + 2]);
                    Point3d mid = (p0 + p1) / 2;
                    idx = edgeMidPts.ClosestPoint(mid);
                    if (idx == -1 || mid.DistanceTo(edgeMidPts[idx].Location) > 0.01)
                    {
                        edgeMidPts.Add(mid);
                        stretching.Add(tempStretching[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.stiffnesses;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b2a9db3d-68f8-491a-a892-4aebec32285f"); }
        }
    }
}
