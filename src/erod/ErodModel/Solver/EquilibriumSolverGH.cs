using System;
using System.Collections.Generic;
using System.Linq;
using ErodDataLib.Types;
using ErodModel.Model;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Solver
{
    public class EquilibriumSolverGH : GH_Component
    {
        private bool run, equilibrium=false;
        private ElasticModel copy;
        private ConvergenceReport report;
        private NewtonSolverOpts options;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public EquilibriumSolverGH()
          : base("EquilibriumSolver", "EquilibriumSolver",
            "Equilibrium solver.",
            "Erod", "Solvers")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ElasticModel", "EModel", "Elastic model to solve equilibrium. Either a rod or a linkage", GH_ParamAccess.item);
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
            pManager.AddGenericParameter("ElasticModel", "EModel", "Elastic model.", GH_ParamAccess.item);
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
            options = new NewtonSolverOpts(20,1);

            DA.GetData(0, ref model);
            DA.GetData(1, ref options);
            DA.GetData(2, ref run);
            DA.GetData(3, ref reset);

            if (reset || copy == null)
            {
                this.Message = "Reset";
                copy = (ElasticModel) model.Clone();
                report = new ConvergenceReport();

                equilibrium = false;
            }

            if (run)
            {
                if (!equilibrium)
                {
                    this.Message = "Computing";

                    double[] forces = copy.GetForceVars(options.IncludeForces);
                    int[] supports = copy.GetFixedVars(false, 0.0); // Don't include temporary support for equilibrium solve

                    equilibrium = NewtonSolver.Optimize(copy, supports, forces, options, out report);
                }
            }

            DA.SetData(0, copy);
            DA.SetData(1, report);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.solver_equilibrium;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("18b9ddc9-257d-4623-af57-438ae3af7b7c"); }
        }
    }
}
