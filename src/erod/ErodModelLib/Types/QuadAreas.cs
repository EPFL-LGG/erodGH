using System;
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
        public double[] NormalizedData { get; private set; }
        private Color[] _colors;
        private Mesh _mesh;
        public Point3d[] Centroids { get; private set; }
        private static readonly int alpha = 75;

        // Define colors in the plasma colormap matplotlib
        private Color[] plasmaColorMap = {
                                    Color.FromArgb(alpha,13, 8, 135),
                                    Color.FromArgb(alpha,53, 26, 196),
                                    Color.FromArgb(alpha,89, 15, 197),
                                    Color.FromArgb(alpha,123, 49, 202),
                                    Color.FromArgb(alpha,155, 82, 202),
                                    Color.FromArgb(alpha,186, 116, 205),
                                    Color.FromArgb(alpha,214, 151, 207),
                                    Color.FromArgb(alpha,239, 188, 218),
                                    Color.FromArgb(alpha,249, 229, 228),
                                    Color.FromArgb(alpha,251, 255, 186)
                                };

        // Define colors in the GnBu colormap matplotlib
        private Color[] gnBuColormap = {
                                    Color.FromArgb(alpha, 247, 252, 253),
                                    Color.FromArgb(alpha,229, 245, 249),
                                    Color.FromArgb(alpha,204, 236, 230),
                                    Color.FromArgb(alpha,153, 216, 201),
                                    Color.FromArgb(alpha,102, 194, 164),
                                    Color.FromArgb(alpha,65, 174, 118),
                                    Color.FromArgb(alpha,35, 139, 69),
                                    Color.FromArgb(alpha,0, 109, 44),
                                    Color.FromArgb(alpha,0, 68, 27)
        };

        // Define colors in the Blues colormap matplotlib
        private Color[] bluesColormap = {
            Color.FromArgb(alpha, 247, 251, 255),
            Color.FromArgb(alpha, 222, 235, 247),
            Color.FromArgb(alpha, 198, 219, 239),
            Color.FromArgb(alpha, 158, 202, 225),
            Color.FromArgb(alpha, 107, 174, 214),
            Color.FromArgb(alpha, 66, 146, 198),
            Color.FromArgb(alpha, 33, 113, 181),
            Color.FromArgb(alpha, 8, 81, 156),
            Color.FromArgb(alpha, 8, 48, 107)
        };

        public QuadAreas(Curve[] edges, bool useAspect=true)
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
                if (useAspect) _mesh.Faces.GetFaceAspectRatio(i);
                else Data[i] = AreaMassProperties.Compute(new PolylineCurve(new Point3d[] { p0, p1, p2, p3, p0 })).Area;

                Centroids[i] = _mesh.Faces.GetFaceCenter(i);
            }

            double min = Data.Min();
            double max= Data.Max();
            double range = max - min;

            for (int i = 0; i < numFaces; i++)
            {
                NormalizedData[i] = (Data[i] - min) / range;

                var colormap = bluesColormap;
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
            int numFaces = _mesh.Faces.Count;

            for (int i = 0; i < numFaces; i++)
            {
                Point3f p0, p1, p2, p3;
                _mesh.Faces.GetFaceVertices(i, out p0, out p1, out p2, out p3);
                args.Pipeline.DrawPolygon(new Point3d[] { p0, p1, p2, p3 }, _colors[i], true);
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

