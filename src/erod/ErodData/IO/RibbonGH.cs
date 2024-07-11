using System;
using System.Collections.Generic;
using ErodDataLib.Types;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ErodData.IO
{
    public class RibbonsGH : GH_Component
    {
        int edgeLabel;
        List<List<string>> edgeAttributes;
        List<string> selections;
        bool buildAttributes = true;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RibbonsGH()
          : base("Ribbons", "Ribbons",
              "Build a ribbon from a curve with parameters to define the joints.",
              "Erod", "IO")
        {
        }

        public override void CreateAttributes()
        {
            if (buildAttributes)
            {
                FunctionToSetSelectedContent(0, 0);
                buildAttributes = false;
            }
            m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, edgeAttributes, selections, spacerDescriptionText);
        }

        public void FunctionToSetSelectedContent(int dropdownListId, int selectedItemId)
        {
            if (edgeAttributes == null)
            {
                edgeAttributes = new List<List<string>>();
                selections = new List<string>();
                edgeAttributes.Add(edgeLabelContent);
                selections.Add(edgeLabelContent[edgeLabel]);
            }

            if (dropdownListId == 0)
            {
                edgeLabel = selectedItemId;
                selections[0] = edgeAttributes[0][selectedItemId];
            }

            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        #region dropdownmenu content
        readonly List<string> spacerDescriptionText = new List<string>(new string[] { "Rod Label" });

        readonly List<string> edgeLabelContent = new List<string>(new string[]
        {
            "Undefined",
            "Label_A",
            "Label_B",
        });
        #endregion

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "Crv", "Curve or line to build the ribbon.", GH_ParamAccess.item);
            pManager.AddNumberParameter("t", "t", "Parameters defining the positions of the joints.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Subdivision", "Subdivision", "Number of edges per ribbon segment. The minimum number is 5.", GH_ParamAccess.list, new List<int>() { 10 });
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance to use for checking linearity.", GH_ParamAccess.item, 0.01);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Segments", "Segments", "Collection of segments defining the ribbon.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = null;
            List<double> t = new List<double>();
            List<int> subd = new List<int>();
            double tol = 0.01;

            DA.GetData(0, ref crv);
            if(!DA.GetDataList(1, t)) return;
            DA.GetDataList(2, subd);
            DA.GetData(3, ref tol);

            SegmentLabels eLabel;
            switch (edgeLabel)
            {
                case 0:
                    eLabel = SegmentLabels.Undefined;
                    break;
                case 1:
                    eLabel = SegmentLabels.RodA;
                    break;
                case 2:
                    eLabel = SegmentLabels.RodB;
                    break;
                default:
                    eLabel = SegmentLabels.Undefined;
                    break;
            }

            Curve[] segments = crv.Split(t);

            List<SegmentIO> edges = new List<SegmentIO>();
            int idx = 0;
            for(int i=0; i< segments.Length; i++)
            {
                Curve c = segments[i];
                Point3d p1 = crv.PointAtStart;
                Point3d p2 = crv.PointAtEnd;
                int subdivision = subd.Count == segments.Length ? subd[i] : subd[0];
                if (subdivision < 5) subdivision = 5;

                double l = c.GetLength();
                if (l > 0.01)
                {
                    SegmentIO edge = new SegmentIO(c, subdivision, tol);
                    edge.SegmentIndexInRibbon = idx;
                    edge.EdgeLabel = eLabel;
                    edges.Add(edge);
                    idx++;
                }
            }

            DA.SetDataList(0, edges);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("edgeLabel", edgeLabel);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.TryGetInt32("edgeLabel", ref edgeLabel))
            {
                FunctionToSetSelectedContent(0, edgeLabel);
                m_attributes = new DropDownAttributesGH(this, FunctionToSetSelectedContent, edgeAttributes, selections, spacerDescriptionText);
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
                return Properties.Resources.Resources.ribbon_io;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c194637b-35e6-468b-b5aa-6592e70c8376"); }
        }
    }
}
