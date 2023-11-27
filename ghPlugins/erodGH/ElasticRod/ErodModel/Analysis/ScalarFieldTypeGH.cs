using System;
using System.Collections.Generic;
using ErodModel.Model;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Analysis
{
    public class ScalarFieldTypeGH : GH_Component
    {
        int fieldType;
        List<List<string>> fieldAttributes;
        List<string> selection;
        bool buildAttributes = true;
        readonly List<string> categories = new List<string>(new string[] { "ScalarField Type" });
        readonly List<string> fieldContent = new List<string>(new string[]
        {
            "SqrtBendingEnergies",
            "MaxBendingStresses",
            "MinBendingStresses",
            "TwistingStresses"
        });

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ScalarFieldTypeGH()
          : base("ScalarField", "ScalarField",
            "Scalar field RodLinkage.",
            "Erod", "Analysis")
        {
        }

        public override void CreateAttributes()
        {
            if (buildAttributes)
            {
                FunctionToSetSelectedContent(0, 0);
                buildAttributes = false;
            }
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, fieldAttributes, selection, categories);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (fieldAttributes == null)
            {
                fieldAttributes = new List<List<string>>();
                selection = new List<string>();
                fieldAttributes.Add(fieldContent);
                selection.Add(fieldContent[fieldType]);
            }

            if (dropdownListId == 0)
            {
                fieldType = selectedItemId;
                selection[0] = fieldAttributes[0][selectedItemId];
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Field", "Field", "Scalar field.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage model = null;
            DA.GetData(0, ref model);

            double[] field;
            switch (fieldType)
            {
                case 0:
                    field = model.GetScalarFieldSqrtBendingEnergies();
                    break;
                case 1:
                    field = model.GetScalarFieldMaxBendingStresses();
                    break;
                case 2:
                    field = model.GetScalarFieldMinBendingStresses();
                    break;
                case 3:
                    field = model.GetScalarFieldTwistingStresses();
                    break;
                default:
                    field = model.GetScalarFieldSqrtBendingEnergies();
                    break;
            }

            DA.SetDataList(0, field);
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
            get { return new Guid("7e50fa56-ea94-4fd7-bd68-873896f3b63e"); }
        }
    }
}
