using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Interop
{
    public class InteropDataGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public InteropDataGH()
          : base("BaseData", "BaseData",
            "Base full linkage data for interoperability with Speckle.",
            "Erod", "Interop")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("DataLinkage", "DataLinkage", "RodLinkage data. This data contains the curve network used for initialization.", GH_ParamAccess.item);
            pManager.AddNumberParameter("TargetAngle", "TargetAngle", "Target deployment angle (in degrees).", GH_ParamAccess.item);
            pManager.AddGenericParameter("BaseFlatLinkage", "BaseFlatLinkage", "Flat base linkage.", GH_ParamAccess.item);
            pManager.AddGenericParameter("BaseDeployLinkage", "BaseDeployLinkage", "Deploy base linkage.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "tol", "Tolerance for curves DoF.", GH_ParamAccess.item, 1e-3);
            //pManager.AddGenericParameter("BaseTargetSrf", "BaseTargetSrf", "Base Target surface mesh.", GH_ParamAccess.item);
            //pManager.AddMeshParameter("EditedMesh", "EditedMesh", "Edited target surface mesh.", GH_ParamAccess.list);
            //pManager.AddNumberParameter("Tolerance", "Tolerance", "Distance threshold for searching feature joints.", GH_ParamAccess.item, 0.1);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            //pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BaseData", "BaseData", "Base full data for interoperability.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkageData data = null;
            double angle = 0;
            BaseLinkage flat = null;
            BaseLinkage deploy = null;
            double tol = 1e-3;
            //BaseTargetSurface tM = null;
            //List<Mesh> eM = new List<Mesh>();
            //double tol = 0.1;
            DA.GetData(0, ref data);
            DA.GetData(1, ref angle);
            DA.GetData(2, ref flat);
            DA.GetData(3, ref deploy);
            DA.GetData(4, ref tol);
            //DA.GetData(4, ref tM);
            //DA.GetDataList(5, eM);
            //DA.GetData(6, ref tol);

            if (data == null) return;

            try
            {
                BaseCurveNetwork crvData = new BaseCurveNetwork(data, angle, tol);

                //BaseLinkage flatData = null;
                //if(flat!=null) flatData = new BaseLinkage(flat);

                //BaseLinkage deployData = null;
                /*Dictionary<string, BaseTargetSurface> editedMeshes = null;
                if (deploy != null)
                {
                    deployData = new BaseLinkage(deploy, tM);

                    // Edited Surface
                    if (eM.Count != 0)
                    {
                        if (tM == null) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Missing a valid base target mesh.");
                        var refMesh = tM.GetUnderlyingMesh();

                        // Base meshpoints
                        int jCount = deploy.Joints.Length;
                        MeshPoint[] meshpoints = new MeshPoint[jCount];
                        for (int i=0; i<jCount; i++)
                        {
                            var pos = deploy.Joints[i].GetPosition();
                            meshpoints[i] = refMesh.ClosestMeshPoint(new Point3d(pos[0], pos[1], pos[2]), 0.0);
                        }


                        editedMeshes = new Dictionary<string, BaseTargetSurface>();
                        int count = eM.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Mesh tempM = eM[i];
                            if(tempM.Vertices.Count != refMesh.Vertices.Count || tempM.Faces.Count != refMesh.Faces.Count) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid edited mesh at index(" + i +")");
                            BaseTargetSurface bM = new BaseTargetSurface(eM[i], meshpoints, tol);
                            editedMeshes.Add("Surface_"+i, bM);
                        }
                    }
                }*/


                BaseData obj = new BaseData(crvData, null, deploy);//, editedMeshes);

                DA.SetData(0, obj);
            }


            catch (Exception e) { AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, e.ToString()); }
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
            get { return new Guid("6bf7f049-b755-4910-a8ce-85e7e3897479"); }
        }
    }
}
