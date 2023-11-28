using ErodDataLib.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ErodData.Data
{
    public class SupportGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SupportGH class.
        /// </summary>
        public SupportGH()
          : base("Support", "Support",
              "Define conditions.",
              "Erod", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Pt", "Position of the joint.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("X", "X", "Fix translation along the X-axis.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Y", "Y", "Fix translation along the Y-axis.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Z", "Z", "Fix translation along the Z-axis.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("XX", "XX", "Fix rotation along the XX-axis.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("YY", "YY", "Fix rotation along the YY-axis.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ZZ", "ZZ", "Fix rotation along the ZZ-axis.", GH_ParamAccess.item, true);
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
            Point3d pos = new Point3d();
            bool[] x = new bool[6];
            bool isTemp = false;
            DA.GetData(0, ref pos);
            DA.GetData(1, ref x[0]);
            DA.GetData(2, ref x[1]);
            DA.GetData(3, ref x[2]);
            DA.GetData(4, ref x[3]);
            DA.GetData(5, ref x[4]);
            DA.GetData(6, ref x[5]);
            DA.GetData(7, ref isTemp);

            List<int> dof = new List<int>();
            for(int i=0; i<6; i++)
            {
                if (x[i])
                {
                    dof.Add(i);
                }
            }

            SupportData support = new SupportData(pos, dof.ToArray(), isTemp);

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
            get { return new Guid("2a14f942-9167-4f86-9d1c-67236ea1d7b3"); }
        }
    }
}