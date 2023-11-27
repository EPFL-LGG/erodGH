using ErodDataLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ErodData.Data
{
    public class EdgeDataGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Edge class.
        /// </summary>
        public EdgeDataGH()
          : base("Edge-Beam", "Edge-Beam",
              "Edge-beam data.",
              "Erod", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Crv", "Curve or line defining the edge-beam.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Subdivision", "Subdivision", "Number of edges per edge-beam. The minimum number is 5.", GH_ParamAccess.item,10);
            pManager.AddNumberParameter("Length", "Length", "Set the rest length of the edge-beam. If no length is set, the current length is taken.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance to use for checking linearity.", GH_ParamAccess.item, 0.01);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Edge-beam", "E", "Edge-beam data.", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = null;
            double length = 0, tol = 0.01;
            int subd = 10;
            DA.GetData(0, ref crv);
            DA.GetData(1, ref subd);
            DA.GetData(2, ref length);
            DA.GetData(3, ref tol);
            if (length <= 0) length = crv.GetLength();
            if (subd < 5) subd = 5;

            Point3d p1 = crv.PointAtStart;
            Point3d p2 = crv.PointAtEnd;

            SegmentData edge = new SegmentData(p1,p2, crv, length, subd, tol);

            DA.SetData(0, edge);
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
            get { return new Guid("6a4274c5-be91-4bc4-956d-df748b09002e"); }
        }
    }
}