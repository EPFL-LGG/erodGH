using System;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ErodModel.Analysis
{
    public class QuadDistributionGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public QuadDistributionGH()
          : base("Quad-Area Distribution", "Q-Area Distribution",
                "Quad area distribution of a RodLinkage.",
                "Erod", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("UseAspectRatio", "UseAspectRatio", "Use quads aspect ratio", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("ShowLaplacian", "ShowLaplacian", "Show the difference of the quad area with its neighbors", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Data", "Data", "Quad areas/aspect-ratio.", GH_ParamAccess.list);
            pManager.AddPointParameter("Centroids", "Centroids", "Quad centroids.", GH_ParamAccess.list);
            pManager.AddGenericParameter("QuadData", "QuadData", "Quad data visualization.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage linkage = null;
            bool show = false, useAspect = true, showLap = false;
            if (!DA.GetData(0, ref linkage)) return;
            DA.GetData(1, ref useAspect);
            DA.GetData(2, ref showLap);
            DA.GetData(3, ref show);

            int numSeg = linkage.Segments.Count();
            Curve[] edges = new Curve[numSeg];
            for (int i = 0; i < numSeg; i++)
            {
                int idx0 = linkage.Segments[i].GetStartJoint();
                int idx1 = linkage.Segments[i].GetEndJoint();

                double[] p0 = linkage.Joints[idx0].GetPosition();
                double[] p1 = linkage.Joints[idx1].GetPosition();

                Point3d pos0 = new Point3d(p0[0], p0[1], p0[2]);
                Point3d pos1 = new Point3d(p1[0], p1[1], p1[2]);

                edges[i] = new LineCurve(pos0, pos1);
            }

            QuadAreas quadMesh = new QuadAreas(edges, useAspect, showLap);
            double[] areas;

            if (showLap) areas = quadMesh.DataLaplacian;
            else areas = quadMesh.Data;
            
            if (show) GraphPlotter.HistogramAreas(areas, useAspect);

            DA.SetDataList(0, areas);
            DA.SetDataList(1, quadMesh.Centroids);
            DA.SetData(2, quadMesh);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
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
            get { return new Guid("da343203-fc38-42fc-a61f-8913c95cc411"); }
        }
    }
}

