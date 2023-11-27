using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Tools
{
    public class DeconstructElasticRodGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public DeconstructElasticRodGH()
          : base("Deconstruct RodData", "Deconstruct RodData",
            "Deconstruct an ElasticRod.",
            "Erod", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "ElasticRod model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("CenterLine", "CenterLine", "Center line of the elastic rod.", GH_ParamAccess.item);
            pManager.AddPointParameter("Nodes", "Nodes", "Nodes positions.", GH_ParamAccess.list);
            pManager.AddNumberParameter("RestLengths", "RestLengths", "Rest lengths.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stretching", "Stretching", "Stretching stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Twisting", "Twisting", "Twisting stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("MaxBend", "MaxBend", "Maximum bending stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("MinBend", "MinBend", "Minimum bending stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("SqrtBend", "SqrtBend", "Sqrt bending energies.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ElasticRod rod = null;
            DA.GetData(0, ref rod);

            double[] coords = rod.GetVertexCoordinates();
            double[] stretch = rod.GetStretchingStresses();
            double[] twist = rod.GetTwistingStresses();
            double[] rlengths = rod.GetRestLengths();
            double[] maxStress = rod.GetMaxBendingStresses();
            double[] minStress = rod.GetMinBendingStresses();
            double[] sqrtBend = rod.GetSqrtBendingEnergies();

            int count = (int)coords.Length / 3;
            Point3d[] pts = new Point3d[count];
            for (int i = 0; i < count; i++)
            {
                pts[i] = new Point3d(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
            }

            PolylineCurve crv = new PolylineCurve(pts);

            DA.SetData(0, crv);
            DA.SetDataList(1, pts);
            DA.SetDataList(2, rlengths);
            DA.SetDataList(3, stretch);
            DA.SetDataList(4, twist);
            DA.SetDataList(5, maxStress);
            DA.SetDataList(6, minStress);
            DA.SetDataList(7, sqrtBend);
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
            get { return new Guid("fd6463c0-a5d3-44a2-bf10-d7049b86e9ab"); }
        }
    }
}
