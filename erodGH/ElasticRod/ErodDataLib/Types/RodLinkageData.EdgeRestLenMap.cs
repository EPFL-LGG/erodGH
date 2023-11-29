using System;
using System.Collections.Generic;
using ErodDataLib.Utils;

namespace ErodDataLib.Types
{
    public partial class RodLinkageData
    {
        internal bool ProcessJoint(int jointIdx, int segmentIdx, int[][] controllersForJoint)
        {
            if (jointIdx == -1) return false;
            var j = Joints[jointIdx];
            int controller = controllersForJoint[jointIdx][j.SegmentABOffset(segmentIdx)];
            if (controller == -1) throw new Exception("ProcessJoint Error");
            if (controller != segmentIdx) return true;
            return false;
        }

        internal void ProcessJointWithInfluence(int segmentIdx, int localJointIndex, int[][] controllersForJoint, int[] numEdgesForSegment, double numFreeIntervals, ref Influence[] infl)
        {
            SegmentData s = Segments[segmentIdx];
            int ne = numEdgesForSegment[segmentIdx];

            int jointIndex = s.JointAt(localJointIndex);
            if (jointIndex == -1) return;
            var j = Joints[jointIndex];
            int controller = controllersForJoint[jointIndex][j.SegmentABOffset(segmentIdx)];

            if (controller == -1) throw new Exception("ProcessJointWithInfluence Error");

            if (controller == segmentIdx)
            {
                infl[0].val -= (0.5 * (1.0 / (ne - 1))) / numFreeIntervals;
                return;
            }

            infl[localJointIndex + 1].idx = controller;
            infl[localJointIndex + 1].val = -(0.5 * (1.0 / (numEdgesForSegment[controller] - 1))) / numFreeIntervals;
        }

        internal class Influence
        {
            public int idx { get; set; }
            public double val { get; set; }

            public Influence()
            {
                idx = -1;
                val = 0;
            }
        }

        // Construct the *transpose* of the map from a vector holding the (rest) lengths
        // of each segment to a vector holding a (rest) length for every rod length in the
        // entire network. The vector output by this map is ordered as follows: all
        // lengths for segments' interior and free edges, followed by two lengths for each joint.
        // (We use build the transpose instead of the map itself to efficiently support
        // the iteration needed to assemble the Hessian chain rule term)
        // This is a fixed linear map for the lifetime of the linkage, though
        // it depends on the initial distribution of segment lengths:
        // to prevent edge "flips" when a long edge meets a short edge at a joint, we
        // use the minimum of the two lengths to define the joint edge length. To
        // prevent the map from being non-differentiable, we decide at linkage
        // construction time which the "short" edge is. (Another solution would be to
        // use a soft minimum, but this would require computing an additional Hessian
        // term). We finally space the remaining length evenly across the unconstrained
        // edges.
        public void ConstructSegmentRestLenToEdgeRestLenMapTranspose()
        {
            int numSegments = Segments.Count;
            int numJoints = Joints.Count;

            double[] segmentRestLenGuess = new double[numSegments];
            for (int i = 0; i < Segments.Count; i++) segmentRestLenGuess[i] = Segments[i].RestLength;

            // Get the initial ideal rest length for the edges of each segment; this is
            // used to decide which segments control which terminal edges.
            double[] idealEdgeLenForSegment = new double[numSegments];
            int[] numEdgesForSegment = new int[numSegments];
            for (int i = 0; i < numSegments; i++)
            {
                numEdgesForSegment[i] = Segments[i].GetEdgeCount();
                idealEdgeLenForSegment[i] = segmentRestLenGuess[i] / (numEdgesForSegment[i] - 1.0);
            }

            // Decide who controls each joint edge: the shorter ideal rest length
            // wins. Ties are broken arbitrarily.
            int[][] controllersForJoint = new int[numJoints][];
            for (int i = 0; i < numJoints; i++)
            {
                var jt = Joints[i];
                int[] c = new int[] { jt.SegmentsA[0], jt.SegmentsB[0] };
                if ((jt.SegmentsA[1] != -1) && (idealEdgeLenForSegment[jt.SegmentsA[1]] < idealEdgeLenForSegment[c[0]])) c[0] = jt.SegmentsA[1];
                if ((jt.SegmentsB[1] != -1) && (idealEdgeLenForSegment[jt.SegmentsB[1]] < idealEdgeLenForSegment[c[1]])) c[1] = jt.SegmentsB[1];

                controllersForJoint[i] = c;
            }

            // Determine the number of nonzeros in the map.
            // Each free edge in a segment segment is potentially influenced by segment
            // lengths in the stencil:
            //      +-----+-----+-----+
            //               ^
            // (The segment always influences its own edges, but neighbors controlling the incident joints influence the edges too).
            int totalFreeEdges = 0, nz = 0;
            // Count the entries in the columns corresponding to segments' internal/free ends
            for (int si = 0; si < numSegments; ++si)
            {
                var s = Segments[si];
                int numFreeEdges = numEdgesForSegment[si] - s.HasStartJoint() - s.HasEndJoint();
                totalFreeEdges += numFreeEdges;

                nz += numFreeEdges; // The segment influences all of its own free edges.
                                    // A controlling neighbor also influences all of the free edges:

                if (ProcessJoint(s.StartJoint, si, controllersForJoint)) nz += numFreeEdges;
                if (ProcessJoint(s.EndJoint, si, controllersForJoint)) nz += numFreeEdges;
            }


            // The two columns for each joint have only a single entry (one controlling segment)
            nz += numJoints * 2;

            int m = numSegments, n = totalFreeEdges + 2 * numJoints;
            EdgeRestLenMapTranspose = new SparseMatrixData(m, n, nz);

            EdgeRestLenMapTranspose.Ap.Add(0); // col 0 begin

            ///////////////
            ///    // Segments are split into (ne - 1) intervals spanning between
            // the incident joint positions (graph nodes); half an interval
            // extends past the joints at each end.
            // Joints control the lengths of the intervals surrounding them,
            // specifying the length of half a subdivision interval on the incident
            // segments. The remaining length of each segment is then
            // distributed evenly across the "free" intervals.
            // First, build the columns for the free edges of each segment:
            for (int si = 0; si < numSegments; si++)
            {
                var s = Segments[si];
                // Determine the influencers for each internal/free edge length on this segment.

                Influence[] infl = new Influence[3];
                for (int j = 0; j < 3; j++) infl[j] = new Influence();
                int ne = numEdgesForSegment[si];
                double numFreeIntervals = (ne - 1) - 0.5 * (s.HasEndJoint() + s.HasStartJoint());
                infl[0].idx = si;
                infl[0].val = 1.0 / numFreeIntervals; // length is distributed evenly across the free intervals

                // The incident joint edges subtract half their length from the amount distributed to the free intervals.
                ProcessJointWithInfluence(si, 0, controllersForJoint, numEdgesForSegment, numFreeIntervals, ref infl);
                ProcessJointWithInfluence(si, 1, controllersForJoint, numEdgesForSegment, numFreeIntervals, ref infl);

                Array.Sort(infl, (i1, i2) => i1.idx.CompareTo(i2.idx));

                // Visit each internal/free edge:
                for (int ei = s.HasStartJoint(); ei < (s.HasEndJoint() == 1 ? ne - 1 : ne); ei++)
                {
                    // Add entries for each present influencer.
                    for (int i = 0; i < 3; ++i)
                    {
                        if (infl[i].idx == -1) continue;
                        EdgeRestLenMapTranspose.Ai.Add(infl[i].idx);
                        EdgeRestLenMapTranspose.Ax.Add(infl[i].val);
                    }
                    EdgeRestLenMapTranspose.Ap.Add(EdgeRestLenMapTranspose.Ai.Count); // col end
                }
            }

            // Build the columns for the joint edges
            for (int ji = 0; ji < numJoints; ++ji)
            {
                for (int ab = 0; ab < 2; ++ab)
                {
                    int c = controllersForJoint[ji][ab];
                    EdgeRestLenMapTranspose.Ai.Add(c);
                    EdgeRestLenMapTranspose.Ax.Add(1.0 / (numEdgesForSegment[c] - 1));
                    EdgeRestLenMapTranspose.Ap.Add(EdgeRestLenMapTranspose.Ai.Count); // col end
                }
            }

            if(EdgeRestLenMapTranspose.Ax.Count != nz) throw new Exception("Invalid fill of the value array (Ax)");
            if(EdgeRestLenMapTranspose.Ai.Count != nz) throw new Exception("Invalid fill of the column pointer and row index array (Ai)");
            if(EdgeRestLenMapTranspose.Ap.Count != n + 1) throw new Exception("Invalid fill of the column pointer and row index array (Ap)");
        }
    }
}
