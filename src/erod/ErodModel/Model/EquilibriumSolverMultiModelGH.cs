using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class EquilibriumSolverMultiModelGH : GH_Component
    {
        private bool run, equilibrium = false;
        private List<ElasticModel> copies;
        private List<ConvergenceReport> reports;
        private NewtonSolverOpts options;
        private List<int> modelsInEquilibrium;
        private int numModels;

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
        public EquilibriumSolverMultiModelGH()
      : base("SolverEquilibriumMulti", "SolverEquilibriumMulti",
            "Compute equilibrium of multiple models.",
            "Erod", "Model")
        {
            copies = new List<ElasticModel>();
            reports = new List<ConvergenceReport>();
            modelsInEquilibrium = new List<int>();
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
            else if (equilibrium) this.Message = modelsInEquilibrium.Count + " Equilibriums";
            else this.Message = "Stop";
        }

        private void ScheduleCallback(GH_Document doc)
        {
            this.ExpireSolution(false);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Elastic Model.", GH_ParamAccess.list);
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
            pManager.AddGenericParameter("Model", "Model", "RodLinkage Model.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Report", "Report", "Convergence report", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<ElasticModel> models = new List<ElasticModel>();
            bool reset = false;
            run = false;
            options = new NewtonSolverOpts(20, 1);

            DA.GetDataList(0, models);
            DA.GetData(1, ref options);
            DA.GetData(2, ref run);
            DA.GetData(3, ref reset);

            if (reset || copies.Count==0)
            {
                copies = new List<ElasticModel>();
                reports = new List<ConvergenceReport>();
                modelsInEquilibrium = new List<int>();

                this.Message = "Reset";
                for (int i=0; i<models.Count; i++)
                {
                    ElasticModel m = models[i];
                    if (m != null)
                    {
                        copies.Add((ElasticModel)m.Clone());
                        reports.Add(new ConvergenceReport());
                    }
                }

                equilibrium = false;
                numModels = copies.Count;
            }

            if (run)
            {
                if (!equilibrium)
                {
                    this.Message = "Computing";
                    for (int i = 0; i < numModels; i++)
                    {
                        if (!modelsInEquilibrium.Contains(i))
                        {
                            ConvergenceReport r;
                            bool modelEquilibrium;
                            if (graphics == 0) modelEquilibrium = NewtonSolver.Optimize(copies[i], options, out r);
                            else modelEquilibrium = NewtonSolver.Optimize(copies[i], options, out r, false);
                            reports[i] = r;

                            if (modelEquilibrium) modelsInEquilibrium.Add(i);
                        }
                    }

                    if (modelsInEquilibrium.Count == numModels) equilibrium = true;
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
            if (reader.TryGetInt32("interleaving", ref graphics))
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
            get { return new Guid("dbf8d4e5-c486-47d0-98d2-02e34c30764a"); }
        }
    }
}
