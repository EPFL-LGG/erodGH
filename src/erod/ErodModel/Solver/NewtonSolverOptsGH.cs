using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace ErodModel.Model
{
    public class OpenLinkageOptsGH : GH_Component
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
        public OpenLinkageOptsGH()
          : base("SolverOptions", "SolverOpts",
            "Newton solver options.",
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
            pManager.AddNumberParameter("GradTol", "GradTol", "Set the gradient tolerance.", GH_ParamAccess.item, 1e-8);
            pManager.AddNumberParameter("ReleaseStep", "ReleaseStep", "Specify the step at which temporary supports are released; if no step is provided, the middle of the total number of steps is used.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("IncludeForces", "IncludeForces", "Specify whether to include external forces or not.", GH_ParamAccess.item, false);
            pManager[3].Optional = true;
            pManager[4].Optional = true;
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
            bool includeForces = false;
            double gradTol = 1e-8;
            int steps = 20, iter = 20, releaseStep = -1;

            DA.GetData(0, ref steps);
            DA.GetData(1, ref iter);
            DA.GetData(2, ref gradTol);
            DA.GetData(3, ref releaseStep);
            DA.GetData(4, ref includeForces);

            NewtonSolverOpts opts = new NewtonSolverOpts(iter, steps);
            opts.GradTol = gradTol;
            opts.WriteConvergenceReport = reportType;
            opts.IncludeForces = includeForces;
            if(releaseStep!=-1) opts.SetReleaseStep(releaseStep);

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
                return Properties.Resources.Resources.solver_options;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a302fb4c-b409-4b4d-a03e-433d1eb92deb"); }
        }
    }
}
