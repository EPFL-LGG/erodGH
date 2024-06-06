using System;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;

namespace ErodModel.Model
{
    public class OpenLinkageWithTargetPositionsGH : GH_Component
    {
        private bool run, includeTemporarySupports;
        private int steps = 1;//, openingSteps = 0;
        private RodLinkage copy;
        private NewtonSolverOpts opts;
        private ConvergenceReport report;

        double refStep = 0;

        int graphics;
        List<List<string>> graphicAttributes;
        List<string> selection;
        bool buildAttributes = true;
        readonly List<string> categories = new List<string>(new string[] { "Graphics Update" });
        readonly List<string> graphicContent = new List<string>(new string[]
        {
            "Per Iteration",
            "Per Equilibrium",
        });

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public OpenLinkageWithTargetPositionsGH()
          : base("SolverDeploymentWithPositions", "SolverDeploymentWithPositions",
            "Open linkage with target positions.",
            "Erod", "Model")
        {
        }

        public override void CreateAttributes()
        {
            if (buildAttributes)
            {
                FunctionToSetSelectedContent(0, 0);
                buildAttributes = false;
            }
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, graphicAttributes, selection, categories);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (graphicAttributes == null)
            {
                graphicAttributes = new List<List<string>>();
                selection = new List<string>();
                graphicAttributes.Add(graphicContent);
                selection.Add(graphicContent[graphics]);
            }

            if (dropdownListId == 0)
            {
                graphics = selectedItemId;
                selection[0] = graphicAttributes[0][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
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
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.item);
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
            DA.GetData(0, ref model);
            if (!DA.GetData(1, ref opts)) opts = new NewtonSolverOpts(20, 20);
            DA.GetData(2, ref run);
            DA.GetData(3, ref reset);

            if (model.ModelType != ModelTypes.RodLinkage)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input model should be a RodLinkage. The current model is a " + model.ModelType.ToString());
            }
            else
            {
                double suppIndicator = opts.OpeningSteps * opts.TemporarySupportIndicator;

                if (reset || copy == null)
                {
                    this.Message = "Reset";
                    copy = (RodLinkage)model.Clone();
                    report = new ConvergenceReport();
                    includeTemporarySupports = true;

                    refStep = 1.0 / (opts.OpeningSteps-1);
                    steps = 1;
                }

                if (run)
                {
                    if (steps >= suppIndicator) includeTemporarySupports = false;
                    if (steps < opts.OpeningSteps)
                    {
                        this.Message = "Opening Step " + steps;

                        double[] dofs = copy.GetDoFs();
                        // Update positions of supports
                        foreach (Support sp in copy.Supports)
                        {
                            // Compute linear interpolation between initial position and target position
                            Line ln = new Line(sp.InitialPosition, sp.TargetPosition);
                            sp.Position = ln.PointAt(refStep * (steps-1));
                            // Only update dofs linked with the position
                            dofs[sp.LockedDoFs[0]] = sp.Position.X;
                            dofs[sp.LockedDoFs[1]] = sp.Position.Y;
                            dofs[sp.LockedDoFs[2]] = sp.Position.Z;
                        }
                        copy.SetDoFs(dofs);

                        if (graphics == 0) NewtonSolver.Optimize(copy, opts, out report, true, 0, false, includeTemporarySupports);
                    }
                    else NewtonSolver.Optimize(copy, opts, out report, false, 0, false, includeTemporarySupports);
                    report.OpeningStep = steps;
                    steps++;
                    if (steps == opts.OpeningSteps) this.Message = "Final Step";
                }
                else if (steps == opts.OpeningSteps)
                {
                    if (graphics == 0) NewtonSolver.Optimize(copy, opts, out report, true, 0, true);
                    else NewtonSolver.Optimize(copy, opts, out report, false, 0, true);
                    report.OpeningStep = steps;
                    steps++;
                }
            }

            DA.SetData(0, copy);
            DA.SetData(1, report);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("graphics", graphics);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("graphics", ref graphics))
            {
                FunctionToSetSelectedContent(0, graphics);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, graphicAttributes, selection, categories);
            }
            return base.Read(reader);
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
            get { return new Guid("ed855f8d-440d-47cb-a26f-75650e1a76c1"); }
        }
    }
}

