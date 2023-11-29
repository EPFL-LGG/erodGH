using System;
using Grasshopper.Kernel;
using ErodDataLib.Types;
using static ErodDataLib.Types.OptimizationOptions;

namespace ErodData.Interop
{
    public class XShellDataWriteGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public XShellDataWriteGH()
          : base("Write Data", "Write Data",
            "Write a Json file with the linkage data.",
            "Erod", "Interop")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "Data", "Data to write.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Settings", "Settings", "Optimization settings", GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "Path", "Directory path.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "Filename", "Name of the file.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "Write", "Write file.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "Log", "Json files saved.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            string filename = "";
            bool write = false;
            RodLinkageData data = null;
            OptimizationOptions opt = new OptimizationOptions(false, 1.0, 1.0, OptimizationStages.OneStep);
            DA.GetData(0, ref data);
            DA.GetData(1, ref opt);
            DA.GetData(2, ref path);
            DA.GetData(3, ref filename);
            DA.GetData(4, ref write);

            data.OptimizationSettings = opt;

            if (write) data.WriteJsonFile(path, filename);

            path += filename;

            DA.SetData(0, path);
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
            get { return new Guid("10d5dd81-5f6f-4edb-b874-2273ab42482d"); }
        }
    }
}
