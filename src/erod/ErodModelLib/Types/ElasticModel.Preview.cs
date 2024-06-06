using System;
using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
    public abstract partial class ElasticModel : IGH_PreviewData, IGH_BakeAwareData
    {
        public BoundingBox ClippingBox
        {
            get
            {
                return new BoundingBox(MeshVis.Vertices.ToPoint3dArray());
            }
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            if (att == null) att = doc.CreateDefaultAttributes();

            string id = Guid.NewGuid().ToString();
            int idxGr = doc.Groups.Add(ToString() + id);

            ObjectAttributes att1 = att.Duplicate();
            att1.AddToGroup(idxGr);

            doc.Objects.AddMesh(MeshVis);

            return true;
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            args.Pipeline.DrawMeshShaded(MeshVis, args.Material);
            args.Pipeline.DrawPoints(Supports.GetFixSupportsAsPoint3dArray(), Rhino.Display.PointStyle.RoundControlPoint, (float)7, System.Drawing.Color.Blue);
            if(Supports.ShowTemporarySupports) args.Pipeline.DrawPoints(Supports.GetTemporarySupportsAsPoint3dArray(), Rhino.Display.PointStyle.Simple, (float)4, System.Drawing.Color.GreenYellow);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawMeshWires(MeshVis, args.Color);
            args.Pipeline.DrawPoints(Supports.GetFixSupportsAsPoint3dArray(), Rhino.Display.PointStyle.RoundControlPoint, (float)7, System.Drawing.Color.Blue);
            args.Pipeline.DrawPoints(Supports.GetFixSupportsAsPoint3dArray(), Rhino.Display.PointStyle.RoundControlPoint, (float)7, System.Drawing.Color.Blue);
            if (Supports.ShowTemporarySupports) args.Pipeline.DrawPoints(Supports.GetTemporarySupportsAsPoint3dArray(), Rhino.Display.PointStyle.Simple, (float)4, System.Drawing.Color.GreenYellow);
        }
    }
}
