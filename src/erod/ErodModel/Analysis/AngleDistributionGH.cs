using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Commands;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class AngleDistributionGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public AngleDistributionGH()
          : base("Angle Distribution", "Angle Distribution",
                "Angle distribution of a RodLinkage.",
                "Erod", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Angles", "Angles", "Joint angles in radians.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage linkage = null;
            bool show = false;
            if (!DA.GetData(0, ref linkage)) return;
            DA.GetData(1, ref show);


            if (show)
            {
                double[] angles = new double[0];
                GraphPlotter.HistogramAngles("Angle Distribution", angles);
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
            get { return new Guid("5ce8f3c6-e3b7-4625-9b7c-b4b9ca3cec3f"); }
        }
    }
}