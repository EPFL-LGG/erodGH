using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace ErodModel.Analysis
{
    public class AnglesGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public AnglesGH()
          : base("Angles", "Angles",
            "Compute the minimum, maximum, and average joint angles of the linkage.",
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
            pManager.AddNumberParameter("MinimumAngle", "MinAng", "Minimum joint angle in degrees.", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaximumAngle", "MaxAng", "Maximum joint angle in degrees.", GH_ParamAccess.item);
            pManager.AddNumberParameter("AverageAngle", "AvrAng", "Average joint angle in degrees.", GH_ParamAccess.item);
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

            double[] angles = model.GetJointAngles();
            double minAng = 180 / Math.PI * angles.Min();
            double maxAng = 180 / Math.PI * angles.Max();
            double avrAng = 180 / Math.PI * model.GetAverageJointAngle();

            DA.SetData(0, minAng);
            DA.SetData(1, maxAng);
            DA.SetData(2, avrAng);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.angles;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("71062e3b-f99c-4009-b30e-afbbe6485378"); }
        }
    }
}
