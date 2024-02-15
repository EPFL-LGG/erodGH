using System;
using ErodModelLib.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Microsoft.FSharp.Core.ByRefKinds;
using Plotly.NET.LayoutObjects;
using GH_IO.Serialization;

namespace ErodModelLib.Types
{
    public class QuadAreas : IGH_PreviewData, IGH_Goo
    {
        public double[] Data { get;private set;}
        public double[] DataLaplacian { get; private set;}
        public double[] NormalizedData { get; private set; }
        public double[] NormalizedDataLaplacian { get; private set; }
        public double[] Averages { get; private set; }
        private Color[] _colors;
        private Color[] _colors_lap;
        private Mesh _mesh;
        private bool _showLaplacian;
        public Point3d[] Centroids { get; private set; }
        private static readonly int alpha = 75;

        // Define colors in the Blues colormap matplotlib
        private Color[] bluesColormap = ColorMaps.Blues.GetColors(alpha);

        public QuadAreas(Curve[] edges, bool useAspect=true, bool showLaplacian=false)
        {
            _mesh = Mesh.CreateFromLines(edges, 4, 1e-3);

            int numFaces = _mesh.Faces.Count;
            Data = new double[numFaces];
            DataLaplacian = new double[numFaces];
            NormalizedData = new double[numFaces];
            NormalizedDataLaplacian = new double[numFaces];
            Averages = new double[numFaces];
            Centroids = new Point3d[numFaces];
            _colors = new Color[numFaces];
            _colors_lap = new Color[numFaces];
            _showLaplacian = showLaplacian;

            for (int i = 0; i < numFaces; i++)
            {
                Point3f p0, p1, p2, p3;
                _mesh.Faces.GetFaceVertices(i, out p0, out p1, out p2, out p3);
                if (useAspect) _mesh.Faces.GetFaceAspectRatio(i);
                Data[i] = AreaMassProperties.Compute(new PolylineCurve(new Point3d[] { p0, p1, p2, p3, p0 })).Area;

                Centroids[i] = _mesh.Faces.GetFaceCenter(i);
            }

            double min = Data.Min();
            double max = Data.Max();
            double range = max - min;

            for (int i = 0; i < numFaces; i++)
            {
                NormalizedData[i] = (Data[i] - min) / range;

                var colormap = bluesColormap;
                int colorIndex = (int)(NormalizedData[i] * (colormap.Length - 1));
                _colors[i] = colormap[colorIndex];
            }

            for (int i = 0; i < numFaces; i++)
            {
                int[] neighbors = _mesh.Faces.AdjacentFaces(i);
                Averages[i] = 0.0;
                foreach (var idn in neighbors) Averages[i] = Averages[i] + Data[idn];
                Averages[i] = Averages[i] / neighbors.Count();
                DataLaplacian[i] = Data[i] - Averages[i];
            }

            double minLaplacian = DataLaplacian.Min();
            double maxLaplacian = DataLaplacian.Max();
            double rangeLaplacian = maxLaplacian - minLaplacian;

            for (int i = 0; i < numFaces; i++)
            {
                NormalizedDataLaplacian[i] = (DataLaplacian[i] - minLaplacian) / rangeLaplacian;
                var colormap = bluesColormap;
                int colorIndex = (int)(NormalizedDataLaplacian[i] * (colormap.Length - 1));
                _colors_lap[i] = colormap[colorIndex];
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
                if (_showLaplacian) args.Pipeline.DrawPolygon(new Point3d[] { p0, p1, p2, p3 }, _colors_lap[i], true);
                else args.Pipeline.DrawPolygon(new Point3d[] { p0, p1, p2, p3 }, _colors[i], true);
            }
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            int numFaces = _mesh.Faces.Count;

            for (int i = 0; i < numFaces; i++)
            {
                Point3f p0, p1, p2, p3;
                _mesh.Faces.GetFaceVertices(i, out p0, out p1, out p2, out p3);
                if (_showLaplacian) args.Pipeline.DrawPolygon(new Point3d[] { p0, p1, p2, p3 }, _colors_lap[i], true);
                else args.Pipeline.DrawPolygon(new Point3d[] { p0, p1, p2, p3 }, _colors[i], true);
            }
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

        public string TypeName => "QuadsQuantities";

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

