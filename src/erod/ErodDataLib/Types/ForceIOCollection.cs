using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
	public class ForceIOCollection : IList<ForceIO>, ICloneable
    {
        private List<ForceIO> _forces;

		public ForceIOCollection()
		{
            _forces = new List<ForceIO>();
		}

        public ForceIOCollection(ForceIOCollection forces)
        {
            _forces = new List<ForceIO>(forces._forces);
        }

        public ForceIOCollection(IEnumerable<ForceIO> forces)
        {
            _forces = new List<ForceIO>(forces);
        }

        public ForceIO this[int index] { get => _forces[index]; set => _forces[index]=value; }

        public int Count => _forces.Count;

        public bool IsReadOnly => false;

        public void Add(ForceIO item)
        {
            _forces.Add(item);
        }

        public void Clear()
        {
            _forces.Clear();
        }

        public object Clone()
        {
            return new ForceIOCollection(this);
        }

        public bool Contains(ForceIO item)
        {
            return _forces.Contains(item);
        }

        public void CopyTo(ForceIO[] array, int arrayIndex)
        {
            _forces.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<ForceIO> GetEnumerator()
        {
            return _forces.GetEnumerator();
        }

        public int IndexOf(ForceIO item)
        {
            return _forces.IndexOf(item);
        }

        public void Insert(int index, ForceIO item)
        {
            _forces.Insert(index, item);
        }

        public bool Remove(ForceIO item)
        {
            return _forces.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _forces.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _forces.GetEnumerator();
        }
    }
}

