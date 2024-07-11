using System;
using ErodDataLib.Types;
using System.Collections.Generic;
using System.Collections;
using Rhino.Geometry;
using System.Linq;

namespace ErodModelLib.Types
{
	public class JointCollection : IReadOnlyList<Joint>, ICloneable
    {
        private List<Joint> _joints;
        private PointCloud _cloud;

		public JointCollection()
		{
            _cloud = new PointCloud();
            _joints = new List<Joint>();
		}

        public JointCollection(JointCollection joints)
        {
            _cloud = new PointCloud(joints._cloud);
            _joints = new List<Joint>(joints._joints);
        }

        public Joint this[int index] { get => _joints[index]; }

        public int Count => _joints.Count;

        public bool IsReadOnly => true;

        public void Add(Joint item)
        {
            _joints.Add(item);
            _cloud.Add(item.Position);
        }

        public int ClosestJoint(Point3d pt)
        {
            return _cloud.ClosestPoint(pt);
        }

        public void Clear()
        {
            _joints.Clear();
            _cloud = new PointCloud();
        }

        public object Clone()
        {
            return new JointCollection(this);
        }

        public IEnumerator<Joint> GetEnumerator()
        {
            return _joints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _joints.GetEnumerator();
        }

        public void UpdateJointPositions()
        {
            for (int i=0; i<Count; i++)
            {
                var jt = _joints[i];
                jt.UpdatePosition();
                _cloud[i].Location = jt.Position;
            }
        }
    }
}

