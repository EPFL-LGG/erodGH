using System;
using System.Collections.Generic;
using ErodModelLib.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using System.Linq;
using ErodModel.Model;
using GH_IO.Serialization;

namespace ErodModel.Model
{
    public class InferSurfaceGH : GH_Component
    {
        int constructionType;
        List<List<string>> constructionAttributes;
        List<string> selection;
        bool buildAttributes = true;
        readonly List<string> categories = new List<string>(new string[] { "Construction Type" });
        readonly List<string> constructionContent = new List<string>(new string[]
        {
            "RodLinkage",
            "Rhino",
            "Both"
        });

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public InferSurfaceGH()
          : base("InferSurface", "InferSrf",
            "Construct a surface that best fits the deployed geometry of an elastic linkage.",
            "Erod", "Models")
        {
        }

        public override void CreateAttributes()
        {
            if (buildAttributes)
            {
                FunctionToSetSelectedContent(0, 0);
                buildAttributes = false;
            }
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, constructionAttributes, selection, categories);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (constructionAttributes == null)
            {
                constructionAttributes = new List<List<string>>();
                selection = new List<string>();
                constructionAttributes.Add(constructionContent);
                selection.Add(constructionContent[constructionType]);
            }

            if (dropdownListId == 0)
            {
                constructionType = selectedItemId;
                selection[0] = constructionAttributes[0][selectedItemId];
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
            pManager.AddIntegerParameter("Subdivisions", "Subd", "Number of subdivisions.", GH_ParamAccess.item,1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Infer Surface", "InferSrf", "Inferred surface. The surface is continues when using the Rhino method and discontinues when using the RodLinkage method.", GH_ParamAccess.list);
            pManager.AddCurveParameter("SplineBeamsA", "SBeamsA", "Spline-beams with label A.", GH_ParamAccess.list);
            pManager.AddCurveParameter("SplineBeamsA", "SBeamsA", "Spline-beams with label B.", GH_ParamAccess.list);
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
            int nsubd = 1, extensionLayers = 1;
            DA.GetData(1, ref nsubd);

            List<GeometryBase> outSrf = new List<GeometryBase>();
            List<Curve> cA = new List<Curve>();
            List<Curve> cB = new List<Curve>();

            if (constructionType != 0)
            {
                if (model.ModelIO.Layout.ContainsLayoutData())
                {
                    var splinesA = model.ModelIO.Layout.RibbonsFamilyA;
                    foreach (int key in splinesA.Keys)
                    {
                        var edgeIndexes = splinesA[key];
                        List<Point3d> pts = new List<Point3d>();
                        for (int j = 0; j < edgeIndexes.Count; j++)
                        {
                            int eIdx = edgeIndexes.ElementAt(j);
                            if (j == 0)
                            {
                                pts.AddRange(model.Segments[eIdx].GetCenterLinePositionsAsPoint3d());
                            }
                            else
                            {
                                pts.AddRange(model.Segments[eIdx].GetCenterLinePositionsAsPoint3d().Skip(2));
                            }
                        }

                        cA.Add(Curve.CreateInterpolatedCurve(pts, 3));
                    }

                    var splinesB = model.ModelIO.Layout.RibbonsFamilyB;
                    foreach (int key in splinesB.Keys)
                    {
                        var edgeIndexes = splinesB[key];
                        List<Point3d> pts = new List<Point3d>();
                        for (int j = 0; j < edgeIndexes.Count; j++)
                        {
                            int eIdx = edgeIndexes.ElementAt(j);
                            if (j == 0)
                            {
                                pts.AddRange(model.Segments[eIdx].GetCenterLinePositionsAsPoint3d());
                            }
                            else
                            {
                                pts.AddRange(model.Segments[eIdx].GetCenterLinePositionsAsPoint3d().Skip(2));
                            }
                        }

                        cB.Add(Curve.CreateInterpolatedCurve(pts, 3));
                    }

                    int error;
                    Surface srf = NurbsSurface.CreateNetworkSurface(cA, 1, 1, cB, 1, 1, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAngleToleranceRadians, out error);

                    if (srf == null)
                    {
                        List<Curve> geom = new List<Curve>();
                        geom.AddRange(cA);
                        geom.AddRange(cB);
                        srf = Brep.CreatePatch(geom, 10, 10, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).Surfaces[0];
                    }
                    outSrf.Add(srf);
                }
                else
                {
                    int count = model.Segments.Count();
                    List<Curve> edges = new List<Curve>();
                    for(int i=0; i<count; i++)
                    {
                        var seg = model.Segments.ElementAt(i);
                        edges.Add(seg.GetInterpolatedCurve());
                    }
                    Surface srf = Brep.CreatePatch(edges, 10, 10, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance).Surfaces[0];

                    outSrf.Add(srf);
                }
            }

            if (constructionType != 1)
            {
                outSrf.Add(model.InferTargetSurface(nsubd, extensionLayers));
            }

            DA.SetDataList(0, outSrf);
            DA.SetDataList(1, cA);
            DA.SetDataList(2, cB);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("constructionType", constructionType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("constructionType", ref constructionType))
            {
                FunctionToSetSelectedContent(0, constructionType);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, constructionAttributes, selection, categories);
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
                return Properties.Resources.Resources.infer_surface;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("48249531-7225-475e-8d61-dbb6f0331307"); }
        }
    }
}
