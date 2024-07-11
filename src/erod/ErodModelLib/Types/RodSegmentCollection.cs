using System;
using System.Collections;
using System.Collections.Generic;
using ErodModelLib.Creators;
using System.Runtime.InteropServices;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
	public class RodSegmentCollection : IReadOnlyList<RodSegment>, ICloneable
    {
        private List<RodSegment> _segments;
        private PointCloud _nodes;
        private IntPtr _model;

        public Point3d[] Nodes => _nodes.GetPoints();

        public RodSegmentCollection(IntPtr model)
		{
            _segments = new List<RodSegment>();
            _nodes = new PointCloud();
            _model = model;
		}

        public RodSegmentCollection(RodSegmentCollection segments)
        {
            _segments = new List<RodSegment>(segments._segments);
            _nodes = new PointCloud(segments._nodes);
            _model = segments._model;
        }

        public void Add(RodSegment item)
        {
            _segments.Add(item);
        }

        public int ClosestNode(Point3d pt)
        {
            return _nodes.ClosestPoint(pt);
        }

        public Point3d GetNode(int index)
        {
            return _nodes[index].Location;
        }

        public void UpdateNodePositions()
        {
            int numCoords;
            IntPtr cPtr;
            Kernel.RodLinkage.ErodXShellGetCenterLinePositions(_model, out cPtr, out numCoords);

            double[] pos = new double[numCoords];
            Marshal.Copy(cPtr, pos, 0, numCoords);
            Marshal.FreeCoTaskMem(cPtr);

            _nodes = new PointCloud();
            for (int i = 0; i < pos.Length / 3; i++) _nodes.Add(new Point3d(pos[i * 3], pos[i * 3 + 1], pos[i * 3 + 2]));
        }

        public RodSegment this[int index] => _segments[index];

        public int Count => _segments.Count;

        public IEnumerator<RodSegment> GetEnumerator()
        {
            return _segments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _segments.GetEnumerator();
        }

        public void Clear()
        {
            _segments.Clear();
            _nodes = new PointCloud();
        }

        public object Clone()
        {
            return new RodSegmentCollection(this);
        }
    }
}

