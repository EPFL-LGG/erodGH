using System;
using System.Numerics;
using ErodModelLib.Creators;
using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
	public class CableForce : Force, IGH_PreviewData, IGH_BakeAwareData
    {
        public Point3d[] Positions { get; private set; }
        public double E { get; private set; }
        public double A { get; private set; }
        public double RestLength { get; private set; }

        public CableForce(Point3d[] positions, int[] indices, bool[] isJoint, double modulus, double area, double restLength) : base(indices, isJoint)
		{
            E = modulus;
            A = area;
            RestLength = restLength;
            Positions = positions;
		}

        // Calculation of forces based on Hooke's law
        public override Vector3d[] CalculateForces(ElasticModel model)
        {
            // Update positions
            if (model.ModelType == ModelTypes.ElasticRod || model.ModelType == ModelTypes.PeriodicRod)
            {
                var rod = (ElasticRod) model;
                // TODO: include connections with nodes. Only joints can be connected with cables
                return new Vector3d[0];
            }
            else if (model.ModelType == ModelTypes.RodLinkage)
            {
                // TODO: include connections with nodes. Only joints can be connected with cables
                var linkage = (RodLinkage) model;
                for(int i=0; i<2; i++) Positions[i] = linkage.Joints[Indices[i]].GetPositionAsPoint3d();
            }
            else { return new Vector3d[0]; }

            Line cable = GetEdgeLine();
            double k = E*A / RestLength * (cable.Length - RestLength) * 0.5;
            Vector3d f1 = (cable.To - cable.From) / cable.Length * 0.01*k;
            Vector3d f2 = (cable.From - cable.To) / cable.Length * 0.01*k;

            return new Vector3d[] { f1, f2 };
        }

        public Line GetEdgeLine()
        {
            return new Line(Positions[0], Positions[1]);
        }

        #region GH_Preview
        public BoundingBox ClippingBox => GetEdgeLine().BoundingBox;

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            args.Pipeline.DrawLine(GetEdgeLine(), System.Drawing.Color.Black, 5);
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawLine(GetEdgeLine(), System.Drawing.Color.Black, 5);
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            obj_guid = Guid.Empty;

            if (att == null) att = doc.CreateDefaultAttributes();

            string id = Guid.NewGuid().ToString();
            int idxGr = doc.Groups.Add(ToString() + id);

            ObjectAttributes att1 = att.Duplicate();
            att1.AddToGroup(idxGr);

            doc.Objects.AddLine(GetEdgeLine());

            return true;
        }
        #endregion
    }
}

