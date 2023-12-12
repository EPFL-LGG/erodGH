using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class StressesRodSegmentGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StressesRodSegmentGH()
          : base("RodSegment Stresses ", "RodSegment Stresses ",
            "Stress analysis of a RodSegment",
            "Erod", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Segment", "Segment", "Rod segment.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
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
            RodSegment seg = null;
            DA.GetData(0, ref seg);

            double[] stretch = seg.GetStretchingStresses();
            double[] twist = seg.GetTwistingStresses();
            double[] maxStress = seg.GetMaxBendingStresses();
            double[] minStress = seg.GetMinBendingStresses();
            double[] sqrtBend = seg.GetSqrtBendingEnergies();

            DA.SetDataList(0, stretch);
            DA.SetDataList(1, twist);
            DA.SetDataList(2, maxStress);
            DA.SetDataList(3, minStress);
            DA.SetDataList(4, sqrtBend);
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
            get { return new Guid("84a0301b-4e85-413b-a568-ea59e02df0f8"); }
        }
    }
}
