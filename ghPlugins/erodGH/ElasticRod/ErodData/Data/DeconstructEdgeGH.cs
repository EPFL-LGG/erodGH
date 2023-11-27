using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.Data
{
    public class DeconstructEdgeGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public DeconstructEdgeGH()
          : base("Deconstruct EdgeData", "Deconstruct EdgeData",
            "Extract the initial data of an edge.",
            "Erod", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Edge", "Edge", "Edge to deconstruct.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes", "Nodes", "Interior nodes.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("StartJoint", "StartJoint", "Joint index at the start of the edge-beam. For terminal edges without a joint at the start, the index is equal to -1.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("EndJoint", "EndJoint", "Joint index at the end of the edge-beam. For terminal edges without a joint at the end, the index is equal to -1.", GH_ParamAccess.item);
            pManager.AddVectorParameter("StartVector", "StartVector", "Vector defining the orientation at the start of the edge-beam.", GH_ParamAccess.item);
            pManager.AddVectorParameter("EndVector", "EndVector", "Vector defining the orientation at the end of the edge-beam.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "Length", "Rest length of the edge-beam", GH_ParamAccess.item);
            pManager.AddTextParameter("Label", "Label", "Label of the edge-beam.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("SplineBeam Index", "SIdx", "Global index of the spline-beam containing this edge-beam. -1 if the edge is Undefined.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("EdgeBeam Index", "EIdx", "Local index of the edge-beam within the spline-beam. -1 if the edge is Undefined.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            SegmentData eData = default;
            DA.GetData(0, ref eData);
            int? jStart = eData.StartJoint;
            if(jStart==-1) jStart = null;
            int? jEnd = eData.EndJoint;
            if (jEnd == -1) jEnd = null;

            DA.SetDataList(0, eData.CurvePoints);
            DA.SetData(1, jStart);
            DA.SetData(2, jEnd);
            DA.SetData(3, eData.GetStartVector());
            DA.SetData(4, eData.GetEndVector());
            DA.SetData(5, eData.RestLength);
            DA.SetData(6, eData.EdgeLabel.ToString());
            DA.SetData(7, eData.SplineBeamGlobalIndex);
            DA.SetData(8, eData.EdgeBeamLocalIndex);
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
            get { return new Guid("211e1ed1-b34d-494c-9286-3d6962abb836"); }
        }
    }
}
