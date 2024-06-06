using ErodDataLib.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace ErodData.Data
{
    public class UnaryForceDataGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ForceVectorGH class.
        /// </summary>
        public UnaryForceDataGH()
          : base("Force", "Force",
              "External force.",
              "Erod", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Pt", "Position of the joint.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vector", "Vec", "Force vector.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Force", "F", "Force data.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d pos = new Point3d();
            Vector3d vec = new Vector3d();
            DA.GetData(0, ref pos);
            DA.GetData(1, ref vec);

            UnaryForceData force = new UnaryForceData(pos);
            force.Vector = vec;

            DA.SetData(0, force);
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
            get { return new Guid("b7be44f5-2904-4b7f-a1a8-d1acebb1919b"); }
        }
    }
}