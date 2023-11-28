using System;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class NormalData : ElementData
    {
        public int[] Indexes { get; set; }

        public Vector3d Vector { get; set; }

        public NormalData(Point3d p) : base(p)
        {
            Indexes = new int[1];
        }

        public override string ToString()
        {
            return "NormalData";
        }
    }
}
