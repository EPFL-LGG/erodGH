using System;
using System.Drawing;
using System.Linq;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using static ErodModelLib.Utils.ColorMaps;

namespace ErodModelLib.Metrics
{
	public class RodLinkageMetrics : IGH_PreviewData, IGH_Goo, IGH_BakeAwareData
    {
        public enum LinkageMetricTypes {   SqrtBendingEnergies, MaxBendingStresses, MinBendingStresses, TwistingStresses }

        public double[] Data { get; private set; }
        public double[] NormalizedData { get; private set; }
        public LinkageMetricTypes LTypes { get; private set; }

        private Mesh _mesh;
        private Color _wireColor;

        public RodLinkageMetrics(RodLinkage linkage, LinkageMetricTypes lType, ColorMapTypes cMap, int alpha)
		{
            LTypes = lType;
            _mesh = linkage.MeshVis.DuplicateMesh();

            switch (LTypes)
            {
                case LinkageMetricTypes.SqrtBendingEnergies:
                    Data = linkage.GetScalarFieldSqrtBendingEnergies();
                    break;
                case LinkageMetricTypes.MaxBendingStresses:
                    Data = linkage.GetScalarFieldMaxBendingStresses();
                    break;
                case LinkageMetricTypes.MinBendingStresses:
                    Data = linkage.GetScalarFieldMinBendingStresses();
                    break;
                case LinkageMetricTypes.TwistingStresses:
                    Data = linkage.GetScalarFieldTwistingStresses();
                    break;
                default:
                    Data = linkage.GetScalarFieldSqrtBendingEnergies();
                    break;
            }

            double min = Data.Min();
            double max = Data.Max();
            double range = max - min;

            Color[] colormap = ColorMaps.GetColorMap(cMap, alpha==0 ? 1 : alpha);
            _wireColor = Color.FromArgb(alpha,250,235,215); // AntiqueWhite
            NormalizedData = new double[Data.Length];
            for (int i = 0; i < Data.Length; i++)
            {
                NormalizedData[i] = (Data[i] - min) / range;

                int colorIndex = (int)(NormalizedData[i] * (colormap.Length - 1));
                _mesh.VertexColors.Add(colormap[colorIndex]);
            }
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            if (att == null) att = doc.CreateDefaultAttributes();

            string id = Guid.NewGuid().ToString();
            int idxGr = doc.Groups.Add(ToString() + id);

            ObjectAttributes att1 = att.Duplicate();
            att1.AddToGroup(idxGr);

            doc.Objects.AddMesh(_mesh);

            return true;
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return new BoundingBox(_mesh.Vertices.ToPoint3dArray());
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            args.Pipeline.DrawMeshFalseColors(_mesh);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawMeshWires(_mesh, _wireColor);
        }

        #region GH_Methods
        public bool IsValid
        {
            get
            {
                return true;
            }
        }

        public string IsValidWhyNot => "";

        public string TypeName => "RodLinkageMetrics";

        public string TypeDescription => "";

        public IGH_Goo Duplicate()
        {
            return (IGH_Goo)this.MemberwiseClone();
        }

        public IGH_GooProxy EmitProxy()
        {
            return null;
        }

        public bool CastFrom(object source)
        {
            return false;
        }

        public bool CastTo<T>(out T target)
        {
            target = default(T);
            return false;
        }

        public object ScriptVariable()
        {
            return null;
        }

        public bool Write(GH_IWriter writer)
        {
            return false;
        }

        public bool Read(GH_IReader reader)
        {
            return false;
        }
        #endregion
    }
}

