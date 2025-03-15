using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public struct SupportIO
    {
        public Point3d VisualizationPosition { get; set; }
        public Point3d ReferencePosition { get; private set; }
        public Point3d TargetPosition { get; private set; }
        public int IndexMap { get; set; }
        public int[] IndicesDoFs { get; private set; }
        public bool[] LockedDoFs { get; private set; }
        public bool IsTemporary { get; set; }
        public bool IsJointSupport { get; set; }
        public bool ContainsTarget { get; private set; }
        public double ReleaseCoefficient { get; private set; }

    public SupportIO(JToken data)
        {
            // Reference position
            var token = data["ReferencePosition"];
            ReferencePosition = new Point3d( (double)token[0], (double)token[1], (double)token[2] );
            VisualizationPosition = new Point3d((double)token[0], (double)token[1], (double)token[2]);

            // Indexes
            IndexMap = (int) data["IndexMap"];

            // Locked DOF
            token = data["LockedDoFs"];
            int count = token.Count();
            LockedDoFs = new bool[count];
            for (int i = 0; i < count; i++) LockedDoFs[i] = (bool)token[i];

            // Indices DOF
            token = data["IndicesDoFs"];
            count = token.Count();
            IndicesDoFs = new int[count];
            for (int i = 0; i < count; i++) IndicesDoFs[i] = (int)token[i];

            // Temporary
            IsTemporary = (bool) data["IsTemporary"];

            // JointSupport
            IsJointSupport = (bool)data["IsJointSupport"];

            // ContainsTarget
            ContainsTarget = (bool)data["ContainsTarget"];

            // Target position
            if (ContainsTarget)
            {
                token = data["TargetPosition"];
                TargetPosition = new Point3d((double)token[0], (double)token[1], (double)token[2]);
            }
            else TargetPosition = Point3d.Unset;

            // Release coefficient 
            ReleaseCoefficient = (double)data["ReleaseCoefficient"];
        }

        public SupportIO(Point3d p, Point3d target=default)
        {
            ReferencePosition = p;
            IndexMap = -1;
            IndicesDoFs = new int[] { -1, -1, -1 };
            LockedDoFs = new bool[] { true, true, true };
            IsTemporary = false;
            TargetPosition = target == default ? Point3d.Unset : target;
            ContainsTarget = TargetPosition == Point3d.Unset ? false : true;
            IsJointSupport = false;
            VisualizationPosition = p;
            ReleaseCoefficient = 0.0;
        }

        public void SetTemporarySupport(double releaseCoefficient)
        {
            IsTemporary = true;
            if (releaseCoefficient < 0) ReleaseCoefficient = 0;
            else if (releaseCoefficient > 1) ReleaseCoefficient = 1.0; 
            else ReleaseCoefficient = releaseCoefficient;
        }

        public void UpdateReferencePosition(Point3d pos)
        {
            ReferencePosition = pos;
            VisualizationPosition = pos;
        }

        public void SetIndicesDoFs(int[] indices)
        {
            if (indices.Length != 3) throw new Exception("Invalid number of indices. The number of indices should be 3 for translation");
            IndicesDoFs = indices;
        }

        public void SetIndexDoF(int index, int indexDoF)
        {
            IndicesDoFs[index] = indexDoF;
        }

        public void FixTranslation(bool flag)
        {
            LockedDoFs[0] = flag;
            LockedDoFs[1] = flag;
            LockedDoFs[2] = flag;
        }

        public void FixTranslation(bool[] flags)
        {
            for (int i = 0; i < LockedDoFs.Length; i++) LockedDoFs[i] = flags[i];
        }

        public void FixTranslationAlongX(bool flag) => LockedDoFs[0] = flag;
        public void FixTranslationAlongY(bool flag) => LockedDoFs[1] = flag;
        public void FixTranslationAlongZ(bool flag) => LockedDoFs[2] = flag;

        public override string ToString()
        {
            return IsTemporary ? "TemporarySupport" : "Support";
        }
    }
}
