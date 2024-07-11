using System;
using ErodDataLib.Types;
using Grasshopper.Kernel;

namespace ErodData.Interop
{
    public class LoadGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public LoadGH()
          : base("Load", "Load",
            "Load a JSON file with input data to build an elastic model.",
            "Erod", "Interop")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filename", "Filename", "Name of the file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "Data", "Input data of an elastic model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Joints", "Joints", "Joints.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Edges", "Edges", "Edges.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string fileName = "";
            DA.GetData(0, ref fileName);

            LinkageIO data = new LinkageIO(fileName);

            DA.SetData(0, data);
            DA.SetDataList(1, data.Joints);
            DA.SetDataList(2, data.Segments);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.load;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("49973232-97f2-46bc-9e02-0bb5f08e0be3"); }
        }
    }
}
