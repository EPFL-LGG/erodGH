using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class OpenLinkageWithCopiesGH : GH_Component
    {
        private bool run;
        private int steps = 1;//, openingSteps = 0;
        private RodLinkage mainCopy;
        private NewtonSolverOpts opts;
        private ConvergenceReport report;
        private List<RodLinkage> copies;

        double closedAngle = 0, refAngle = 0;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public OpenLinkageWithCopiesGH()
          : base("SolverStepDeployment", "SolverStepDeployment",
            "Open a linkage by generating a copy at each opening step.",
            "Erod", "Model")
        {
            copies = new List<RodLinkage>();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Opts", "Opts", "Newton solver options.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "Angle", "Target deployment angle for opening the linkage (in degrees).", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Compute equilibrium.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Restart computation.", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Models", "Models", "List of RodLinkage Model for each opening step.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Report", "Report", "Convergence report", GH_ParamAccess.item);
        }

        protected override void AfterSolveInstance()
        {
            if (run && (steps <= opts.OpeningSteps))
            {
                GH_Document document = base.OnPingDocument();
                if (document != null)
                {
                    GH_Document.GH_ScheduleDelegate callback = new GH_Document.GH_ScheduleDelegate(this.ScheduleCallback);
                    document.ScheduleSolution(1, callback);
                }
            }
            else if (steps > opts.OpeningSteps) this.Message = "Linkage Opened";
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
            double angleDegrees = 0;
            DA.GetData(0, ref model);
            if (!DA.GetData(1, ref opts)) opts = new NewtonSolverOpts(20,20);
            DA.GetData(2, ref angleDegrees);
            DA.GetData(3, ref run);
            DA.GetData(4, ref reset);

            if (model.ModelType != ModelTypes.RodLinkage)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input model should be a RodLinkage. The current model is a " + model.ModelType.ToString());
            }
            else
            {
                double deployedAngle = angleDegrees * Math.PI / 180;

                if (reset || mainCopy == null)
                {
                    this.Message = "Reset";
                    mainCopy = (RodLinkage)model.Clone();
                    report = new ConvergenceReport();

                    closedAngle = mainCopy.GetAverageJointAngle();
                    refAngle = (deployedAngle - closedAngle) / opts.OpeningSteps;
                    steps = 1;

                    copies = new List<RodLinkage>();
                    copies.Add((RodLinkage)mainCopy.Clone());
                }

                if (run)
                {
                    if (steps < opts.OpeningSteps)
                    {
                        this.Message = "Opening Step " + steps;
                        double angle = closedAngle + refAngle * steps;

                        NewtonSolver.Optimize(mainCopy, opts, out report, true, angle);
                        copies.Add((RodLinkage)mainCopy.Clone());
 
                        report.OpeningStep = steps;
                        steps++;
                        if (steps == opts.OpeningSteps) this.Message = "Final Step";
                    }
                    else if (steps == opts.OpeningSteps)
                    {
                        NewtonSolver.Optimize(mainCopy, opts, out report, true, deployedAngle, true);
                        copies.Add((RodLinkage)mainCopy.Clone());
                        report.OpeningStep = steps;
                        steps++;
                    }
                }

                DA.SetDataList(0, copies);
                DA.SetData(1, report);
            }
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
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("59202505-c586-4b36-bb78-483f40666df4"); }
        }
    }
}
