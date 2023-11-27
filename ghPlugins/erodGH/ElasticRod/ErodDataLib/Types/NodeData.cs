using System;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class NodeData : ElementData
    { 
        public NodeData(Point3d p) : base(p)
        {
        }

        public override string ToString()
        {
            return "NodeData";
        }
    }
}
