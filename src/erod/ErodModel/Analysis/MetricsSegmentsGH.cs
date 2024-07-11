using System;
using System.Collections.Generic;
using System.Linq;
using ErodModel.Model;
using ErodModelLib.Metrics;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using static ErodModelLib.Metrics.LinkageSegmentMetrics;
using static ErodModelLib.Metrics.RodMetrics;

namespace ErodModel.Analysis
{
    public class MetricsRodsGH : GH_Component
    {
        int metrics;
        List<List<string>> metricsAttributes;
        List<string> selection;
        bool buildAttributes = true;

        #region dropdownmenu content
        readonly List<string> categories = new List<string>(new string[] { "Metrics" });
        readonly List<string> metricTypes = ((SegmentMetricTypes[])Enum.GetValues(typeof(SegmentMetricTypes))).Select(t => t.ToString()).ToList();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MetricsRodsGH()
          : base("MetricsSegments", "MeSegments",
            "Metrics of a rod segment from a linkage or an elastic rod.",
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
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, metricsAttributes, selection, categories);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (metricsAttributes == null)
            {
                metricsAttributes = new List<List<string>>();
                selection = new List<string>();
                metricsAttributes.Add(metricTypes);
                selection.Add(metricTypes[metrics]);
            }

            if (dropdownListId == 0)
            {
                metrics = selectedItemId;
                selection[0] = metricsAttributes[0][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rods", "Rods", "List of rod segments or elastic rods.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Alpha", "Alpha", "Set the alpha value (from 0.0 to 1.0) to control the transparency of the visualization", GH_ParamAccess.item, 0.3);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Metrics", "Metrics", "Rod metrics visualization.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Data", "Data", "Data calculated based on the metric type.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<object> rods = new List<object>();
            bool show = false;
            double alpha = 0.3;

            DA.GetDataList(0, rods);
            DA.GetData(1, ref alpha);
            DA.GetData(2, ref show);

            SegmentMetricTypes segmentType = ((SegmentMetricTypes[]) Enum.GetValues(typeof(SegmentMetricTypes)))[metrics];

            List<ElasticRod> rodList = rods.OfType<ElasticRod>().ToList();
            List<RodSegment> segmentList = rods.OfType<RodSegment>().ToList();

            List<IGH_Goo> metricsList = new List<IGH_Goo>();
            GH_Structure<GH_Number> data = new GH_Structure<GH_Number>();
            int path = 0;
            if (rodList.Count > 0)
            {
                RodMetrics rM = new RodMetrics(rodList, segmentType, (int)(alpha * 255));
                metricsList.Add(rM);
                data.AppendRange(rM.Data.Select(d => new GH_Number(Math.Abs(d))).ToArray(), new GH_Path(path));
                path++;
                if (show) GraphPlotter.HistogramSegments(rM.Data.Select(d => Math.Abs(d)).ToArray(), segmentType.ToString());
            }
            if (segmentList.Count > 0)
            {
                LinkageSegmentMetrics sM = new LinkageSegmentMetrics(segmentList, segmentType, (int)(alpha * 255));
                metricsList.Add(sM);
                data.AppendRange(sM.Data.Select(d => new GH_Number(Math.Abs(d))).ToArray(), new GH_Path(path));
                path++;
                if (show) GraphPlotter.HistogramSegments(sM.Data.Select(d => Math.Abs(d)).ToArray(), segmentType.ToString());
            }

            DA.SetDataList(0, metricsList);
            DA.SetDataTree(1, data);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("metrics", metrics);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("metrics", ref metrics))
            {
                FunctionToSetSelectedContent(0, metrics);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, metricsAttributes, selection, categories);
            }
            return base.Read(reader);
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
                return Properties.Resources.Resources.metrics_rods;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7089d431-324b-4dbc-9b8f-ace25402c682"); }
        }
    }
}
