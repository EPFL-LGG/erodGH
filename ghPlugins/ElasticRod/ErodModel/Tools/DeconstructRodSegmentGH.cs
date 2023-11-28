using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Tools
{
    public class RodSegmentGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RodSegmentGH()
          : base("Deconstruct RodSegment", "Deconstruct RodSegment",
            "Deconstruct a RodSegment.",
            "Erod", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Segment", "Segment", "Rod segment.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("CenterLine", "CenterLine", "Center line of the rod segment.", GH_ParamAccess.item);
            pManager.AddPointParameter("Nodes", "Nodes", "Nodes positions.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("StartJoint", "StartJoint", "Index of the joint at the start of the segment (If it exists).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("EndJoint", "EndJoint", "Index of the joint at the end of the segment (If it exists).", GH_ParamAccess.item);
            pManager.AddNumberParameter("RestLengths", "RestLengths", "Rest lengths.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Frames", "Frames", "Material frames (per edge).", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodSegment seg = null;
            DA.GetData(0, ref seg);

            double[] coords = seg.GetCenterLineCoordinates();
            double[] rLengths = seg.GetRestLengths();

            int count = (int)coords.Length / 3;
            Point3d[] pts = new Point3d[count];
            for (int i = 0; i < count; i++)
            {
                pts[i] = new Point3d(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
            }

            PolylineCurve crv = new PolylineCurve(pts);

            int? start = seg.GetStartJoint();
            if (start == -1) start = null;
            int? end = seg.GetEndJoint();
            if (end == -1) end = null;

            Plane[] frames = seg.GetMaterialFames();


            DA.SetData(0, crv);
            DA.SetDataList(1, pts);
            DA.SetData(2, start);
            DA.SetData(3, end);
            DA.SetDataList(4, rLengths);
            DA.SetDataList(5, frames);
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
            get { return new Guid("a1fe1daa-7a1a-4831-8d95-c3659109fec3"); }
        }
    }
}
