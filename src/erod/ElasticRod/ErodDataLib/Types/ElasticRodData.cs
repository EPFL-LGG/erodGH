using System;
using System.Collections.Generic;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class ElasticRodData : IGH_Goo
    {
        public List<NodeData> Nodes { get; set; }
        public List<SupportData> Supports { get; set; }
        public List<ForceData> Forces { get; set; }
        public List<MaterialData> MaterialData { get; set; }
        public double MinLength { get; set; }
        public double MaxLength { get; set; }
        protected internal PointCloud Cloud { get; set; }
        public bool IsPeriodic { get; set; }
        public bool RemoveRestCurvature { get; set; }

        public ElasticRodData()
        {
            Nodes = new List<NodeData>();
            Supports = new List<SupportData>();
            Forces = new List<ForceData>();
            MaterialData = new List<MaterialData>();
            Cloud = new PointCloud();
            IsPeriodic = false;
            RemoveRestCurvature = true;
        }

        public override string ToString()
        {
            if (IsPeriodic) return "PeriodicRod";
            else return "ElasticRod";
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "Not enough data has been provided";

        public string TypeName => ToString();

        public string TypeDescription => "Initialization data for building an ElasticRod.";

        public IGH_Goo Duplicate()
        {
            return (IGH_Goo)this.MemberwiseClone();
        }

        public IGH_GooProxy EmitProxy()
        {
            return null;
        }

        public bool CastFrom(object source)
        {
            return false;
        }

        public bool CastTo<T>(out T target)
        {
            target = default(T);
            return false;
        }

        public object ScriptVariable()
        {
            return null;
        }

        public bool Write(GH_IWriter writer)
        {
            return false;
        }

        public bool Read(GH_IReader reader)
        {
            return false;
        }
        #endregion
    }
}
