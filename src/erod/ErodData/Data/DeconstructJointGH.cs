using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.Data
{
    public class DeconstructJointGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public DeconstructJointGH()
          : base("Deconstruct JointData", "Deconstruct JointData",
            "Extract the initial data of a joint.",
            "Erod", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint", "Joint", "Joint to deconstruct.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "Pos", "Position of the joint.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Normal", "Normal", "Joint normal. Null if the normal is not initialized.", GH_ParamAccess.item);
            pManager.AddVectorParameter("EdgeVectorA", "EdgeA", "Edge vector associated with rod label A.", GH_ParamAccess.item);
            pManager.AddVectorParameter("EdgeVectorB", "EdgeB", "Edge vector associated with rod label B.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SegmentsA", "SegmentsA", "Segments associated with rod label A", GH_ParamAccess.list);
            pManager.AddIntegerParameter("SegmentsB", "SegmentsB", "Segments associated with rod label B", GH_ParamAccess.list);
            pManager.AddBooleanParameter("StartA", "StartA", "True if this joint is the origin of the corresponding segment A.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("StartB", "StartB", "True if this joint is the origin of the corresponding segment B.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            JointData jData = default;
            DA.GetData(0, ref jData);

            DA.SetData(0, jData.Position);
            DA.SetData(1, jData.Normal);
            DA.SetData(2, jData.EdgeA);
            DA.SetData(3, jData.EdgeB);
            DA.SetDataList(4, jData.SegmentsA);
            DA.SetDataList(5, jData.SegmentsB);
            DA.SetDataList(6, jData.IsStartA);
            DA.SetDataList(7, jData.IsStartB);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary; }
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
            get { return new Guid("fcf4b14f-0ecd-4048-a89d-c9538e90438e"); }
        }
    }
}
