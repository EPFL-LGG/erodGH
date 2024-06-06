using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
	public abstract class Force
	{
		public int[] Indices { get; private set;}
		public bool[] IsJoint { get; private set; }

		public Force(int[] indices, bool[] isJoint){
			Indices = indices;
			IsJoint = isJoint;
		}

		public abstract Vector3d[] CalculateForces(ElasticModel linkage);
    }
}

