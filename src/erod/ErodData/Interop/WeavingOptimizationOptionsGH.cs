using System;
using System.Collections.Generic;
using ErodData.Data;
using ErodData.IO;
using ErodDataLib.Types;
using ErodDataLib.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using static ErodDataLib.Utils.WeavingOptimizationOptions;

namespace ErodData.Interop
{
    public class WeavingOptimizationOptionsGH : GH_Component
    {
        int optimizationStage;
        List<List<string>> optimizationStagesAttributes;
        List<string> selections;
        bool buildAttributes = true;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public WeavingOptimizationOptionsGH()
          : base("OptimizationSettings", "OptSettings",
            "Settings for running optimization",
            "Erod", "Interop")
        {
        }
        public override void CreateAttributes()
        {
            if (buildAttributes)
            {
                FunctionToSetSelectedContent(0, 0);
                buildAttributes = false;
            }
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, optimizationStagesAttributes, selections, spacerDescriptionText);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (optimizationStagesAttributes == null)
            {
                optimizationStagesAttributes = new List<List<string>>();
                selections = new List<string>();
                optimizationStagesAttributes.Add(OptimizationContent);
                selections.Add(OptimizationContent[optimizationStage]);
            }

            if (dropdownListId == 0)
            {
                optimizationStage = selectedItemId;
                selections[0] = optimizationStagesAttributes[0][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        #region dropdownmenu content
        readonly List<string> spacerDescriptionText = new List<string>(new string[] { "Optimization Stages" });

        readonly List<string> OptimizationContent = new List<string>(new string[]
        {
            "One-Stage",
            "Two-Stage",
            "Three-Stage",
        });
        #endregion

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("AutoWidth", "AutoWidth", "Automatic scaling of the width based on scaling factors.", GH_ParamAccess.item);
            pManager.AddNumberParameter("MinWidthFactor", "MinWidthFactor", "Minimum width scaling factor", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("MaxWidthFactor", "MaxWidthFactor", "Maximun width scaling factor", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("OptimizationSettings", "OptSettings", "Optimization settings", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool autoWidth = true;
            double minWidthFactor = 1;
            double maxWidthFactor = 1;
            DA.GetData(0, ref autoWidth);
            DA.GetData(1, ref minWidthFactor);
            DA.GetData(2, ref maxWidthFactor);

            // Interleaving type
            OptimizationStages oType;
            switch (optimizationStage)
            {
                case 0:
                    oType = OptimizationStages.OneStep;
                    break;
                case 1:
                    oType = OptimizationStages.TwoSteps;
                    break;
                case 2:
                    oType = OptimizationStages.ThreeSteps;
                    break;
                default:
                    oType = OptimizationStages.OneStep;
                    break;
            }

            WeavingOptimizationOptions opt = new WeavingOptimizationOptions(autoWidth, minWidthFactor, maxWidthFactor, oType);

            DA.SetData(0, opt);
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
                return Properties.Resources.Resources.weaving_settings;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bb121e8d-9981-4020-be74-2d57deb24a29"); }
        }
    }
}
