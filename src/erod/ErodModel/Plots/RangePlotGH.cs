using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using ErodModelLib.Utils;

namespace ErodModel.Plots
{
    public class RangePlotGH : GH_Component
    {

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RangePlotGH()
          : base("RangePlot", "RangePlot",
            "Creates a chart to indicate some property of data that lies in a certain range around a central value.",
            "Erod", "Plots")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("DataX", "DataX", "Sets the x coordinates of the plotted data as well as the sample data to be binned on the x axis.", GH_ParamAccess.list);
            pManager.AddNumberParameter("DataY", "DataY", "Sets the y coordinates of the plotted data as well as the sample data to be binned on the y axis.", GH_ParamAccess.list);
            pManager.AddNumberParameter("UpperY", "UpperY", "Sets the upper data.", GH_ParamAccess.list);
            pManager.AddNumberParameter("LowerY", "LowerY", "Sets the lower data.", GH_ParamAccess.list);
            pManager.AddTextParameter("DataLabels", "DataLabels", "Sets the labels of the plotted data", GH_ParamAccess.list);
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Settings", "Settings", "Sets the plotter settings", GH_ParamAccess.item);
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
            List<double> upperY = new List<double>();
            List<double> lowerY = new List<double>();
            List<string> dataLabels = new List<string>();
            List<string> upperLabels = new List<string>();
            List<string> lowerLabels = new List<string>();
            bool showLegend=true, show = false;
            GraphPlotterOptions options = new GraphPlotterOptions();

            DA.GetDataList(0, dataX);
            DA.GetDataList(1, dataY);
            DA.GetDataList(2, upperY);
            DA.GetDataList(3, lowerY);
            DA.GetDataList(4, dataLabels);
            DA.GetData(5, ref show);
            DA.GetData(6, ref options);

            if (show)
            {
                GraphPlotter.RangePlots(options, dataX.ToArray(), dataY.ToArray(), upperY.ToArray(), lowerY.ToArray(), dataLabels.ToArray(), upperLabels.ToArray(), lowerLabels.ToArray(), showLegend);
            }
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
                return Properties.Resources.Resources.range_plot;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("eb9ce120-2afd-4ef8-86c0-f3219fa6dbde"); }
        }
    }
}
