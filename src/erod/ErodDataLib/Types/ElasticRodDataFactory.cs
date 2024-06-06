using System;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public static class ElasticRodDataFactory
    {
        public static double Tolerance = 0.01;

        public static void AddPoint(Point3d pt, ref ElasticRodData data)
        {
            int idx = data.Cloud.ClosestPoint(pt);
            if (idx == -1)
            {
                data.Cloud.Add(pt);
                data.Nodes.Add(new NodeData(pt));
            }
            else
            {
                if (pt.DistanceTo(data.Cloud[idx].Location) > Tolerance)
                {
                    data.Cloud.Add(pt);
                    data.Nodes.Add(new NodeData(pt));
                }
            }
        }

        public static void AddSupport(SupportData support, ref ElasticRodData data)
        {
            Point3d p = support.GetPoint(0);

            int idx = data.Cloud.ClosestPoint(p);
            if (idx != -1)
            {
                support.Indexes[0] = idx;
                data.Supports.Add(support);
            }
        }

        public static void AddForce(UnaryForceData force, ref ElasticRodData data)
        {
            Point3d p = force.GetPoint(0);

            int idx = data.Cloud.ClosestPoint(p);
            if (idx != -1)
            {
                force.Indices[0] = idx;

                data.Forces.Add(force);
            }
        }

        public static void AddMaterial(MaterialData material, ref ElasticRodData data)
        {
            if (material.GetPointCount() == 1)
            {
                Point3d p = material.GetPoint(0);
                int idx = data.Cloud.ClosestPoint(p);

                if (idx != -1)
                {
                    material.Indexes[0] = idx;
                    data.MaterialData.Add(material);
                }
            }
            else
            {
                data.MaterialData.Add(material);
            }
        }
    }
}
