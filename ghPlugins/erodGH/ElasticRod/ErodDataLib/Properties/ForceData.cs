using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class ForceData : ElementData
    {
        public int[] Indexes { get; set; }
        public Vector3d Vector { get; set; }
        public ForceData(JToken data) : base(1)
        {
            // Indexes
            var token = data["Indexes"];
            int count = token.Count();
            Indexes = new int[count];
            for (int i = 0; i < count; i++)
            {
                var p = token[i];
                Indexes[i] = (int)token[i];
            }

            // Vector force
            token = data["Vector"];
            Vector = new Vector3d( (double)token["X"], (double)token["Y"], (double)token["Z"] );
        }

        public ForceData(Point3d p) : base(p)
        {
            Indexes = new int[1];
        }

        public override string ToString()
        {
            return "ExternalForceData";
        }
    }
}
