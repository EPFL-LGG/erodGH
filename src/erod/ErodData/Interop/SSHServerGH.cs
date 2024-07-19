using System;
using System.Collections.Generic;
using ErodDataLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.Interop
{
    public class SSHServerGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SSHServerGH()
          : base("RemoteServer", "RemoteServer",
            "Set the credentials for connecting to a remote server for running optimization tasks.\nEnsure that the server has an instance of the optimization code already deployed.",
            "Erod", "Interop")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Host", "Host", "The address of the remote server.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Port","Port", "The port number to connect to on the remote server.", GH_ParamAccess.item);
            pManager.AddTextParameter("Username","Username", "The username required for authentication.", GH_ParamAccess.item);
            pManager.AddTextParameter("Password", "Password", "The password required for authentication.", GH_ParamAccess.item);
            pManager.AddTextParameter("RunFolder", "RunFolder", "The folder on the server where the python script for running optimization is located.", GH_ParamAccess.item);
            pManager.AddTextParameter("CondaEnv", "CondaEnv", "Name of the conda environment to use for running optimization.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Server", "Server", "Remote server.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string host = "", username = "", password = "", runFolder = "", condaEnv = "";
            int port = 22;
            DA.GetData(0, ref host);
            DA.GetData(1, ref port);
            DA.GetData(2, ref username);
            DA.GetData(3, ref password);
            DA.GetData(4, ref runFolder);
            DA.GetData(5, ref condaEnv);

            SSHServerID server = new SSHServerID(host, port, username, password, runFolder, condaEnv);

            DA.SetData(0, server);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.server;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d4f2c496-3aae-46f8-98cd-cbf21b6da6c6"); }
        }
    }
}
