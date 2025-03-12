using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;
using System;
//using Grasshopper.Kernel.Geometry;

namespace ErodDataLib.Types
{
    public class EdgeGraph : ICloneable
    {
        private PointCloud _nodes { get; set; }
        private Dictionary<int, HashSet<int>> _incidentEdges { get; set; }
        private double[] _edgeLengths { get; set; }
        internal readonly int NONE = -1;

        public Point3d[] Nodes => _nodes.GetPoints();
        public Vector3d[] Normals => _nodes.GetNormals();
        public int[][] EdgeIndices { get; private set; }
        public Curve[] EdgeCurves { get; private set; }
        public int[] EdgeSubdivisions { get; private set; }
        public int NumEdges { get; private set; }
        public int NumNodes => _nodes.Count;

        public EdgeGraph(IEnumerable<SegmentIO> edges, IEnumerable<NormalIO> normals, double toleranceClosestPoint)
		{
            NumEdges = edges.Count();
            EdgeIndices = new int[NumEdges][];
            _nodes = new PointCloud();
            EdgeCurves = new Curve[NumEdges];
            EdgeSubdivisions = new int[NumEdges];
            _edgeLengths = new double[NumEdges];
            _incidentEdges = new Dictionary<int, HashSet<int>>();

            // Init graph edges
            for (int i=0; i<NumEdges; i++)
            {
                var e = edges.ElementAt(i);
                var crv = e.GetUnderlyingCurve();

                // Store curves
                EdgeCurves[i] = crv;
                // Store subdivisions
                EdgeSubdivisions[i] = e.Subdivision;
                // Store edge lengths
                _edgeLengths[i] = crv.GetLength();
                // Add indices
                EdgeIndices[i] = new int[] { AddNode(crv.PointAtStart, toleranceClosestPoint), AddNode(crv.PointAtEnd, toleranceClosestPoint) };
                // Add topology information
                if (!_incidentEdges.ContainsKey(EdgeIndices[i][0])) _incidentEdges.Add(EdgeIndices[i][0], new HashSet<int>());
                if (!_incidentEdges.ContainsKey(EdgeIndices[i][1])) _incidentEdges.Add(EdgeIndices[i][1], new HashSet<int>());
                _incidentEdges[EdgeIndices[i][0]].Add(i);
                _incidentEdges[EdgeIndices[i][1]].Add(i);
            }

            // Init node normals
            List<int> nodeIndices = Enumerable.Range(0, _nodes.Count).ToList();

            // Check for normals with unset reference positions 
            // The first normal found with an unset reference point will be applied globally.
            //var nG = normals.FirstOrDefault(n => n.ReferencePosition == Point3d.Unset);
            //if (nG != null) AddGlobalNormal(nG.NormalVector);

            // First: take all the normals with reference positions and update the graph
            var normalsWithReferences = normals.Where(n => n.ReferencePosition != Point3d.Unset).ToList();
            if(normalsWithReferences.Count > 0) foreach (var norm in normalsWithReferences) nodeIndices.Remove(AddNormal(norm)); // Add normals for given inputs
            // Second: Check for normals with unset reference positions 
            // The first normal found with an unset reference point will be applied globally.
            var nG = normals.FirstOrDefault(n => n.ReferencePosition == Point3d.Unset);
            if (nG != null) foreach (var nodeIdx in nodeIndices) AddNormal(nG.NormalVector, nodeIdx);
            else foreach (var nodeIdx in nodeIndices) ComputeNormalFromIncidentEdges(nodeIdx); // Compute normals using incident edges
        }

        public EdgeGraph(EdgeGraph graph)
        {
            NumEdges = graph.NumEdges;
            EdgeIndices = graph.EdgeIndices.ToArray();
            _nodes = (PointCloud)graph._nodes.Duplicate();
            EdgeCurves = graph.EdgeCurves.ToArray();
            EdgeSubdivisions = graph.EdgeSubdivisions.ToArray();
            _edgeLengths = graph._edgeLengths.ToArray();
            _incidentEdges = new Dictionary<int, HashSet<int>>(graph._incidentEdges);
        }

        public object Clone()
        {
            return new EdgeGraph(this);
        }

        public Point3d GetNode(int index) { return _nodes[index].Location; }

        public int GetClosestNode(Point3d pt) { return _nodes.ClosestPoint(pt); }

        public Point3d GetAveragePoint()
        {
            var p = _nodes.GetBoundingBox(false).Center;
            int idx = _nodes.ClosestPoint(p);
            return _nodes[idx].Location;
        }

        public void GetFlattenGraphData(out double[] coords, out double[] normals, out int[] edges)
        {
            coords = new double[NumNodes * 3];
            normals = new double[NumNodes * 3];
            edges = new int[NumEdges * 2];

            // Nodes
            for (int i = 0; i < NumNodes; i++)
            {
                var p = _nodes[i];
                for (int j = 0; j < 3; j++)
                {
                    coords[i * 3 + j] = p.Location[j];
                    normals[i * 3 + j] = p.Normal[j];
                }
            }

            // Edges
            for (int i = 0; i < NumEdges; i++)
            {
                for (int j = 0; j < 2; j++) edges[i * 2 + j] = EdgeIndices[i][j];
            }
        }

        public void ReverseEdges(bool[] keepOrientations)
        {
            if(NumEdges!=keepOrientations.Length)throw new Exception("Invalid number of edge orientation indicators. The number of edges is " + NumEdges + " while the number of indicators is " + keepOrientations.Length);

            for (int edgeIndex=0; edgeIndex<NumEdges; edgeIndex++) 
            {
                if (!keepOrientations[edgeIndex]) ReverseEdge(edgeIndex);
            }
        }

        public void ReverseEdge(int edgeIndex)
        {
            EdgeIndices[edgeIndex] = new int[] { EdgeIndices[edgeIndex][1], EdgeIndices[edgeIndex][0] };
            EdgeCurves[edgeIndex].Reverse();
        }

        private void ComputeNormalFromIncidentEdges(int nodeIndex)
        {
            Point3d pos = _nodes[nodeIndex].Location;
            int nodeValence = _incidentEdges[nodeIndex].Count;

            // Compute the normal of the plane that best fits the edge vectors 
            if (nodeValence > 1)
            {
                int[] sortedIdx = GetSortedIncidentEdges(nodeIndex);
                Vector3d n = new Vector3d();

                for (int i = 0; i < nodeValence; i++)
                {
                    int next = i==nodeValence-1 ? 0 : i + 1;

                    Curve e0 = EdgeCurves[sortedIdx[i]];
                    Curve e1 = EdgeCurves[sortedIdx[next]];
                    Vector3d v0 = GetTangentAtPoint(pos, e0);
                    Vector3d v1 = GetTangentAtPoint(pos, e1);

                    Vector3d cross = Vector3d.CrossProduct(v0, v1);
                    int sign = n * cross >= 0 ? 1 : -1;

                    n += cross * sign;
                }

                n.Unitize();

                _nodes[nodeIndex].Normal = n;
            }
            else _nodes[nodeIndex].Normal = Vector3d.ZAxis;
        }

        private int AddNode(Point3d p, double tolerance)
        {
            int idx = _nodes.ClosestPoint(p);
            if (idx == -1 || _nodes[idx].Location.DistanceTo(p) > tolerance)
            {
                _nodes.Add(p, new Vector3d(0, 0, 1));
                idx = _nodes.Count - 1;
            }
            return idx;
        }

        private int GetAdjacentNodeOnEdge(int nodeIndex, int edgeIdx)
        {
            int[] nodeIndices = EdgeIndices[edgeIdx];
            if (nodeIndices[0] == nodeIndex) return nodeIndices[1];
            if (nodeIndices[1] == nodeIndex) return nodeIndices[0];
            throw new Exception("Edge is not incident v!");
        }

        private double ComputeVectorAngle(Vector3d axis, Vector3d v1, Vector3d v2)
        {
            double sinAngle = Vector3d.CrossProduct(v1, v2) * axis;
            double s = Math.Max(-1.0, Math.Min(1.0, sinAngle));
            double c = Math.Max(-1.0, Math.Min(1.0, v1 * v2));
            return Math.Atan2(s, c);
        }

        private int[] GetSortedIndicesFromValues(double[] values)
        {
            var p = values.Select((x, i) => new KeyValuePair<double, int>(x, i))
                    .OrderBy(x => x.Key)
                    .ToArray();

            return p.Select(x => x.Value).ToArray();
        }

        public int[] GetSortedIncidentEdges(int nodeIdx)
        {
            Point3d pos = _nodes[nodeIdx].Location;
            Vector3d normal = _nodes[nodeIdx].Normal;
            if (normal == Vector3d.Unset)
            {
                Plane plane;
                Plane.FitPlaneToPoints(_incidentEdges[nodeIdx].Select(idx => {
                    var crv = EdgeCurves[idx];
                    return crv.PointAtStart.DistanceTo(pos) < crv.PointAtEnd.DistanceTo(pos) ? crv.PointAtEnd : crv.PointAtStart;
                }), out plane);
                normal = plane.ZAxis;
            }

            var indicesEdges = _incidentEdges[nodeIdx].ToArray();
            int nodeValence = indicesEdges.Length;

            int edgeIdx = indicesEdges[0];
            Vector3d tgt = GetTangentAtPoint(pos, EdgeCurves[edgeIdx]);
            Vector3d v0 = tgt - normal * (normal * tgt);

            double[] angles = new double[nodeValence-1];
            for (int i = 1; i < nodeValence; i++)
            {
                edgeIdx = indicesEdges[i];
                tgt = GetTangentAtPoint(pos, EdgeCurves[edgeIdx]);
                Vector3d vk = tgt - normal * (normal * tgt);
                double theta = ComputeVectorAngle(normal, v0, vk); // angle in [-pi, pi]

                if (theta < 0) theta += 2.0 * Math.PI; // compute angle in [0, 2 pi]
                angles[i - 1] = theta; ;
            }

            // Sort vectors 1, 2, 3  clockwise (ascending angle wrt vector 0), assign alternating labels
            int[] maps = GetSortedIndicesFromValues(angles); // sorted list: [angles[p[0]], angles[p[1]], angles[p[2]]]
            List<int> sortedIdx = new List<int>{ indicesEdges[0] };
            foreach (int idx in maps) sortedIdx.Add(indicesEdges[idx+1]);

            return sortedIdx.ToArray();
        }

        public int[] GetSortedIncidentEdgesWithCircleFit(int nodeIdx)
        {
            Point3d pos = _nodes[nodeIdx].Location;
            var adjacentVtx = _incidentEdges[nodeIdx].Select(idx => {
                var crv = EdgeCurves[idx];
                return crv.PointAtStart.DistanceTo(pos) < crv.PointAtEnd.DistanceTo(pos) ? crv.PointAtEnd : crv.PointAtStart;
            }).ToArray();

            Circle circle;
            Circle.TryFitCircleToPoints(adjacentVtx, out circle);
            var tParams = adjacentVtx.Select(vtx => {
                double t;
                circle.ClosestParameter(vtx, out t);
                return t;
            }).ToList();

            var tParamsSorted = new List<double>(tParams);
            tParamsSorted.Sort();

            var sortedIdx = tParamsSorted.Select(t => tParams.IndexOf(t)).ToArray();

            return sortedIdx.ToArray();
        }

        private int AddNormal(NormalIO normal)
        {
            Point3d p = normal.ReferencePosition;
            int idx = _nodes.ClosestPoint(p);
            _nodes[idx].Normal = normal.NormalVector;
            return idx;
        }

        private void AddGlobalNormal(Vector3d normal)
        {
            foreach (var n in _nodes) n.Normal = normal;
        }

        private void AddNormal(Vector3d normal, int nodeIndex)
        {
            _nodes[nodeIndex].Normal = normal;
        }

        private Vector3d GetTangentAtPoint(Point3d node, Curve edge)
        {
            Vector3d tgt = edge.PointAtStart.DistanceTo(node) < edge.PointAtEnd.DistanceTo(node) ? edge.TangentAtStart : -edge.TangentAtEnd;
            return tgt;
        }

        private bool IsStartNode(Point3d node, Curve edge)
        {
            return edge.PointAtStart.DistanceTo(node) < edge.PointAtEnd.DistanceTo(node) ? true : false;
        }

        public int GetNodeValence(int nodeIndex)
        {
            return _incidentEdges[nodeIndex].Count;
        }

        public int[] GetIncidentEdges(int nodeIndex)
        {
            return _incidentEdges[nodeIndex].ToArray();
        }

        public JointIO GetJointData(int nodeIdx)
        {
            Point3d pos = _nodes[nodeIdx].Location;
            Vector3d normal = _nodes[nodeIdx].Normal;
            var indicesEdges = _incidentEdges[nodeIdx].ToArray();
            int nodeValence = indicesEdges.Length;
            int edgeIdx;

            // Data for initialize a joint
            int[] edgesA = new int[] { NONE, NONE };
            int[] edgesB = new int[] { NONE, NONE };
            bool[] isStartA = new bool[] { false, false };
            bool[] isStartB = new bool[] { false, false };
            int numA=0, numB = 0;
            Vector3d vecA, vecB;
            Vector3d tgt0, tgt1;
            switch (nodeValence)
            {
                case 1:
                    edgesA[0] = indicesEdges[0];
                    isStartA[0] = IsStartNode(pos, EdgeCurves[edgesA[0]]);
                    numA = 1;
                    break;

                case 2:
                    edgesA[0] = indicesEdges[0];
                    isStartA[0] = IsStartNode(pos, EdgeCurves[edgesA[0]]);
                    edgesB[0] = indicesEdges[1];
                    isStartB[0] = IsStartNode(pos, EdgeCurves[edgesB[0]]);
                    numA = numB = 1;
                    break;

                case 3:
                    double minCosTheta = double.MaxValue;
                    int[] localIdx = new int[] { NONE, NONE };

                    for (int j = 0; j < nodeValence; j++)
                    {
                        edgeIdx = indicesEdges[j];
                        tgt0 = GetTangentAtPoint(pos, EdgeCurves[edgeIdx]);

                        for (int k = j + 1; k < nodeValence; k++)
                        {
                            int nextEdgeIdx = indicesEdges[k];
                            tgt1 = GetTangentAtPoint(pos, EdgeCurves[nextEdgeIdx]);
                            double cosTheta = tgt0 * tgt1;

                            if (cosTheta < minCosTheta)
                            {
                                // Check if connecting edges (j, k) creates a triangle instead of a quad. This happens if the joints connected by edges j and k have neighborhoods that share more than "vi" in commmon.
                                int adjacentNodeIdx = GetAdjacentNodeOnEdge(nodeIdx, edgeIdx);
                                int nextAdjacentNodeIdx = GetAdjacentNodeOnEdge(nodeIdx, nextEdgeIdx);

                                List<int> nj = new List<int>(), nk = new List<int>();
                                foreach(int idx in _incidentEdges[adjacentNodeIdx]) nj.Add( GetAdjacentNodeOnEdge(adjacentNodeIdx, idx) );
                                foreach(int idx in _incidentEdges[nextAdjacentNodeIdx]) nk.Add( GetAdjacentNodeOnEdge(nextAdjacentNodeIdx, idx) );
                                if (nj.Intersect(nk).Count() > 1) continue; // connecting (j, k) forms a triangle; forbid it.

                                minCosTheta = cosTheta;
                                localIdx[0] = j;
                                localIdx[1] = k;
                            }
                        }
                    }
                    if (localIdx[0] == NONE) throw new Exception("Failed to link up valence 3 vertex (without creating triangles)");

                    // Map local index to global ones
                    edgesA = new int[] { indicesEdges[localIdx[0]], indicesEdges[localIdx[1]] };
                    isStartA = new bool[] { IsStartNode(pos, EdgeCurves[edgesA[0]]), IsStartNode(pos, EdgeCurves[edgesA[1]]) };
                    edgesB[0] = indicesEdges[3 - (localIdx[0] + localIdx[1])];
                    isStartB[0] = IsStartNode(pos, EdgeCurves[edgesB[0]]);
                    numA = 2;
                    numB = 1;
                    break;

                case 4:
                    int[] sortedIdx = GetSortedIncidentEdges(nodeIdx);
                    edgesA = new int[] { sortedIdx[0], sortedIdx[2] };
                    isStartA = new bool[] { IsStartNode(pos, EdgeCurves[edgesA[0]]), IsStartNode(pos, EdgeCurves[edgesA[1]]) };
                    edgesB = new int[] { sortedIdx[1], sortedIdx[3] };
                    isStartB = new bool[] { IsStartNode(pos, EdgeCurves[edgesB[0]]), IsStartNode(pos, EdgeCurves[edgesB[1]]) };
                    numA = numB = 2;
                    break;
                default:
                    throw new Exception("Invalid valence at node with index " + nodeIdx + ". Node valence is " + nodeValence + ". Valence must be 1, 2, 3, or 4");

            }

            /////////////////////////////////////////////////////////////////////////////
            /// Edge vectors
            /////////////////////////////////////////////////////////////////////////////
            // Determine this joint's edge vectors for rod A and rod B. If there's only one segment for a rod A or B at this joint, then the terminal edge of this segment gives this vector (up to sign).
            // If there are two connecting segments, we must construct an averaged vector.
            // In all cases, we construct the edge that points out of segment 0 and into segment 1.
            // This vector is scaled to be the smallest of the two participating edges (to prevent inversions of the adjacent rod edges).
            // Note: this averaging/scaling operation will change the rest length of the neighboring edges, so rod segments' rest lengths will need to be recomputed.
            vecA = -GetTangentAtPoint(pos, EdgeCurves[edgesA[0]]);
            vecB = -GetTangentAtPoint(pos, EdgeCurves[edgesB[0]]);
            double lenA = _edgeLengths[edgesA[0]] / (EdgeSubdivisions[edgesA[0]] - 1);
            double lenB = _edgeLengths[edgesB[0]] / (EdgeSubdivisions[edgesB[0]] - 1);

            if (numA == 2)
            {
                lenA = Math.Min( lenA, _edgeLengths[edgesA[1]] / (EdgeSubdivisions[edgesA[1]] - 1) );
                vecA += GetTangentAtPoint(pos, EdgeCurves[edgesA[1]]);
            }
            if (numB == 2)
            {
                lenB = Math.Min( lenB, _edgeLengths[edgesB[1]] / (EdgeSubdivisions[edgesB[1]] - 1));
                vecB += GetTangentAtPoint(pos, EdgeCurves[edgesB[1]]);
            }
            vecA.Unitize();
            vecB.Unitize();
            vecA *= lenA;
            vecB *= lenB;

            var joint = new JointIO(pos, normal, vecA, vecB, edgesA, edgesB, isStartA, isStartB, numA, numB);
            return joint;
        }
    }
}

