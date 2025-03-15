using ErodDataLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace ErodData.IO
{
    public class SegmentGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Edge class.
        /// </summary>
        public SegmentGH()
          : base("Segment", "Segment",
              "Build a segment of a ribbon from a curve.",
              "Erod", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Crv", "Curve or line defining the segment.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Subdivision", "Subdivision", "Number of edges per segment. The minimum number is 5.", GH_ParamAccess.item,10);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance to use for checking linearity.", GH_ParamAccess.item, 0.01);
            pManager.AddBooleanParameter("IsStraight", "IsStraight", "Remove rest curvatures.", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Segment", "Segment", "Ribbon segment.", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = null;
            double tol = 0.01;
            int subd = 10;
            bool removeKapas = false;
            DA.GetData(0, ref crv);
            DA.GetData(1, ref subd);
            DA.GetData(2, ref tol);
            DA.GetData(3, ref removeKapas);
            if (subd < 5) throw new Exception("Rods must have at least 5 edges (to prevent conflicting start/end joint constraints and fully separate joint influences in Hessian)");

            SegmentIO edge = new SegmentIO(crv, subd, tol, removeKapas);

            DA.SetData(0, edge);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.edge_io;
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