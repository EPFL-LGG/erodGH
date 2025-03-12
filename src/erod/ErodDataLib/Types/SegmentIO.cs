using System;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public enum SegmentTypes { Undefined = 0, EdgeLine = 1, EdgeCurve = 2 };
    public enum SegmentLabels { Undefined = 0, RodA = 1, RodB = 2 }

    public class SegmentIO : IGH_Goo, ICloneable
    {
        private readonly Curve _curve;
        private const double STEPSIZE = 1e-5;

        public SegmentLabels EdgeLabel { get; set; }
        public int RibbonIndex { get; set; }
        public int SegmentIndexInRibbon { get; set; }

        public int[] Indices { get; set; }
        public Point3d[] ControlPoints { get; private set; }
        public double[] Knots { get; private set; }
        public double[] Weights { get; private set; }
        public int Subdivision { get; set; }
        public double RestLength { get; set; }
        public int Degree { get; private set; }
        public SegmentTypes SegmentType { get; private set; }
        public Point3d[] CurvePoints { get; set; }

        public bool IsPeriodic { get; private set; }
        public bool IsCurvedEdge { get; private set; }
        public int StartJoint { get; set; }
        public int EndJoint { get; set; }

        public SegmentIO(SegmentIO segment)
        {
            _curve = segment._curve.DuplicateCurve();
            EdgeLabel = segment.EdgeLabel;
            RibbonIndex = segment.RibbonIndex;
            SegmentIndexInRibbon = segment.SegmentIndexInRibbon;
            Indices = segment.Indices.ToArray();
            ControlPoints = segment.ControlPoints.ToArray();
            Knots = segment.Knots.ToArray();
            Weights = segment.Weights.ToArray();
            Subdivision = segment.Subdivision;
            RestLength = segment.RestLength;
            Degree = segment.Degree;
            SegmentType = segment.SegmentType;
            CurvePoints = segment.CurvePoints.ToArray();
            IsPeriodic = segment.IsPeriodic;
            IsCurvedEdge = segment.IsCurvedEdge;
            StartJoint = segment.StartJoint;
            EndJoint = segment.EndJoint;
        }

        public SegmentIO(Curve curve, int subdivision = 10, double tol = 1e-06, bool removeRestCurvature = false)
        {
            _curve = curve.ToNurbsCurve();
            if (removeRestCurvature) _curve = new LineCurve(curve.PointAtStart, curve.PointAtEnd);

            Indices = new int[2];
            StartJoint = -1;
            EndJoint = -1;
            Subdivision = subdivision;
            CurvePoints = new Point3d[Subdivision + 1];

            // Curve parameters
            Degree = _curve.Degree;
            RestLength = _curve.GetLength();
            IsCurvedEdge = !_curve.IsLinear(tol);
            IsPeriodic = _curve.IsClosed;

            if (IsCurvedEdge)
            {
                SegmentType = SegmentTypes.EdgeCurve;
                NurbsCurve temp = _curve.ToNurbsCurve();
                Knots = temp.Knots.ToArray();
                var cpList = temp.Points;
                ControlPoints = new Point3d[cpList.Count];
                Weights = new double[cpList.Count];
                for (int i = 0; i < cpList.Count; i++)
                {
                    var cp = cpList[i];
                    ControlPoints[i] = cp.Location;
                    Weights[i] = cp.Weight;
                }
            }
            else
            {
                SegmentType = SegmentTypes.EdgeLine;
                Knots = new double[0];
                ControlPoints = new Point3d[0];
                Weights = new double[0];
            }

            EdgeLabel = SegmentLabels.Undefined;
            RibbonIndex = -1;
            SegmentIndexInRibbon = -1;
        }

        public void CopyRibbonData(SegmentIO segment)
        {
            EdgeLabel = segment.EdgeLabel;
            RibbonIndex = segment.RibbonIndex;
            SegmentIndexInRibbon = segment.SegmentIndexInRibbon;
        }

        public SegmentIO(JToken data)
        {
            // Indexes
            var token = data["Indexes"];
            Indices = new int[] { (int)token[0], (int)token[1] };

            // Subdivision
            Subdivision = (int)data["Subdivision"];

            // Curve, ControlPoints and Degree
            token = data["ControlPoints"];
            int count = token.Count();
            ControlPoints = new Point3d[count];
            for (int i = 0; i < count; i++)
            {
                var p = token[i];
                ControlPoints[i] = new Point3d((double)p["X"], (double)p["Y"], (double)p["Z"]);
            }
            Degree = (int)data["Degree"];
            _curve = Curve.CreateControlPointCurve(ControlPoints, Degree).ToNurbsCurve();

            // RestLength
            RestLength = (double)data["RestLength"];

            // CurvePoints
            token = data["CurvePoints"];
            count = token.Count();
            CurvePoints = new Point3d[count];
            for (int i = 0; i < count; i++)
            {
                var p = token[i];
                CurvePoints[i] = new Point3d((double)p["X"], (double)p["Y"], (double)p["Z"]);
            }

            // IsCurvedEdge, IsPeriodic
            IsCurvedEdge = (bool)data["IsCurvedEdge"];
            IsPeriodic = (bool)data["IsPeriodic"];
            StartJoint = (int)data["StartJoint"];
            EndJoint = (int)data["EndJoint"];

            // EdgeType
            int eT = (int)data["EdgeType"];
            switch (eT)
            {
                case 0:
                    SegmentType = SegmentTypes.Undefined;
                    break;
                case 1:
                    SegmentType = SegmentTypes.EdgeLine;
                    break;
                case 2:
                    SegmentType = SegmentTypes.EdgeCurve;
                    break;
                default:
                    SegmentType = SegmentTypes.Undefined;
                    break;
            }

            // Knots
            token = data["Knots"];
            count = token.Count();
            Knots = new double[count];
            for (int i = 0; i < count; i++)
            {
                Knots[i] = (double)token[i];
            }

            // Weights
            token = data["Weights"];
            count = token.Count();
            Weights = new double[count];
            for (int i = 0; i < count; i++)
            {
                Weights[i] = (double)token[i];
            }

            // Spline beam ids
            int eL = (int)data["EdgeLabel"];
            switch (eL)
            {
                case 0:
                    EdgeLabel = SegmentLabels.Undefined;
                    break;
                case 1:
                    EdgeLabel = SegmentLabels.RodA;
                    break;
                case 2:
                    EdgeLabel = SegmentLabels.RodB;
                    break;
                default:
                    EdgeLabel = SegmentLabels.Undefined;
                    break;
            }
            RibbonIndex = (int)data["SplineBeamGlobalIndex"];
            SegmentIndexInRibbon = (int)data["EdgeBeamLocalIndex"];
        }

        public int GetEdgeCount()
        {
            return CurvePoints.Length - 1;
        }

        public void BuildCenterLine(Vector3d startVec = default, Vector3d endVec = default)
        {
            double t0 = _curve.Domain.T0;
            double t1 = _curve.Domain.T1;
            int jointNum = 0;

            /////////////////////////
            /// First round for checking end-egdes controlled by joints
            /////////////////////////
            CurvePoints = new Point3d[Subdivision + 1];
            if (startVec != default)
            {
                Vector3d start_vec = GetStartVector(_curve, true);
                start_vec *= startVec.Length;
                CurvePoints[0] = _curve.PointAtStart - start_vec;
                CurvePoints[1] = _curve.PointAtStart + start_vec;
                _curve.ClosestPoint(CurvePoints[1], out t0);
                jointNum++;
            }
            if (endVec != default)
            {
                Vector3d end_vec = GetEndVector(_curve, true);
                end_vec *= endVec.Length;
                CurvePoints[Subdivision - 1] = _curve.PointAtEnd + end_vec;
                CurvePoints[Subdivision] = _curve.PointAtEnd - end_vec;
                _curve.ClosestPoint(CurvePoints[Subdivision - 1], out t1);
                jointNum++;
            }

            // Extract subdomain of the curve for computing equal subdivisions
            NurbsCurve subCrv = _curve.ToNurbsCurve(new Interval(t0, t1));
            int subd = Subdivision - jointNum;
            double extendedLength = subCrv.GetLength();
            if (jointNum == 1) extendedLength *= subd / (subd - 0.5);
            double length = extendedLength / subd;

            /////////////////////////
            /// Second round for setting end-egdes not controlled by joints
            /////////////////////////
            if (startVec == default)
            {
                Vector3d start_vec = GetStartVector(_curve, true);
                start_vec *= length * 0.5;
                CurvePoints[0] = _curve.PointAtStart - start_vec;
                CurvePoints[1] = _curve.PointAtStart + start_vec;
                _curve.ClosestPoint(CurvePoints[1], out t0);
            }
            if (endVec == default)
            {
                Vector3d end_vec = GetEndVector(_curve, true);
                end_vec *= length * 0.5;
                CurvePoints[Subdivision - 1] = _curve.PointAtEnd + end_vec;
                CurvePoints[Subdivision] = _curve.PointAtEnd - end_vec;
                _curve.ClosestPoint(CurvePoints[Subdivision - 1], out t1);
            }

            // Subdivision of internal edges
            subCrv = _curve.ToNurbsCurve(new Interval(t0, t1));
            length = subCrv.GetLength() / (Subdivision - 2);
            double[] t = subCrv.DivideByLength(length, false);

            for (int i = 0; i < t.Length; i++) CurvePoints[2 + i] = _curve.PointAt(t[i]);
        }

        public Curve GetUnderlyingCurve()
        {
            return _curve;
        }

        /// <summary>
        /// Access start/end joint indices by label 0/1.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int JointAt(int i)
        {
            if (i == 0) return StartJoint;
            if (i == 1) return EndJoint;
            throw new Exception("Out of bounds.");
        }

        public Vector3d GetStartVector(Curve curve = default, bool unitize = false)
        {
            if (curve == default) curve = _curve;
            Vector3d v = curve.PointAt(curve.Domain.ParameterAt(STEPSIZE)) - curve.PointAtStart;

            if (unitize) v.Unitize();
            return v;
        }

        public Point3d GetPoint(int idx)
        {
            if (idx == 0) return _curve.PointAtStart;
            if (idx == 1) return _curve.PointAtEnd;
            throw new Exception("Index out of");
        }

        public bool IsStartPoint(Point3d pos)
        {
            return _curve.PointAtStart.DistanceTo(pos) < _curve.PointAtEnd.DistanceTo(pos) ? true : false;
        }

        public Vector3d GetEndVector(Curve curve = default, bool unitize = false)
        {
            if (curve == default) curve = _curve;
            Vector3d v = curve.PointAt(curve.Domain.ParameterAt(1-STEPSIZE)) - curve.PointAtEnd;

            if (unitize) v.Unitize();
            return v;
        }

        public void SetCurvePoints(Point3d[] pts)
        {
            CurvePoints = pts;
            Subdivision = pts.Length - 1;
        }

        public int HasStartJoint() { return StartJoint == -1 ? 0 : 1; }

        public int HasEndJoint() { return EndJoint == -1 ? 0 : 1; }

        public override string ToString()
        {
            return SegmentType.ToString() + "[Length:" + RestLength + "]\n";
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "Not enough data has been provided";

        public string TypeName => ToString();

        public string TypeDescription => ToString();

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
            if (typeof(T).Equals(typeof(GH_Curve)))
            {
                target = (T)(object)new GH_Curve(_curve);
                return true;
            }

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

        public object Clone()
        {
            return new SegmentIO(this);
        }
        #endregion
    }
}
