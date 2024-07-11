using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
	public class ForceExternalIO : ForceIO
	{
        public Vector3d Force { get; private set; }

        public ForceExternalIO(JToken data)
        {
            var token = data["Positions"];
            int count = token.Count();
            var pos = new Point3d[count];
            for (int i = 0; i < count; i++)
            {
                var p = token[i];
                pos[i] = new Point3d((double)p[0], (double)p[1], (double)p[2]);
            }
            SetReferencePositions(pos);

            // Indices
            token = data["Indices"];
            count = token.Count();
            int[] indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = (int)token[i];

            // IsJoint
            token = data["IsJoint"];
            count = token.Count();
            bool[] isJoint = new bool[count];
            for (int i = 0; i < count; i++) isJoint[i] = (bool)token[i];

            // IndicesDoFs 
            token = data["IndicesDoFs"];
            count = token.Count();
            for (int i = 0; i < count; i++)
            {
                var dofs = token[i];
                int[] indicesDoFs = new int[dofs.Count()];
                for (int j = 0; j < dofs.Count(); j++) indicesDoFs[j] = (int)dofs[j];

                SetIndexMap(i, indices[i], isJoint[i], indicesDoFs);
            }

            // Vector force
            token = data["Force"];
            Force = new Vector3d((double)token[0], (double)token[1], (double)token[2]);
            ForceType = ForceIOType.External;
        }

        public ForceExternalIO(Point3d pos, Vector3d force) : base( new Point3d[] { pos }, ForceIOType.External)
        {
            Force = force;
        }

        public ForceExternalIO(ForceExternalIO force) : base(force.Positions, ForceIOType.External)
        {
            Force = force.Force;
            Indices = force.Indices;
            IndicesDoFs = force.IndicesDoFs;
            IsJoint = force.IsJoint;
        }

        public override string ToString()
        {
            return "ExternalForce";
        }

        public override Vector3d[] CalculateForce(Point3d[] positions)
        {
            if (Positions.Length != positions.Length) throw new Exception("Invalid number of positions for calculating forces.");
            Positions = positions;
            Vector3d[] forces = new Vector3d[positions.Length];
            for (int i = 0; i < Positions.Length; i++) forces[i] = Force;
            return forces;
        }

        public override object Clone()
        {
            return new ForceExternalIO(this);
        }
    }
}

