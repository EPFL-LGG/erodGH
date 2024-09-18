using System;
using System.Collections.Generic;
using System.Linq;
using ErodDataLib.Types;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.IO
{
    public class RodIOGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RodIOGH()
          : base("RodIO", "RodIO",
              "Assemble all input data to construct an elastic rod.",
              "Erod", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "Pl", "Polyline defining the elastic rod.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Supports", "Supports", "Set of support conditions.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "Material", "Material required to build the elastic rod.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("IsStraight", "IsStraight", "Remove rest curvatures.",GH_ParamAccess.item, true);
            pManager.AddGenericParameter("Forces", "Forces", "Set of forces.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RodIO", "RodIO", "Elastic rod initialization data.", GH_ParamAccess.item);
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
            List<SupportIO> supports = new List<SupportIO>();
            List<ForceIO> forces = new List<ForceIO>();
            List<MaterialIO> materials = new List<MaterialIO>();
            DA.GetData(0, ref crv);
            DA.GetDataList(1, supports);
            DA.GetDataList(2, materials);
            DA.GetData(3, ref removeCurvatures);
            DA.GetDataList(4, forces);

            Polyline poly;
            if (crv.TryGetPolyline(out poly))
            {
                if (supports.Count > 0 && supports.Where(sp => !sp.IsTemporary).Count() == 0) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Only temporary supports have been found. The first temporary support is converted to a permanent support.");

                RodIO data = new RodIO(poly, removeCurvatures);
                data.AddSupports(supports);
                data.AddMaterials(materials);
                data.AddForces(forces);

                DA.SetData(0, data);
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
                return Properties.Resources.Resources.rod_io;
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
