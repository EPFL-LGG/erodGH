using ErodModelLib.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Drawing;
using System.Linq;
using GH_IO.Serialization;
using static ErodModelLib.Utils.ColorMaps;

namespace ErodModelLib.Metrics
{
    public class QuadMetrics : IGH_PreviewData, IGH_Goo
    {
        public enum QuadMetricTypes { Areas=0, AspectRatio=1, Laplacian=2 }

        public double[] Data { get;private set;}
        public double[] NormalizedData { get; private set; }
        public Point3d[] Centroids { get; private set; }
        public QuadMetricTypes QTypes { get; private set; }

        private Color[] _colors;
        private Mesh _mesh;

        public QuadMetrics(Curve[] edges, QuadMetricTypes qType, ColorMapTypes colorMapType=ColorMapTypes.Blues, int alpha=75)
        {
            _mesh = Mesh.CreateFromLines(edges, 4, 1e-3);
            
            int numFaces = _mesh.Faces.Count;
            Data = new double[numFaces];
            NormalizedData = new double[numFaces];
            Centroids = new Point3d[numFaces];
            _colors = new Color[numFaces];

            for (int i = 0; i < numFaces; i++)
            {
                Point3f p0, p1, p2, p3;
                _mesh.Faces.GetFaceVertices(i, out p0, out p1, out p2, out p3);

                switch(qType)
                {
                    case QuadMetricTypes.Areas:
                        Data[i] = AreaMassProperties.Compute(NurbsSurface.CreateFromCorners(p0,p1,p2,p3)).Area;
                        break;

                    case QuadMetricTypes.AspectRatio:
                        Data[i] = _mesh.Faces.GetFaceAspectRatio(i);
                        break;

                    case QuadMetricTypes.Laplacian:
                        int[] neighbors = _mesh.Faces.AdjacentFaces(i);
                        double average = 0.0;
                        double area = AreaMassProperties.Compute(NurbsSurface.CreateFromCorners(p0, p1, p2, p3)).Area;
                        foreach (var idn in neighbors)
                        {
                            _mesh.Faces.GetFaceVertices(idn, out p0, out p1, out p2, out p3);
                            average += AreaMassProperties.Compute(NurbsSurface.CreateFromCorners(p0, p1, p2, p3)).Area;
                        }
                        Data[i] = area - average / neighbors.Count();
                        break;

                    default:
                        Data[i] = AreaMassProperties.Compute(NurbsSurface.CreateFromCorners(p0, p1, p2, p3)).Area;
                        break;
                }
                Centroids[i] = _mesh.Faces.GetFaceCenter(i);
            }

            double min = Data.Min();
            double max = Data.Max();
            double range = max - min;

            Color[] colormap = ColorMaps.GetColorMap(colorMapType, alpha);
            for (int i = 0; i < numFaces; i++)
            {
                NormalizedData[i] = (Data[i] - min) / range;

                int colorIndex = (int)(NormalizedData[i] * (colormap.Length - 1));
                _colors[i] = colormap[colorIndex];
            }
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
            int numFaces = _mesh.Faces.Count;

            for (int i = 0; i < numFaces; i++)
            {
                Point3f p0, p1, p2, p3;
                _mesh.Faces.GetFaceVertices(i, out p0, out p1, out p2, out p3);
                args.Pipeline.DrawPolygon(new Point3d[] { p0, p1, p2, p3 }, _colors[i], true);
            }
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawMeshWires(_mesh, Color.AntiqueWhite);
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

        public string TypeName => "QuadMetrics";

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

