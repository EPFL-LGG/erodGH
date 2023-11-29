using System;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using System.Collections.Generic;

namespace ErodModel.Model
{
	public class LiveNewtonSolverGH : GH_Component
	{
        private bool run;
        private RodLinkage copy;
        private ConvergenceReport report;
        private NewtonSolverOpts options;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public LiveNewtonSolverGH()
          : base("LiveSolverEquilibrium", "LiveSolverEquilibrium",
            "Compute equilibrium.",
            "Erod", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Elastic Model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Opts", "Opts", "Newton solver options.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Compute equilibrium.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Restart computation.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save", "Save", "Save current state.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("JointIndex", "jIdx", "Joint index to slide.", GH_ParamAccess.list);
            pManager.AddNumberParameter("t","t","", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Report", "Report", "Convergence report", GH_ParamAccess.item);
            pManager.AddNumberParameter("ResthLengthsDoFs", "ResthLengthsDoFs", "", GH_ParamAccess.item);
        }

        protected override void AfterSolveInstance()
        {
            if (run)
            {
                GH_Document document = base.OnPingDocument();
                if (document != null)
                {
                    GH_Document.GH_ScheduleDelegate callback = new GH_Document.GH_ScheduleDelegate(this.ScheduleCallback);
                    document.ScheduleSolution(1, callback);
                }
            }
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
            RodLinkage model = null;
            bool reset = false, save = false;
            run = false;
            options = new NewtonSolverOpts(20, 1);
            List<int> jIdx = new List<int>();
            List<double> t = new List<double>();

            DA.GetData(0, ref model);
            DA.GetData(1, ref options);
            DA.GetData(2, ref run);
            DA.GetData(3, ref reset);
            DA.GetData(4, ref save);
            DA.GetDataList(5, jIdx);
            DA.GetDataList(6, t);

            if (reset || copy == null)
            {
                this.Message = "Reset";
                copy = (RodLinkage)model.Clone();
                report = new ConvergenceReport();
            }

            if (run)
            {
                this.Message = "Computing";

                if (jIdx.Count>0)
                {
                    double[] dofs = copy.GetRestLenghtsSolveDoFs();

                    for (int i = 0; i < jIdx.Count; i++)
                    {
                        int e0 = -1, e1 = -1;
                        switch (jIdx[i])
                        {
                            case 0:
                                e0 = 10;
                                e1 = 11;
                                break;
                            case 1:
                                e0 = 0;
                                e1 = 1;
                                break;
                            case 2:
                                e0 = 7;
                                e1 = 6;
                                break;
                            case 3:
                                e0 = 4;
                                e1 = 5;
                                break;
                            case 4:
                                e0 = 9;
                                e1 = 8;
                                break;
                        }

                        if (e0 != -1)
                        {
                            double l0 = 20 * t[i];
                            double l1 = 20 - l0;
                            dofs[dofs.Length - 12 + e0] = l0;
                            dofs[dofs.Length - 12 + e1] = l1;
                        }
                    }
                    copy.SetRestLenghtsSolveDoFs(dofs);
                }

                NewtonSolver.Optimize(copy, options, out report);
            }

            DA.SetData(0, copy);
            DA.SetData(1, report);
            DA.SetDataList(2, copy.GetRestLenghtsSolveDoFs());
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary; }
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
            get { return new Guid("77d5fd50-ac43-467f-a4ae-ccfdfec8f074"); }
        }
    }
}

