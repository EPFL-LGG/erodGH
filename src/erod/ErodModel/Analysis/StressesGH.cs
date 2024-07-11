using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class StressesGH : GH_Component
    {
        PointCloud vertices, edgeMidPts;
        List<double> twisting, maxBend, minBend, sqrtBend, stretching, vonMises;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StressesGH()
          : base("Stresses ", "Stresses ",
            "Computes the stresses of an elastic rod or linkage",
            "Erod", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Input the model to compute energies. The model should be either an elastic rod, a rod segment of a linkage or an elastic linkage.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes", "Nodes", "Sample points where twisting and bending stresses are measured.", GH_ParamAccess.list);
            pManager.AddPointParameter("EdgeNodes", "EdgeNodes", "Sample points where stretching stress is measured.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stretching", "Stretching", "Per edge stretching stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Twisting", "Twisting", "Per node twisting stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("MaxBending", "MaxBend", "Per node maximum bending stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("MinBending", "MinBend", "Per node minimum bending stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("SqrtBending", "SqrtBend", "Per node square-root bending stresses.", GH_ParamAccess.list);
            pManager.AddNumberParameter("VonMises", "VonMises", "Per node Von-Mises stresses.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object obj = null;
            DA.GetData(0, ref obj);

            vertices = new PointCloud();
            edgeMidPts = new PointCloud();
            twisting = new List<double>();
            maxBend = new List<double>();
            minBend = new List<double>();
            sqrtBend = new List<double>();
            stretching = new List<double>();
            vonMises = new List<double>();

            if (obj is RodLinkage)
            {
                RodLinkage model = null;

                foreach(RodSegment rod in model.Segments)
                {
                    double[] coords = rod.GetCenterLineCoordinates();
                    int numVertices = coords.Length / 3;

                    double[] tempTwisting = rod.GetTwistingStresses();
                    double[] tempSqrtBend = rod.GetSqrtBendingEnergies();
                    double[] tempMaxBend = rod.GetMaxBendingStresses();
                    double[] tempMinBend = rod.GetMinBendingStresses();
                    double[] tempStretching = rod.GetStretchingStresses();
                    double[] tempVonMises = rod.GetVonMisesStresses();

                    CollectStresses(numVertices, coords, tempTwisting, tempMaxBend, tempMinBend, tempSqrtBend, tempVonMises, tempStretching);
                }
            }
            else if (obj is RodSegment)
            {
                RodSegment rod = (RodSegment)obj;
                double[] coords = rod.GetCenterLineCoordinates();
                int numVertices = coords.Length / 3;

                double[] tempStretching = rod.GetStretchingStresses();
                double[] tempTwisting = rod.GetTwistingStresses();
                double[] tempMaxBend = rod.GetMaxBendingStresses();
                double[] tempMinBend = rod.GetMinBendingStresses();
                double[] tempSqrtBend = rod.GetSqrtBendingEnergies();
                double[] tempVonMises = rod.GetVonMisesStresses();

                CollectStresses(numVertices, coords, tempTwisting, tempMaxBend, tempMinBend, tempSqrtBend, tempVonMises, tempStretching);
            }else if(obj is ElasticRod)
            {
                ElasticRod rod = (ElasticRod)obj;
                double[] coords = rod.GetCenterLineCoordinates();
                int numVertices = coords.Length / 3;

                double[] tempStretching = rod.GetStretchingStresses();
                double[] tempTwisting = rod.GetTwistingStresses();
                double[] tempMaxBend = rod.GetMaxBendingStresses();
                double[] tempMinBend = rod.GetMinBendingStresses();
                double[] tempSqrtBend = rod.GetSqrtBendingEnergies();
                double[] tempVonMises = rod.GetVonMisesStresses();

                CollectStresses(numVertices, coords, tempTwisting, tempMaxBend, tempMinBend, tempSqrtBend, tempVonMises, tempStretching);
            }
            else throw new Exception("Invalid input type. The type should be an elastic rod, a rod segment of an elastic linkage or an elastic linkage.");

            DA.SetDataList(0, vertices.GetPoints());
            DA.SetDataList(1, edgeMidPts.GetPoints());
            DA.SetDataList(2, stretching);
            DA.SetDataList(3, twisting);
            DA.SetDataList(4, maxBend);
            DA.SetDataList(5, minBend);
            DA.SetDataList(6, sqrtBend);
            DA.SetDataList(7, vonMises);
        }

        private void CollectStresses(int numVertices, double[] coords, double[] tempTwisting, double[] tempMaxBend, double[] tempMinBend, double[] tempSqrtBend, double[] tempVonMises, double[] tempStretching)
        {
            // Stresses per vertex
            for (int j = 0; j < numVertices; j++)
            {
                Point3d p0 = new Point3d(coords[j * 3], coords[j * 3 + 1], coords[j * 3 + 2]);
                int idx = vertices.ClosestPoint(p0);
                if (idx == -1 || p0.DistanceTo(vertices[idx].Location) > 0.01)
                {
                    vertices.Add(p0);
                    twisting.Add(tempTwisting[j]);
                    maxBend.Add(tempMaxBend[j]);
                    minBend.Add(tempMinBend[j]);
                    sqrtBend.Add(tempSqrtBend[j]);
                    vonMises.Add(tempVonMises[j]);
                }

                // Stresses per edge
                if (j < numVertices - 1)
                {
                    Point3d p1 = new Point3d(coords[(j + 1) * 3], coords[(j + 1) * 3 + 1], coords[(j + 1) * 3 + 2]);
                    Point3d mid = (p0 + p1) / 2;
                    idx = edgeMidPts.ClosestPoint(mid);
                    if (idx == -1 || mid.DistanceTo(edgeMidPts[idx].Location) > 0.01)
                    {
                        edgeMidPts.Add(mid);
                        stretching.Add(tempStretching[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.stresses;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0a3ebfca-86a5-457e-95ce-94417289b6aa"); }
        }
    }
}
