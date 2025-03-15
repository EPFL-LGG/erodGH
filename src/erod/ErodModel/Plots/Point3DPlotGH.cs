using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using ErodModelLib.Utils;

namespace ErodModel.Plots
{
    public class Point3DPlotGH : GH_Component
    {

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Point3DPlotGH()
          : base("Point3DPlot", "Point3DPlot",
            "Creates a three-dimensional point chart.",
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
            pManager.AddNumberParameter("DataZ", "DataZ", "Sets the z coordinates of the plotted data as well as the sample data to be binned on the z axis.", GH_ParamAccess.list);
            pManager.AddTextParameter("DataLabels", "DataLabels", "Sets the labels of the plotted data", GH_ParamAccess.list);
            pManager.AddTextParameter("LabelX", "LabelX", "Sets the label of the x axis.", GH_ParamAccess.item, "x");
            pManager.AddTextParameter("LabelY", "LabelY", "Sets the label of the y axis.", GH_ParamAccess.item, "y");
            pManager.AddTextParameter("LabelZ", "LabelZ", "Sets the label of the z axis.", GH_ParamAccess.item, "z");
            pManager.AddBooleanParameter("ShowPlots", "ShowPlots", "Generate graph plots", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Settings", "Settings", "Sets the plotter settings", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
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
            List<string> dataLabels = new List<string>();
            string labelX = "x", labelY= "y", labelZ = "z";
            bool show = false;
            GraphPlotterOptions options = new GraphPlotterOptions();

            DA.GetDataList(0, dataX);
            DA.GetDataList(1, dataY);
            DA.GetDataList(2, dataZ);
            DA.GetDataList(3, dataLabels);
            DA.GetData(4, ref labelX);
            DA.GetData(5, ref labelY);
            DA.GetData(6, ref labelZ);
            DA.GetData(7, ref show);
            DA.GetData(8, ref options);

            if (show)
            {
                GraphPlotter.Point3DChart(options, dataX.ToArray(), dataY.ToArray(), dataZ.ToArray(), dataLabels.ToArray(), labelX, labelY, labelZ);
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
                return Properties.Resources.Resources.point3d_plot;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("947d8138-61be-4a60-8e62-c215d73445af"); }
        }
    }
}
