using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Tools
{
    public class JointGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public JointGH()
          : base("Deconstruct Joint", "Deconstruct Joint",
            "Deconstruct a joint.",
            "Erod", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint", "Joint", "Joint.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "Position", "Position of the joint", GH_ParamAccess.item);
            pManager.AddVectorParameter("Normal", "Normal", "Joint normal", GH_ParamAccess.item);
            pManager.AddVectorParameter("EdgeVectorA", "EdgeA", "Edge vector ascoiated with rod label A.", GH_ParamAccess.item);
            pManager.AddVectorParameter("EdgeVectorB", "EdgeB", "Edge vector ascoiated with rod label B.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SegmentsA", "SegmentsA", "Segments associated with rod label A", GH_ParamAccess.list);
            pManager.AddIntegerParameter("SegmentsB", "SegmentsB", "Segments associated with rod label B", GH_ParamAccess.list);
            pManager.AddBooleanParameter("StartA", "StartA", "True if this joint is the origin of the corresponding segment A.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("StartB", "StartB", "True if this joint is the origin of the corresponding segment B.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Angle","Ang","Joint opening angle (in radians).",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Joint j = null;
            DA.GetData(0, ref j);

            double[] p = j.GetPosition();
            Point3d pos = new Point3d(p[0], p[1], p[2]);
            double[] n = j.GetNormal();
            Vector3d norm = new Vector3d(n[0], n[1], n[2]);
            int[] segA, segB;
            j.GetConnectedSegments(out segA, out segB);
            double[] eA = j.GetEdgeVecA();
            Vector3d eVecA = new Vector3d(eA[0], eA[1], eA[2]);
            double[] eB = j.GetEdgeVecB();
            Vector3d eVecB = new Vector3d(eB[0], eB[1], eB[2]);
            bool[] isStartA = j.GetIsStartA();
            bool[] isStartB = j.GetIsStartB();

            DA.SetData(0, pos);
            DA.SetData(1, norm);
            DA.SetData(2, eVecA);
            DA.SetData(3, eVecB);
            DA.SetDataList(4, segA);
            DA.SetDataList(5, segB);
            DA.SetDataList(6, isStartA);
            DA.SetDataList(7, isStartB);
            DA.SetData(8,j.GetAlpha());
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
            get { return new Guid("1c065c40-a2b3-4c82-9e1a-bcd089a42f04"); }
        }
    }
}
