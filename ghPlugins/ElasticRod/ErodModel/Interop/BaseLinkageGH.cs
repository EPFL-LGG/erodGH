using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Interop
{
    public class BaseLinkageGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BaseLinkageGH()
          : base("BaseLinkage", "BaseLinkage",
            "Base target mesh data for interoperability with Speckle.",
            "Erod", "Interop")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FlatLinkage", "FlatLinkage", "Flat linkage.", GH_ParamAccess.item);
            pManager.AddGenericParameter("DeployLinkage", "DeployLinkage", "Deploy linkage.", GH_ParamAccess.item);
            pManager.AddMeshParameter("TargetMesh", "TargetMesh", "Target surface mesh.", GH_ParamAccess.item);
            pManager.AddPointParameter("JointsPos", "JointsPos", "Enforced joints positions on mesh.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("JointsFeat", "JointFeat", "Enforced feature joints.", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BaseFlatLinkage", "BaseFlatLinkage", "Base flat linkage for interoperability.", GH_ParamAccess.item);
            pManager.AddGenericParameter("BaseDeployLinkage", "BaseDeployLinkage", "Deploy linkage for interoperability.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh tM = null;
            List<Point3d> jPos = new List<Point3d>();
            List<int> jFeat = new List<int>();
            RodLinkage deploy = null;
            RodLinkage flat = null;
            Boolean includeMesh = true;
            DA.GetData(0, ref flat);
            DA.GetData(1, ref deploy);
            DA.GetData(2, ref tM);
            DA.GetDataList(3, jPos);
            DA.GetDataList(4, jFeat);

            BaseLinkage flatData = new BaseLinkage(flat, null, includeMesh);

            BaseTargetSurface bM = null;
            if (tM != null)
            {
                if (jPos.Count > 0) bM = new BaseTargetSurface(tM, jPos, jFeat);
                else bM = new BaseTargetSurface(tM, deploy);
            }

            BaseLinkage deployData = new BaseLinkage(deploy, bM, includeMesh);

            DA.SetData(0, flatData);
            DA.SetData(1, deployData);
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
            get { return new Guid("6a5c9c90-6f6f-4166-a048-3f29888e17a3"); }
        }
    }
}
