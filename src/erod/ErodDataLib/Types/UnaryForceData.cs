using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class UnaryForceData : ElementData
    {
        public int[] Indices { get; set; }
        public Vector3d Vector { get; set; }

        public UnaryForceData(JToken data) : base(1)
        {
            // Indexes
            var token = data["Indices"];
            int count = token.Count();
            Indices = new int[count];
            for (int i = 0; i < count; i++)
            {
                var p = token[i];
                Indices[i] = (int)token[i];
            }

            // Vector force
            token = data["Vector"];
            Vector = new Vector3d( (double)token["X"], (double)token["Y"], (double)token["Z"] );
        }

        public UnaryForceData(Point3d p) : base(p)
        {
            Indices = new int[1];
        }

        public override string ToString()
        {
            return "ExternalForceData";
        }
    }
}
