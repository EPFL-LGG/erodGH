using System;
using System.Collections.Generic;
using Eto.Forms;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class RodIO : ModelIO
    {
        public bool IsPeriodic => ModelType == ElasticModelType.PeriodicRod ? true : false;
        public bool RemoveRestCurvature { get; set; }
        public double RestLength { get; private set; }

        public RodIO(Polyline crv, bool removeRestCurvatures=false) : base(crv.IsClosed ? ElasticModelType.PeriodicRod : ElasticModelType.ElasticRod)
        {
            List<SegmentIO> segments = new List<SegmentIO>();
            foreach (Line ln in crv.GetSegments()) segments.Add(new SegmentIO(ln.ToNurbsCurve()));
            Graph = new EdgeGraph(segments, new NormalIO[0], TOLERANCE_CLOSEST_POINT);

            RemoveRestCurvature = removeRestCurvatures;
            RestLength = crv.Length;
        }

        public RodIO(RodIO modelIO) : base(modelIO.ModelType)
        {
            Graph = (EdgeGraph)modelIO.Graph.Clone();
            Materials = (MaterialIOCollection)modelIO.Materials.Clone();
            Supports = (SupportIOCollection)modelIO.Supports.Clone();
            Forces = (ForceIOCollection)modelIO.Forces.Clone();
            RemoveRestCurvature = modelIO.RemoveRestCurvature;
            RestLength = modelIO.RestLength;
        }

        public double[] GetCenterLineCoordinates()
        {
            int nodeCount = Graph.NumNodes;
            List<double> coords = new List<double>();

            for (int i = 0; i < nodeCount; i++)
            {
                Point3d p = Graph.GetNode(i);
                coords.AddRange(new double[] { p.X, p.Y, p.Z });
            }

            if (IsPeriodic) for (int i = 0; i < 6; i++) coords.Add(coords[i]);

            return coords.ToArray();
        }

        public override object Clone()
        {
            return new RodIO(this);
        }
    }
}
