﻿using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodModel.Materials
{
    public class eMaterialGH : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public eMaterialGH()
          : base("eMaterial", "eMaterial",
            "Analyze a cross-section.",
            "Erod", "Materials")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Material", "Material to analyze.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Area", "Area", "Cross-section area", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ix", "Ix", "Moment of inertia (lambda1)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iy", "Iy", "Moment of inertia (lambda2)", GH_ParamAccess.item);
            pManager.AddNumberParameter("BendingStiffness", "BendingStiffness", "Bending stiffness", GH_ParamAccess.list);
            pManager.AddNumberParameter("StretchingStiffness", "StretchingStiffness", "Stretching stiffness", GH_ParamAccess.item);
            pManager.AddNumberParameter("TwistingStiffness", "TwistingStiffness", "Stretching stiffness", GH_ParamAccess.item);
            pManager.AddNumberParameter("G", "G", "Shear modulus", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            MaterialIO matIO = null;
            DA.GetData(0, ref matIO);

            Material mat = new Material(matIO);

            DA.SetData(0, mat.Area);
            DA.SetData(1, mat.MomentOfInertia.Lambda1);
            DA.SetData(2, mat.MomentOfInertia.Lambda1);
            DA.SetDataList(3, new double[]{mat.BendingStiffness.Lambda1, mat.BendingStiffness.Lambda2});
            DA.SetData(4, mat.StretchingStiffness);
            DA.SetData(5, mat.TwistingStiffness);
            DA.SetData(6, mat.G);
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
                return Properties.Resources.Resources.cs_analyze;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("63a98923-0ed8-4c03-a22e-7afab6846afe"); }
        }
    }
}
