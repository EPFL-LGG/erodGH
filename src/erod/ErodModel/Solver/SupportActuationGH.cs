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
        private bool run, includeTemporarySupports;
        private int steps = 1;//, openingSteps = 0;
        private RodLinkage copy;
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
            pManager.AddGenericParameter("Opts", "Opts", "Newton solver options.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Compute equilibrium.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Restart computation.", GH_ParamAccess.item);
            pManager[1].Optional = true;
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
            if (run && (steps <= opts.DeploymentSteps))
            {
                GH_Document document = base.OnPingDocument();
                if (document != null)
                {
                    GH_Document.GH_ScheduleDelegate callback = new GH_Document.GH_ScheduleDelegate(this.ScheduleCallback);
                    document.ScheduleSolution(1, callback);
                }
            }
            else if (steps > opts.DeploymentSteps) this.Message = "Linkage Opened";
            else this.Message = "Stop at step " + steps;
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
            RodLinkage model = null;
            bool reset = false;
            run = false;
            DA.GetData(0, ref model);
            if (!DA.GetData(1, ref opts)) opts = new NewtonSolverOpts(20, 20);
            DA.GetData(2, ref run);
            DA.GetData(3, ref reset);

            if (model.ModelType != ElasticModelType.RodLinkage)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input model should be a RodLinkage. The current model is a " + model.ModelType.ToString());
            }
            else
            {
                if (reset || copy == null)
                {
                    this.Message = "Reset";
                    copy = (RodLinkage)model.Clone();
                    report = new ConvergenceReport();
                    includeTemporarySupports = true;

                    steps = 1;
                }

                if (run)
                {
                    if (steps > opts.ReleaseStep) includeTemporarySupports = false;

                    double[] forces = copy.GetForceVars(opts.IncludeForces);
                    int[] supports = copy.GetFixedVars(includeTemporarySupports, steps / opts.DeploymentSteps);

                    if (steps < opts.DeploymentSteps)
                    {
                        this.Message = "Opening Step " + steps;
                        NewtonSolver.Optimize(copy, supports, forces, opts, out report, true, 0, false);
                    }

                    report.OpeningStep = steps;
                    steps++;
                    if (steps == opts.DeploymentSteps) this.Message = "Final Step";

                    else if (steps == opts.DeploymentSteps)
                    {
                        NewtonSolver.Optimize(copy, supports, forces, opts, out report, true, 0, true);
                        report.OpeningStep = steps;
                        steps++;
                    }
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

