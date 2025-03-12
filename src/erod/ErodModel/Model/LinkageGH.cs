using System;
using ErodDataLib.Types;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace ErodModel.Model
{
    public class LinkageGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public LinkageGH()
          : base("Linkage", "Linkage",
              "Construct an elastic linkage.",
              "Erod", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("LinkageIO", "LinkageIO", "Input data to construct an elastic linkage.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ConsistentNormals", "CN", "Check normals direction to prevent 180 degree twists. When this flag is set to TRUE the direction of normals might change from the input (if normals were given).", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ConsistentAngle", "CA", "Initialize with consistent angles. This option is always false when interleaving is weaving o triaxial-weave", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("EdgeData", "ED", "Initialize with simple edge graph data. This flag forces all beams to be straight.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Elastic linkage model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            LinkageIO data = null;
            bool initConsistentAngle = true, checkConsistentNormals=true, initEdgeData = false;
            if (!DA.GetData(0, ref data)) return;
            DA.GetData(1, ref checkConsistentNormals);
            DA.GetData(2, ref initConsistentAngle);
            DA.GetData(3, ref initEdgeData);

            if (data.Supports.GetNumberFixSupport() == 0) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Blank, "Only temporary supports have been found. The first temporary support is converted to a permanent support.");
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
                return Properties.Resources.Resources.linkage;
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
