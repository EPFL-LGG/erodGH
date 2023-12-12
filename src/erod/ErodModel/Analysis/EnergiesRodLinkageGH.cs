using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class RodLinkageEnergiesGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RodLinkageEnergiesGH()
          : base("RodLinkage Energies", "RodLinkage Energies",
            "RodLinkage energies.",
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
            pManager.AddNumberParameter("Strain", "Strain", "Maximum strain.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Rod Energy", "Max Rod Energy", "Maximum elastic energy stored in any individual rod.", GH_ParamAccess.item);
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

            double energy = model.GetEnergy();
            double bend = model.GetBendingEnergy();
            double stretch = model.GetStretchingEnergy();
            double twist = model.GetTwistingEnergy();
            double strain = model.GetMaxStrain();

            DA.SetData(0, energy);
            DA.SetData(1, bend);
            DA.SetData(2, stretch);
            DA.SetData(3, twist);
            DA.SetData(4, strain);
            DA.SetData(5, model.GetMaxRodEnergy());
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
            get { return new Guid("55bec234-f943-475d-9ace-604dee580373"); }
        }
    }
}
