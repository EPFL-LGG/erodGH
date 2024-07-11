using System;
using ErodDataLib.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.IO
{
    public class CableGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ForceVectorGH class.
        /// </summary>
        public CableGH()
          : base("Cable", "Cable",
              "Compute forces excerted by an elastic cable.",
              "Erod", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Line", "Line representing the elastic cable.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "Length", "Rest-length of the cable. If no value is provided, the length of the line is taken.", GH_ParamAccess.item);
            pManager.AddNumberParameter("E", "E", "Young's modulus.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Diameter", "Diameter", "# Diameter of the cable's cross-section.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cable", "Cable", "Cable element.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line e = new Line();
            double restLength = 0, E = 1, Diameter = 1; 
            DA.GetData(0, ref e);
            if(!DA.GetData(1, ref restLength)) restLength=e.Length;
            DA.GetData(2, ref E);
            DA.GetData(3, ref Diameter);

            double area = Math.PI * Math.Pow(Diameter / 2, 2);
            restLength += 1e-6; // To avoid dividing by zero

            ForceCableIO force = new ForceCableIO(e, E, area, restLength);

            DA.SetData(0, force);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.cable_io;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("da39b646-acc7-45a4-bb4f-9f7ce4b73e4b"); }
        }
    }
}

