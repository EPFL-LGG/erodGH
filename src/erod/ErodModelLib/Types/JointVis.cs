using System;
using System.Drawing;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
	public class JointVis : IGH_PreviewData, IGH_Goo

    {
        private Point3d _position;
        private float _radius;
        private Color _color;

		public JointVis(Point3d pos, float size, Color color=default)
		{
            _position = pos;
            _radius = size;
            if (color == default) _color = Color.Honeydew;
            else _color = color;
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return new BoundingBox(new Point3d[] { _position });
            }
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            args.Pipeline.DrawPoint(_position, Rhino.Display.PointStyle.RoundSimple, _radius, _color);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawPoint(_position, Rhino.Display.PointStyle.RoundSimple, _radius, _color);
        }

        #region GH_Methods
        public bool IsValid
        {
            get
            {
                return true;
            }
        }

        public string IsValidWhyNot => "";

        public string TypeName => "JointQuantities";

        public string TypeDescription => "";

        public IGH_Goo Duplicate()
        {
            return (IGH_Goo)this.MemberwiseClone();
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

