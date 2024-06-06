using System;
using ErodDataLib.Types;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
    public static class ElasticRodModelFactory
    {
        public static ElasticRod BuildElasticRodModel(ElasticRodData data)
        {
            int nodeCount = data.Nodes.Count;
            double[] coords = new double[nodeCount * 3];
            if(data.IsPeriodic) coords = new double[(2+nodeCount) * 3];

            for (int i = 0; i < nodeCount; i++)
            {
                Point3d p = data.Nodes[i].GetPoint(0);
                coords[i * 3] = p.X;
                coords[i * 3 + 1] = p.Y;
                coords[i * 3 + 2] = p.Z;
            }

            if (data.IsPeriodic)
            {
                for (int i = 0; i < 6; i++)
                {
                    coords[nodeCount * 3 + i] = coords[i];
                }
            }

            ElasticRod model = new ElasticRod(coords, data.RemoveRestCurvature, data.IsPeriodic);
            InitElasticRodModel(data, ref model);

            return model;
        }

        private static void InitElasticRodModel(ElasticRodData data, ref ElasticRod model)
        {
            AddMaterialData(data, ref model);
            AddSupportData(data, ref model);
            AddForceData(data, ref model);
            model.InitMesh();
        }

        private static void AddMaterialData(ElasticRodData data, ref ElasticRod model)
        {
            int mCount = data.MaterialData.Count;
            MaterialData mat = data.MaterialData[0];
            model.SetMaterial((int)mat.CrossSectionType, mat.E, mat.PoisonsRatio, mat.Parameters, (int)mat.Orientation);
        }

        private static void AddSupportData(ElasticRodData data, ref ElasticRod model)
        {
            if (data.Supports.Count > 0)
            {
                foreach (SupportData anchor in data.Supports)
                {
                    model.AddSupports(anchor);
                }
            }
        }

        private static void AddForceData(ElasticRodData data, ref ElasticRod model)
        {
            if (data.Forces.Count > 0)
            {
                foreach (UnaryForceData force in data.Forces)
                {
                    model.AddForces(force);
                }
            }
        }
    }
}
