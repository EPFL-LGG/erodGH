using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
	public class ForceCableIO : ForceIO
	{
        public double E { get; set; }
        public double CrossSectionArea { get; private set; }
        public double RestLength { get; private set; }

        public ForceCableIO(JToken data)
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
            E = (double) data["E"];
            CrossSectionArea = (double)data["CrossSectionArea"];
            RestLength = (double)data["RestLength"];
            ForceType = ForceIOType.Cable;
        }

        public ForceCableIO(Line ln, double modulus, double area, double restLength) : base(new Point3d[] { ln.From, ln.To }, ForceIOType.Cable)
        {
            E = modulus;
            CrossSectionArea = area;
            RestLength = restLength;
        }

        public ForceCableIO(ForceCableIO force) : base(force.Positions, ForceIOType.Cable)
        {
            E = force.E;
            CrossSectionArea = force.CrossSectionArea;
            RestLength = force.RestLength;
            Indices = force.Indices;
            IndicesDoFs = force.IndicesDoFs;
            IsJoint = force.IsJoint;
        }

        public override string ToString()
        {
            return "CableForce";
        }

        public override Vector3d[] CalculateForce(Point3d[] positions)
        {
            // Update positions
            Positions = positions;

            double length = Positions[0].DistanceTo(Positions[1]) + 1e-6;
            Vector3d unitVector = (Positions[1] - Positions[0]) / length;
            double stiffness = (E * CrossSectionArea) / (RestLength + 1e-6);
            double tension = stiffness * (length - RestLength);

            Vector3d f1 = tension * unitVector;
            Vector3d f2 = -f1;

            return new Vector3d[] { f1, f2 }; ;
        }

        public override object Clone()
        {
            return new ForceCableIO(this);
        }
    }
}

