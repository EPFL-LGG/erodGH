using System;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public enum ForceIOType { Undefined=0,  External = 0, Cable = 1 };

    public abstract class ForceIO : ICloneable
	{
        public Point3d[] Positions { get; protected set; }
		public int[] Indices { get; protected set; }
		public int[][] IndicesDoFs { get; protected set; }
        public bool[] IsJoint { get; protected set; }
		public ForceIOType ForceType { get; protected set; }

		public int NumPositions => Positions.Length;

        public ForceIO(){}

        public ForceIO(Point3d[] pts, ForceIOType forceType)
		{
			int numPos = pts.Length;
			ForceType = forceType;
			SetReferencePositions(pts);
        }

		public void SetReferencePositions(Point3d[] pts)
		{
            Positions = pts;
            Indices = new int[Positions.Length];
            IsJoint = new bool[Positions.Length];
            IndicesDoFs = new int[Positions.Length][];
        }

		public void SetIndexMap(int index, int indexMap, bool isJoint, int[] indicesDoF)
		{
			Indices[index] = indexMap;
			IsJoint[index] = isJoint;
			IndicesDoFs[index] = indicesDoF;
		}

        public abstract Vector3d[] CalculateForce(Point3d[] positions);

		public abstract object Clone();
    }
}

