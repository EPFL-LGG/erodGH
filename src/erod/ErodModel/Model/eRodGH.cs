using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class eRodGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public eRodGH()
          : base("eRod", "eRod",
            "Deconstruct an elastic rod or a rod segment from a linkage.",
            "Erod", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rod", "Rod", "An elastic rod or a rod segment from a linkage.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "3D mesh representing the rod segment.", GH_ParamAccess.item);
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
            object seg = null;
            DA.GetData(0, ref seg);

            PolylineCurve crv = null;
            Point3d[] pts = null;
            int? start = null, end = null;
            double[] rLengths = null;
            Plane[] frames = null;
            Mesh mesh = null;

            // Rod segment
            if (seg is RodSegment)
            {
                RodSegment rod = (RodSegment)seg;
                double[] coords = rod.GetCenterLineCoordinates();
                rLengths = rod.GetRestLengths();

                int count = (int)coords.Length / 3;
                pts = new Point3d[count];
                for (int i = 0; i < count; i++)
                {
                    pts[i] = new Point3d(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
                }

                crv = new PolylineCurve(pts);

                start = rod.GetStartJoint();
                if (start == -1) start = null;
                end = rod.GetEndJoint();
                if (end == -1) end = null;

                frames = rod.GetMaterialFames();
                mesh = rod.GetMesh();
            }
            else if (seg is ElasticRod)
            {
                ElasticRod rod = (ElasticRod)seg;
                double[] coords = rod.GetCenterLineCoordinates();
                rLengths = rod.GetRestLengths();

                int count = (int)coords.Length / 3;
                pts = new Point3d[count];
                for (int i = 0; i < count; i++) pts[i] = new Point3d(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);

                crv = new PolylineCurve(pts);
                frames = rod.GetMaterialFames();
                mesh = rod.GetMesh();
            }
            else throw new Exception("Invalid input type. The type should be an elastic rod or a rod segment of an elastic linkage.");

            DA.SetData(0, mesh);
            DA.SetData(1, crv);
            DA.SetDataList(2, pts);
            DA.SetData(3, start);
            DA.SetData(4, end);
            DA.SetDataList(5, rLengths);
            DA.SetDataList(6, frames);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.deconstruct_rodSegment;
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
