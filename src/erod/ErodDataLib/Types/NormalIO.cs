using System;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class NormalIO
    {
        public int IndexMap { get; set; }
        public Vector3d NormalVector { get; private set; }
        public Point3d ReferencePosition { get; private set; }

        public NormalIO(Point3d pos, Vector3d normal)
        {
            ReferencePosition = pos;
            IndexMap = -1;
            NormalVector = normal;
        }

        public override string ToString()
        {
            return "NormalData";
        }
    }
}
