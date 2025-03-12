using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Solver
{
    public class CableActuationGH : GH_Component
    {
        private bool run, equilibrium;
        private int steps = 1, numIterations;
        private Line[] cables;
        private ElasticModel copy;
        private ConvergenceReport report;
        private NewtonSolverOpts options;
        private double maxBeta = 1e-2, minBeta=1e-8, refBeta = 0;
        NewtonSolverOpts optionsCopy;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public CableActuationGH()
          : base("CableActuation", "CableActuation",
            "Deployment via cable actuation.",
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
            pManager.AddGenericParameter("Cables", "Cables", "Cables represented as lines.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Beta","Beta", "Beta", GH_ParamAccess.item);
        }

        protected override void AfterSolveInstance()
        {
            if (run && (steps <= options.NumDeploymentSteps) && !equilibrium)
            {
                GH_Document document = base.OnPingDocument();
                if (document != null)
                {
                    GH_Document.GH_ScheduleDelegate callback = new GH_Document.GH_ScheduleDelegate(this.ScheduleCallback);
                    document.ScheduleSolution(1, callback);
                }
            }
            else if (equilibrium) this.Message = "Equilibrium Found";
            else if (steps > options.NumDeploymentSteps) this.Message = "Model Deployed";
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
            ElasticModel model = null;
            bool reset = false;
            run = false;
            options = new NewtonSolverOpts(500, 1);
            options.WriteConvergenceReport = 1;

            DA.GetData(0, ref model);
            DA.GetData(1, ref options);
            DA.GetData(2, ref run);
            DA.GetData(3, ref reset);

            if (model.ContainsTemporarySupports() && !model.ContainsRollingSupports()) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Temporary supports detected. This solver only operates with permanent supports. Temporary supports will be disabled.");
            if (model.ContainsRollingSupports() && !model.ContainsTemporarySupports()) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Rolling supports detected. This solver only operates with fixed supports. Rolling supports will be fixed.");
            if (model.ContainsTemporarySupports() && model.ContainsRollingSupports()) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Rolling and temporary supports detected. This solver only operates with fixed and permanent supports. Rolling supports will be fixed and temporary supports will be disabled.");

            if (reset || copy == null)
            {
                this.Message = "Reset";
                copy = (ElasticModel) model.Clone();
                report = new ConvergenceReport();
                cables = copy.GetCablesAsLines();

                // Make a copy of netwon options
                optionsCopy = (NewtonSolverOpts) options.Duplicate();

                numIterations = options.NumIterations;
                minBeta = options.Beta; // For calculating newton step

                equilibrium = false;
                if (minBeta > maxBeta) maxBeta = minBeta;
                refBeta = Math.Abs((maxBeta - minBeta)) / (options.NumDeploymentSteps*options.NumIterations);
                steps = 1;
            }

            if (run)
            {
                // Cables are included in the simulation as external forces. Thus, the number of sub iterations needs to be 1 because positions need to be updated.
                // TODO Add cable forces in CPP
                optionsCopy.NumIterations = 1;

                if (steps <= options.NumDeploymentSteps && !equilibrium)
                {
                    this.Message = "Opening Step " + steps;

                    for (int i = 0; i < 1; i++)// options.NumIterations; i++)
                    {
                        double[] forces = copy.GetForceVars(options.IncludeForces, true);
                        int[] supports = copy.GetFixedVars(false, 0.0);

                        equilibrium = NewtonSolver.Optimize(copy, supports, forces, optionsCopy, out report);
                        if (copy.ModelType == ElasticModelType.RodLinkage)
                        {
                            forces = copy.GetForceVars(options.IncludeForces, false);
                            equilibrium = NewtonSolver.Optimize(copy, supports, forces, options, out report, true, ((RodLinkage)copy).GetAverageJointAngle());
                        }
                        cables = copy.GetCablesAsLines();
                        minBeta += refBeta;
                        //optionsCopy.Beta = minBeta;

                        if (equilibrium) break;
                    }

                    report.OpeningStep = steps;
                    steps++;
                }
            }

            DA.SetData(0, copy);
            DA.SetData(1, report);
            DA.SetDataList(2, cables);
            DA.SetData(3, minBeta);
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
                return Properties.Resources.Resources.cable_actuation;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cc85e2f7-b119-4315-97ab-a6b122953714"); }
        }
    }
}
