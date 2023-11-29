using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class StiffnessesRodLinkageGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StiffnessesRodLinkageGH()
          : base("RodLinkage Stiffnesses", "RodLinkage Stiffnesses",
            "Bending, twisting ans tretching stiffnesses of a RodLinkage.",
            "Erod", "Analysis")
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
            pManager.AddPointParameter("Vertices", "Vertices", "Sample points.", GH_ParamAccess.list);
            pManager.AddPointParameter("EdgeMidPoints", "EdgeMidPoints", "Sample points for edges.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stretching", "Stretching", "Stretching stiffnesses (per edge).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Twisting", "Twisting", "Twisting stiffnesses (per node).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Lambda1", "Lambda1", "Bending stiffnesses (per node).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Lambda2", "Lambda2", "ending stiffnesses (per node).", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage model = null;
            DA.GetData(0, ref model);

            int numRodSegments = model.Segments.Length;
            PointCloud vertices = new PointCloud();
            PointCloud edgeMidPts = new PointCloud();
            List<double> twisting = new List<double>();
            List<double> lambda1 = new List<double>();
            List<double> lambda2 = new List<double>();
            List<double> stretching = new List<double>();
            Point3d p0, p1, mid;

            for (int i = 0; i < numRodSegments; i++)
            {
                RodSegment seg = model.Segments[i];
                double[] coords = seg.GetCenterLineCoordinates();

                double[] tempTwisting = seg.GetTwistingStiffnesses();
                double[] tempLambda1, tempLambda2;
                seg.GetBendingStiffnesses(out tempLambda1, out tempLambda2);
                double[] tempStretching = seg.GetStretchingStiffnesses();

                int numVertices = seg.VerticesCount;
                // Stresses per vertex
                for (int j = 0; j < numVertices; j++)
                {
                    p0 = new Point3d(coords[j * 3], coords[j * 3 + 1], coords[j * 3 + 2]);
                    int idx = vertices.ClosestPoint(p0);
                    if (idx == -1)
                    {
                        vertices.Add(p0);
                        twisting.Add(tempTwisting[j]);
                        lambda1.Add(tempLambda1[j]);
                        lambda2.Add(tempLambda2[j]);
                    }
                    else
                    {
                        if (p0.DistanceTo(vertices[idx].Location) > 0.01)
                        {
                            vertices.Add(p0);
                            twisting.Add(tempTwisting[j]);
                            lambda1.Add(tempLambda1[j]);
                            lambda2.Add(tempLambda2[j]);
                        }
                    }

                    // Stresses per edge
                    if (j < numVertices - 1)
                    {
                        p1 = new Point3d(coords[(j + 1) * 3], coords[(j + 1) * 3 + 1], coords[(j + 1) * 3 + 2]);
                        mid = (p0 + p1) / 2;
                        idx = edgeMidPts.ClosestPoint(mid);
                        if (idx == -1)
                        {
                            edgeMidPts.Add(mid);
                            stretching.Add(tempStretching[j]);
                        }
                        else
                        {
                            if (mid.DistanceTo(edgeMidPts[idx].Location) > 0.01)
                            {
                                edgeMidPts.Add(mid);
                                stretching.Add(tempStretching[j]);
                            }
                        }
                    }
                }
            }

            DA.SetDataList(0, vertices.GetPoints());
            DA.SetDataList(1, edgeMidPts.GetPoints());
            DA.SetDataList(2, stretching);
            DA.SetDataList(3, twisting);
            DA.SetDataList(4, lambda1);
            DA.SetDataList(5, lambda2);
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
            get { return new Guid("b2a9db3d-68f8-491a-a892-4aebec32285f"); }
        }
    }
}
