using System;
using ErodDataLib.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;

namespace ErodData.IO
{
    public class TargetSupportGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SupportGH class.
        /// </summary>
        public TargetSupportGH()
          : base("TargetSupport", "TargetSupport",
              "Set support conditions using a reference and target point.",
              "Erod", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Pt", "Reference position to set support conditions.", GH_ParamAccess.item);
            pManager.AddPointParameter("Target", "Target", "Target position to set support conditions..", GH_ParamAccess.item);
            pManager.AddBooleanParameter("IsTemporary", "IsTemp", "Set this support as temporary. [IMPORTANT] Temporary supports are only considered for deployment solvers. Equilibrium solve disables temporary supports.", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "Support", "Rolling support conditions.", GH_ParamAccess.item);
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

            SupportIO support = new SupportIO(pos,isTemp, target);
            support.FixTranslation(true);
            support.FixRotation(false);

            DA.SetData(0, support);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.rolling_support;
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

