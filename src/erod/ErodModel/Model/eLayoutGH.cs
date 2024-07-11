using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class eLayoutGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public eLayoutGH()
          : base("eLayout", "eLayout",
            "Extract the layout of ribbon families (if they exist) from an elastic linkage",
            "Erod", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Linkage model to extract ribbon layout.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("RodsA", "RodsA", "Indices of rod segments belonging to family A. Each branch of the tree represents a ribbon.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("RodsB", "RodsB", "Indices of rod segments belonging to family B. Each branch of the tree represents a ribbon.", GH_ParamAccess.tree);
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

            if (model.ModelIO.Layout.ContainsLayoutData())
            {
                GH_Structure<GH_Integer> rodA = new GH_Structure<GH_Integer>();
                foreach(int key in model.ModelIO.Layout.RibbonsFamilyA.Keys)
                {
                    foreach(int idx in model.ModelIO.Layout.RibbonsFamilyA[key])
                    {
                        rodA.Append(new GH_Integer(idx), new GH_Path(key));
                    }
                }

                GH_Structure<GH_Integer> rodB = new GH_Structure<GH_Integer>();
                foreach (int key in model.ModelIO.Layout.RibbonsFamilyB.Keys)
                {
                    foreach (int idx in model.ModelIO.Layout.RibbonsFamilyB[key])
                    {
                        rodB.Append(new GH_Integer(idx), new GH_Path(key));
                    }
                }

                DA.SetDataTree(0, rodA);
                DA.SetDataTree(1, rodB);
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.linkage_layout;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ae1ff8a6-40e8-44a9-ad65-3c323637374e"); }
        }
    }
}
