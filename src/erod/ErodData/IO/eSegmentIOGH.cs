using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;

namespace ErodData.IO
{
    public class DeconstructEdgeGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public DeconstructEdgeGH()
          : base("eRodIO", "eRodIO",
            "Deconstruct an elastic rod or a rod segment from a linkage.",
            "Erod", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RodIO", "RodIO", "An IO elastic rod or a rod segment from a linkage to deconstruct.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes", "Nodes", "Interior nodes.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("StartJoint", "StartJoint", "Joint index at the start of the edge-beam. For terminal edges without a joint at the start, the index is equal to -1.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("EndJoint", "EndJoint", "Joint index at the end of the edge-beam. For terminal edges without a joint at the end, the index is equal to -1.", GH_ParamAccess.item);
            pManager.AddVectorParameter("StartVector", "StartVector", "Vector defining the orientation at the start of the edge-beam.", GH_ParamAccess.item);
            pManager.AddVectorParameter("EndVector", "EndVector", "Vector defining the orientation at the end of the edge-beam.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "Length", "Rest length of the edge-beam", GH_ParamAccess.item);
            pManager.AddTextParameter("Label", "Label", "Label of the edge-beam.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SplineBeam Index", "SIdx", "Global index of the spline-beam containing this edge-beam. -1 if the edge is Undefined.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("EdgeBeam Index", "EIdx", "Local index of the edge-beam within the spline-beam. -1 if the edge is Undefined.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object seg = null;
            DA.GetData(0, ref seg);

            Point3d[] pts = null;
            int? start = null, end = null;
            Vector3d startVector = Vector3d.Unset, endVector = Vector3d.Unset;
            double restLength = 0;
            string edgeLabel = SegmentLabels.Undefined.ToString();
            int ribbonIndex = -1, segmentIndexInRibbon = -1;

            if (seg is SegmentIO)
            {
                SegmentIO eData = (SegmentIO)seg;

                pts = eData.CurvePoints;
                start = eData.StartJoint;
                if (start == -1) start = null;
                end = eData.EndJoint;
                if (end == -1) end = null;

                startVector = eData.GetStartVector();
                endVector = eData.GetEndVector();

                restLength = eData.RestLength;
                edgeLabel = eData.EdgeLabel.ToString();

                ribbonIndex = eData.RibbonIndex;
                segmentIndexInRibbon = eData.SegmentIndexInRibbon;
            }
            else if (seg is RodIO)
            {
                RodIO rod = (RodIO)seg;
                double[] coords = rod.GetCenterLineCoordinates();
                restLength = rod.RestLength;

                int count = (int)coords.Length / 3;
                pts = new Point3d[count];
                for (int i = 0; i < count; i++) pts[i] = new Point3d(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);

                startVector = pts[1] - pts[0];
                endVector = pts[pts.Length - 1] - pts[pts.Length - 2];
            }
            else throw new Exception("Invalid input type. The type should be an IO elastic rod or rod segment of a linkage.");

            DA.SetDataList(0, pts);
            DA.SetData(1, start);
            DA.SetData(2, end);
            DA.SetData(3, startVector);
            DA.SetData(4, endVector);
            DA.SetData(5, restLength);
            DA.SetData(6, edgeLabel);
            DA.SetData(7, ribbonIndex);
            DA.SetData(8, segmentIndexInRibbon);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.edge_io_deconstruct;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("211e1ed1-b34d-494c-9286-3d6962abb836"); }
        }
    }
}
