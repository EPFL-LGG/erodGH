using System;
using System.Collections.Generic;
using ErodModel.Model;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using System.Linq;
using ErodModelLib.Utils;
using static ErodModelLib.Utils.GraphPlotter;

namespace ErodModel.Plots
{
    public class Line3DPlotGH : GH_Component
    {
        int colorscaleIdx;
        List<List<string>> menuAttributes;
        List<string> selection;
        bool buildAttributes = true;

        #region dropdownmenu content
        readonly List<string> categories = new List<string>(new string[] { "ColorScales" });
        readonly List<string> colorscalesTypes = ((ColorScales[])Enum.GetValues(typeof(ColorScales))).Select(t => t.ToString()).ToList();
        #endregion

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Line3DPlotGH()
          : base("Line3DPlot", "Line3DPlot",
            "Creates a line 3D plot.",
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
                selection.Add(colorscalesTypes[colorscaleIdx]);
            }

            if (dropdownListId == 0)
            {
                colorscaleIdx = selectedItemId;
                selection[0] = menuAttributes[0][selectedItemId];
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
            pManager.AddNumberParameter("DataZ", "DataZ", "Sets the z coordinates of the plotted data as well as the sample data to be binned on the z axis.", GH_ParamAccess.list);
            pManager.AddNumberParameter("DataW", "DataW", "Sets the sizes of the plotted data.", GH_ParamAccess.list);
            pManager.AddTextParameter("DataLabels", "DataLabels", "Sets the labels of the plotted data", GH_ParamAccess.list);
            pManager.AddTextParameter("LabelX", "LabelX", "Sets the label of the x axis.", GH_ParamAccess.item, "x");
            pManager.AddTextParameter("LabelY", "LabelY", "Sets the label of the y axis.", GH_ParamAccess.item, "y");
            pManager.AddTextParameter("LabelZ", "LabelZ", "Sets the label of the z axis.", GH_ParamAccess.item, "z");
            pManager.AddNumberParameter("LineWidth", "LineWidth", "Sets the width of the line.", GH_ParamAccess.item, 10);
            pManager.AddBooleanParameter("ShowMarkers", "ShowMarkers", "Show markers.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Settings", "Settings", "Sets the plotter settings", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
            pManager[10].Optional = true;
            pManager[11].Optional = true;
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
            List<double> dataZ = new List<double>();
            List<double> dataW = new List<double>();
            List<string> dataLabels = new List<string>();
            string labelX = "x", labelY = "y", labelZ = "z";
            bool showMarkers= true, show = false;
            double lineWidth = 10;
            GraphPlotterOptions options = new GraphPlotterOptions();

            DA.GetDataList(0, dataX);
            DA.GetDataList(1, dataY);
            DA.GetDataList(2, dataZ);
            DA.GetDataList(3, dataW);
            DA.GetDataList(4, dataLabels);
            DA.GetData(5, ref labelX);
            DA.GetData(6, ref labelY);
            DA.GetData(7, ref labelZ);
            DA.GetData(8, ref lineWidth);
            DA.GetData(9, ref showMarkers);
            DA.GetData(10, ref show);
            DA.GetData(11, ref options);

            ColorScales colorScales = ((ColorScales[])Enum.GetValues(typeof(ColorScales)))[colorscaleIdx];

            if (show)
            {
                GraphPlotter.Line3DChart(options, dataX.ToArray(), dataY.ToArray(), dataZ.ToArray(), dataW.ToArray(), dataLabels.ToArray(), labelX, labelY, labelZ, lineWidth, colorScales, showMarkers);
            }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("colorscaleIdx", colorscaleIdx);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
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
                return Properties.Resources.Resources.line3d_plot;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2bf319a0-a26d-4fa0-afed-3b6104949d0b"); }
        }
    }
}
