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
using static ErodDataLib.Types.OptimizationOptions;

namespace ErodData.Interop
{
    public class WeavingOptimizationGH : GH_Component
    {
        public WeavingOpt weavingOptimizer = null;

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
            "Weaving optimization [WIP].",
            "Erod", "Interop")
        {
            weavingOptimizer = new WeavingOpt();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name of the model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Data", "Data", "RodLinkage data.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Optimize", "Optimize", "run optimization", GH_ParamAccess.item);
            pManager.AddBooleanParameter("DeleteCache", "DeleteCache", "Keep pickles.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Settings", "Settings", "Optimization settings", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "Log", "Log", GH_ParamAccess.item);
            pManager.AddIntegerParameter("JointsIdx", "JointsIdx", "Joint indexes", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Ribbons2D", "Ribbons2D", "Flat ribbons", GH_ParamAccess.list);
            pManager.AddPointParameter("Joints2D", "Joints2D", "Flat joint positions", GH_ParamAccess.tree);
            pManager.AddCurveParameter("CenterLine2D", "CenterLine2D", "Flat center lines", GH_ParamAccess.list);
            pManager.AddPointParameter("CenterLinePositions2D", "CenterLinePositions2D", "Flat center-line positions", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Ribbons3D", "Ribbons3D", "Deformed ribbons", GH_ParamAccess.list);
            pManager.AddPointParameter("Joints3D", "Joints3D", "Deformed joint positions", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkageData data = null;
            OptimizationOptions opt = new OptimizationOptions(false, 1.0, 1.0, OptimizationStages.OneStep);
            DA.GetData(0, ref name);
            DA.GetData(1, ref data);
            DA.GetData(2, ref run);
            DA.GetData(3, ref deleteCache);
            DA.GetData(4, ref opt);

            data.OptimizationSettings = opt;

            weavingOptimizer.Data = data;

            if (run)
            {
                Message = "Computing...";
                var result = MessageBox.Show("Do you want to start optimizing this model?\n", "Weaving optimization", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK) weavingOptimizer.RunAsyncOptimization(this, name, deleteCache);
                else Message = "Done!";
            }

            DA.SetData(0, weavingOptimizer.Log);
            DA.SetDataTree(1, weavingOptimizer.flat_joint_indexes);
            DA.SetDataList(2, weavingOptimizer.flat_ribbons);
            DA.SetDataTree(3, weavingOptimizer.flat_joint_positions);
            DA.SetDataList(4, weavingOptimizer.flat_center_line);
            DA.SetDataTree(5, weavingOptimizer.flat_center_line_positions);
            DA.SetDataList(6, weavingOptimizer.deformed_mesh_ribbons);
            DA.SetDataTree(7, weavingOptimizer.deformed_joints_per_ribbon);
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
            get { return new Guid("b3110b25-e165-4d1e-8447-2386ae8f88f4"); }
        }
    }
}
