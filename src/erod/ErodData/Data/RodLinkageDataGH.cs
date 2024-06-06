using ErodDataLib.Types;
using ErodDataLib.Utils;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ErodData.Data
{
    public class RodLinkageDataGH : GH_Component
    {
        int interleaving;
        List<List<string>> xShellParamAttributes;
        List<string> selections;
        bool buildAttributes = true;
        bool byPassTrias = false;
        string inTrias = "Include Triangles";

        /// <summary>
        /// Initializes a new instance of the LinkageDataGH class.
        /// </summary>
        public RodLinkageDataGH()
          : base("RodLinkageData", "RodLinkageData",
              "Generate all the required data to build an RodLinkage.",
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
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, xShellParamAttributes, selections, spacerDescriptionText);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (xShellParamAttributes == null)
            {
                xShellParamAttributes = new List<List<string>>();
                selections = new List<string>();
                xShellParamAttributes.Add(interleavingContent);
                selections.Add(interleavingContent[interleaving]);
            }

            if (dropdownListId == 0)
            {
                interleaving = selectedItemId;
                selections[0] = xShellParamAttributes[0][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        #region dropdownmenu content
        readonly List<string> spacerDescriptionText = new List<string>(new string[] { "Interleaving" });

        readonly List<string> interleavingContent = new List<string>(new string[]
        {
            "XShell",
            "Weaving",
            "NoOffset",
            "TriaxialWeave"
        });
        #endregion

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Edges", "E", "Edges.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "S", "Supports.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Forces", "F", "Forces.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Normals", "Norm", "Normals.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "Mat", "Material.", GH_ParamAccess.list);
            pManager.AddGenericParameter("TargetSurface", "TargetSrf", "TargetSurface.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Cables", "C", "Cables.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data","Data", "XShell data.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Joints","Joints", "Joints.",GH_ParamAccess.list);
            pManager.AddGenericParameter("Edges", "Edges", "Edges.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<SegmentData> edges = new List<SegmentData>();
            List<SupportData> supports = new List<SupportData>();
            List<UnaryForceData> forces = new List<UnaryForceData>();
            List<CableForceData> cables = new List<CableForceData>();
            List<NormalData> normals = new List<NormalData>();
            List<MaterialData> materials = new List<MaterialData>();
            TargetSurfaceData targetSrf = null;
            DA.GetDataList(0, edges);
            DA.GetDataList(1, supports);
            DA.GetDataList(2, forces);
            DA.GetDataList(3, normals);
            DA.GetDataList(4, materials);
            DA.GetData(5, ref targetSrf);
            DA.GetDataList(6, cables);

            // Set worldZ in case no normal is provided
            if (normals.Count == 0)
            {
                NormalData normal = new NormalData(Point3d.Unset);
                normal.Vector = Vector3d.ZAxis;
                normals.Add(normal);
            }

            // Interleaving type
            InterleavingType iType;
            switch (interleaving)
            {
                case 0:
                    iType = InterleavingType.xshell;
                    break;
                case 1:
                    iType = InterleavingType.weaving;
                    break;
                case 2:
                    iType = InterleavingType.noOffset;
                    break;
                case 3:
                    iType = InterleavingType.triaxialWeave;
                    break;
                default:
                    iType = InterleavingType.noOffset;
                    break;
            }

            RodLinkageData data = new RodLinkageData(edges, normals, iType, byPassTrias);
            if(targetSrf!=null) data.TargetSurface = targetSrf;

            foreach (SupportData s in supports)
            {
                data.AddSupport(s);
            }
            foreach (UnaryForceData f in forces)
            {
                data.AddForce(f);
            }
            foreach (CableForceData f in cables)
            {
                data.AddCable(f);
            }
            foreach (MaterialData m in materials)
            {
                data.AddMaterial(m);
            }

            DA.SetData(0, data);
            DA.SetDataList(1, data.Joints);
            DA.SetDataList(2, data.Segments);
        }

        public bool ByPassTriasCheck
        {
            get { return byPassTrias; }
            set
            {
                byPassTrias = value;
            }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem item = Menu_AppendItem(menu, inTrias, Menu_CheckPreviewClicked, true, ByPassTriasCheck);
            item.ToolTipText = "By-pass checking the creation of triangles in the linkage.";
        }

        private void Menu_CheckPreviewClicked(object sender, EventArgs e)
        {
            RecordUndoEvent(inTrias);
            ByPassTriasCheck = !ByPassTriasCheck;
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean(inTrias, ByPassTriasCheck);
            writer.SetInt32("interleaving", interleaving);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("interleaving", ref interleaving))
            {
                FunctionToSetSelectedContent(0, interleaving);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, xShellParamAttributes, selections, spacerDescriptionText);
            }
            bool refFlag = false;
            if (reader.TryGetBoolean(inTrias, ref refFlag))
            {
                ByPassTriasCheck = refFlag;
            }

            return base.Read(reader);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4319e82a-44d2-49e6-baa8-36ba9968f0cb"); }
        }
    }
}