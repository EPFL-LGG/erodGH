﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ErodDataLib.Types;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
	public class SupportIOCollection : IList<SupportIO>, ICloneable
    {
        private List<SupportIO> _supports;
        public bool ShowTemporarySupports { get; set; }

		public SupportIOCollection()
		{
            _supports = new List<SupportIO>();
            ShowTemporarySupports = true;
        }

        public SupportIOCollection(SupportIOCollection supports)
        {
            _supports = new List<SupportIO>(supports._supports);
            ShowTemporarySupports = true;
        }

        public SupportIO this[int index] { get => _supports[index]; set => _supports[index] = value; }

        public int Count => _supports.Count;

        public bool IsReadOnly => false;

        public void Add(SupportIO support)
        {
            _supports.Add(support);
        }

        public void Clear()
        {
            _supports.Clear();
        }

        public bool Contains(SupportIO support)
        {
            return _supports.Contains(support);
        }

        public void CopyTo(SupportIO[] array, int arrayIndex)
        {
            _supports.ToList().CopyTo(array, arrayIndex);
        }

        public IEnumerator<SupportIO> GetEnumerator()
        {
            return _supports.GetEnumerator();
        }

        public int IndexOf(SupportIO support)
        {
            return _supports.IndexOf(support);
        }

        public void Insert(int index, SupportIO support)
        {
            _supports.Insert(index, support);
        }

        public bool Remove(SupportIO support)
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

        public SupportIO[] GetTemporarySupports()
        {
            return _supports.Where(sp => sp.IsTemporary).ToArray();
        }

        public SupportIO[] GetFixSupports()
        {
            return _supports.Where(sp => !sp.IsTemporary).ToArray();
        }

        public int GetNumberFixSupport()
        {
            return _supports.Where(sp => !sp.IsTemporary).Count();
        }

        public int GetNumberTemporarySupport()
        {
            return _supports.Where(sp => sp.IsTemporary).Count();
        }

        public int GetNumberRollingSupport()
        {
            return _supports.Where(sp => sp.ContainsTarget).Count();
        }

        public Point3d[] GetTemporarySupportsAsPoint3dArray()
        {
            return GetTemporarySupports().Select( sp => sp.VisualizationPosition).ToArray();
        }

        public Point3d[] GetFixSupportsAsPoint3dArray()
        {
            return GetFixSupports().Select(sp => sp.VisualizationPosition).ToArray();
        }

        public int[] GetSupportsDoFsIndices(bool includeTemporarySupport=true)
        {
            if (includeTemporarySupport) return _supports.Select(sp => sp.IndicesDoFs).SelectMany(dof => dof).Where(idx => idx > -1).ToHashSet().ToArray();
            else return GetFixSupports().Select(sp => sp.IndicesDoFs).SelectMany(dof => dof).Where(idx => idx > -1).ToHashSet().ToArray();
        }

        public int[] GetSupportsDoFsIndices(int numDeploymentSteps, int deploymentStep)
        {
            List<int> indices = new List<int>();
            foreach(var sp in _supports)
            {
                if(!sp.IsTemporary) indices.AddRange(sp.IndicesDoFs.Where(idx => idx > -1).ToHashSet());
                else
                {
                    int releaseStep = (int) Math.Floor(sp.ReleaseCoefficient * (numDeploymentSteps-1));
                    if(deploymentStep<releaseStep) indices.AddRange(sp.IndicesDoFs.Where(idx => idx > -1).ToHashSet());
                }
            }
            return indices.ToArray();
        }

        public object Clone()
        {
            return new SupportIOCollection(this);
        }
    }
}

