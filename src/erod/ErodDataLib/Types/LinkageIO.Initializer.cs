using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using ErodDataLib.Utils;
using Rhino.Geometry;

namespace ErodDataLib.Types
{

    public partial class LinkageIO
    {
        private bool[] GenerateIndicationForRodOrientation(List<SegmentIO> segments, List<JointIO> joints)
        {
            bool[] keepOrientation = new bool[segments.Count];
            bool[] visitedSegments = Enumerable.Repeat(false, segments.Count).ToArray();

            // Function to check valid indices
            Func<int, bool> isValidSegmentIndex = delegate (int i) { return i < segments.Count && i >= 0; };

            // Function to trace rods
            Action<bool, int> traceRod = delegate (bool isStartToEnd, int segmentIndex)
            {
                int currentSegmentIndex = segmentIndex;
                int nextJointIndex = isStartToEnd ? (segments[currentSegmentIndex].EndJoint) : (segments[currentSegmentIndex].StartJoint);
                int nextSegmentIndex = joints[nextJointIndex].ContinuationSegment(currentSegmentIndex);

                if (isValidSegmentIndex(nextSegmentIndex))
                {
                    while (isValidSegmentIndex(nextSegmentIndex) && (!visitedSegments[nextSegmentIndex]))
                    {
                        visitedSegments[nextSegmentIndex] = true;
                        int nextStartJoint = segments[nextSegmentIndex].StartJoint;
                        int nextEndJoint = segments[nextSegmentIndex].EndJoint;

                        if ((isStartToEnd ? nextStartJoint : nextEndJoint) == nextJointIndex)
                        {
                            keepOrientation[nextSegmentIndex] = true;
                            nextJointIndex = (isStartToEnd ? nextEndJoint : nextStartJoint);
                        }
                        else
                        {
                            keepOrientation[nextSegmentIndex] = false;
                            nextJointIndex = (isStartToEnd ? nextStartJoint : nextEndJoint);
                        }
                        currentSegmentIndex = nextSegmentIndex;

                        if (nextJointIndex != NONE) nextSegmentIndex = joints[nextJointIndex].ContinuationSegment(nextSegmentIndex);
                        else nextSegmentIndex = NONE;
                    }
                }
            };

            for (int si = 0; si < segments.Count; si++)
            {
                if (visitedSegments[si]) continue;
                visitedSegments[si] = true;
                keepOrientation[si] = true;

                // Trace along the startJoint -> endJoint direction.
                if (segments[si].EndJoint != NONE) traceRod(true, si);

                // Trace along the endJoint -> startJoint direction
                if (segments[si].StartJoint != NONE) traceRod(false, si);
            }

            return keepOrientation;
        }

        private void Init()
        {
            int numNodes = Graph.NumNodes;

            // Generate joints at the valence 2, 3, and 4 vertices.
            FirstJointNodeMap = NONE; // Index of a vertex corresponding to a joint (used to initiate BFS below)
            NodeToJointMaps = Enumerable.Repeat(NONE, numNodes).ToArray();

            // First round of the edge and joint assignment:
            // the result of temp segments and temp joints can have rods in the same ribbon / physical rod with different orientation.
            List<JointIO> tempJoints = new List<JointIO>();
            List<SegmentIO> tempSegments = Segments.GetSegmentsAsList();
            for (int nodeIndex = 0; nodeIndex < numNodes; nodeIndex++)
            {
                Point3d pos = Graph.GetNode(nodeIndex);
                int nodeValence = Graph.GetNodeValence(nodeIndex);
                int[] incidentEdges = Graph.GetIncidentEdges(nodeIndex);

                if (nodeValence == 1) continue; 
                if (nodeValence > 4) throw new Exception("Invalid valence at node with index " + nodeIndex + ". Valence must be 2, 3, or 4");
                if (FirstJointNodeMap == NONE) FirstJointNodeMap = nodeIndex;

                int jointIndex = tempJoints.Count();
                NodeToJointMaps[nodeIndex] = jointIndex;
                tempJoints.Add(Graph.GetJointData(nodeIndex));

                // Link the incident segments to this joint.
                foreach(int segmentIndex in incidentEdges)
                {
                    SegmentIO s = tempSegments[segmentIndex];
                    if (s.IsStartPoint(pos)) s.StartJoint = jointIndex;
                    else s.EndJoint = jointIndex;
                }
            }
            
            // Build indicator for rod orientation and rebuild segments
            Graph.ReverseEdges(GenerateIndicationForRodOrientation(tempSegments, tempJoints));
            Segments = new SegmentIOCollection();
            for (int segmentIndex = 0; segmentIndex < Graph.NumEdges; segmentIndex++)
            {
                var crv = Graph.EdgeCurves[segmentIndex];
                var subd = Graph.EdgeSubdivisions[segmentIndex];
                var segment = new SegmentIO(crv, subd);
                segment.CopyRibbonData(tempSegments[segmentIndex]);
                Segments.Add(segment);
            }

            Joints = new JointIOCollection();
            // Generate joints at the valence 2, 3, and 4 vertices.
            FirstJointNodeMap = NONE; // Index of a vertex corresponding to a joint (used to initiate BFS below)
            for (int nodeIndex = 0; nodeIndex < numNodes; nodeIndex++)
            {
                Point3d pos = Graph.GetNode(nodeIndex);
                int nodeValence = Graph.GetNodeValence(nodeIndex);
                int[] incidentEdges = Graph.GetIncidentEdges(nodeIndex);

                if (nodeValence == 1) continue;
                if (nodeValence > 4) throw new Exception("Invalid valence at node with index " + nodeIndex + ". Valence must be 2, 3, or 4");
                if (FirstJointNodeMap == NONE) FirstJointNodeMap = nodeIndex;

                int jointIndex = Joints.Count();
                NodeToJointMaps[nodeIndex] = jointIndex;
                Joints.Add(Graph.GetJointData(nodeIndex));

                // Link the incident segments to this joint.
                foreach (int segmentIndex in incidentEdges)
                {
                    SegmentIO s = Segments[segmentIndex];
                    s.Indices = Graph.EdgeIndices[segmentIndex];
                    if (s.IsStartPoint(pos)) s.StartJoint = jointIndex;
                    else s.EndJoint = jointIndex;
                }
            }

            if (FirstJointNodeMap == NONE) throw new Exception("There must be at least one joint in the network");

            // Initialize rod segments
            for (int si = 0; si < Segments.Count; si++)
            {
                SegmentIO e = Segments[si];

                Vector3d startVec = default, endVec = default;
                if (e.StartJoint != NONE)
                {
                    var startJoint = Joints[e.StartJoint];
                    startVec = startJoint.SegmentABOffset(si) == 0 ? startJoint.EdgeA * 0.5 : startJoint.EdgeB * 0.5;
                }
                if (e.EndJoint != NONE)
                {
                    var endJoint = Joints[e.EndJoint];
                    endVec = endJoint.SegmentABOffset(si) == 0 ? endJoint.EdgeA * 0.5 : endJoint.EdgeB * 0.5;
                }

                e.BuildCenterLine(startVec, endVec);
            }

            Layout = new RibbonsLayout(Segments);
        }
    }
}
