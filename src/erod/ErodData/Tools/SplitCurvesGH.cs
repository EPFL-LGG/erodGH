using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino;
using System.Linq;

namespace ErodData.Tools
{
    public class SplitCurves : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SplitCurves()
          : base("Split", "Split",
            "Split curves by computing the intersection of multiple curves.",
            "Erod", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("CurvesA", "CrvA", "First family of curves.", GH_ParamAccess.list);
            pManager.AddCurveParameter("CurvesB", "CrvB", "Second family of curves.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tol", "Intersection tolerance", GH_ParamAccess.item, 0.001);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("tA", "tA", "Parameters where the intersection with the second family of curves occurred.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("tB", "tB", "Parameters where the intersection with the first family of curves occurred.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> crvA = new List<Curve>();
            List<Curve> crvB = new List<Curve>();
            double tol = 0.001;
            DA.GetDataList(0, crvA);
            DA.GetDataList(1, crvB);
            DA.GetData(2, ref tol);

            GH_Structure<GH_Number> tA = new GH_Structure<GH_Number>();
            for (int i = 0; i < crvA.Count; i++)
            {
                Curve ca = crvA[i];
                List<GH_Number> param = new List<GH_Number>();

                for (int j = 0; j < crvB.Count; j++)
                {
                    Curve cb = crvB[j];

                    CurveIntersections inter = Intersection.CurveCurve(ca, cb, tol, tol);

                    if (inter.Count > 0)
                    {
                        foreach (var e in inter)
                        {
                            param.Add(new GH_Number(e.ParameterA));
                        }
                    }
                }

                double t0 = ca.Domain.T0 + 1e-3;
                double t1 = ca.Domain.T1 - 1e-3;
                List<GH_Number> sortParam = param.Where(val => val.Value > t0 && val.Value < t1).OrderBy(o => o.Value).ToList();

                tA.AppendRange(sortParam, new GH_Path(i));
            }

            GH_Structure<GH_Number> tB = new GH_Structure<GH_Number>();
            for (int i = 0; i < crvB.Count; i++)
            {
                Curve cb = crvB[i];
                List<GH_Number> param = new List<GH_Number>();

                for (int j = 0; j < crvA.Count; j++)
                {
                    Curve ca = crvA[j];

                    CurveIntersections inter = Intersection.CurveCurve(cb, ca, tol, tol);

                    if (inter.Count > 0)
                    {
                        foreach (var e in inter)
                        {
                            param.Add(new GH_Number(e.ParameterA));
                        }
                    }
                }

                double t0 = cb.Domain.T0 + 1e-3;
                double t1 = cb.Domain.T1 - 1e-3;
                List<GH_Number> sortParam = param.Where(val => val.Value > t0 && val.Value < t1).OrderBy(o => o.Value).ToList();

                tB.AppendRange(sortParam, new GH_Path(i));
            }

            DA.SetDataTree(0, tA);
            DA.SetDataTree(1, tB);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.split_curves;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cb8dc93b-8236-479b-adca-878a70c9d6df"); }
        }
    }
}
