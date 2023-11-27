using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class EnergiesRodSegmentGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public EnergiesRodSegmentGH()
          : base("RodSegment Energies", "RodSegment Energies",
            "RodSegment energies.",
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
            pManager.AddNumberParameter("Energy", "Energy", "Total energy of the model.", GH_ParamAccess.item);
            pManager.AddNumberParameter("BendEnergy", "BendEnergy", "Bending energy.", GH_ParamAccess.item);
            pManager.AddNumberParameter("StretchEnergy", "StretchEnergy", "Stretching energy.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Twist", "TwistEnergy", "Twisting energy.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodSegment model = null;
            DA.GetData(0, ref model);

            double energy = model.GetEnergy();
            double bend = model.GetBendingEnergy();
            double stretch = model.GetStretchingEnergy();
            double twist = model.GetTwistingEnergy();

            DA.SetData(0, energy);
            DA.SetData(1, bend);
            DA.SetData(2, stretch);
            DA.SetData(3, twist);
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
            get { return new Guid("0e8a81b3-058a-4a5b-abfb-370252373cdf"); }
        }
    }
}
