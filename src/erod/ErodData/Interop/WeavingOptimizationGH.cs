using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using ErodDataLib.Types;
using ErodDataLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using static ErodDataLib.Utils.WeavingOptimizationOptions;

namespace ErodData.Interop
{
    public class WeavingOptimizationGH : GH_Component
    {
        public SSHWeavingOptimization weavingOptimizer = null;

        public bool run;
        string name;
        bool deleteCache;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public WeavingOptimizationGH()
          : base("WeavingOptimizer", "WeavingOptimizer",
            "Weaving optimizer running on a remote server.",
            "Erod", "Interop")
        {
            weavingOptimizer = new SSHWeavingOptimization();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Server", "Server", "Remote server for running weaving optimization.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "Name", "Name of the model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("LinkageIO", "LinkageIO", "Elastic linkage initialization data.", GH_ParamAccess.item);
            pManager.AddGenericParameter("TargetSurfaceIO", "TargetSrfIO", "Target surface to attract the linkage.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Optimize", "Optimize", "Start optimization on the remote server.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("DeleteCache", "DeleteCache", "Delete pickle files from the server. Pre-computed models will be deleted.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Settings", "Settings", "Optimization settings", GH_ParamAccess.item);
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "Log", "The log of the optimization process.", GH_ParamAccess.item);
            pManager.AddCurveParameter("Contours", "Contours", "Returns a list of polyline curves representing the contours of ribbons for fabrication.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Indices", "Indices", "Returns the indices of the joints, structured as a tree structure. Each branch represents a ribbon.", GH_ParamAccess.tree);
            pManager.AddPointParameter("2dJoints", "2dJoints", "Returns the 2d position of the joints, structured as a tree structure. Each branch represents a flat ribbon.", GH_ParamAccess.tree);
            pManager.AddPointParameter("3dJoints", "3dJoints", "Returns the 3d position of the joints, structured as a tree structure. Each branch represents a deformed ribbon.", GH_ParamAccess.tree);
            pManager.AddCurveParameter("2dCenterlines", "2dCenterlines", "Returns the centerlines of flat ribbons.", GH_ParamAccess.list);
            pManager.AddCurveParameter("3dCenterlines", "3dCenterlines", "Returns the centerlines of deformed ribbons.", GH_ParamAccess.list);
            pManager.AddMeshParameter("Ribbons", "Ribbons", "Returns a list of meshes representing the deformed ribbons.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            LinkageIO data = null;
            TargetSurfaceIO target = null;
            SSHServerID server = new SSHServerID();
            WeavingOptimizationOptions opt = new WeavingOptimizationOptions(false, 1.0, 1.0, OptimizationStages.OneStep);

            DA.GetData(0, ref server);
            DA.GetData(1, ref name);
            DA.GetData(2, ref data);
            DA.GetData(3, ref target);
            DA.GetData(4, ref run);
            DA.GetData(5, ref deleteCache);
            DA.GetData(6, ref opt);

            if (run)
            {
                weavingOptimizer.DataIO = new JsonWeaving(data, target, opt);
                Message = "Computing...";
                var result = MessageBox.Show("Do you want to start optimizing this model?\n", "Weaving optimization", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK) weavingOptimizer.RunAsyncOptimization(this, server, name, deleteCache);
                else Message = "Done!";
            }


            DA.SetData(0, weavingOptimizer.Log);
            DA.SetDataList(1, weavingOptimizer.RibbonContours);
            DA.SetDataTree(2, weavingOptimizer.JointIndices);
            DA.SetDataTree(3, weavingOptimizer.Joints2D);
            DA.SetDataTree(4, weavingOptimizer.Joints3D);
            DA.SetDataList(5, weavingOptimizer.flat_center_line);
            DA.SetDataList(6, weavingOptimizer.DeformedCenterlines);
            DA.SetDataList(7, weavingOptimizer.Ribbons);
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
                return Properties.Resources.Resources.weaving_optimization;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b3110b25-e165-4d1e-8447-2386ae8f88f4"); }
        }
    }
}
