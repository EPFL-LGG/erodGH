using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ErodDataLib.Types
{
	public class SegmentIOCollection : IList<SegmentIO>, ICloneable
    {
        private List<SegmentIO> _segments;

        public SegmentIOCollection()
		{
            _segments = new List<SegmentIO>();
        }

        public SegmentIOCollection(SegmentIOCollection segments)
        {
            _segments = new List<SegmentIO>(segments._segments);
        }

        public SegmentIOCollection(IEnumerable<SegmentIO> segments)
        {
            _segments = new List<SegmentIO>(segments);
        }

        public SegmentIO this[int index] { get => _segments[index] ; set => _segments[index]=value; }

        public int Count => _segments.Count;

        public bool IsReadOnly => false;

        public void Add(SegmentIO item)
        {
            _segments.Add(item);
        }

        public void Clear()
        {
            _segments.Clear();
        }

        public bool Contains(SegmentIO item)
        {
            return _segments.Contains(item);
        }

        public void CopyTo(SegmentIO[] array, int arrayIndex)
        {
            _segments.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<SegmentIO> GetEnumerator()
        {
            return _segments.GetEnumerator();
        }

        public int IndexOf(SegmentIO item)
        {
            return _segments.IndexOf(item);
        }

        public void Insert(int index, SegmentIO item)
        {
            _segments.Insert(index, item);
        }

        public bool Remove(SegmentIO item)
        {
            return _segments.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _segments.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _segments.GetEnumerator();
        }

        public List<SegmentIO> GetSegmentsAsList()
        {
            return _segments.ToList();
        }

        public object Clone()
        {
            return new SegmentIOCollection(this);
        }
    }
}

