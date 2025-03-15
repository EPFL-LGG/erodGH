using System;
using Grasshopper.Kernel;
using ErodModelLib.Utils;

namespace ErodModel.Plots
{
    public class PlotterOptionsGH : GH_Component
    {

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PlotterOptionsGH()
          : base("PlotSettings", "PlotSettings",
            "Graph plotter Settings.",
            "Erod", "Plots")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Width", "Width", "Sets the width of the plot in pixels.", GH_ParamAccess.item, 800);
            pManager.AddIntegerParameter("Length", "Length", "Sets the length of the plot in pixels.", GH_ParamAccess.item, 800);
            pManager.AddTextParameter("Title", "Title", "Sets the title of the plot.", GH_ParamAccess.item, "");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PlotSettings", "PlotSettings", "Set the graph plot settings.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string title = "";
            int width = 800, length=800;
            DA.GetData(0, ref width);
            DA.GetData(1, ref length);
            DA.GetData(2, ref title);

            GraphPlotterOptions options = new GraphPlotterOptions(width, length, title);

            DA.SetData(0, options);
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
                return Properties.Resources.Resources.options_plot;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1c50a6d5-c0e9-46fd-8515-af44a092dabf"); }
        }
    }
}
