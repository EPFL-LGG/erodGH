using System;
using System.Collections.Generic;
using System.Linq;
using ErodDataLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodData.Data
{
    public class DeconstructTopologyGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public DeconstructTopologyGH()
          : base("Deconstruct Topology", "Deconstruct Topology",
            "Deconstruct the topology of a linkage.",
            "Erod", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "Data", "Linkage to deconstruct.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Vertices", "Vertices", "Vertices in the linkage.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("IsJoint", "IsJoint", "Retunr true if the vertex is a joint.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Topology", "Topology", "Resulting topology from the collection of lines.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkageData data = null;
            DA.GetData(0, ref data);

            Dictionary<int, HashSet<int>> incidentEdges = data.GetIncidentEdges();
            bool[] isJoint = new bool[incidentEdges.Count()];

            // Calculate the average point
            GH_Structure<GH_Integer> result = new GH_Structure<GH_Integer>();
            foreach (int key in incidentEdges.Keys)
            {
                var edges = incidentEdges[key];
                int count = edges.Count();
                GH_Path path = new GH_Path(key);

                for (int i = 0; i < count; i++)
                {
                    result.Append(new GH_Integer(edges.ElementAt(i)), path);
                }

                isJoint[key] = count >= 2 && count <= 4;
            }

            DA.SetDataList(0, data.GetVertices());
            DA.SetDataList(1, isJoint);
            DA.SetDataTree(2, result);
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

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("20ff8177-5b83-48af-a9d6-6307a874f0e9"); }
        }
    }
}
