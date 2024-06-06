using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ErodDataLib.Types;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
	public class SupportCollection : IList<Support>, ICloneable
    {
        private List<Support> _supports;
        public bool ShowTemporarySupports { get; set; }

		public SupportCollection()
		{
            _supports = new List<Support>();
            ShowTemporarySupports = true;
        }

        public SupportCollection(SupportCollection supports)
        {
            _supports = new List<Support>(supports._supports);
        }

        public Support this[int index] { get => _supports[index]; set => _supports[index] = value; }

        public int Count => _supports.Count;

        public bool IsReadOnly => false;

        public void Add(Support support)
        {
            _supports.Add(support);
        }

        public void Clear()
        {
            _supports.Clear();
        }

        public bool Contains(Support support)
        {
            return _supports.Contains(support);
        }

        public void CopyTo(Support[] array, int arrayIndex)
        {
            _supports.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<Support> GetEnumerator()
        {
            return _supports.GetEnumerator();
        }

        public int IndexOf(Support support)
        {
            return _supports.IndexOf(support);
        }

        public void Insert(int index, Support support)
        {
            _supports.Insert(index, support);
        }

        public bool Remove(Support support)
        {
            return _supports.Remove(support);
        }

        public void RemoveAt(int index)
        {
            _supports.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _supports.GetEnumerator();
        }

        public Support[] GetTemporarySupports()
        {
            return _supports.Where(sp => sp.IsTemporary).ToArray();
        }

        public Support[] GetFixSupports()
        {
            return _supports.Where(sp => !sp.IsTemporary).ToArray();
        }

        public Point3d[] GetTemporarySupportsAsPoint3dArray()
        {
            return GetTemporarySupports().Select( sp =>sp.Position).ToArray();
        }

        public Point3d[] GetFixSupportsAsPoint3dArray()
        {
            return GetFixSupports().Select(sp => sp.Position).ToArray();
        }

        public int[] GetSupportsDoFsIndices(bool includeTemporarySupport=true)
        {
            if (includeTemporarySupport) return _supports.Select(sp => sp.LockedDoFs).Select( dof => dof).SelectMany(row => row).ToHashSet().ToArray();
            else return GetFixSupports().Select(sp => sp.LockedDoFs).Select(dof => dof).SelectMany(row => row).ToHashSet().ToArray();
        }

        public object Clone()
        {
            return new SupportCollection(this);
        }
    }
}

