using System;
using System.Collections.Generic;
using System.Linq;
using ErodDataLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodData.IO
{
    public class eTopologyGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public eTopologyGH()
          : base("eTopology", "eTopology",
            "Deconstruct the topology of a linkage.",
            "Erod", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("LinkageIO", "LinkageIO", "LinkageIO to extract the topological information.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Vertices", "Vertices", "Vertices of the edge graph that defines the linkage.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("IsJoint", "IsJoint", "Collection of flags indicating joints. <True> if the vertex is a joint.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Topology", "Topology", "Incident edges.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            LinkageIO data = null;
            DA.GetData(0, ref data);

            int numNodes = data.Graph.NumNodes;
            bool[] isJoint = new bool[numNodes];

            // Calculate the average point
            GH_Structure<GH_Integer> result = new GH_Structure<GH_Integer>();
            for(int i=0; i< numNodes; i++)
            {
                var edges = data.Graph.GetIncidentEdges(i);
                for (int j = 0; j < edges.Length; j++) result.Append(new GH_Integer(edges[j]), new GH_Path(i));

                isJoint[i] = edges.Length >= 2 && edges.Length <= 4;
            }

            DA.SetDataList(0, data.Graph.Nodes);
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
                return Properties.Resources.Resources.topology_data_deconstruct;
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
