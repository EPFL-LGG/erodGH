using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.IO
{
  public class TargetSurfaceGH : GH_Component
  {
        /// <summary>
        /// Initializes a new instance of the Edge class.
        /// </summary>
        public TargetSurfaceGH()
          : base("TargetSurface", "TargetSrf",
              "Set a target surface from a mesh to attract the linkage.",
              "Erod", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh approximating the target surface.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight", "Weight", "Weight of the attraction term.", GH_ParamAccess.item, 0.0001);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("TargetSurfaceIO", "TargetSrfIO", "Targetn surface data.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            double weight = 0.0001;
            DA.GetData(0, ref mesh);
            DA.GetData(1, ref weight);

            TargetSurfaceIO data = new TargetSurfaceIO(mesh, weight);

            DA.SetData(0, data);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.target_io;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
    {
      get { return new Guid("e215c17d-66e4-4dea-a382-162a1fa70065"); }
    }
  }
}
