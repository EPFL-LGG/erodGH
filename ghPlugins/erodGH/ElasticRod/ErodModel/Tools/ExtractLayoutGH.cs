using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodModel.Tools
{
    public class ExtractLayoutGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExtractLayoutGH()
          : base("LinkageLayout", "LinkageLayout",
            "Extract the rod layout (if it exists) from a RodLinkage.",
            "Erod", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model to extract the layout.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("RodA", "RodA", "RodSegments of family A.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("RodB", "RodB", "RodSegments of family B.", GH_ParamAccess.tree);
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

            if (model.Layout.ContainsLayoutData())
            {
                GH_Structure<GH_Integer> rodA = new GH_Structure<GH_Integer>();
                foreach(int key in model.Layout.SplineBeamsA.Keys)
                {
                    foreach(int idx in model.Layout.SplineBeamsA[key])
                    {
                        rodA.Append(new GH_Integer(idx), new GH_Path(key));
                    }
                }

                GH_Structure<GH_Integer> rodB = new GH_Structure<GH_Integer>();
                foreach (int key in model.Layout.SplineBeamsB.Keys)
                {
                    foreach (int idx in model.Layout.SplineBeamsB[key])
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
            get { return new Guid("ae1ff8a6-40e8-44a9-ad65-3c323637374e"); }
        }
    }
}
