using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class SupportData : ElementData
    {
        public int[] Indexes { get; set; }
        public int[] LockedDOF { get; set; }
        public bool IsTemporary { get; set; }

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

            IsTemporary = (bool) data["IsTemporary"];
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
        }

        public override string ToString()
        {
            return "SupportData";
        }
    }
}
