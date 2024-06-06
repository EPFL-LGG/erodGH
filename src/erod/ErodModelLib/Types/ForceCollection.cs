using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ErodModelLib.Creators;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
	public class ForceCollection : IList<Force>, ICloneable
    {
        private List<Force> _forces;

		public ForceCollection()
		{
            _forces = new List<Force>();
		}

        public ForceCollection(ForceCollection forces)
        {
            _forces = new List<Force>(forces._forces);
        }

        public Force this[int index] { get => _forces[index] ; set => _forces[index] = value; }

        public int Count => _forces.Count;

        public bool IsReadOnly => false;

        public void Add(Force force)
        {
            _forces.Add(force);
        }

        public void Clear()
        {
            _forces.Clear();
        }

        public object Clone()
        {
           return new ForceCollection(this);
        }

        public bool Contains(Force force)
        {
            return _forces.Contains(force);
        }

        public void CopyTo(Force[] array, int arrayIndex)
        {
            _forces.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<Force> GetEnumerator()
        {
            return _forces.GetEnumerator();
        }

        public int IndexOf(Force force)
        {
            return _forces.IndexOf(force);
        }

        public void Insert(int index, Force force)
        {
            _forces.Insert(index, force);
        }

        public bool Remove(Force force)
        {
             return _forces.Remove(force);
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

