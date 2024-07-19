using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class EnergiesGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public EnergiesGH()
          : base("Energies", "Energies",
            "Computes the energies of an elastic rod or linkage.",
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
            pManager.AddNumberParameter("Total", "Total", "Total elastic energy of the linkage.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending", "Bending", "Bending energy.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stretching", "Stretching", "Stretching energy.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Twisting", "Twisting", "Twisting energy.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Strain", "Strain", "Maximum strain.", GH_ParamAccess.item);
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

            double total=0, bend=0, stretch=0, twist=0, strain=0;
            // Linkage
            if (obj is RodLinkage)
            {
                RodLinkage model = (RodLinkage)obj;
                total = model.GetEnergy();
                bend = model.GetBendingEnergy();
                stretch = model.GetStretchingEnergy();
                twist = model.GetTwistingEnergy();
                strain = model.GetMaxStrain();
            }
            else if (obj is ElasticRod)
            {
                ElasticRod model = (ElasticRod)obj;

                total = model.GetEnergy();
                bend = model.GetBendingEnergy();
                stretch = model.GetStretchingEnergy();
                twist = model.GetTwistingEnergy();
                strain = model.GetMaxStrain();
            }
            else if (obj is RodSegment)
            {
                RodSegment model = (RodSegment) obj;

                total = model.GetEnergy();
                bend = model.GetBendingEnergy();
                stretch = model.GetStretchingEnergy();
                twist = model.GetTwistingEnergy();
                strain = model.GetMaxStrain();
            }
            else throw new Exception("Invalid input type. The type should be an elastic rod, a rod segment of an elastic linkage or an elastic linkage.");

            DA.SetData(0, total);
            DA.SetData(1, bend);
            DA.SetData(2, stretch);
            DA.SetData(3, twist);
            DA.SetData(4, strain);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.energies;
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
