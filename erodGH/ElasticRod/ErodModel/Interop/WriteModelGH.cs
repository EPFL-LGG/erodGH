using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using System.IO;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using static ErodDataLib.Types.OptimizationOptions;
using Newtonsoft.Json;
using ErodModelLib.Utils;

namespace ErodModel.Interop
{
    public class WriteModelGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public WriteModelGH()
          : base("WriteModel", "WriteModel",
                "Write a Json with the linkage model.",
                "Erod", "Interop")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage model.", GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "Path", "Directory path.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "Filename", "Name of the file.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "Write", "Write file.", GH_ParamAccess.item);
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
            RodLinkage model = null;
            string filename = "";
            string path = "";
            bool write = false;
            DA.GetData(0, ref model);
            DA.GetData(1, ref path);
            DA.GetData(2, ref filename);
            DA.GetData(3, ref write);

            BaseLinkage l = new BaseLinkage(model, null, true);
            if (write) l.WriteJsonFile(path, filename);

            DA.SetData(0, path + filename);
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
            get { return new Guid("4ca00698-6349-4885-8749-6cd17c18fda4"); }
        }
    }
}
