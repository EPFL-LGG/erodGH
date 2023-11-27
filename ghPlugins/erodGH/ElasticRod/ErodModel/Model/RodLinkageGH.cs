using System;
using ErodDataLib.Types;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace ErodModel.Model
{
    public class XShellModelGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public XShellModelGH()
          : base("RodLinkage", "RodLinkage",
            "Build an RodLinkage model.",
            "Erod", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "Data", "RodLinkage data.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ConsistentNormals", "CN", "Check normals direction to prevent 180 degree twists. When this flag is set to TRUE the direction of normals might change from the input (if normals were given).", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ConsistentAngle", "CA", "Initialize with consistent angles.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("EdgeData", "ED", "Initialize with edge data.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkageData data = null;
            bool initConsistentAngle = true, checkConsistentNormals=true, initEdgeData = false;
            if (!DA.GetData(0, ref data)) return;
            DA.GetData(1, ref checkConsistentNormals);
            DA.GetData(2, ref initConsistentAngle);
            DA.GetData(3, ref initEdgeData);
                
            RodLinkage model = new RodLinkage(data, checkConsistentNormals, initConsistentAngle, initEdgeData);

            DA.SetData(0, model);
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
            get { return new Guid("052e7a91-55a4-4edf-b453-889a2a062456"); }
        }
    }
}
