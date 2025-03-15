using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using ErodModelLib.Utils;
using static ErodModelLib.Utils.GraphPlotter;
using System.Linq;
using ErodModel.Model;
using GH_IO.Serialization;

namespace ErodModel.Plots
{
    public class HistogramPlotGH : GH_Component
    {
        int normalizationIdx, functionIdx;
        List<List<string>> menuAttributes;
        List<string> selection;
        bool buildAttributes = true;

        #region dropdownmenu content
        readonly List<string> categories = new List<string>(new string[] { "Normalizations", "Functions" });
        readonly List<string> normalizationTypes = ((HistogramNormalization[])Enum.GetValues(typeof(HistogramNormalization))).Select(t => t.ToString()).ToList();
        readonly List<string> functionTypes = ((HistogramFunction[])Enum.GetValues(typeof(HistogramFunction))).Select(t => t.ToString()).ToList();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public HistogramPlotGH()
          : base("HistogramPlot", "HistogramPlot",
            "Visualizes the distribution of the input data as a histogram. A histogram is an approximate representation of the distribution of numerical data.",
            "Erod", "Plots")
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
                menuAttributes.Add(normalizationTypes);
                menuAttributes.Add(functionTypes);
                selection.Add(normalizationTypes[normalizationIdx]);
                selection.Add(functionTypes[functionIdx]);
            }

            if (dropdownListId == 0)
            {
                normalizationIdx = selectedItemId;
                selection[0] = menuAttributes[0][selectedItemId];
            }

            if (dropdownListId == 1)
            {
                functionIdx = selectedItemId;
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
            pManager.AddNumberParameter("Data", "Data", "Sets the sample data.", GH_ParamAccess.list);
            pManager.AddTextParameter("GroupNames", "GroupNames", "Sets the group names for each datum.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("MaxNumGroups", "MaxNumGroups", "Specifies the maximum number of desired groups.", GH_ParamAccess.item, 5);
            pManager.AddBooleanParameter("ShowLegend", "ShowLegend", "Determines whether or not an item corresponding to this trace is shown in the legend.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Settings", "Settings", "Sets the plotter settings", GH_ParamAccess.item);
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
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> dataX= new List<double>();
            List<string> groupNames = new List<string>();
            int numGroups = 5;
            bool show = false, showLegend=false;
            GraphPlotterOptions options = new GraphPlotterOptions();

            DA.GetDataList(0, dataX);
            DA.GetDataList(1, groupNames);
            DA.GetData(2, ref numGroups);
            DA.GetData(3, ref showLegend);
            DA.GetData(4, ref show);
            DA.GetData(5, ref options);

            HistogramNormalization normalization = ((HistogramNormalization[])Enum.GetValues(typeof(HistogramNormalization)))[normalizationIdx];
            HistogramFunction function = ((HistogramFunction[])Enum.GetValues(typeof(HistogramFunction)))[functionIdx];
            if (show)
            {
                GraphPlotter.Histogram(options, dataX.ToArray(), groupNames.ToArray(), numGroups, normalization, function, showLegend);
            }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("normalizationIdx", normalizationIdx);
            writer.SetInt32("functionIdx", functionIdx);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("normalizationIdx)", ref normalizationIdx))
            {
                FunctionToSetSelectedContent(0, normalizationIdx);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, menuAttributes, selection, categories);
            }

            if (reader.TryGetInt32("functionIdx", ref functionIdx))
            {
                FunctionToSetSelectedContent(1, functionIdx);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, menuAttributes, selection, categories);
            }

            return base.Read(reader);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.histogram_plot;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("031702be-2cdf-42a8-8b3a-c37b750960a6"); }
        }
    }
}
