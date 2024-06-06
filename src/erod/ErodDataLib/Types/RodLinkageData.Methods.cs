using System.Collections.Generic;
using Rhino.Geometry;
using Newtonsoft.Json;
using System.IO;

namespace ErodDataLib.Types
{
    public partial class RodLinkageData
    {
        public Point3d[] GetVertices()
        {
            return Vertices.GetPoints();
        }

        public void AddCentralSupport()
        {
            BoundingBox bb = new BoundingBox(Vertices.GetPoints());
            Point3d p = bb.PointAt(0.5, 0.5, 0);

            SupportData support = new SupportData(p, new int[] { 0,1,2 });

            AddSupport(support);
        }

        public Dictionary<int, HashSet<int>> GetIncidentEdges()
        {
            return incidentEdges;
        }

        public int AddEdge(SegmentData edge)
        {
            int edgeIndex = Segments.Count;
            if (edge.IsCurvedEdge) ContainsStraightSegments = false;

            Point3d[] pE = new Point3d[2];
            int vertexIdx;
            for (int i = 0; i < 2; i++)
            {
                pE[i] = edge.GetPoint(i);

                vertexIdx = Vertices.ClosestPoint(pE[i]);
                if (vertexIdx == -1)
                {
                    edge.Indexes[i] = AddVertex(pE[i]);
                }
                else
                {
                    Point3d pp = Vertices[vertexIdx].Location;
                    if (pE[i].DistanceTo(pp) > Tolerance)
                    {
                        edge.Indexes[i] = AddVertex(pE[i]);
                    }
                    else
                    {
                        edge.Indexes[i] = vertexIdx;
                    }
                }

                if (!incidentEdges.ContainsKey(edge.Indexes[i])) incidentEdges.Add(edge.Indexes[i], new HashSet<int>());
                incidentEdges[edge.Indexes[i]].Add(edgeIndex);
            }

            // Check if the edge beam contains spline-beam information
            if (edge.EdgeBeamLocalIndex != -1)
            {
                // Create a new spline beam index when a starting edge is found (edge-beams needs to be sorted) 
                if (edge.EdgeBeamLocalIndex == 0) splineBeamIdx++;
                edge.SplineBeamGlobalIndex = splineBeamIdx;
                Layout.AddSplineBeamReference(edge.EdgeLabel, splineBeamIdx, edgeIndex);
            }


            Segments.Add(edge);

            return edgeIndex;
        }

        public void AddSupport(SupportData support)
        {
            Point3d p = support.GetPoint(0);

            int idx = Vertices.ClosestPoint(p);
            if (idx != -1)
            {
                support.Indexes[0] = idx;
                Supports.Add(support);
            }
        }

        public void AddForce(UnaryForceData force)
        {
            Point3d p = force.GetPoint(0);

            int idx = Vertices.ClosestPoint(p);
            if (idx != -1)
            {
                force.Indices[0] = idx;

                Forces.Add(force);
            }
        }

        public void AddCable(CableForceData force)
        {
            Point3d p0 = force.GetPoint(0);
            int idx0 = Vertices.ClosestPoint(p0);
            if (idx0 != -1) force.Indices[0] = idx0;

            Point3d p1 = force.GetPoint(1);
            int idx1 = Vertices.ClosestPoint(p1);
            if (idx1 != -1) force.Indices[1] = idx1;

            Cables.Add(force);
        }

        public void AddNormal(NormalData normal)
        {
            Point3d p = normal.GetPoint(0);
            if (p != Point3d.Unset)
            {
                int idx = Vertices.ClosestPoint(p);
                normal.Indexes[0] = idx;
                Vertices[idx].Normal = normal.Vector;
            }
            else
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i].Normal = normal.Vector;
                }
            }
        }

        public void AddMaterial(MaterialData material)
        {
            if (material.GetPointCount() == 1)
            {
                Point3d p = material.GetPoint(0);
                int idx = Vertices.ClosestPoint(p);

                if (idx != -1)
                {
                    material.Indexes[0] = idx;
                    MaterialData.Add(material);
                }
            }
            else
            {
                MaterialData.Add(material);
            }
        }

        private int AddVertex(Point3d p)
        {
            Vertices.Add(p, new Vector3d(0,0,1));
            Joints.Add(new JointData(p));
            return Vertices.Count - 1;
        }

        public void WriteJsonFile(string path, string filename)
        {
            // Serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(@path + filename + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }
    }
}
