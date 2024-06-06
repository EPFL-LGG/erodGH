using System;
using ErodDataLib.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;

namespace ErodData.Data
{
    public class SupportWithTargetGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SupportGH class.
        /// </summary>
        public SupportWithTargetGH()
          : base("TargetSupport", "TargetSupport",
              "Define support condition with target position.",
              "Erod", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Pt", "Position of the joint.", GH_ParamAccess.item);
            pManager.AddPointParameter("Target", "Target", "Target position of the joint.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Temporary", "Temp", "Set a temporary support.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "S", "Curved X-Shell support data.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d pos = new Point3d(), target = new Point3d();
            bool[] x = new bool[3];
            bool isTemp = false;
            DA.GetData(0, ref pos);
            DA.GetData(1, ref target);
            DA.GetData(2, ref isTemp);

            SupportData support = new SupportData(pos, new int[] { 0,1,2 }, isTemp);
            support.TargetPosition = target;

            DA.SetData(0, support);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("43605b4e-1043-4658-b8cf-70cf17493d19"); }
        }
    }
}

