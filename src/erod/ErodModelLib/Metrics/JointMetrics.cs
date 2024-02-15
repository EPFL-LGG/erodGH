using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Windows.Forms;
using ErodModelLib.Utils;
using System.Drawing.Imaging;
using static ErodModelLib.Utils.ColorMaps;

namespace ErodModelLib.Metrics
{
	public class JointMetrics : IGH_PreviewData, IGH_Goo

    {
        public enum JointMetricTypes { Angles = 0, AngleDeviations = 1 }

        public Point3d[] Positions { get; private set; }
        public double[] Data { get; set; }
        public double[] NormalizedData { get; private set; }
        public JointMetricTypes JType { get; private set; }

        private float[] _radius;
        private Color[] _color;

		public JointMetrics(IEnumerable<Joint> joints, JointMetricTypes jType, double sizeScale=1.0, bool uniformSize=true)
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
                Data[i] = jt.GetAlpha();
            }

            // Normalize data
            double min = Data.Min();
            double max = Data.Max();
            double mean = Data.Average();
            double scale = 1.0 / (1.0e-8 + 2.0 * (max - mean > mean - min ? max - mean : mean - min));
            double range = max - min;

            Color[] colormap;
            switch (JType)
            {
                case JointMetricTypes.Angles:
                    colormap = ColorMaps.GetColorMap(ColorMapTypes.Plasma, 255);
                    break;
                case JointMetricTypes.AngleDeviations:
                    colormap = ColorMaps.GetColorMap(ColorMapTypes.CoolWarm, 255);
                    break;
                default:
                    colormap = ColorMaps.GetColorMap(ColorMapTypes.CoolWarm, 255);
                    break;
            }

            for (int i = 0; i < numJoints; i++)
            {
                double normData;
                switch (JType) {
                    case JointMetricTypes.Angles:
                        normData = (Data[i] - min) / (1.0e-8 + range);
                        _radius[i] = uniformSize ? (float)sizeScale : (float) (normData * sizeScale);
                        NormalizedData[i] = normData;
                        break;

                    case JointMetricTypes.AngleDeviations:
                        normData = 0.5 + scale * (Data[i] - mean);
                        _radius[i] = uniformSize ? (float) sizeScale : (float)(normData * sizeScale);
                        NormalizedData[i] = normData;
                        break;

                    default:
                        normData = (Data[i] - min) / (1.0e-8 + range);
                        _radius[i] = uniformSize ? (float)sizeScale : (float)(normData * sizeScale);
                        NormalizedData[i] = normData;
                        break;
                }

                int colorIndex = (int)(normData * (colormap.Length - 1));
                _color[i] = colormap[colorIndex];
            }
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

