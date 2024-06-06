using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ErodDataLib.Utils;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public enum InterleavingType { xshell = 0, weaving = 1, noOffset = 2, triaxialWeave = 3 };
    public enum CrossSectionType { rectangle = 0, ellipse = 1, I = 2, L = 3, cross = 4, custom=5 };
    public enum StiffAxis { tangent = 0, normal = 1 };

    public partial class RodLinkageData
    {
        internal double Tolerance = 0.01;
        internal readonly int NONE = -1;

        private int firstJointVtx, splineBeamIdx = -1;
        private int[][] collect_segmentsA, collect_segmentsB;
        private int[] jointForVertex, collect_numA, collect_numB;
        private List<JointData> tempJoints;
        protected PointCloud Vertices { get; private set; }
        protected Dictionary<int, HashSet<int>> incidentEdges { get; private set; }
        internal bool ContainsJointData { get; private set; }

        public bool ContainsStraightSegments { get; private set; }
        public RodLinkageLayout Layout { get; protected set; }
        public List<JointData> Joints { get; set; }
        public List<SegmentData> Segments { get; set; }
        public List<SupportData> Supports { get; set; }
        public List<UnaryForceData> Forces { get; set; }
        public List<CableForceData> Cables { get; set; }
        public List<MaterialData> MaterialData { get; set; }
        public int Interleaving { get; set; }
        public SparseMatrixData EdgeRestLenMapTranspose { get; protected set; }
        public bool ByPassTriasCheck { get; set; }
        public TargetSurfaceData TargetSurface { get; set; }
        public OptimizationOptions OptimizationSettings { get; set; }

        public void Init(IEnumerable<SegmentData> edges, IEnumerable<NormalData> normals = default)
        {
            ContainsStraightSegments = false;
            for (int i = 0; i < edges.Count(); i++) AddEdge(edges.ElementAt(i));

            InitNormals(normals);
            InitLabeling();
            InitJoints();
            ConstructSegmentRestLenToEdgeRestLenMapTranspose();
            ContainsJointData = true;
            OptimizationSettings = new OptimizationOptions(false, 1,1, OptimizationOptions.OptimizationStages.OneStep);
        }

        public int[] GetJointForVertexMaps()
        {
            return jointForVertex;
        }

        public int GetFirstJointVertex()
        {
            return firstJointVtx;
        }

        private int GetNeighbor(int v, int localEdgeIdx)
        {
            SegmentData e = Segments[incidentEdges[v].ElementAt(localEdgeIdx)];
            if (e.Indexes[0] == v) return e.Indexes[1];
            if (e.Indexes[1] == v) return e.Indexes[0];
            throw new Exception("Edge is not incident v!");
        }

        private double Angle(Vector3d axis, Vector3d v1, Vector3d v2)
        {
            double sinAngle = Vector3d.CrossProduct(v1, v2) * axis;
            double s = Math.Max(-1.0, Math.Min(1.0, sinAngle));
            double c = Math.Max(-1.0, Math.Min(1.0, v1 * v2));
            return Math.Atan2(s, c);
        }

        private int[] SortPermutation(double[] values)
        {
            var p = values.Select((x, i) => new KeyValuePair<double, int>(x, i))
                    .OrderBy(x => x.Key)
                    .ToArray();

            return p.Select(x => x.Value).ToArray();
        }

        private void InitNormals(IEnumerable<NormalData> normals = default)
        {
            int nv = Vertices.Count;

            // Step 1
            // If the given input of normals is not equal to the number of vertices,
            // then initialize all normals based on the cross-prudct of edges (This secures the initialization of all normals).
            //if (normals == default || normals.Count() != nv)
            //{
            //    // Compute a joint normals.
            //    // TODO: Check construction of normals 
            //    for (int vi = 0; vi < nv; vi++)
            //    {
            //        int jointValence = incidentEdges[vi].Count;

            //        Vector3d n = new Vector3d();

            //        if (jointValence >= 2 && jointValence <= 4)
            //        {
            //            for (int k = 0; k < jointValence; k++)
            //            {
            //                int next = k + 1;
            //                if (k == jointValence - 1) next = 0;

            //                Vector3d v0 = Segments[incidentEdges[vi].ElementAt(k)].GetStartVector();
            //                if (Segments[incidentEdges[vi].ElementAt(k)].Indexes[0] != vi) v0 = Segments[incidentEdges[vi].ElementAt(k)].GetEndVector();

            //                Vector3d v1 = Segments[incidentEdges[vi].ElementAt(next)].GetStartVector();
            //                if (Segments[incidentEdges[vi].ElementAt(next)].Indexes[0] != vi) v1 = Segments[incidentEdges[vi].ElementAt(next)].GetEndVector();
            //                v0.Unitize();
            //                v1.Unitize();

            //                Vector3d cross = Vector3d.CrossProduct(v0, v1);
            //                int sign = n * cross >= 0 ? 1 : -1;

            //                n += cross * sign;
            //            }
            //            n.Unitize();
            //        }

            //        Vertices[vi].Normal = n;
            //    }
            //}

            // Step 2
            // Overwrite normals for given inputs
            for (int i = 0; i < normals.Count(); i++) AddNormal(normals.ElementAt(i));
        }

       
        private Vector3d ComputeNormalFromEdgeVectors(int idx)
        {
            int jointValence = incidentEdges[idx].Count;

            Vector3d n = new Vector3d();

            // Compute the normal of the plane that best fits the edge vectors 
            if (jointValence >= 2 && jointValence <= 4)
            {
                for (int k = 0; k < jointValence; k++)
                {
                    int next = k + 1;
                    if (k == jointValence - 1) next = 0;

                    Vector3d v0 = Segments[incidentEdges[idx].ElementAt(k)].GetStartVector();
                    if (Segments[incidentEdges[idx].ElementAt(k)].Indexes[0] != idx) v0 = Segments[incidentEdges[idx].ElementAt(k)].GetEndVector();

                    Vector3d v1 = Segments[incidentEdges[idx].ElementAt(next)].GetStartVector();
                    if (Segments[incidentEdges[idx].ElementAt(next)].Indexes[0] != idx) v1 = Segments[incidentEdges[idx].ElementAt(next)].GetEndVector();
                    v0.Unitize();
                    v1.Unitize();

                    Vector3d cross = Vector3d.CrossProduct(v0, v1);
                    int sign = n * cross >= 0 ? 1 : -1;

                    n += cross * sign;
                }
                n.Unitize();
                return n;
            }
            else throw new Exception("Invalid joint valence when computing the normal of the plane that best fits the edge vectors");
        }

        private bool[] GenerateIndicationForRodOrientation(List<SegmentData> temp_segments, List<JointData> temp_joints)
        {
            bool[] m_rod_orientation_indicator = new bool[temp_segments.Count];
            Func<int, bool> is_valid_segment_index = delegate (int i) { return i < temp_segments.Count && i >= 0; };

            bool[] visited_segment = Enumerable.Repeat(false, temp_segments.Count).ToArray();

            Action<bool, int> trace_rod = delegate (bool start_to_end, int si)
            {
                int curr_index = si;
                int next_joint_index = start_to_end ? (temp_segments[curr_index].EndJoint) : (temp_segments[curr_index].StartJoint);
                int next_index = temp_joints[next_joint_index].ContinuationSegment(curr_index);
                if (is_valid_segment_index(next_index))
                {
                    while (is_valid_segment_index(next_index) && (!visited_segment[next_index]))
                    {
                        visited_segment[next_index] = true;
                        int next_start_joint = temp_segments[next_index].StartJoint;
                        int next_end_joint = temp_segments[next_index].EndJoint;
                        if ((start_to_end ? next_start_joint : next_end_joint) == next_joint_index)
                        {
                            m_rod_orientation_indicator[next_index] = true;
                            next_joint_index = (start_to_end ? next_end_joint : next_start_joint);
                        }
                        else
                        {
                            m_rod_orientation_indicator[next_index] = false;
                            next_joint_index = (start_to_end ? next_start_joint : next_end_joint);
                        }
                        curr_index = next_index;
                        if (next_joint_index != NONE)
                            next_index = temp_joints[next_joint_index].ContinuationSegment(next_index);
                        else
                            next_index = NONE;
                    }
                }
            };

            for (int si = 0; si < temp_segments.Count; si++)
            {
                if (visited_segment[si]) continue;
                visited_segment[si] = true;
                m_rod_orientation_indicator[si] = true;
                // Trace along the startJoint -> endJoint direction.
                if (temp_segments[si].EndJoint != NONE)
                {
                    trace_rod(true, si);
                }
                // Trace along the endJoint -> startJoint direction
                if (temp_segments[si].StartJoint != NONE)
                {
                    trace_rod(false, si);
                }
            }

            return m_rod_orientation_indicator;
        }

        /// <summary>
        /// First round of the edge and joint assignment:
        /// the result of temp segments and temp joints can have rods in the same ribbon / physical rod with different orientation.
        /// </summary>
        private void InitLabeling()
        {
            tempJoints = new List<JointData>();

            int nv = Vertices.Count;

            // Generate joints at the valence 2, 3, and 4 vertices.
            firstJointVtx = NONE; // Index of a vertex corresponding to a joint (used to initiate BFS below)
            jointForVertex = Enumerable.Repeat(NONE, nv).ToArray();
            collect_segmentsA = Enumerable.Repeat(new int[] { NONE, NONE }, nv).ToArray();
            collect_segmentsB = Enumerable.Repeat(new int[] { NONE, NONE }, nv).ToArray();
            collect_numA = new int[nv];
            collect_numB = new int[nv];

            for (int vi = 0; vi < nv; vi++)
            {
                int jointValence = incidentEdges[vi].Count;
                if (jointValence == 1) continue; // free end; no joint
                if (jointValence > 4) throw new Exception("Invalid vertex valence " + jointValence + "; must be 1, 2, 3, or 4");

                // Valence 2, 3, or 4:
                if (firstJointVtx == NONE) firstJointVtx = vi;
                // Group the incident edges into pairs that connect to form
                // mostly-straight rods
                // Do this by considering the *outward-pointing* edge vectors:
                Vector3d[] edgeVecs = new Vector3d[4]; // unit edge vectors as columns
                double[] edgeVecLens = new double[4];
                bool[] isStartPt = new bool[4];

                for (int k = 0; k < jointValence; k++)
                {
                    SegmentData e = Segments[incidentEdges[vi].ElementAt(k)];

                    edgeVecs[k] = Vertices[e.Indexes[1]].Location - Vertices[e.Indexes[0]].Location;
                    edgeVecLens[k] = edgeVecs[k].Length;
                    edgeVecs[k].Unitize();

                    isStartPt[k] = (e.Indexes[0] == vi);
                    if (isStartPt[k]) continue;
                    if (e.Indexes[1] == vi) edgeVecs[k] *= -1.0;
                }

                // Partition the segments into those forming "Rod A" and those forming "Rod B"
                int[] segmentsA = new int[] { NONE, NONE };
                int[] segmentsB = new int[] { NONE, NONE };
                int numA = 0, numB = 0;
                bool[] isStartA = new bool[] { false, false };
                bool[] isStartB = new bool[] { false, false };

                if (jointValence == 2)
                {
                    // There are no continuation edges if the valence is 2; one segment belongs to "Rod A" and the other to "Rod B"
                    // No terminal edge averaging needs to be done.
                    numA = numB = 1;
                    segmentsA[0] = 0;
                    segmentsB[0] = 1;
                }
                if (jointValence == 3)
                {
                    // Determine which two of the 3 incident edges best connect to form Rod A
                    // (Try to pick the two that connect the straightest, but verify that this
                    //  preserves a quad topology; if not, the straightest valid connection must be made.)
                    double minCosTheta = double.MaxValue;
                    for (int j = 0; j < jointValence; j++)
                    {
                        for (int k = j + 1; k < jointValence; k++)
                        {
                            double cosTheta = edgeVecs[j] * edgeVecs[k];
                            if (cosTheta < minCosTheta)
                            {
                                // Check if connecting edges (j, k) creates a triangle
                                // instead of a quad. This happens if the joints
                                // connected by edges j and k have neighborhoods that
                                // share more than "vi" in commmon.
                                int vj = GetNeighbor(vi, j);
                                int vk = GetNeighbor(vi, k);

                                List<int> nj = new List<int>();
                                List<int> nk = new List<int>();
                                for (int i = 0; i < incidentEdges[vj].Count; i++) nj.Add(GetNeighbor(vj, i));
                                for (int i = 0; i < incidentEdges[vk].Count; i++) nk.Add(GetNeighbor(vk, i));

                                nj.Sort();
                                nk.Sort();
                                IEnumerable<int> nboth = nj.AsQueryable().Intersect(nk);

                                if (!ByPassTriasCheck && nboth.Count() > 1) continue; // connecting (j, k) forms a triangle; forbid it.

                                minCosTheta = cosTheta;
                                segmentsA[0] = j;
                                segmentsA[1] = k;
                            }
                        }
                    }
                    if (segmentsA[0] == NONE) throw new Exception("Failed to link up valence 3 vertex (without creating triangles)");
                    numA = 2; numB = 1;
                    segmentsB[0] = 3 - (segmentsA[0] + segmentsA[1]); // all indices add up to 3; complement by subtraction
                }
                if (jointValence == 4)
                {
                    // Order the edges clockwise around the joint normal and assign them alternating rod labels A, B, A, B.
                    // Compute the angles between edge 0 and every other edge.
                    Vector3d normal = ComputeNormalFromEdgeVectors(vi);// Vertices[vi].Normal;
                    double[] angles = new double[3];
                    Vector3d v0 = edgeVecs[0] - normal * (normal * edgeVecs[0]);
                    for (int k = 1; k < jointValence; k++)
                    {
                        Vector3d vk = edgeVecs[k] - normal * (normal * edgeVecs[k]);
                        double theta = Angle(normal, v0, vk); // angle in [-pi, pi]

                        if (theta < 0) theta += 2.0 * Math.PI; // compute angle in [0, 2 pi]
                        angles[k - 1] = theta; ;
                    }

                    // Sort vectors 1, 2, 3  clockwise (ascending angle wrt vector 0), assign alternating labels
                    int[] p = SortPermutation(angles); // sorted list: [angles[p[0]], angles[p[1]], angles[p[2]]]

                    segmentsA = new int[] { 0, 1 + p[1] };
                    segmentsB = new int[] { 1 + p[0], 1 + p[2] };

                    numA = numB = 2;
                }

                collect_segmentsA[vi] = new int[2];
                Array.Copy(segmentsA, collect_segmentsA[vi], 2);
                collect_segmentsB[vi] = new int[2];
                Array.Copy(segmentsB, collect_segmentsB[vi], 2);
                collect_numA[vi] = numA;
                collect_numB[vi] = numB;

                for (int k = 0; k < numA; k++) isStartA[k] = isStartPt[segmentsA[k]];
                for (int k = 0; k < numB; k++) isStartB[k] = isStartPt[segmentsB[k]];

                // Determine this joint's edge vectors for rod A and rod B. If there's only one
                // segment for a rod A or B at this joint, then the terminal edge of this segment gives this
                // vector (up to sign). If there are two connecting segments, we must construct an averaged vector.
                // In all cases, we construct the edge that points out of segment 0 and into segment 1.
                // This vector is scaled to be the smallest of the two participating edges (to prevent inversions of the adjacent rod edges).
                // Note: this averaging/scaling operation will change the rest
                // length of the neighboring edges, so rod segments' rest lengths
                // will need to be recomputed.
                Vector3d edgeA = new Vector3d(edgeVecs[segmentsA[0]]);
                edgeA.Reverse();
                Vector3d edgeB = new Vector3d(edgeVecs[segmentsB[0]]); // get vector pointing out of segment 0
                edgeB.Reverse();
                double segmentFracLenA = 1.0 / (Segments[incidentEdges[vi].ElementAt(segmentsA[0])].Subdivision - 1); // only (subdivision - 1) segment lengths fit between the endpoints; rod extends half a segment past each endpoint.
                double segmentFracLenB = 1.0 / (Segments[incidentEdges[vi].ElementAt(segmentsB[0])].Subdivision - 1);
                double lenA = edgeVecLens[segmentsA[0]] * segmentFracLenA;
                double lenB = edgeVecLens[segmentsB[0]] * segmentFracLenB;
                if (numA == 2)
                {
                    segmentFracLenA = 1.0 / (Segments[incidentEdges[vi].ElementAt(segmentsA[1])].Subdivision - 1);
                    lenA = Math.Min(lenA, edgeVecLens[segmentsA[1]] * segmentFracLenA);
                    edgeA += edgeVecs[segmentsA[1]];
                }
                if (numB == 2)
                {
                    segmentFracLenB = 1.0 / (Segments[incidentEdges[vi].ElementAt(segmentsB[1])].Subdivision - 1);
                    lenB = Math.Min(lenB, edgeVecLens[segmentsB[1]] * segmentFracLenB);
                    edgeB += edgeVecs[segmentsB[1]];
                }
                edgeA *= lenA / edgeA.Length;
                edgeB *= lenB / edgeB.Length;

                // Convert to global segment indices
                for (int k = 0; k < numA; k++) segmentsA[k] = incidentEdges[vi].ElementAt(segmentsA[k]);
                for (int k = 0; k < numB; k++) segmentsB[k] = incidentEdges[vi].ElementAt(segmentsB[k]);

                int ji = tempJoints.Count();
                jointForVertex[vi] = ji;

                tempJoints.Add(new JointData(Vertices[vi].Location, Vertices[vi].Normal, edgeA, edgeB, segmentsA, segmentsB, isStartA, isStartB, numA, numB));

                // Link the incident segments to this joint.
                for (int k = 0; k < jointValence; k++)
                {
                    int idx = incidentEdges[vi].ElementAt(k);
                    SegmentData s = Segments[idx];
                    if (isStartPt[k]) s.StartJoint = ji;
                    else s.EndJoint = ji;

                    Segments[idx] = s;
                }
            }
        }

        /// <summary>
        /// Second round of segment and joint generation.
        /// With the orientation indication vector, we can flip rod when necessary to ensure the consistency of their orientation.
        /// </summary>
        private void InitJoints()
        {
            bool[] orientationIndicator = GenerateIndicationForRodOrientation(Segments, tempJoints);

            Joints = new List<JointData>();
            int nv = Vertices.Count;
            int ne = Segments.Count;

            // Unlink the temp incident segments to joints, so the temp segment start and end joint are None.
            for (int ei = 0; ei < ne; ei++)
            {
                SegmentData s = Segments[ei];
                s.StartJoint = NONE;
                s.EndJoint = NONE;
                Segments[ei] = s;
            }

            // Generate joints at the valence 2, 3, and 4 vertices.
            firstJointVtx = NONE; // Index of a vertex corresponding to a joint (used to initiate BFS below)
            for (int vi = 0; vi < nv; vi++)
            {
                int jointValence = incidentEdges[vi].Count;
                if (jointValence == 1) continue; // free end; no joint
                if (jointValence > 4) throw new Exception("Invalid vertex valence " + jointValence + "; must be 1, 2, 3, or 4");

                // Valence 2, 3, or 4:
                if (firstJointVtx == NONE) firstJointVtx = vi;
                // Group the incident edges into pairs that connect to form
                // mostly-straight rods
                // Do this by considering the *outward-pointing* edge vectors:
                Vector3d[] edgeVecs = new Vector3d[4]; // unit edge vectors as columns
                double[] edgeVecLens = new double[4];
                bool[] isStartPt = new bool[4];
                for (int k = 0; k < jointValence; k++)
                {
                    int eIdx = incidentEdges[vi].ElementAt(k);
                    SegmentData e = Segments[eIdx];
                    bool keepOrientation = orientationIndicator[eIdx];
                    // Use the fact that int(true) == 1 to determine edge orientation. 
                    isStartPt[k] = (keepOrientation) ? (e.Indexes[0] == vi) : (e.Indexes[1] == vi);

                    Curve localCopy = e.GetUnderlyingCurve().DuplicateCurve();
                    if (!keepOrientation) localCopy.Reverse();

                    // Estimate the tangent direction at the joint and use that as edge vectors.
                    edgeVecs[k] = isStartPt[k] ? e.GetStartVector(localCopy) : e.GetEndVector(localCopy);

                    edgeVecs[k] *= e.RestLength / edgeVecs[k].Length;
                    edgeVecLens[k] = edgeVecs[k].Length;
                }

                // Partition the segments into those forming "Rod A" and those forming "Rod B"
                int[] segmentsA = collect_segmentsA[vi];
                int[] segmentsB = collect_segmentsB[vi];
                int numA = collect_numA[vi];
                int numB = collect_numB[vi];
                bool[] isStartA = new bool[] { false, false };
                bool[] isStartB = new bool[] { false, false };

                for (int k = 0; k < numA; k++) isStartA[k] = isStartPt[segmentsA[k]];
                for (int k = 0; k < numB; k++) isStartB[k] = isStartPt[segmentsB[k]];

                // Determine this joint's edge vectors for rod A and rod B. If there's only one
                // segment for a rod A or B at this joint, then the terminal edge of this segment gives this
                // vector (up to sign). If there are two connecting segments, we must construct an averaged vector.
                // In all cases, we construct the edge that points out of segment 0 and into segment 1.
                // This vector is scaled to be the smallest of the two participating edges (to prevent inversions of the adjacent rod edges).
                // Note: this averaging/scaling operation will change the rest
                // length of the neighboring edges, so rod segments' rest lengths
                // will need to be recomputed.
                Vector3d edgeA = new Vector3d(edgeVecs[segmentsA[0]]);
                edgeA.Reverse();
                Vector3d edgeB = new Vector3d(edgeVecs[segmentsB[0]]); // get vector pointing out of segment 0
                edgeB.Reverse();
                int subdivisionA = Segments[incidentEdges[vi].ElementAt(segmentsA[0])].Subdivision;
                int subdivisionB = Segments[incidentEdges[vi].ElementAt(segmentsB[0])].Subdivision;
                double segmentFracLenA = 1.0 / (subdivisionA - 1); // only (subdivision - 1) segment lengths fit between the endpoints; rod extends half a segment past each endpoint.
                double segmentFracLenB = 1.0 / (subdivisionB - 1);
                double lenA = edgeVecLens[segmentsA[0]] * segmentFracLenA;
                double lenB = edgeVecLens[segmentsB[0]] * segmentFracLenB;

                if (numA == 2)
                {
                    subdivisionA = Segments[incidentEdges[vi].ElementAt(segmentsA[1])].Subdivision;
                    segmentFracLenA = 1.0 / (subdivisionA - 1);
                    lenA = Math.Min(lenA, edgeVecLens[segmentsA[1]] * segmentFracLenA);
                    edgeA += edgeVecs[segmentsA[1]];
                }
                if (numB == 2)
                {
                    subdivisionB = Segments[incidentEdges[vi].ElementAt(segmentsB[1])].Subdivision;
                    segmentFracLenB = 1.0 / (subdivisionB - 1);
                    lenB = Math.Min(lenB, edgeVecLens[segmentsB[1]] * segmentFracLenB);
                    edgeB += edgeVecs[segmentsB[1]];
                }
                edgeA *= lenA / edgeA.Length;
                edgeB *= lenB / edgeB.Length;

                // Convert to global segment indices
                for (int k = 0; k < numA; k++) segmentsA[k] = incidentEdges[vi].ElementAt(segmentsA[k]);
                for (int k = 0; k < numB; k++) segmentsB[k] = incidentEdges[vi].ElementAt(segmentsB[k]);

                int ji = Joints.Count();
                Joints.Add(new JointData(Vertices[vi].Location, Vertices[vi].Normal, edgeA, edgeB, segmentsA, segmentsB, isStartA, isStartB, numA, numB));

                // Link the incident segments to this joint.
                for (int k = 0; k < jointValence; k++)
                {
                    int idx = incidentEdges[vi].ElementAt(k);
                    SegmentData e = Segments[idx];
                    if (isStartPt[k]) e.StartJoint = ji;
                    else e.EndJoint = ji;
                    Segments[idx] = e;
                }
            }

            if (firstJointVtx == NONE) throw new Exception("There must be at least one joint in the network");

            // Initialize rod segments
            for (int si = 0; si < ne; si++)
            {
                SegmentData e = Segments[si];

                double start_len = default;
                double end_len = default;
                if (e.StartJoint != NONE)
                {
                    var startJoint = Joints[e.StartJoint];
                    start_len = startJoint.SegmentABOffset(si) == 0 ? startJoint.EdgeA.Length / 2.0 : startJoint.EdgeB.Length / 2.0;
                }
                if (e.EndJoint != NONE)
                {
                    var endJoint = Joints[e.EndJoint];
                    end_len = endJoint.SegmentABOffset(si) == 0 ? endJoint.EdgeA.Length / 2.0 : endJoint.EdgeB.Length / 2.0;
                }

                e.BuildCenterLine(orientationIndicator[si], start_len, end_len);

                Segments[si] = e;
            }
        }

        // Propagate consistent joint normals throughout the graph using BFS
        // (to prevent 180 degree twists); warn if the graph is disconnected.
        private void PropagateConsistentJointNormals()
        {
            int numVertices = Vertices.Count;
            Queue<int> bfsQueue = new Queue<int>();
            bool[] visited = Enumerable.Repeat(false, numVertices).ToArray();
            visited[firstJointVtx] = true;
            bfsQueue.Enqueue(firstJointVtx);
            int numVisited = 1;
            while (bfsQueue.Count()!=0)
            {
                int u = bfsQueue.Peek();
                int ju = jointForVertex[u];
                if (ju == -1) throw new Exception("Invalid Joint for Vertex");

                bfsQueue.Dequeue();
                for (int k = 0; k<incidentEdges[u].Count; k++)
                {
                    var e = Segments[incidentEdges[u].ElementAt(k)];
                    if((Vertices.ClosestPoint(e.GetPoint(0)) == u) == (Vertices.ClosestPoint(e.GetPoint(1)) == u)) throw new Exception("RodSegment with length.");
                    int v = (Vertices.ClosestPoint(e.GetPoint(0)) == u) ? Vertices.ClosestPoint(e.GetPoint(1)) : Vertices.ClosestPoint(e.GetPoint(0));
                    if (visited[v]) continue;
                    int jv = jointForVertex[v];
                    visited[v] = true;
                    numVisited++;
                    if (jv == NONE) continue; // terminate search at valence 1 vertices
                    // TODO
                    //Joints[jv].MakeNormalConsistent(Joints[ju]);
                    bfsQueue.Enqueue(v);
                }
            }

            if (numVisited != numVertices)
            {
                throw new Exception("Disconnected edge graph. Find connected component with " + numVisited + " vertices, but there are " + numVertices + " vertices in total.");
            }
        }
    }
}
