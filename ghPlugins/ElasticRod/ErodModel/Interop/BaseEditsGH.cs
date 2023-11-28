using System;
using System.Collections.Generic;
using ErodModelLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodModel.Interop
{
    public class BaseEditsGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BaseEditsGH()
          : base("BaseEdits", "BaseEdits",
            "Base edited target meshes data for interoperability with Speckle.",
            "Erod", "Interop")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BaseData", "BaseData", "Base data for interoperability.", GH_ParamAccess.item);
            pManager.AddMeshParameter("EditedMeshes", "EditedMeshes", "Edited target surface mesh.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Distance threshold for searching feature joints if not enforced.", GH_ParamAccess.list, 0.1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BaseData", "BaseData", "Base full data with edits for interoperability.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BaseData data = null;
            List<Mesh> eM = new List<Mesh>();
            List<double> tol = new List<double>();
            DA.GetData(0, ref data);
            DA.GetDataList(1, eM);
            DA.GetDataList(2, tol);

            if (data.ContainsDeployLinkage)
            {
                Mesh tM = data.DeployLinkage.TargetSurface.GetUnderlyingMesh();

                Dictionary<string, BaseTargetSurface> editedMeshes;
                // Edited Surface
                if (eM.Count != 0)
                {
                    if (tM == null) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Missing a valid base target mesh.");

                    // Base meshpoints
                    var joints = data.DeployLinkage.Joints;
                    int jCount = joints.Length;
                    MeshPoint[] meshpoints = new MeshPoint[jCount];
                    for (int i = 0; i < jCount; i++)
                    {
                        var pos = joints[i].Position;
                        meshpoints[i] = tM.ClosestMeshPoint(new Point3d(pos[0], pos[1], pos[2]), 0.0);
                    }


                    editedMeshes = new Dictionary<string, BaseTargetSurface>();
                    int count = eM.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Mesh tempM = eM[i];
                        double t = tol[0];

                        if (tol.Count == count) t = tol[i];
                        if (tempM.Vertices.Count != tM.Vertices.Count || tempM.Faces.Count != tM.Faces.Count) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid edited mesh at index(" + i + ")");

                        BaseTargetSurface bM = new BaseTargetSurface(tempM, meshpoints, t);
                        editedMeshes.Add("Surface_" + i, bM);
                    }

                    data.AddEditedSurfaces(editedMeshes);
                }
            }

            DA.SetData(0, data);
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
            get { return new Guid("49abae89-64ad-480e-a7c9-9ba383cb063a"); }
        }
    }
}
