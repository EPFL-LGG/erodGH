using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class SupportIO
    {
        public Point3d ReferencePosition { get; set; }
        public Point3d TargetPosition { get; private set; }
        public int IndexMap { get; set; }
        public int[] IndicesDoFs { get; private set; }
        public bool[] LockedDoFs { get; private set; }
        public bool IsTemporary { get; private set; }
        public bool IsJointSupport { get; set; }
        public bool ContainsTarget { get; private set; }

    public SupportIO(JToken data)
        {
            // Reference position
            var token = data["ReferencePosition"];
            ReferencePosition = new Point3d( (double)token[0], (double)token[1], (double)token[2] );

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
        }

        public SupportIO(Point3d p, bool temporarySupport = false, Point3d target=default)
        {
            ReferencePosition = p;
            IndexMap = -1;
            IndicesDoFs = new int[] { -1, -1, -1, -1, -1, -1 };
            LockedDoFs = new bool[] { true, true, true, true, true, true };
            IsTemporary = temporarySupport;
            TargetPosition = target == default ? Point3d.Unset : target;
            ContainsTarget = TargetPosition == Point3d.Unset ? false : true;
            IsJointSupport = false;
        }

        public void SetIndicesDoFs(int[] indices)
        {
            if (indices.Length != 6) throw new Exception("Invalid number of indices. The number of indices should be 6 (3 for translation and 3 for rotation)");
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

        public void FixRotation(bool flag)
        {
            LockedDoFs[3] = flag;
            LockedDoFs[4] = flag;
            LockedDoFs[5] = flag;
        }

        public void FixTranslationAndRotation(bool[] flags)
        {
            for (int i = 0; i < LockedDoFs.Length; i++) LockedDoFs[i] = flags[i];
        }

        public void FixTranslationAlongX(bool flag) => LockedDoFs[0] = flag;
        public void FixTranslationAlongY(bool flag) => LockedDoFs[1] = flag;
        public void FixTranslationAlongZ(bool flag) => LockedDoFs[2] = flag;
        public void FixRotationAlongX(bool flag) => LockedDoFs[3] = flag;
        public void FixRotationAlongY(bool flag) => LockedDoFs[4] = flag;
        public void FixRotationAlongZ(bool flag) => LockedDoFs[5] = flag;

        public override string ToString()
        {
            return "SupportData";
        }
    }
}
