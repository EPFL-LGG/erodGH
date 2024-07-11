using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ErodDataLib.Types
{
	public class JointIOCollection : IList<JointIO>, ICloneable
    {
        private List<JointIO> _joints;

        public JointIOCollection()
		{
            _joints = new List<JointIO>();
		}

        public JointIOCollection(JointIOCollection joints)
        {
            _joints = new List<JointIO>(joints._joints);
        }

        public JointIOCollection(IEnumerable<JointIO> joints)
        {
            _joints = new List<JointIO>(joints);
        }

        public JointIO this[int index] { get => _joints[index]; set => _joints[index]=value; }

        public int Count => _joints.Count;

        public bool IsReadOnly => false;

        public void Add(JointIO item)
        {
            _joints.Add(item);
        }

        public void Clear()
        {
            _joints.Clear();
        }

        public object Clone()
        {
            return new JointIOCollection(this);
        }

        public bool Contains(JointIO item)
        {
            return _joints.Contains(item);
        }

        public void CopyTo(JointIO[] array, int arrayIndex)
        {
            _joints.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<JointIO> GetEnumerator()
        {
            return _joints.GetEnumerator();
        }

        public int IndexOf(JointIO item)
        {
            return _joints.IndexOf(item);
        }

        public void Insert(int index, JointIO item)
        {
            _joints.Insert(index, item);
        }

        public bool Remove(JointIO item)
        {
            return _joints.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _joints.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _joints.GetEnumerator();
        }
    }
}

