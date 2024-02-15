using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Commands;
using Rhino.Geometry;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace ErodModel.Analysis
{
    public class AngleDistributionGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public AngleDistributionGH()
          : base("Angle Distribution", "Angle Distribution",
                "Angle distribution of a RodLinkage.",
                "Erod", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "Size", "Size of the joints.", GH_ParamAccess.item, 10.0);
            pManager.AddBooleanParameter("UniformSize", "UniformSize", "Set uniform size.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ToDegrees", "ToDeg", "Angles in degrees", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("ShowDeviations", "ShowDeviations", "Whether we want to show deviation instead of angles", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Angles", "Angles", "Joint angles in radians.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Joints", "Joints", "Joints visualization.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage linkage = null;
            bool show = false, deg=false, uSize=true, showDevs=false;
            double size = 10.0;
            if (!DA.GetData(0, ref linkage)) return;
            DA.GetData(1, ref size);
            DA.GetData(2, ref uSize);
            DA.GetData(3, ref deg);
            DA.GetData(4, ref show);
            DA.GetData(5, ref showDevs);

            double[] angles = linkage.GetJointAngles();
            int numJoints = angles.Length;
            List<JointVis> joints = new List<JointVis>();

            double min = angles.Min();
            double max = angles.Max();
            double mean = angles.Average();
            double scale = 1.0 / (1.0e-8 + 2.0 * (max - mean > mean - min ? max - mean : mean - min));
            double range = max - min;

            Color[] colorRange = ColorMaps.Plasma.GetColors();
            Color[] colorRangeDev = ColorMaps.Coolwarm.GetColors();


            for (int i=0; i<numJoints; i++)
            {
                double[] p = linkage.Joints[i].GetPosition();
                Point3d pos = new Point3d(p[0], p[1], p[2]);

                double normalizedData;
                if (showDevs) normalizedData = 0.5 + scale * (angles[i] - mean);
                else normalizedData = (angles[i] - min) / (1.0e-8 + range);
                double radius = normalizedData * size;

                int colorIndex = (int)(normalizedData * (colorRange.Length - 1));
                Color color;
                if (showDevs) color = colorRangeDev[colorIndex];
                else color = colorRange[colorIndex];

                JointVis jt = new JointVis(pos, uSize ? (float)size : (float)radius +1, color);
                joints.Add(jt);
            }
            linkage.Joints[0].GetPosition();
            if (show) GraphPlotter.HistogramAngles(angles, deg);

            DA.SetDataList(0, angles);
            DA.SetDataList(1, joints);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
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
            get { return new Guid("5ce8f3c6-e3b7-4625-9b7c-b4b9ca3cec3f"); }
        }
    }
}