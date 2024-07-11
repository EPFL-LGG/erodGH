using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodData.Tools
{
    public class EdgeTopologyGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public EdgeTopologyGH()
          : base("EdgeTopology", "EdgeTopology",
            "Build the topology of a collection of curves.",
            "Erod", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Lines", "Ln", "Collection of curves.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Topology", "Topology", "Resulting topology from the collection of curves.", GH_ParamAccess.tree);
            pManager.AddPointParameter("Vertices", "Vertices", "List of unique vertices.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> lines = new List<Curve>();
            DA.GetDataList(0, lines);

            PointCloud cloud = new PointCloud();
            Dictionary<int, HashSet<int>> topo = new Dictionary<int, HashSet<int>>();

            foreach (Curve ln in lines)
            {

                Point3d p1 = ln.PointAtStart;
                Point3d p2 = ln.PointAtEnd;

                int idx1 = cloud.ClosestPoint(p1);
                if (idx1 == -1)
                {
                    cloud.Add(p1);
                    idx1 = 0;
                }
                else
                {
                    if (p1.DistanceTo(cloud[idx1].Location) > 0.01)
                    {
                        cloud.Add(p1);
                        idx1 = cloud.Count - 1;
                    }
                }

                int idx2 = cloud.ClosestPoint(p2);
                if (p2.DistanceTo(cloud[idx2].Location) > 0.01)
                {
                    cloud.Add(p2);
                    idx2 = cloud.Count - 1;
                }

                if (!topo.ContainsKey(idx1)) topo.Add(idx1, new HashSet<int>());
                if (!topo.ContainsKey(idx2)) topo.Add(idx2, new HashSet<int>());
                topo[idx1].Add(idx2);
                topo[idx2].Add(idx1);
            }

            // Calculate the average point
            GH_Structure<GH_Integer> sortedTopo = new GH_Structure<GH_Integer>();
            foreach (int key in topo.Keys)
            {
                List<int> indexes = topo[key].ToList();
                int count = indexes.Count;
                Point3d ctr = new Point3d();
                Point3d[] pts = new Point3d[count];
                for (int i = 0; i < count; i++)
                {
                    pts[i] = cloud[indexes[i]].Location;
                    ctr += pts[i];
                }
                ctr /= pts.Length;

                Plane pl;
                Plane.FitPlaneToPoints(pts, out pl);

                List<double> ang = new List<double>();
                for (int i = 0; i < pts.Length; i++)
                {
                    Vector3d v = pts[i] - ctr;
                    v.Unitize();

                    double dX = pl.XAxis * v;
                    double dY = pl.YAxis * v;

                    ang.Add(Math.Atan2(dX, dY));
                }

                var temp = ang
                    .Select((angle, idx) => new KeyValuePair<double, int>(angle, idx))
                    .OrderBy(pair => pair.Key)
                    .ToList();

                GH_Integer[] sortedIdx = new GH_Integer[count];
                for (int i = 0; i < pts.Length; i++)
                {
                    sortedIdx[i] = new GH_Integer(indexes[temp[i].Value]);
                }

                sortedTopo.AppendRange(sortedIdx, new GH_Path(key));
            }

            DA.SetDataTree(0, sortedTopo);
            DA.SetDataList(1, cloud.GetPoints());
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.topology;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ed5bb9ca-a45f-4e21-9d9c-d0cc9406e311"); }
        }
    }
}
