using System;
using ErodModelLib.Creators;
using Rhino.Geometry;
using ErodDataLib.Utils;
using ErodDataLib.Types;
using System.Collections.Generic;
using System.Linq;

namespace ErodModelLib.Types
{
    public partial class RodLinkage : ElasticModel
    {
        protected PointCloud _jointsCloud, _nodesCloud;

        public RodSegment[] Segments { get; private set; }
        public Joint[] Joints { get; private set; }
        public double TargetAngle { get; set; }
        public RodLinkageLayout Layout { get; private set; }
        public MaterialData HomogenousMaterial { get; private set; }
        public Material[] RodMaterials { get; private set; }

        private void AddMaterialData(RodLinkageData data)
        {
            int numMaterials = data.MaterialData.Count;
            int numJoints = data.Joints.Count;
            MaterialData mat = data.MaterialData[0];
            HomogenousMaterial = mat;
            RodMaterials = new Material[numMaterials];

            if (numMaterials == 0) throw new Exception("No material has been assigned to the linkage.");

            if (numMaterials == 1) RodMaterials[0] = new Material(mat);
            else
            {
                RodMaterials = new Material[numJoints];

                // First: Assign the first material to all the joints
                for (int i = 0; i < numJoints; i++) RodMaterials[i] = new Material(mat);

                // Second: Re-assign materials to specific joints
                for (int i = 0; i < numMaterials; i++)
                {
                    mat = data.MaterialData[i];

                    if (mat.Indexes.Length == 1)
                    {
                        int idx = mat.Indexes[0];
                        if (idx == -1) new IndexOutOfRangeException("Material Data without joint reference.");

                        RodMaterials[idx] = new Material(mat);
                    }
                }
            }
            Kernel.Material.ErodMaterialSetToLinkage(RodMaterials.Select(m => m.Model).ToArray(), RodMaterials.Length, Model);
        }

        private void AddSupportData(RodLinkageData data)
        {
            if (data.Supports.Count > 0) foreach (SupportData anchor in data.Supports) AddSupports(anchor);
            else AddCentralSupport();
        }


        private void AddForceData(RodLinkageData data)
        {
            if (data.Forces.Count > 0)
            {
                foreach (UnaryForceData force in data.Forces)
                {
                    AddForces(force);
                }
            }

            if (data.Cables.Count > 0)
            {
                foreach (CableForceData force in data.Cables)
                {
                    AddForces(force);
                }
            }
        }

        protected void InitSegments()
        {
            int count = Kernel.RodLinkage.ErodXShellRodGetSegmentsCount(Model);
            Segments = new RodSegment[count];

            for (int i = 0; i < count; i++)
            {
                Segments[i] = new RodSegment(Model, i);
            }
        }

        protected void InitJoints()
        {
            int count = Kernel.RodLinkage.ErodXShellGetJointsCount(Model);
            Joints = new Joint[count];
            _jointsCloud = new PointCloud();

            double[] pos;
            for (int i = 0; i < count; i++)
            {
                Joints[i] = new Joint(Model, i);
                pos = Joints[i].GetPosition();
                _jointsCloud.Add(new Point3d(pos[0], pos[1], pos[2]));
            }

        }

        protected void InitNodes()
        {
            // Centerline positions
            _nodesCloud = new PointCloud();

            double[] pos = GetCenterLinePositions();
            for (int i = 0; i < pos.Length / 3; i++)
            {
                _nodesCloud.Add(new Point3d(pos[i * 3], pos[i * 3 + 1], pos[i * 3 + 2]));
            }
        }

        public override void InitMesh()
        {
            // Build visualization mesh
            double[] outCoords;
            int[] outQuads;
            GetMeshData(out outCoords, out outQuads);

            MeshVis = Helpers.GetQuadMesh(outCoords, outQuads);
            //if (!MeshVis.IsValid) throw new Exception("Errors during the construction of the xshell. There might be some incompatibilities with the joint normals.");
        }
    }
}
