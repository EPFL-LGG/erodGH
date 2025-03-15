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
using static ErodModelLib.Utils.GraphPlotter;

namespace ErodModel.Plots
{
    public class PointDensityPlotGH : GH_Component
    {
        int normalizationIdx, colorscaleIdx;
        List<List<string>> menuAttributes;
        List<string> selection;
        bool buildAttributes = true;

        #region dropdownmenu content
        readonly List<string> categories = new List<string>(new string[] { "ColorScales", "Normalizations"});
        readonly List<string> colorscalesTypes = ((ColorScales[])Enum.GetValues(typeof(ColorScales))).Select(t => t.ToString()).ToList();
        readonly List<string> normalizationTypes = ((HistogramNormalization[])Enum.GetValues(typeof(HistogramNormalization))).Select(t => t.ToString()).ToList();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PointDensityPlotGH()
          : base("PointDensityPlot", "PointDensityPlot",
            "Creates a point density plot which combines a scatter plot and a histogram with 2d contours.",
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
                menuAttributes.Add(colorscalesTypes);
                menuAttributes.Add(normalizationTypes);
                selection.Add(colorscalesTypes[colorscaleIdx]);
                selection.Add(normalizationTypes[normalizationIdx]);
            }

            if (dropdownListId == 0)
            {
                colorscaleIdx = selectedItemId;
                selection[0] = menuAttributes[0][selectedItemId];
            }

            if (dropdownListId == 1)
            {
                normalizationIdx = selectedItemId;
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
            pManager.AddNumberParameter("DataX", "DataX", "Sets the x coordinates of the plotted data as well as the sample data to be binned on the x axis.", GH_ParamAccess.list);
            pManager.AddNumberParameter("DataY", "DataY", "Sets the y coordinates of the plotted data as well as the sample data to be binned on the y axis.", GH_ParamAccess.list);
            pManager.AddTextParameter("ScaleLabel", "ScaleLabel", "Sets the label of the scale color.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("NumContours", "NumContours", "Number of contours.", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("ShowContours", "ShowContours", "Shows the contour line.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Settings", "Settings", "Sets the plotter settings", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
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
            List<double> dataX = new List<double>();
            List<double> dataY = new List<double>();
            string label = "";
            int numContours = 0;
            bool showContours = false, show = false;
            GraphPlotterOptions options = new GraphPlotterOptions();

            DA.GetDataList(0, dataX);
            DA.GetDataList(1, dataY);
            DA.GetData(2, ref label);
            DA.GetData(3, ref numContours);
            DA.GetData(4, ref showContours);
            DA.GetData(5, ref show);
            DA.GetData(6, ref options);

            HistogramNormalization normalization = ((HistogramNormalization[])Enum.GetValues(typeof(HistogramNormalization)))[normalizationIdx];
            ColorScales colorScales = ((ColorScales[])Enum.GetValues(typeof(ColorScales)))[colorscaleIdx];

            if (show)
            {
                GraphPlotter.PointDensity(options, dataX.ToArray(), dataY.ToArray(), label, colorScales, normalization, showContours, numContours);
            }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("normalizationIdx", normalizationIdx);
            writer.SetInt32("colorscaleIdx", colorscaleIdx);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("normalizationIdx)", ref normalizationIdx))
            {
                FunctionToSetSelectedContent(0, normalizationIdx);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, menuAttributes, selection, categories);
            }

            if (reader.TryGetInt32("colorscaleIdx", ref colorscaleIdx))
            {
                FunctionToSetSelectedContent(1, colorscaleIdx);
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
                return Properties.Resources.Resources.pointdensity_plot;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("dc5dc77e-8f9c-4a1a-9c4b-68906dbd9e65"); }
        }
    }
}
