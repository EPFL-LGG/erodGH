using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class RodLinkageStressesGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RodLinkageStressesGH()
          : base("RodLinkage Stresses ", "RodLinkage Stresses ",
            "Stress analysis of a RodLinkage.",
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
            pManager.AddNumberParameter("Stretching", "Stretching", "Stretching stresses (per edge).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Twisting", "Twisting", "Twisting stresses (per node).", GH_ParamAccess.list);
            pManager.AddNumberParameter("MaxBend", "MaxBend", "Maximum bending stresses (per node).", GH_ParamAccess.list);
            pManager.AddNumberParameter("MinBend", "MinBend", "Minimum bending stresses (per node).", GH_ParamAccess.list);
            pManager.AddNumberParameter("SqrtBend", "SqrtBend", "Sqrt bending energies (per node).", GH_ParamAccess.list);
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
            List<double> maxBend = new List<double>();
            List<double> minBend = new List<double>();
            List<double> sqrtBend = new List<double>();
            List<double> stretching = new List<double>();
            Point3d p0, p1, mid;
            for (int i = 0; i < numRodSegments; i++)
            {
                RodSegment seg = model.Segments[i];
                double[] coords = seg.GetCenterLineCoordinates();

                double[] tempTwisting = seg.GetTwistingStresses();
                double[] tempSqrtBend = seg.GetSqrtBendingEnergies();
                double[] tempMaxBend = seg.GetMaxBendingStresses();
                double[] tempMinBend = seg.GetMinBendingStresses();
                double[] tempStretching = seg.GetStretchingStresses();

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
                        maxBend.Add(tempMaxBend[j]);
                        minBend.Add(tempMinBend[j]);
                        sqrtBend.Add(tempSqrtBend[j]);
                    }
                    else
                    {
                        if (p0.DistanceTo(vertices[idx].Location) > 0.01)
                        {
                            vertices.Add(p0);
                            twisting.Add(tempTwisting[j]);
                            maxBend.Add(tempMaxBend[j]);
                            minBend.Add(tempMinBend[j]);
                            sqrtBend.Add(tempSqrtBend[j]);
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
            DA.SetDataList(4, maxBend);
            DA.SetDataList(5, minBend);
            DA.SetDataList(6, sqrtBend);
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
            get { return new Guid("0a3ebfca-86a5-457e-95ce-94417289b6aa"); }
        }
    }
}
