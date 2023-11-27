using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.Data
{
    public class ElasticRodDataGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ElasticRodDataGH()
          : base("ElasticRodData", "ElasticRodData",
              "Generate all the required data to build an ElasticRod.",
              "Erod", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "Pl", "Polyline shaping the rod.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Supports", "S", "Supports.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Forces", "F", "Forces.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Materials", "M", "Material.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Kappas","K","Remove rest curvatures.",GH_ParamAccess.item, true);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "Data", "XShell data.", GH_ParamAccess.item);
            pManager.AddPointParameter("Vertices", "V", "Vertices", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Indexes", "Idx", "Indexes of the vertices", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = null;
            bool removeCurvatures = true;
            List<SupportData> supports = new List<SupportData>();
            List<ForceData> forces = new List<ForceData>();
            List<MaterialData> materials = new List<MaterialData>();
            DA.GetData(0, ref crv);
            DA.GetDataList(1, supports);
            DA.GetDataList(2, forces);
            DA.GetDataList(3, materials);

            Polyline poly;
            if (crv.TryGetPolyline(out poly))
            {
                ElasticRodData data = new ElasticRodData();
                data.RemoveRestCurvature = removeCurvatures;

                for(int i=0; i<poly.Count; i++)
                {
                    ElasticRodDataFactory.AddPoint(poly[i], ref data);
                }
                if (poly.IsClosed)
                {
                    ElasticRodDataFactory.AddPoint(poly[1], ref data);
                    data.IsPeriodic = true;
                }
                foreach (SupportData s in supports)
                {
                    ElasticRodDataFactory.AddSupport(s, ref data);
                }
                foreach (ForceData f in forces)
                {
                    ElasticRodDataFactory.AddForce(f, ref data);
                }
                foreach (MaterialData m in materials)
                {
                    ElasticRodDataFactory.AddMaterial(m, ref data);
                }

                int jointsCount = data.Nodes.Count;
                int[] indexes = new int[jointsCount];
                Point3d[] pts = new Point3d[jointsCount];
                for (int i = 0; i < jointsCount; i++)
                {
                    indexes[i] = i;
                    pts[i] = data.Nodes[i].GetPoint(0);
                }

                DA.SetData(0, data);
                DA.SetDataList(1, pts);
                DA.SetDataList(2, indexes);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input curve should be a polyline.");
            }
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
            get { return new Guid("935fd11c-deb1-4db8-9fd7-530ab4934cc7"); }
        }
    }
}
