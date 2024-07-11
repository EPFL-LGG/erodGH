using System;
using System.Collections.Generic;
using ErodModel.Model;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Solver
{
    public class OpenLinkageOptsExtGH : GH_Component
    {
        int reportType;
        List<List<string>> reportAttributes;
        List<string> selection;
        bool buildAttributes = true;
        readonly List<string> categories = new List<string>(new string[] { "Convergence Report" });
        readonly List<string> reportContent = new List<string>(new string[]
        {
            "No Report",
            "Last-Step Report",
            "Step-By-Step Report"
        });

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public OpenLinkageOptsExtGH()
          : base("ExtendedSolverOptions", "ExSolverOpts",
            "Extended Newton solver options.",
            "Erod", "Solvers")
        {
        }

        public override void CreateAttributes()
        {
            if (buildAttributes)
            {
                FunctionToSetSelectedContent(0, 0);
                buildAttributes = false;
            }
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, reportAttributes, selection, categories);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (reportAttributes == null)
            {
                reportAttributes = new List<List<string>>();
                selection = new List<string>();
                reportAttributes.Add(reportContent);
                selection.Add(reportContent[reportType]);
            }

            if (dropdownListId == 0)
            {
                reportType = selectedItemId;
                selection[0] = reportAttributes[0][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Steps", "Steps", "Number of deployment steps. Deployment steps are not used for equilibrium solve.", GH_ParamAccess.item, 20);
            pManager.AddIntegerParameter("Iter", "Iterations", "Maximum number of newton iterations per opening step.", GH_ParamAccess.item, 20);
            pManager.AddIntegerParameter("IterMult", "IterMultiplier", "Iteration multiplier for the last deployment step.", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("GradTol", "GradTol", "Set the gradient tolerance.", GH_ParamAccess.item, 1e-8);
            pManager.AddNumberParameter("Beta", "Beta", "Set the minimum beta for calculating a Newton step.", GH_ParamAccess.item, 1e-8);
            pManager.AddBooleanParameter("UseIdentityMetric", "IdentityMetric", "Whether to force the use of the identity matrix for Hessian modification (instead of the problem's custom metric).", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("UseNegativeCurvatureDirection", "NegCurvDir", "Whether to compute and move in negative curvature directions to escape from saddle points.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("FeasibilitySolve", "FeasibilitySolve", "Whether to solve for a feasible starting point or rely on the problem to jump to feasible parameters.", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("ReleaseStep", "ReleaseStep", "Specify the step at which temporary supports are released; if no step is provided, the middle of the total number of steps is used.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("IncludeForces", "IncludeForces", "Specify whether to include external forces or not.", GH_ParamAccess.item, false);
            pManager[8].Optional = true;
            pManager[9].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("NewtonSolverOpts", "NewtonSolverOpts", "Newton solver options.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool verbose = true, useIdentityMetric = false, verboseNonPosDef = true,
                useNegativeCurvatureDirection = true, feasibilitySolve = true, includeForces = false;
            double gradTol = 1e-8, beta = 1e-8;
            int steps = 20, iter = 20, iterMult = 1, releaseStep = -1;

            DA.GetData(0, ref steps);
            DA.GetData(1, ref iter);
            DA.GetData(2, ref iterMult);
            DA.GetData(3, ref gradTol);
            DA.GetData(4, ref beta);
            DA.GetData(5, ref useIdentityMetric);
            DA.GetData(6, ref useNegativeCurvatureDirection);
            DA.GetData(7, ref feasibilitySolve);
            DA.GetData(8, ref releaseStep);
            DA.GetData(9, ref includeForces);

            NewtonSolverOpts opts = new NewtonSolverOpts(iter, steps);
            opts.GradTol = gradTol;
            opts.Beta = beta;
            opts.UseIdentityMetric = useIdentityMetric;
            opts.UseNegativeCurvatureDirection = useNegativeCurvatureDirection;
            opts.FeasibilitySolve = feasibilitySolve;
            opts.VerboseNonPosDef = verboseNonPosDef;
            opts.WriteConvergenceReport = reportType;
            opts.Verbose = verbose;
            opts.IncludeForces = includeForces;
            opts.IterationMultiplier = iterMult;
            if (releaseStep != -1) opts.SetReleaseStep(releaseStep);

            DA.SetData(0, opts);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("reportType", reportType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("reportType", ref reportType))
            {
                FunctionToSetSelectedContent(0, reportType);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, reportAttributes, selection, categories);
            }
            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.solver_options_extended;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7c01dd8c-84d8-4f4f-a1ed-fcb48b726f21"); }
        }
    }
}
