using System;
using System.Collections.Generic;
using System.Linq;
using ErodDataLib.Types;
using ErodDataLib.Utils;
using Rhino.Geometry;

namespace ErodModelLib.Utils
{
    public class BaseTargetSurface
    {
        public double[][] Vertices { get; set; }
        public int[][] Faces { get; set; }
        public double[] JointPosOnMesh { get; private set; }
        public int[] FeatureJoints { get; private set; }

        private Mesh mesh { get; set; }

        public BaseTargetSurface(Mesh targetMesh, LinkageIO linkage) : base()
        {
            Init(targetMesh);
            ComputeJointOnMesh(linkage);
        }

        public BaseTargetSurface(Mesh targetMesh, List<Point3d> pts, List<int> featureJoints) : base()
        {
            Init(targetMesh);
            ComputeJointOnMesh(pts, featureJoints);
        }

        public BaseTargetSurface(Mesh targetMesh, IEnumerable<MeshPoint> pts, double tol)
        {
            Init(targetMesh);
            ComputeJointOnMesh(pts, tol);
        }

        private void Init(Mesh targetMesh)
        {
            mesh = targetMesh;

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

        private void ComputeJointOnMesh(LinkageIO linkage)
        {
            // Store closest joint position on target mesh
            double[] jointsOnMesh = new double[linkage.Joints.Count * 3];
            for (int i = 0; i < linkage.Joints.Count; i++)
            {
                var pos = linkage.Joints[i].Position;
                Point3d meshpoint = mesh.ClosestPoint(new Point3d(pos[0], pos[1], pos[2]));

                jointsOnMesh[i * 3] = meshpoint.X;
                jointsOnMesh[i * 3 + 1] = meshpoint.Y;
                jointsOnMesh[i * 3 + 2] = meshpoint.Z;
            }
            JointPosOnMesh = jointsOnMesh;
        }

        private void ComputeJointOnMesh(IEnumerable<MeshPoint> pts, double tol)
        {
            double[] jointsOnMesh = new double[pts.Count() * 3];
            List<int> featureJoints = new List<int>();
            for (int i = 0; i < pts.Count(); i++)
            {
                var oldP = pts.ElementAt(i);
                Point3d newP = mesh.PointAt(oldP);

                jointsOnMesh[i * 3] = newP.X;
                jointsOnMesh[i * 3 + 1] = newP.Y;
                jointsOnMesh[i * 3 + 2] = newP.Z;

                if (oldP.Point.DistanceTo(newP) > tol) featureJoints.Add(i);
            }
            JointPosOnMesh = jointsOnMesh;
            FeatureJoints = featureJoints.ToArray();
        }

        private void ComputeJointOnMesh(List<Point3d> pts, List<int> featureJoints)
        {
            double[] jointsOnMesh = new double[pts.Count() * 3];
            for (int i = 0; i < pts.Count(); i++)
            {
                Point3d newP = mesh.ClosestPoint(pts.ElementAt(i));

                jointsOnMesh[i * 3] = newP.X;
                jointsOnMesh[i * 3 + 1] = newP.Y;
                jointsOnMesh[i * 3 + 2] = newP.Z;
            }
            JointPosOnMesh = jointsOnMesh;
            FeatureJoints = featureJoints.ToArray();
        }

        public override string ToString()
        {
            return "BaseTargetSurface";
        }

        public Mesh GetUnderlyingMesh()
        {
            return mesh;
        }
    }
}