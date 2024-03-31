using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ErodModelLib.Utils;
using static ErodModelLib.Utils.ColorMaps;
using Rhino;
using Rhino.DocObjects;
using System;

namespace ErodModelLib.Metrics
{
	public class JointMetrics : IGH_PreviewData, IGH_Goo, IGH_BakeAwareData

    {
        public enum JointMetricTypes { Angles = 0, RestAngles=1, AngleDeviations = 2, AngleIncrements = 3 }

        public Point3d[] Positions { get; private set; }
        public double[] Data { get; set; }
        public double[] NormalizedData { get; private set; }
        public JointMetricTypes JType { get; private set; }

        private float[] _radius;
        private Color[] _color;

		public JointMetrics(IEnumerable<Joint> joints, JointMetricTypes jType, ColorMapTypes cMap, double sizeScale=1.0, bool uniformSize=true, double lowerBound = default, double upperBound = default)
		{
            int numJoints = joints.Count();
            Positions = new Point3d[numJoints];
            Data = new double[numJoints];
            NormalizedData = new double[numJoints];
            JType = jType;

            _radius = new float[numJoints];
            _color = new Color[numJoints];

            // Colect data
            for (int i = 0; i < numJoints; i++)
            {
                var jt = joints.ElementAt(i);
                Positions[i] = jt.GetPositionAsPoint3d();

                double d;
                switch (JType)
                {
                    case JointMetricTypes.Angles:
                        d = jt.GetAlpha();
                        break;
                    case JointMetricTypes.RestAngles:
                        d = jt.RestAlpha;
                        break;
                    case JointMetricTypes.AngleDeviations:
                        d = jt.GetAlpha();
                        break;
                    case JointMetricTypes.AngleIncrements:
                        d = jt.GetAlpha()-jt.RestAlpha;
                        break;
                    default:
                        d = jt.GetAlpha();
                        break;
                }

                Data[i] = d;
            }

            // Normalize data
            double tol = +1.0e-8;
            double min = lowerBound == default ? Data.Min() : lowerBound == 0 ? tol : lowerBound;
            double max = upperBound == default ? Data.Max() : upperBound;
            double mean = Data.Average();
            double scale = 1.0 / (tol + 2.0 * (max - mean > mean - min ? max - mean : mean - min));
            double range = max - min + tol;

            Color[] colormap = ColorMaps.GetColorMap(cMap, 255);
            int lastColor = colormap.Length - 1;

            for (int i = 0; i < numJoints; i++)
            {
                double normData;
                switch (JType) {
                    case JointMetricTypes.Angles:
                        normData = (Data[i] - min) / range;
                        _radius[i] = uniformSize ? (float)sizeScale : (float) (normData * sizeScale);
                        NormalizedData[i] = normData;
                        break;

                    case JointMetricTypes.RestAngles:
                        normData = (Data[i] - min) / range;
                        _radius[i] = uniformSize ? (float)sizeScale : (float)(normData * sizeScale);
                        NormalizedData[i] = normData;
                        break;

                    case JointMetricTypes.AngleDeviations:
                        normData = 0.5 + scale * (Data[i] - mean);
                        _radius[i] = uniformSize ? (float) sizeScale : (float)(normData * sizeScale);
                        NormalizedData[i] = normData;
                        break;

                    case JointMetricTypes.AngleIncrements:
                        normData = (Data[i] - min) / range;
                        _radius[i] = uniformSize ? (float)sizeScale : (float)(normData * sizeScale);
                        NormalizedData[i] = normData;
                        break;

                    default:
                        normData = (Data[i] - min) / range;
                        _radius[i] = uniformSize ? (float)sizeScale : (float)(normData * sizeScale);
                        NormalizedData[i] = normData;
                        break;
                }

                int colorIndex = (int)(normData * lastColor);
                if (colorIndex < 0) colorIndex = 0;
                else if (colorIndex > lastColor) colorIndex = lastColor;
                _color[i] = colormap[colorIndex];
            }
        }

        public override string ToString()
        {
            return "JointMetrics";
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            if (att == null) att = doc.CreateDefaultAttributes();

            string id = Guid.NewGuid().ToString();
            int idxGr = doc.Groups.Add(ToString() + id);

            ObjectAttributes att1 = att.Duplicate();
            att1.AddToGroup(idxGr);

            for (int i = 0; i < Positions.Length; i++)
            {
                var m = Mesh.CreateFromSphere(new Sphere(Positions[i], _radius[i]), 10, 10);
                m.VertexColors.SetColors(Enumerable.Repeat(_color[i], m.Vertices.Count).ToArray());
                doc.Objects.AddMesh(m);
            }

            return true;
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return new BoundingBox( Positions );
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            for (int i=0; i<Positions.Length; i++) {
                args.Pipeline.DrawPoint(Positions[i], Rhino.Display.PointStyle.RoundSimple, _radius[i], _color[i]);
            }
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            for (int i = 0; i < Positions.Length; i++)
            {
                args.Pipeline.DrawPoint(Positions[i], Rhino.Display.PointStyle.RoundSimple, _radius[i], _color[i]);
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

        public string TypeName => "JointMetrics";

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

