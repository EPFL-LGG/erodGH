using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace ErodModel.Analysis
{
    public class RestLengthsGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RestLengthsGH()
          : base("RestLengths", "RestLengths",
            "Compute the minimum, maximum, average and total rest-lengths of the linkage.",
            "Erod", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Linkage model to analyse.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("MinimumLength", "MinLength", "Minimum rest length.", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaximumLength", "MaxLength", "Maximum rest length.", GH_ParamAccess.item);
            pManager.AddNumberParameter("AverageLength", "AvrLength", "Average rest length.", GH_ParamAccess.item);
            pManager.AddNumberParameter("TotalLength", "TotalLength", "Total rest lengths.", GH_ParamAccess.item);
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

            double[] lengths = model.GetPerSegmentRestLenghts();
            double minL = lengths.Min();
            double maxL = lengths.Max();
            double avrL = lengths.Average();
            double total = model.GetTotalRestLengths();

            DA.SetData(0, minL);
            DA.SetData(1, maxL);
            DA.SetData(2, avrL);
            DA.SetData(3, total);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.rest_lengths;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b0380d0b-2243-458d-94d4-aea22284a077"); }
        }
    }
}
