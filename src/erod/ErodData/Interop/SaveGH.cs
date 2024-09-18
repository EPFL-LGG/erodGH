using System;
using Grasshopper.Kernel;
using ErodDataLib.Types;
using ErodDataLib.Utils;
using static ErodDataLib.Utils.WeavingOptimizationOptions;
using System.Collections.Generic;
using ErodData.IO;
using GH_IO.Serialization;
using Rhino.Geometry;

namespace ErodData.Interop
{
    public class SaveGH : GH_Component
    {
        int saveFormat;
        List<List<string>> formatAttributes;
        List<string> selections;
        bool buildAttributes = true;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SaveGH()
          : base("Save", "Save",
            "Write a JSON file with input data to run a Jupyter notebook.",
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
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, formatAttributes, selections, spacerDescriptionText);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (formatAttributes == null)
            {
                formatAttributes = new List<List<string>>();
                selections = new List<string>();
                formatAttributes.Add(fileFormat);
                selections.Add(fileFormat[saveFormat]);
            }

            if (dropdownListId == 0)
            {
                saveFormat = selectedItemId;
                selections[0] = formatAttributes[0][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        #region dropdownmenu content
        readonly List<string> spacerDescriptionText = new List<string>(new string[] { "Format" });

        readonly List<string> fileFormat = new List<string>(new string[]
        {
            "Default",
            "CShellOptim"
        });
        #endregion

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "Data", "Data to write.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "Angle", "Target average joint angle for deployment in degrees.", GH_ParamAccess.item);
            pManager.AddMeshParameter("TargetSurface", "TargetSrf", "Target surface to attract the linkage.", GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "Path", "Directory path.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filename", "Filename", "Name of the file.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "Write", "Write file.", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "Log", "Json files saved.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            string filename = "";
            bool write = false;
            LinkageIO data = null;
            double ang = 0;
            Mesh srf = null;

            DA.GetData(0, ref data);
            DA.GetData(1, ref ang);
            DA.GetData(2, ref srf);
            DA.GetData(3, ref path);
            DA.GetData(4, ref filename);
            DA.GetData(5, ref write);

            double angRad = ang * Math.PI / 180;
            if (write)
            {
                switch (saveFormat)
                {
                    case 0:
                        data.WriteJsonFile(path, filename);
                        break;
                    case 1:
                        BaseCurveNetwork cn = new BaseCurveNetwork(data, angRad, srf);
                        cn.WriteJsonFile(path, filename);
                        break;
                    default:
                        data.WriteJsonFile(path, filename);
                        break;
                }
                
            }

            DA.SetData(0, path+filename);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("saveFormat", saveFormat);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("saveFormat", ref saveFormat))
            {
                FunctionToSetSelectedContent(0, saveFormat);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, formatAttributes, selections, spacerDescriptionText);
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
                return Properties.Resources.Resources.save;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("10d5dd81-5f6f-4edb-b874-2273ab42482d"); }
        }
    }
}
