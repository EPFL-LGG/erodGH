using System;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;
using ErodDataLib.Types;

namespace ErodModel.Model
{
    public class SupportActuationGH : GH_Component
    {
        private bool run, equilibrium = false;
        private double refParam = 0.0;
        private ElasticModel copy;
        private NewtonSolverOpts opts;
        private ConvergenceReport report;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SupportActuationGH()
          : base("SupportActuation", "SupportActuation",
            "Deployment via support actuation.",
            "Erod", "Solvers")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ElasticModel", "EModel", "Elastic model to actuate. Either a rod or a linkage", GH_ParamAccess.item);
            pManager.AddNumberParameter("StepSize", "StepSize", "Step size to move the support.", GH_ParamAccess.item, 0.01);
            pManager.AddGenericParameter("Opts", "Opts", "Newton solver options.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Compute equilibrium.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Restart computation.", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ElasticModel", "EModel", "Deployed elastic model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Report", "Report", "Convergence report", GH_ParamAccess.item);
        }

        protected override void AfterSolveInstance()
        {
            if (run && !equilibrium)
            {
                GH_Document document = base.OnPingDocument();
                if (document != null)
                {
                    GH_Document.GH_ScheduleDelegate callback = new GH_Document.GH_ScheduleDelegate(this.ScheduleCallback);
                    document.ScheduleSolution(1, callback);
                }
            }
            else if (equilibrium) this.Message = "Equilibrium";
            else this.Message = "Stop";
        }

        private void ScheduleCallback(GH_Document doc)
        {
            this.ExpireSolution(false);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ElasticModel model = null;
            bool reset = false;
            run = false;
            double stepSize = 0.01;
            DA.GetData(0, ref model);
            DA.GetData(1, ref stepSize);
            if (!DA.GetData(2, ref opts)) opts = new NewtonSolverOpts(20, 20);
            DA.GetData(3, ref run);
            DA.GetData(4, ref reset);

            if (reset || copy == null)
            {
                this.Message = "Reset";
                copy = (ElasticModel) model.Clone();

                report = new ConvergenceReport();

                equilibrium = false;
                refParam = 0;
            }

            if (run)
            {
                if (!equilibrium)
                {
                    this.Message = "Computing";

                    double[] forces = copy.GetForceVars(opts.IncludeForces);
                    int[] supports = copy.GetFixedVars(100, (int) (100*refParam), refParam);

                    bool flag = NewtonSolver.Optimize(copy, supports, forces, opts, out report, true, 0);

                    if (refParam > 1.0)
                    {
                        refParam = 1.0;
                        if (flag) equilibrium = true;
                    }
                    else refParam += Math.Abs(stepSize);
                }
            }

            DA.SetData(0, copy);
            DA.SetData(1, report);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.support_actuation;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ed855f8d-440d-47cb-a26f-75650e1a76c1"); }
        }
    }
}

