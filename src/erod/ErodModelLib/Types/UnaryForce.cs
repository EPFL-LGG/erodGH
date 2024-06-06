using System;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
	public class UnaryForce : Force
    {
		public Vector3d ForceVector { get; set; }

		public UnaryForce(int index, Vector3d force, bool isJoint=false) : base(new int[] { index }, new bool[] { isJoint })
        {
            ForceVector = force;
		}

        public override Vector3d[] CalculateForces(ElasticModel linkage)
        {
            return new Vector3d[] { ForceVector };
        }
    }
}

