using ErodDataLib.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ErodData.IO
{
    public class SupportGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SupportGH class.
        /// </summary>
        public SupportGH()
          : base("Support", "Support",
              "Set support condition using a reference point.",
              "Erod", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Pt", "Reference position to set support conditions.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("X", "X", "Fix translation along the X-axis.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Y", "Y", "Fix translation along the Y-axis.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Z", "Z", "Fix translation along the Z-axis.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("IsTemporary", "IsTemporary", "Set a temporary support.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("ReleaseCoefficient", "ReleaseCoef", "The release coefficient is a numerical factor that determines how supports are released throughout the deployment process. It multiplies the number of deployment steps to determine the specific step at which a support is released. The coefficient value should be between 0 and 1, where 0 means a support is released at the very beginning of the deployment, and 1 means a support is released at the final step.", GH_ParamAccess.item, 0.5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "Support", "Support conditions.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d pos = new Point3d();
            bool[] flags = new bool[3];
            bool isTemp = false;
            double rCoef = 0.5;
            DA.GetData(0, ref pos);
            DA.GetData(1, ref flags[0]);
            DA.GetData(2, ref flags[1]);
            DA.GetData(3, ref flags[2]);
            DA.GetData(4, ref isTemp);
            DA.GetData(5, ref rCoef);

            SupportIO support = new SupportIO(pos);
            if(isTemp) support.SetTemporarySupport(rCoef);
            support.FixTranslation(flags);

            DA.SetData(0, support);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.support;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2a14f942-9167-4f86-9d1c-67236ea1d7b3"); }
        }
    }
}