using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ErodDataLib.Types
{
    public class RodLinkageLayout
    {
        private Dictionary<int, HashSet<int>>[] layout;

        public Dictionary<int, HashSet<int>> SplineBeamsA => layout[0];
        public Dictionary<int, HashSet<int>> SplineBeamsB => layout[1];

        public RodLinkageLayout()
        {
            layout = new Dictionary<int, HashSet<int>>[2];
            layout[0] = new Dictionary<int, HashSet<int>>();
            layout[1] = new Dictionary<int, HashSet<int>>();
        }

        public RodLinkageLayout(JToken data)
        {
            layout = new Dictionary<int, HashSet<int>>[2];
            layout[0] = new Dictionary<int, HashSet<int>>();
            layout[1] = new Dictionary<int, HashSet<int>>();

            var spData = data["SplineBeamsA"].Children();
            for (int i = 0; i < spData.Count(); i++)
            {
                var eData = spData.ElementAt(i).Children();

                for (int j = 0; j < eData.Count(); j++)
                {
                    AddSplineBeamReference(SegmentLabels.RodA, i, (int)eData.ElementAt(0)[j]);
                }
            }

            spData = data["SplineBeamsB"].Children();
            for (int i=0; i< spData.Count(); i++)
            {
                var eData = spData.ElementAt(i).Children();

                for(int j=0; j<eData.Count(); j++)
                {
                    AddSplineBeamReference(SegmentLabels.RodB, i, (int)eData.ElementAt(0)[j]);
                }
            }

        }

        public RodLinkageLayout(RodLinkageLayout layout)
        {
            this.layout = (Dictionary<int, HashSet<int>>[]) layout.layout.Clone();
        }

        public void AddSplineBeamReference(SegmentLabels label, int splineBeamIndex, int edgeBeamIndex)
        {
            if (label == SegmentLabels.RodA)
            {
                if (!layout[0].ContainsKey(splineBeamIndex))
                {
                    layout[0].Add(splineBeamIndex, new HashSet<int>());
                }

                layout[0][splineBeamIndex].Add(edgeBeamIndex);
            }

            if (label == SegmentLabels.RodB)
            {
                if (!layout[1].ContainsKey(splineBeamIndex))
                {
                    layout[1].Add(splineBeamIndex, new HashSet<int>());
                }

                layout[1][splineBeamIndex].Add(edgeBeamIndex);
            }
        }

        public bool ContainsLayoutData()
        {
            return layout[0].Count > 0 && SplineBeamsB.Count > 0 ? true : false;
        }
    }
}
