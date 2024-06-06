using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public partial class RodLinkageData : IGH_Goo
    {
        public RodLinkageData(string fileName)
        {
            JObject data = JObject.Parse(File.ReadAllText(fileName));

            // Joints
            var jData = data["Joints"];
            Joints = new List<JointData>();
            foreach (var j in jData)
            {
                Joints.Add(new JointData(j));
            }

            // Edges
            var eData = data["Edges"];
            Segments = new List<SegmentData>();
            foreach (var e in eData)
            {
                Segments.Add(new SegmentData(e));
            }

            // Supports
            var sData = data["Supports"];
            Supports = new List<SupportData>();
            foreach (var s in sData)
            {
                Supports.Add(new SupportData(s));
            }

            // Forces
            var fData = data["Forces"];
            Forces = new List<UnaryForceData>();
            foreach (var f in fData)
            {
                Forces.Add(new UnaryForceData(f));
            }

            // Materials
            var mData = data["MaterialData"];
            MaterialData = new List<MaterialData>();
            foreach (var m in mData)
            {
                MaterialData.Add(new MaterialData(m));
            }

            // Interleaving type
            Interleaving = (int)data["Interleaving"];

            // RodLinkage layout
            Layout = new RodLinkageLayout(data["Layout"]);

            OptimizationSettings = new OptimizationOptions(data["OptimizationSettings"]);
        }
   
        public RodLinkageData(IEnumerable<SegmentData> edges, IEnumerable<NormalData> normals, InterleavingType interleavingType = InterleavingType.noOffset, bool byPassTriasCheck=false)
        {
            Joints = new List<JointData>();
            Segments = new List<SegmentData>();
            Supports = new List<SupportData>();
            Forces = new List<UnaryForceData>();
            Cables = new List<CableForceData>();
            MaterialData = new List<MaterialData>();
            Interleaving = (int)interleavingType;
            Vertices = new PointCloud();
            incidentEdges = new Dictionary<int, HashSet<int>>();
            Layout = new RodLinkageLayout();
            ByPassTriasCheck = byPassTriasCheck;
            TargetSurface = null;
            Init(edges, normals);
        }

        public override string ToString()
        {
            return "RodLinkageData";
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "Not enough data has been provided";

        public string TypeName => ToString();

        public string TypeDescription => "Initialization data for building a RodLinkage model.";

        public IGH_Goo Duplicate()
        {
            return (IGH_Goo) this.MemberwiseClone();
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
