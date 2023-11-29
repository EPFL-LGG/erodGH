using System;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public abstract class ElementData : IGH_Goo
    {
        protected int NONE = -1;
        protected Point3d[] Positions;

        public ElementData(Point3d p1, Point3d p2)
        {
            Positions = new Point3d[2];
            Positions[0] = p1;
            Positions[1] = p2;
        }

        public ElementData(Point3d p)
        {
            Positions = new Point3d[] { p };
        }

        public ElementData(Point3d[] points)
        {
            Positions = points;
        }

        public ElementData(int numVertices)
        {
            Positions = new Point3d[numVertices];
        }

        public int GetPointCount()
        {
            return Positions.GetLength(0);
        }

        public Point3d GetPoint(int idx)
        {
            return Positions[idx];
        }

        public override string ToString()
        {
            return "Element";
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "Not enough data has been provided";

        public string TypeName => ToString();

        public string TypeDescription => ToString();

        public IGH_Goo Duplicate()
        {
            return (IGH_Goo) this.MemberwiseClone();
        }

        public IGH_GooProxy EmitProxy()
        {
            return null;
        }

        public bool CastFrom(object source)
        {
            return false;
        }

        public bool CastTo<T>(out T target)
        {
            target = default(T);
            return false;
        }

        public object ScriptVariable()
        {
            return null;
        }

        public bool Write(GH_IWriter writer)
        {
            return false;
        }

        public bool Read(GH_IReader reader)
        {
            return false;
        }
        #endregion
    }
}
