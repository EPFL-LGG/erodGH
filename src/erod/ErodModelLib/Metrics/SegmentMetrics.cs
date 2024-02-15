using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ErodModelLib.Types;
using ErodModelLib.Utils;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Display;
using Rhino.Geometry;
using static ErodModelLib.Utils.ColorMaps;

namespace ErodModelLib.Metrics
{
	public class SegmentMetrics : IGH_PreviewData, IGH_Goo
    {
        public enum SegmentMetricTypes { RestLengths = 0, RestKappas=1 }

        public double[] Data { get; private set; }
        public double[] NormalizedData { get; private set; }
        public SegmentMetricTypes SType { get; private set; }

		private Mesh[] _meshes;
        private Point3d[] _points;
        private Color[] _colors;
        private Color _wireColor;
        private double[] _radius;
        private readonly double _scaleFactor = 3;
        private readonly int _initialSize = 2;

        public SegmentMetrics(IEnumerable<RodSegment> segments, SegmentMetricTypes sType, int alpha = 75)
		{
            SType = sType;
            Color[] colormap;
            int numSegments;
            double min, max, range;
            _wireColor = Color.FromArgb(alpha, 250, 235, 215); // AntiqueWhite

            switch (SType){
                case SegmentMetricTypes.RestLengths:
                    numSegments = segments.Count();

                    Data = new double[numSegments];
                    NormalizedData = new double[numSegments];

                    _radius = new double[] { };
                    _meshes = new Mesh[numSegments];
                    _colors = new Color[numSegments];
                    _points = new Point3d[numSegments];

                    for (int i = 0; i < numSegments; i++)
                    {
                        var sg = segments.ElementAt(i);
                        Data[i] = sg.GetInitialMinRestLength();

                        _meshes[i] = sg.GetMesh();
                        _points[i] = sg.GetInterpolatedCurve().PointAtNormalizedLength(0.5);

                    }

                    min = Data.Min();
                    max = Data.Max();
                    range = max - min;

                    colormap = ColorMaps.GetColorMap(ColorMapTypes.Turbo, alpha);
                    for (int i = 0; i < numSegments; i++)
                    {
                        NormalizedData[i] = (Data[i] - min) / range;

                        int colorIndex = (int)(NormalizedData[i] * (colormap.Length - 1));
                        _colors[i] = colormap[colorIndex];
                    }
                    break;

                case SegmentMetricTypes.RestKappas:
                    numSegments = segments.Count();

                    List<double> tempData = new List<double>();
                    List<Point3d> tempPts = new List<Point3d>();

                    _meshes = new Mesh[0];

                    for (int i = 0; i < numSegments; i++)
                    {
                        var sg = segments.ElementAt(i);
                        var sgPts = sg.GetCenterLinePositionsAsPoint3d();
                        var sgKappas = sg.GetRestKappas();
                        for (int j=0; j< sgPts.Count(); j++)
                        {
                            tempPts.Add(sgPts[j]);
                            tempData.Add(sgKappas[j*2]);
                        }
                    }

                    min = tempData.Min();
                    max = tempData.Max();
                    double mean = tempData.Average();
                    double scale = 1.0 / (1.0e-8 + 2.0 * (max - mean > mean - min ? max - mean : mean - min));
                    range = max - min;

                    colormap = ColorMaps.GetColorMap(ColorMapTypes.CoolWarm, 255);

                    int numData = tempData.Count();
                    Data = tempData.ToArray();
                    NormalizedData = new double[numData];
                    _radius = new double[numData];
                    _colors = new Color[numData];
                    _points = tempPts.ToArray();

                    for (int i = 0; i < numData; i++)
                    {
                        NormalizedData[i] = 0.5 + scale * (Data[i] - mean);
                        _radius[i] = _initialSize + _scaleFactor * Math.Abs(NormalizedData[i]-0.5);

                        int colorIndex = (int)(NormalizedData[i] * (colormap.Length - 1));
                        _colors[i] = colormap[colorIndex];
                    }
                    break;
                default:
                    Data = new double[] { };
                    NormalizedData = new double[] { };
                    _meshes = new Mesh[] { };
                    _colors = new Color[] { };
                    _points = new Point3d[] { };
                    break;
            }
        }

        public override string ToString()
        {
            return "SegmentMetrics"; 
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return new BoundingBox(_points);
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            switch (SType)
            {
                case SegmentMetricTypes.RestLengths:
                    for (int i = 0; i < _meshes.Length; i++)
                    {
                        var m = _meshes[i];
                        int numFaces = m.Faces.Count;

                        Point3f p0, p1, p2, p3;
                        for (int j = 0; j < numFaces; j++)
                        {
                            m.Faces.GetFaceVertices(j, out p0, out p1, out p2, out p3);
                            args.Pipeline.DrawPolygon(new Point3d[] { p0, p1, p2, p3 }, _colors[i], true);
                        }
                    }
                    break;
                case SegmentMetricTypes.RestKappas:
                    for (int i = 0; i < _points.Length; i++) args.Pipeline.DrawPoint(_points[i], PointStyle.RoundSimple, (float)(_radius[i]), _colors[i]);
                    break;
                default:
                    break;
            }
            
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            switch (SType)
            {
                case SegmentMetricTypes.RestLengths:
                    for (int i = 0; i < _meshes.Length; i++) args.Pipeline.DrawMeshWires(_meshes[i], _wireColor);
                    break;
                case SegmentMetricTypes.RestKappas:
                    for (int i = 0; i < _points.Length; i++) args.Pipeline.DrawPoint(_points[i], PointStyle.RoundSimple, (float)(_radius[i]), _colors[i]);
                    break;
                default:
                    break;
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

        public string TypeName => "SegmentMetrics";

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

