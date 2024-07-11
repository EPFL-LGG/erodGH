using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ErodDataLib.Types
{
    public class RibbonsLayout : ICloneable
    {
        private Dictionary<int, HashSet<int>>[] _layout;

        public Dictionary<int, HashSet<int>> RibbonsFamilyA => _layout[0];
        public Dictionary<int, HashSet<int>> RibbonsFamilyB => _layout[1];

        public RibbonsLayout()
        {
            _layout = new Dictionary<int, HashSet<int>>[2];
            _layout[0] = new Dictionary<int, HashSet<int>>();
            _layout[1] = new Dictionary<int, HashSet<int>>();
        }

        public RibbonsLayout(RibbonsLayout layout)
        {
            _layout = new Dictionary<int, HashSet<int>>[2];
            _layout[0] = new Dictionary<int, HashSet<int>>(layout.RibbonsFamilyA);
            _layout[1] = new Dictionary<int, HashSet<int>>(layout.RibbonsFamilyB);
        }

        public RibbonsLayout(IEnumerable<SegmentIO> edges)
        {
            _layout = new Dictionary<int, HashSet<int>>[2];
            _layout[0] = new Dictionary<int, HashSet<int>>();
            _layout[1] = new Dictionary<int, HashSet<int>>();

            int splineBeamIdx = -1;
            for (int edgeIndex=0; edgeIndex<edges.Count(); edgeIndex++)
            {
                var edge = edges.ElementAt(edgeIndex);

                if (edge.SegmentIndexInRibbon != -1)
                {
                    // Create a new spline beam index when a starting edge is found (edge-beams needs to be sorted) 
                    if (edge.SegmentIndexInRibbon == 0) splineBeamIdx++;
                    edge.RibbonIndex = splineBeamIdx;
                    AddSplineBeamReference(edge.EdgeLabel, splineBeamIdx, edgeIndex);
                }
            }
        }

        public RibbonsLayout(JToken data)
        {
            _layout = new Dictionary<int, HashSet<int>>[2];
            _layout[0] = new Dictionary<int, HashSet<int>>();
            _layout[1] = new Dictionary<int, HashSet<int>>();

            var spData = data["RibbonsFamilyA"].Children();
            for (int i = 0; i < spData.Count(); i++)
            {
                var eData = spData.ElementAt(i).Children();

                for (int j = 0; j < eData.Count(); j++)
                {
                    AddSplineBeamReference(SegmentLabels.RodA, i, (int)eData.ElementAt(0)[j]);
                }
            }

            spData = data["RibbonsFamilyB"].Children();
            for (int i=0; i< spData.Count(); i++)
            {
                var eData = spData.ElementAt(i).Children();

                for(int j=0; j<eData.Count(); j++)
                {
                    AddSplineBeamReference(SegmentLabels.RodB, i, (int)eData.ElementAt(0)[j]);
                }
            }

        }

        public object Clone()
        {
            return new RibbonsLayout(this);
        }

        public void AddSplineBeamReference(SegmentLabels label, int splineBeamIndex, int edgeBeamIndex)
        {
            if (label == SegmentLabels.RodA)
            {
                if (!_layout[0].ContainsKey(splineBeamIndex))
                {
                    _layout[0].Add(splineBeamIndex, new HashSet<int>());
                }

                _layout[0][splineBeamIndex].Add(edgeBeamIndex);
            }

            if (label == SegmentLabels.RodB)
            {
                if (!_layout[1].ContainsKey(splineBeamIndex))
                {
                    _layout[1].Add(splineBeamIndex, new HashSet<int>());
                }

                _layout[1][splineBeamIndex].Add(edgeBeamIndex);
            }
        }

        public bool ContainsLayoutData()
        {
            return _layout[0].Count > 0 && RibbonsFamilyB.Count > 0 ? true : false;
        }
    }
}
