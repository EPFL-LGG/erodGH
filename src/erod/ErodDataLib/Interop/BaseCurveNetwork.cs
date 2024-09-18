using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErodDataLib.Types;
using ErodModelLib.Utils;
using Newtonsoft.Json;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace ErodDataLib.Utils
{
    public class BaseCurveNetwork
    {
        public GraphObject Graph { get; set; }
        public List<SplineCurveObject> Curves { get; set; }
        public List<double> CurvesDoF { get; set; }
        public int NumJoints { get; set; }
        public int Subdivision { get; set; }
        public int Mult { get; set; }
        public double Angle { get; set; }
        public double E { get; set; }
        public double PoisonsRatio { get; set; }
        public double[] MatParameters { get; set; }
        public double Height { get; set; }
        public string MatType { get; set; }
        public BaseTargetSurface TargetSurface {get; private set;}
        private PointCloud cloud;

        public BaseCurveNetwork(LinkageIO linkage, double angle, Mesh targetSurface=default, double tol = 1e-3)
        {
            // Underlying graph data
            Graph = new GraphObject(linkage);
            Subdivision = 0;

            // Angle
            Angle = angle;

            // Material data
            var mat = linkage.Materials[0];
            E = mat.E;
            PoisonsRatio = mat.PoisonsRatio;
            MatParameters = mat.Parameters;

            switch (mat.CrossSectionType)
            {
                case 0:
                    MatType = "RECTANGLE";
                    break;
                case 1:
                    MatType = "ELLIPSE";
                    break;
                case 2:
                    MatType = "I";
                    break;
                case 3:
                    MatType = "L";
                    break;
                case 4:
                    MatType = "+";
                    break;
                default:
                    MatType = "RECTANGLE";
                    break;
            }

            // Curves DoFs (only linkages with curves layouts)
            if (linkage.Layout.ContainsLayoutData())
            {
                cloud = new PointCloud();
                CurvesDoF = new List<double>();
                Curves = new List<SplineCurveObject>();

                for (int i = 0; i < linkage.Segments.Count; i++)
                {
                    var segment = linkage.Segments[i];
                    Curve eCrv = segment.GetUnderlyingCurve();
                    Subdivision += segment.Subdivision;

                    Point3d p0 = eCrv.PointAtStart;
                    int start = cloud.ClosestPoint(p0);
                    if (start == -1)
                    {
                        cloud.Add(p0);
                        CurvesDoF.AddRange(new double[] { p0.X, p0.Y });
                    }
                    else if (p0.DistanceTo(cloud[start].Location) > tol)
                    {
                        cloud.Add(p0);
                        CurvesDoF.AddRange(new double[] { p0.X, p0.Y });
                    }

                    Point3d p1 = eCrv.PointAtEnd;
                    int end = cloud.ClosestPoint(p1);
                    if (p1.DistanceTo(cloud[end].Location) > tol)
                    {
                        cloud.Add(p1);
                        CurvesDoF.AddRange(new double[] { p1.X, p1.Y });
                    }
                }
                NumJoints = linkage.Joints.Count;
                Subdivision /= linkage.Segments.Count;

                // Splines of family type A
                foreach (int key in linkage.Layout.RibbonsFamilyA.Keys)
                {
                    WriteSplineBeamData(linkage, SegmentLabels.RodA, key);
                }

                // Splines of family type B
                foreach (int key in linkage.Layout.RibbonsFamilyB.Keys)
                {
                    WriteSplineBeamData(linkage, SegmentLabels.RodB, key);
                }
            }
            else
            {
                for (int i = 0; i < linkage.Segments.Count; i++)
                {
                    var segment = linkage.Segments[i];
                    Subdivision += segment.Subdivision;
                }
                NumJoints = linkage.Joints.Count;
                Subdivision /= linkage.Segments.Count;
            }

            if (targetSurface == default) TargetSurface = null;
            else TargetSurface = new BaseTargetSurface(targetSurface, linkage);
        }

        private void WriteSplineBeamData(LinkageIO linkage, SegmentLabels label, int splineBeamKey)
        {
            SplineCurveObject obj = new SplineCurveObject();
            // Initialize
            var indexes = new HashSet<int>();
            var numControlPoints = new List<int>();

            // Hierarchical structure: spline->segment->edge
            // Spline data
            HashSet<int> splineBeam;
            if (label == SegmentLabels.RodA)
            {
                splineBeam = linkage.Layout.RibbonsFamilyA[splineBeamKey];
                obj.CurveFamily = 0;
            }
            else
            {
                splineBeam = linkage.Layout.RibbonsFamilyB[splineBeamKey];
                obj.CurveFamily = 1;
            }

            // Loop segment data
            List<Curve> tempCrv = new List<Curve>();
            for (int i = 0; i < splineBeam.Count; i++)
            {
                var sData = linkage.Segments[splineBeam.ElementAt(i)];
                //var edge = linkage.Segments[splineBeam.ElementAt(i)];

                // Add joints indices
                Curve crv = sData.GetUnderlyingCurve();
                indexes.Add(cloud.ClosestPoint(crv.PointAtStart));
                indexes.Add(cloud.ClosestPoint(crv.PointAtEnd));
                tempCrv.Add(crv);

                // Add DoFs for control points (offsets)
                int numCP = crv.ToNurbsCurve().Points.Count() - 2;
                if (numCP == 0) // Case of a line
                {
                    numControlPoints.Add(1);
                    CurvesDoF.Add(0);
                }
                else
                { // Case of a curve
                    if (numCP > 4) numCP = 4;
                    numControlPoints.Add(numCP);

                    double t = 1.0 / (numCP + 1);
                    Vector3d vecX = crv.PointAtEnd - crv.PointAtStart;
                    Vector3d vecZ = Vector3d.ZAxis;
                    Vector3d vecY = Vector3d.CrossProduct(vecZ, vecX);

                    for (int j = 0; j < numCP; j++)
                    {
                        Point3d orig = crv.PointAtStart + vecX * (t * (j + 1));
                        Point3d proj = Intersection.CurveLine(crv, new Line(orig, orig + vecY), 0.001, 0.001).ElementAt(0).PointA;

                        Vector3d vec = proj - orig;
                        double offset = vec.Length;
                        if (vec * vecY < 0) offset *= -1;

                        CurvesDoF.Add(offset);
                    }
                }

            }

            obj.Indexes = indexes;
            obj.NumControlPoints = numControlPoints;

            Curves.Add(obj);
        }

        public void WriteJsonFile(string path, string filename)
        {
            // Serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(@path + filename + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }

        public override string ToString()
        {
            return "InteropData";
        }
    }

    public class SurfaceMesh
    {
        public double[][] Vertices { get; set; }
        public int[][] Faces { get; set; }

        public SurfaceMesh(Mesh mesh)
        {
            int vCount = mesh.Vertices.Count;
            Vertices = new double[vCount][];
            for (int i = 0; i < vCount; i++)
            {
                Point3d v = mesh.Vertices[i];
                Vertices[i] = new double[] { v.X, v.Y, v.Z };
            }

            int fCount = mesh.Faces.Count;
            Faces = new int[fCount][];
            for (int i = 0; i < fCount; i++)
            {
                MeshFace f = mesh.Faces[i];
                Faces[i] = new int[] { 0, f.A, f.B, f.C };
            }
        }
    }

    public class SplineCurveObject
    {
        public HashSet<int> Indexes { get; set; }
        public List<int> NumControlPoints { get; set; }
        public int CurveFamily { get; set; }

        public SplineCurveObject()
        {
            Indexes = new HashSet<int>();
            NumControlPoints = new List<int>();
        }
    }

    public class GraphObject
    {
        public List<int[]> Edges { get; set; }
        public List<double[]> Vertices { get; set; }

        public GraphObject()
        {
            Edges = new List<int[]>();
            Vertices = new List<double[]>();
        }

        public GraphObject(LinkageIO linkage)
        {
            Vertices = new List<double[]>();
            var tempV = linkage.Joints;// GetVertices();
            foreach (var joint in tempV)
            {
                var p = joint.Position;
                Vertices.Add(new double[] { p.X, p.Y, p.Z });
            }

            Edges = new List<int[]>();
            var tempE = linkage.Segments;
            foreach (var e in tempE) Edges.Add(new int[] { e.StartJoint, e.EndJoint });
        }
    }
}