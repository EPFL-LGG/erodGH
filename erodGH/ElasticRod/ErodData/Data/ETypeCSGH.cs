using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ErodDataLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.Data
{
    public class ETypeCSGH : GH_Component
    {
        int orientation;
        List<List<string>> matParamAttributes;
        List<string> selections;
        bool buildAttributes = true;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ETypeCSGH()
          : base("Ellipse-CrossSection", "E-CS",
              "Rectangular Cross-Section.",
              "Erod", "Data")
        {
        }

        public override void CreateAttributes()
        {
            if (buildAttributes)
            {
                FunctionToSetSelectedContent(0, 0);
                buildAttributes = false;
            }
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, matParamAttributes, selections, spacerDescriptionText);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (matParamAttributes == null)
            {
                matParamAttributes = new List<List<string>>();
                selections = new List<string>();
                matParamAttributes.Add(orientationContent);
                selections.Add(orientationContent[orientation]);
            }

            if (dropdownListId == 0)
            {
                orientation = selectedItemId;
                selections[0] = matParamAttributes[0][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        readonly List<string> spacerDescriptionText = new List<string>(new string[] { "Orientation" });
        readonly List<string> orientationContent = new List<string>(new string[] { "Tangent", "Normal" });

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("E", "E", "Young's Modulus.", GH_ParamAccess.item);
            pManager.AddNumberParameter("nu", "nu", "Poisson's Ratio.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "Cross-section width.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "Height", "Cross-section height.", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "Pt", "[Optional] Position of the joint linked with this cross-section.", GH_ParamAccess.item);
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Material data.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double e = 0, nu = 0, width = 0, height = 0;
            Point3d pt = Point3d.Unset;
            DA.GetData(0, ref e);
            DA.GetData(1, ref nu);
            DA.GetData(2, ref width);
            DA.GetData(3, ref height);
            DA.GetData(4, ref pt);

            if (e <= 0 || nu <= 0 || width <= 0 || height <= 0) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid material parameters.");
            double[] sectionParams = new double[] { width, height };

            MaterialData mat;
            if (pt == Point3d.Unset) mat = new MaterialData((int)CrossSectionType.ellipse, orientation, sectionParams, e, nu);
            else mat = new MaterialData(pt, (int)CrossSectionType.ellipse, orientation, sectionParams, e, nu);


            DA.SetData(0, mat);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("orientation", orientation);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("orientation", ref orientation))
            {
                FunctionToSetSelectedContent(0, orientation);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, matParamAttributes, selections, spacerDescriptionText);
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
            get { return new Guid("8a1c4341-ec48-4408-892e-bd61b0bcdd3b"); }
        }
    }
}
