using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class SupportData : ElementData
    {
        public int[] Indexes { get; private set; }
        public int[] LockedDOF { get; private set; }
        public bool IsTemporary { get; private set; }
        public Point3d TargetPosition { get; set; }

        public SupportData(JToken data) : base(1)
        {
            // Indexes
            var token = data["Indexes"];
            int count = token.Count();
            Indexes = new int [count];
            for (int i = 0; i < count; i++)
            {
                var p = token[i];
                Indexes[i] = (int)token[i];
            }

            // Locked DOF
            token = data["LockedDOF"];
            count = token.Count();
            Indexes = new int[count];
            for (int i = 0; i < count; i++)
            {
                var p = token[i];
                LockedDOF[i] = (int)token[i];
            }

            // Temporary
            IsTemporary = (bool) data["IsTemporary"];

            // Locked DOF
            token = data["TargetPosition"];
            TargetPosition = new Point3d((double)token[0], (double)token[1], (double)token[2]);
        }

        public SupportData(Point3d p, int[] DOF = default, bool temporarySupport = false) : base(p)
        {
            Indexes = new int[1];
            if (DOF == default || DOF.Length > 6)
            {
                LockedDOF = new int[] { 0, 1, 2, 3, 4, 5 };
            }
            else
            {
                LockedDOF = DOF;
            }

            IsTemporary = temporarySupport;
            TargetPosition = Point3d.Unset;
        }

        public override string ToString()
        {
            return "SupportData";
        }
    }
}
