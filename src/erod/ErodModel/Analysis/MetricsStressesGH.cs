using System;
using System.Collections.Generic;
using ErodModel.Model;
using ErodModelLib.Metrics;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using static ErodModelLib.Metrics.LinkageStressesMetrics;
using System.Linq;
using static ErodModelLib.Utils.ColorMaps;
using ErodModelLib.Utils;

namespace ErodModel.Analysis
{
    public class MetricsStressesGH : GH_Component
    {
        int metricIdx, cmapIdx;
        List<List<string>> menuAttributes;
        List<string> selection;
        bool buildAttributes = true;

        #region dropdownmenu content
        readonly List<string> categories = new List<string>(new string[] { "Metrics", "ColorMaps" });
        readonly List<string> metricTypes = ((LinkageMetricTypes[])Enum.GetValues(typeof(LinkageMetricTypes))).Select( t => t.ToString() ).ToList();
        readonly List<string> cmapTypes = ((ColorMapTypes[])Enum.GetValues(typeof(ColorMapTypes))).Select(t => t.ToString()).ToList();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MetricsStressesGH()
          : base("MetricsStresses", "MeStresses",
            "Visualization of stresses.",
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
            pManager.AddGenericParameter("ElasticModel", "EModel", "Elastic model to analyse.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Alpha", "Alpha", "Set the alpha value (from 0.0 to 1.0) to control the transparency of the visualization", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("LowerBound", "LowerBound", "Lower bound of the data set. If no value is explicitly provided, the minimum value of the data set is assumed.", GH_ParamAccess.item);
            pManager.AddNumberParameter("UpperBound", "UpperBound", "Upper bound of the data set. If no value is explicitly provided, the maximum value of the data set is assumed.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "RodLinkage mesh color coded with selected metrics.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Data", "Data", "Sacalr data field calculated based on the metric type.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object model = null;
            bool show = false;
            double alpha = 0.3, lowerBound=default, upperBound=default;
            if (!DA.GetData(0, ref model)) return;
            DA.GetData(1, ref alpha);
            DA.GetData(2, ref lowerBound);
            DA.GetData(3, ref upperBound);
            DA.GetData(4, ref show);

            LinkageMetricTypes linkageType = ((LinkageMetricTypes[])Enum.GetValues(typeof(LinkageMetricTypes)))[metricIdx];
            ColorMapTypes cmapType = ((ColorMapTypes[])Enum.GetValues(typeof(ColorMapTypes)))[cmapIdx];

            if (model is RodLinkage)
            {
                LinkageStressesMetrics linkageMetrics = new LinkageStressesMetrics((RodLinkage)model, linkageType, cmapType, (int)(alpha * 255), lowerBound, upperBound);
                double[] data = linkageMetrics.Data;

                if (show) GraphPlotter.HistogramLinkagesScalarFields(data, linkageType.ToString());

                DA.SetData(0, linkageMetrics.Mesh);
                DA.SetDataList(1, data);
            }
            else if(model is ElasticRod)
            {
                RodStressesMetrics linkageMetrics = new RodStressesMetrics((ElasticRod)model, linkageType, cmapType, (int)(alpha * 255), lowerBound, upperBound);
                double[] data = linkageMetrics.Data;

                if (show) GraphPlotter.HistogramLinkagesScalarFields(data, linkageType.ToString());

                DA.SetData(0, linkageMetrics.Mesh);
                DA.SetDataList(1, data);
            }
            else throw new Exception("Invalid input type. The type should be an elastic rod, a rod segment of an elastic linkage or an elastic linkage.");
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
                return Properties.Resources.Resources.metrics_linkage;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7e50fa56-ea94-4fd7-bd68-873896f3b63e"); }
        }
    }
}
