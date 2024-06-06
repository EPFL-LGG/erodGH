using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
	public class CableForceData : ElementData
	{
        public int[] Indices { get; set; }
        public double E { get; set; }
        public double A { get; private set; }
        public double RestLength { get; private set; }

        public CableForceData(JToken data) : base(1)
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
            E = (double) data["E"];
            A = (double)data["A"];
            RestLength = (double)data["RestLength"];
        }

        public CableForceData(Line ln, double modulus, double area, double restLength) : base(ln.From, ln.To)
        {
            Indices = new int[2];
            E = modulus;
            A = area;
            RestLength = restLength;
        }

        public override string ToString()
        {
            return "ExternalForceData";
        }
    }
}

