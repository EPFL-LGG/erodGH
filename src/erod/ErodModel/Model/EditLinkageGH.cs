using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Model
{
    public class EditLinkageGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public EditLinkageGH()
          : base("EditLinkage", "EditLinkage",
            "Modify an elastic linkage.",
            "Erod", "Models")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Linkage model to modify.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Supports", "Supports", "Set of support conditions.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Forces", "Forces", "Set of forces.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Cables", "Cables", "Set of tensile cables.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("CleanSupports", "CleanSupports", "Remove previous support conditions", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("CleanForces", "CleanForces", "Remove previous forces", GH_ParamAccess.item, true);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Elastic linkage model.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage model = null;
            List<SupportIO> supports = new List<SupportIO>();
            List<ForceExternalIO> forces = new List<ForceExternalIO>();
            List<ForceCableIO> cables = new List<ForceCableIO>();
            bool cleanSp = true, cleanFs = true;
            DA.GetData(0, ref model);
            DA.GetDataList(1, supports);
            DA.GetDataList(2, forces);
            DA.GetDataList(3, cables);
            DA.GetData(4, ref cleanSp);
            DA.GetData(5, ref cleanFs);

            if (model.ModelType != ElasticModelType.RodLinkage) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The input model should be an elastic linkage. The current model is a " + model.ModelType.ToString());

            RodLinkage copy = (RodLinkage) model.Clone();

            if (cleanSp) copy.ModelIO.CleanSupports();
            copy.ModelIO.AddSupports(supports);
            if (copy.ModelIO.Supports.Count == 0) copy.ModelIO.AddCentralSupport();
            copy.InitSupports();

            // Forces
            if (cleanFs) copy.ModelIO.CleanForces();
            if (forces.Count > 0) copy.ModelIO.AddForces(forces);
            if (cables.Count > 0) copy.ModelIO.AddForces(cables);
            copy.InitForces();

            DA.SetData(0, copy);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.edit_linkage;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("82510bd1-8244-44f5-afdf-3e81cbfc39b8"); }
        }
    }
}
