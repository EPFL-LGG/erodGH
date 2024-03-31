using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using Grasshopper.Kernel;
using System.Linq;
using ErodModel.Model;
using GH_IO.Serialization;
using ErodModelLib.Metrics;
using static ErodModelLib.Metrics.JointMetrics;
using static ErodModelLib.Metrics.RodLinkageMetrics;
using static ErodModelLib.Utils.ColorMaps;

namespace ErodModel.Analysis
{
    public class MetricsJointsGH : GH_Component
    {
        int metricIdx, cmapIdx;
        List<List<string>> menuAttributes;
        List<string> selection;
        bool buildAttributes = true;

        #region dropdownmenu content
        readonly List<string> categories = new List<string>(new string[] { "Metrics", "ColorMaps" });
        readonly List<string> metricTypes = ((JointMetricTypes[])Enum.GetValues(typeof(JointMetricTypes))).Select(t => t.ToString()).ToList();
        readonly List<string> cmapTypes = ((ColorMapTypes[])Enum.GetValues(typeof(ColorMapTypes))).Select(t => t.ToString()).ToList();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MetricsJointsGH()
          : base("MetricsJoints", "MeJoints",
                "Joint metrics of a linkage.",
                "Erod", "Analysis")
        {
        }

        public override void CreateAttributes()
        {
            if (buildAttributes)
            {
                FunctionToSetSelectedContent(0, 0);
                buildAttributes = false;
            }
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, menuAttributes, selection, categories);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (menuAttributes == null)
            {
                menuAttributes = new List<List<string>>();
                selection = new List<string>();
                menuAttributes.Add(metricTypes);
                menuAttributes.Add(cmapTypes);
                selection.Add(metricTypes[metricIdx]);
                selection.Add(cmapTypes[cmapIdx]);
            }

            if (dropdownListId == 0)
            {
                metricIdx = selectedItemId;
                selection[0] = menuAttributes[0][selectedItemId];
            }

            if (dropdownListId == 1)
            {
                cmapIdx = selectedItemId;
                selection[1] = menuAttributes[1][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "Size", "Size of the joints.", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("UniformSize", "UniformSize", "Set uniform size.", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("LowerBound", "LowerBound", "Lower bound of the data set. If no value is explicitly provided, the minimum value of the data set is assumed.", GH_ParamAccess.item);
            pManager.AddNumberParameter("UpperBound", "UpperBound", "Upper bound of the data set. If no value is explicitly provided, the maximum value of the data set is assumed.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Metrics", "Metrics", "Joint metrics visualization.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Data", "Data", "Data calculated based on the metric type.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage linkage = null;
            bool show = false, uSize=true;
            double size = 10.0, lowerBound = default, upperBound = default;
            if (!DA.GetData(0, ref linkage)) return;
            DA.GetData(1, ref size);
            DA.GetData(2, ref uSize);
            DA.GetData(3, ref lowerBound);
            DA.GetData(4, ref upperBound);
            DA.GetData(5, ref show);

            JointMetricTypes jType = ((JointMetricTypes[])Enum.GetValues(typeof(JointMetricTypes)))[metricIdx];
            ColorMapTypes cmapType = ((ColorMapTypes[])Enum.GetValues(typeof(ColorMapTypes)))[cmapIdx];

            JointMetrics jointMetrics = new JointMetrics(linkage.Joints, jType, cmapType, size, uSize, lowerBound, upperBound);
            double[] data;
            switch (jType)
            {
                case JointMetricTypes.Angles:
                    data = jointMetrics.Data;
                    break;
                case JointMetricTypes.AngleDeviations:
                    data = jointMetrics.NormalizedData;
                    break;
                default:
                    data = jointMetrics.Data;
                    break;
            }

            if (show) GraphPlotter.HistogramAngles(data, jType);

            DA.SetData(0, jointMetrics);
            DA.SetDataList(1, data);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("metricsIdx", metricIdx);
            writer.SetInt32("cmapIdx", cmapIdx);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("metricsIdx", ref metricIdx))
            {
                FunctionToSetSelectedContent(0, metricIdx);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, menuAttributes, selection, categories);
            }

            if (reader.TryGetInt32("cmapIdx", ref cmapIdx))
            {
                FunctionToSetSelectedContent(1, cmapIdx);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, menuAttributes, selection, categories);
            }
            return base.Read(reader);
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