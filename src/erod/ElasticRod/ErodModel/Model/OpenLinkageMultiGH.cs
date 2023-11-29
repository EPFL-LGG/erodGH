using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class OpenLinkageMultiGH : GH_Component
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
        private List<double> deployedAngle;

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
        public OpenLinkageMultiGH()
          : base("SolverDeploymentMulti", "SolverDeploymentMulti",
            "Open multiple linkages.",
            "Erod", "Model")
        {
            copies = new List<RodLinkage>();
            reports = new List<ConvergenceReport>();
            modelsInEquilibrium = new List<int>();
            refAngle = new List<double>();
            closedAngle = new List<double>();
            deployedAngle = new List<double>();
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
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.list);
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
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Report", "Report", "Convergence report", GH_ParamAccess.list);
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
            else if (steps > opts.OpeningSteps) this.Message = numModels + " Deployed Linkages";
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

            double suppIndicator = opts.OpeningSteps * opts.TemporarySupportIndicator;
            if (reset || copies.Count == 0)
            {
                copies = new List<RodLinkage>();
                reports = new List<ConvergenceReport>();
                modelsInEquilibrium = new List<int>();

                this.Message = "Reset";
                for (int i = 0; i < models.Count; i++)
                {
                    ElasticModel m = models[i];
                    if (m.ModelType != ModelTypes.RodLinkage) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input model should be a RodLinkage. The current model is a " + m.ModelType.ToString());
                    if (m != null)
                    {
                        var c = (RodLinkage)m.Clone();
                        copies.Add(c);
                        reports.Add(new ConvergenceReport());

                        double averAng = c.GetAverageJointAngle();
                        double tgtAng = angleDegrees.Count == models.Count ? angleDegrees[i] * Math.PI / 180 : angleDegrees[0] * Math.PI / 180;
                        double stepAng = (tgtAng - averAng) / opts.OpeningSteps;

                        closedAngle.Add(averAng);
                        deployedAngle.Add(tgtAng);
                        refAngle.Add(stepAng);
                    }
                }

                numModels = copies.Count;
                steps = 1;
            }

            if (run)
            {
                if (steps < opts.OpeningSteps)
                {
                    this.Message = "Opening Step " + steps;

                    for (int i = 0; i < numModels; i++)
                    {
                        RodLinkage c = copies[i];
                        if (steps >= suppIndicator) c.ClearTemporarySupports();
                        double angle = closedAngle[i] + refAngle[i] * steps;

                        ConvergenceReport r;
                        if (graphics == 0) NewtonSolver.Optimize(c, opts, out r, true, angle);
                        else NewtonSolver.Optimize(c, opts, out r, false, angle);
                        r.OpeningStep = steps;
                        reports[i] = r;
                    }
                    steps++;
                    if (steps == opts.OpeningSteps) this.Message = "Final Step";
                }
                else if (steps == opts.OpeningSteps)
                {
                    for (int i = 0; i < numModels; i++)
                    {
                        RodLinkage c = copies[i];
                        ConvergenceReport r;
                        if (graphics == 0) NewtonSolver.Optimize(c, opts, out r, true, deployedAngle[i], true);
                        else NewtonSolver.Optimize(c, opts, out r, false, deployedAngle[i], true);
                        r.OpeningStep = steps;
                        reports[i] = r;
                    }
                    steps++;
                }
            }

            DA.SetDataList(0, copies);
            DA.SetDataList(1, reports);

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
            get { return new Guid("6bae462d-9980-4549-934d-d254c227dc8e"); }
        }
    }
}
