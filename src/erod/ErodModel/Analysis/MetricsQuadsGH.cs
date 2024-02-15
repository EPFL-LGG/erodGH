using System;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using ErodModelLib.Metrics;
using System.Security.Cryptography;
using ErodModel.Model;
using GH_IO.Serialization;
using static ErodModelLib.Metrics.JointMetrics;
using static ErodModelLib.Metrics.QuadMetrics;

namespace ErodModel.Analysis
{
    public class MetricsQuadsGH : GH_Component
    {
        int metrics;
        List<List<string>> metricsAttributes;
        List<string> selection;
        bool buildAttributes = true;

        #region dropdownmenu content
        readonly List<string> categories = new List<string>(new string[] { "Metrics" });
        readonly List<string> metricTypes = ((QuadMetricTypes[])Enum.GetValues(typeof(QuadMetricTypes))).Select(t => t.ToString()).ToList();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MetricsQuadsGH()
          : base("MetricsQuads", "MeQuads",
                "Quad metrics of a linkage.",
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
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Alpha", "Alpha", "Set the alpha value (from 0.0 to 1.0) to control the transparency of the visualization", GH_ParamAccess.item, 0.3);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Metrics", "Metrics", "Quads metrics visualization.", GH_ParamAccess.item);
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
            bool show = false;
            double alpha = 0.3;
            if (!DA.GetData(0, ref linkage)) return;
            DA.GetData(1, ref alpha);
            DA.GetData(2, ref show);

            QuadMetricTypes quadType = ((QuadMetricTypes[])Enum.GetValues(typeof(QuadMetricTypes)))[metrics]; 
            Curve[] edges = linkage.GetSegmentsAsLines();

            QuadMetrics quadMetrics = new QuadMetrics(edges, quadType, ColorMaps.ColorMapTypes.Blues, (int)(alpha * 255));
            double[] data = quadMetrics.Data;

            if (show) GraphPlotter.HistogramAreas(data, quadType.ToString());

            DA.SetData(0, quadMetrics);
            DA.SetDataList(1, quadMetrics.Data);
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
            get { return new Guid("da343203-fc38-42fc-a61f-8913c95cc411"); }
        }
    }
}

