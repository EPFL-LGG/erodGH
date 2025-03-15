using System;
using System.Drawing;
using System.Linq;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using static ErodModelLib.Metrics.LinkageStressesMetrics;
using static ErodModelLib.Utils.ColorMaps;

namespace ErodModelLib.Metrics
{
    public class RodStressesMetrics : IGH_PreviewData, IGH_Goo, IGH_BakeAwareData
    {
        public double[] Data { get; private set; }
        public double[] DataPerNode { get; private set; }
        public double[] NormalizedData { get; private set; }
        public LinkageMetricTypes LTypes { get; private set; }

        public Mesh Mesh { get; private set; }
        private Color _wireColor;

        public RodStressesMetrics(ElasticRod rod, LinkageMetricTypes lType, ColorMapTypes cMap, int alpha, double lowerBound = default, double upperBound = default)
        {
            LTypes = lType;
            Mesh = rod.MeshVis.DuplicateMesh();

            switch (LTypes)
            {
                case LinkageMetricTypes.SqrtBendingEnergies:
                    Data = rod.GetScalarFieldSqrtBendingEnergies();
                    DataPerNode = rod.GetSqrtBendingEnergies();
                    break;
                case LinkageMetricTypes.MaxBendingStresses:
                    Data = rod.GetScalarFieldMaxBendingStresses();
                    DataPerNode = rod.GetMaxBendingStresses();
                    break;
                case LinkageMetricTypes.MinBendingStresses:
                    Data = rod.GetScalarFieldMinBendingStresses();
                    DataPerNode = rod.GetMinBendingStresses();
                    break;
                case LinkageMetricTypes.TwistingStresses:
                    Data = rod.GetScalarFieldTwistingStresses();
                    DataPerNode = rod.GetTwistingStresses();
                    break;
                case LinkageMetricTypes.VonMises:
                    Data = rod.GetScalarFieldVonMisesStresses();
                    DataPerNode = rod.GetVonMisesStresses();
                    break;
                default:
                    Data = rod.GetScalarFieldSqrtBendingEnergies();
                    DataPerNode = rod.GetSqrtBendingEnergies();
                    break;
            }
            double tol = +1.0e-8;
            double min = lowerBound == default ? Data.Min() : lowerBound == 0 ? tol : lowerBound;
            double max = upperBound == default ? Data.Max() : upperBound;
            double range = max - min + tol;

            Color[] colormap = ColorMaps.GetColorMap(cMap, alpha == 0 ? 1 : alpha);
            int lastColor = colormap.Length - 1;
            _wireColor = Color.FromArgb(alpha, 250, 235, 215); // AntiqueWhite
            NormalizedData = new double[Data.Length];
            for (int i = 0; i < Data.Length; i++)
            {
                NormalizedData[i] = (Data[i] - min) / range;

                int colorIndex = (int)(NormalizedData[i] * lastColor);
                if (colorIndex < 0) colorIndex = 0;
                else if (colorIndex > lastColor) colorIndex = lastColor;
                Mesh.VertexColors.Add(colormap[colorIndex]);
            }
        }

        public override string ToString()
        {
            return "RodFieldMetrics";
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            if (att == null) att = doc.CreateDefaultAttributes();

            string id = Guid.NewGuid().ToString();
            int idxGr = doc.Groups.Add(ToString() + id);

            ObjectAttributes att1 = att.Duplicate();
            att1.AddToGroup(idxGr);

            doc.Objects.AddMesh(Mesh);

            return true;
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return new BoundingBox(Mesh.Vertices.ToPoint3dArray());
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            args.Pipeline.DrawMeshFalseColors(Mesh);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawMeshWires(Mesh, _wireColor);
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

        public string TypeName => "RodFieldMetrics";

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

