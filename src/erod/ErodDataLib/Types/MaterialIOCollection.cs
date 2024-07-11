using System;
using System.Collections;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;

namespace ErodDataLib.Types
{
	public class MaterialIOCollection : IList<MaterialIO>, ICloneable
    {
        private List<MaterialIO> _materials;

        public MaterialIOCollection()
        {
            _materials = new List<MaterialIO>();
        }

        public MaterialIOCollection(MaterialIOCollection materials)
        {
            _materials = new List<MaterialIO>(materials._materials);
        }

        public MaterialIO this[int index] { get => _materials[index]; set => _materials[index] = value; }

        public int Count => _materials.Count;

        public bool IsReadOnly => false;

        public void Add(MaterialIO material)
        {
            _materials.Add(material);
        }

        public double GetAverageWidth()
        {
            double result = 0;
            foreach (var mt in _materials) result += mt.Parameters[0];
            result /= Count;
            return result;
        }

        public double GetMaxWidth()
        {
            double result = 0;
            foreach (var mt in _materials) if (mt.Parameters[0]>result) result = mt.Parameters[0];
            return result;
        }

        public void Clear()
        {
            _materials.Clear();
        }

        public bool Contains(MaterialIO material)
        {
            return _materials.Contains(material);
        }

        public void CopyTo(MaterialIO[] array, int arrayIndex)
        {
            _materials.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<MaterialIO> GetEnumerator()
        {
            return _materials.GetEnumerator();
        }

        public int IndexOf(MaterialIO material)
        {
            return _materials.IndexOf(material);
        }

        public void Insert(int index, MaterialIO material)
        {
            _materials.Insert(index, material);
        }

        public bool Remove(MaterialIO material)
        {
            return _materials.Remove(material);
        }

        public void RemoveAt(int index)
        {
            _materials.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _materials.GetEnumerator();
        }

        public object Clone()
        {
            return new MaterialIOCollection(this);
        }
    }
}

