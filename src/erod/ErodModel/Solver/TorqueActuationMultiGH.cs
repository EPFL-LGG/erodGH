using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class TorqueActuationMultiGH : GH_Component
    {
        private bool run;
        private int steps = 1;//, openingSteps = 0;
        private NewtonSolverOpts opts;
        private List<RodLinkage> copies;
        private List<ConvergenceReport> reports;
        private List<int> modelsInEquilibrium;
        private int numModels;
        private List<double> closedAngle;
        private List<double> refAngle;
        private List<double> refStep;
        private List<double> deployedAngle;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TorqueActuationMultiGH()
          : base("TorqueActuationMulti", "TorqueActuationMulti",
            "Deployment via torque actuation for multiple models.",
            "Erod", "Solvers")
        {
            copies = new List<RodLinkage>();
            reports = new List<ConvergenceReport>();
            modelsInEquilibrium = new List<int>();
            refAngle = new List<double>();
            refStep = new List<double>();
            closedAngle = new List<double>();
            deployedAngle = new List<double>();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkages", "Linkages", "Set of linkages to deploy.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Opts", "Opts", "Newton solver options.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "Angle", "Target deployment angle for opening the linkage (in degrees).", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Run", "Run", "Compute equilibrium.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Restart computation.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkages", "Linkages", "List of deployed linkages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Report", "Report", "Convergence report", GH_ParamAccess.list);
        }

        protected override void AfterSolveInstance()
        {
            if (run && (steps <= opts.NumDeploymentSteps))
            {
                GH_Document document = base.OnPingDocument();
                if (document != null)
                {
                    GH_Document.GH_ScheduleDelegate callback = new GH_Document.GH_ScheduleDelegate(this.ScheduleCallback);
                    document.ScheduleSolution(1, callback);
                }
            }
            else if (steps > opts.NumDeploymentSteps) this.Message = numModels + " Deployed Linkages";
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
            List<RodLinkage> models = new List<RodLinkage>();
            bool reset = false;
            run = false;
            List<double> angleDegrees = new List<double>(); ;
            DA.GetDataList(0, models);
            if (!DA.GetData(1, ref opts)) opts = new NewtonSolverOpts(20, 20);
            DA.GetDataList(2, angleDegrees);
            DA.GetData(3, ref run);
            DA.GetData(4, ref reset);

            if (reset || copies.Count == 0)
            {
                copies = new List<RodLinkage>();
                reports = new List<ConvergenceReport>();
                modelsInEquilibrium = new List<int>();
                refAngle = new List<double>();
                refStep = new List<double>();
                closedAngle = new List<double>();
                deployedAngle = new List<double>();

                this.Message = "Reset";
                for (int i = 0; i < models.Count; i++)
                {
                    ElasticModel m = models[i];
                    if (m.ModelType != ElasticModelType.RodLinkage) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input model should be a RodLinkage. The current model is a " + m.ModelType.ToString());
                    if (m != null)
                    {
                        var c = (RodLinkage)m.Clone();
                        copies.Add(c);
                        reports.Add(new ConvergenceReport());

                        double averAng = c.GetAverageJointAngle();
                        double tgtAng = angleDegrees.Count == models.Count ? angleDegrees[i] * Math.PI / 180 : angleDegrees[0] * Math.PI / 180;
                        double stepAng = (tgtAng - averAng) / (opts.NumDeploymentSteps-1);
                        double stepRef = 1.0 / (opts.NumDeploymentSteps - 1);

                        closedAngle.Add(averAng);
                        deployedAngle.Add(tgtAng);
                        refAngle.Add(stepAng);
                        refStep.Add(stepRef);
                    }
                }

                numModels = copies.Count;
                steps = 0;
            }

            if (run)
            {
                if (steps < opts.NumDeploymentSteps)
                {
                    this.Message = "Opening Step " + steps;

                    for (int i = 0; i < numModels; i++)
                    {
                        RodLinkage c = copies[i];
                        double angle = closedAngle[i] + refAngle[i] * steps;

                        double[] forces = c.GetForceVars(opts.IncludeForces);
                        int[] supports = c.GetFixedVars(opts.NumDeploymentSteps, steps, steps * refStep[i]);

                        ConvergenceReport r;
                        NewtonSolver.Optimize(c, supports, forces, opts, out r, true, angle, false);
                        r.OpeningStep = steps;
                        reports[i] = r;
                    }
                    steps++;
                    if (steps == opts.NumDeploymentSteps) this.Message = "Final Step";
                }
                else if (steps == opts.NumDeploymentSteps)
                {
                    for (int i = 0; i < numModels; i++)
                    {
                        RodLinkage c = copies[i];
                        ConvergenceReport r;

                        double[] forces = c.GetForceVars(opts.IncludeForces);
                        int[] supports = c.GetFixedVars(opts.NumDeploymentSteps, steps, 1.0);

                        NewtonSolver.Optimize(c, supports, forces, opts, out r, true, deployedAngle[i], true);
                        r.OpeningStep = steps;
                        reports[i] = r;
                    }
                    steps++;
                }
            }

            DA.SetDataList(0, copies);
            DA.SetDataList(1, reports);

        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.torque_actuation_multi;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6bae462d-9980-4549-934d-d254c227dc8e"); }
        }
    }
}
