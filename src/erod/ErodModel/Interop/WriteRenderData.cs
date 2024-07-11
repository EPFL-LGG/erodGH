using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Interop
{
    public class WriteRenderData : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public WriteRenderData()
          : base("SaveRender", "SaverRender",
            "Write a JSON file with data for rendering.",
            "Erod", "Interop")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flat", "Flat", "Undeployed linkage model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Deploy", "Deploy", "Deployed linkage model.", GH_ParamAccess.list);
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
            string path = "";
            string filename = "";
            bool write = false;
            RodLinkage flat = null;
            List<RodLinkage> deploy = new List<RodLinkage>();
            DA.GetData(0, ref flat);
            DA.GetDataList(1, deploy);
            DA.GetData(2, ref path);
            DA.GetData(3, ref filename);
            DA.GetData(4, ref write);


            string log = "";
            if (write)
            {
                var data = new RenderData(flat);
                data.WriteJsonFile(path, filename + "_flat");
                log += filename + "_flat.json\n";

                for (int i=0; i<deploy.Count; i++)
                {
                    data = new RenderData(deploy[i]);
                    data.WriteJsonFile(path, filename + "_deploy_" + i);
                    log += filename + "_deploy_" + i +".json\n";
                }
            }

            DA.SetData(0, log);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.save_render;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0604037c-f38c-4d16-9942-aecef4135ee5"); }
        }
    }
}
