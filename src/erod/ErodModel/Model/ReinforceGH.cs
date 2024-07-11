using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class ReinforceGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ReinforceGH()
          : base("Reinforce", "Reinforce",
            "Adjust the bending and twisting stiffness within specified regions.",
            "Erod", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Elastic linkage model to modify.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor", "Factor", "Scaling factor for adjusting stiffnesses.", GH_ParamAccess.item, 1.0);
            pManager.AddBoxParameter("Boxes", "Boxes", "Boxes defining the areas to reinforce.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Modified elastic linkage model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage model = null;
            double factor = 1.0;
            List<Box> boxes = new List<Box>();
            DA.GetData(0, ref model);
            DA.GetData(1, ref factor);
            DA.GetDataList(2, boxes);

            model.AddStiffenRegion(boxes.ToArray(), factor);

            DA.SetData(0, model);
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
                return Properties.Resources.Resources.stiffen_area;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("420fc29a-baa6-4a5b-bb07-2ca103041537"); }
        }
    }
}
