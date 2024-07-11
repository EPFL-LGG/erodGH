using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErodDataLib.Utils;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public enum InterleavingType { xshell = 0, weaving = 1, noOffset = 2, triaxialWeave = 3 };
    public enum CrossSectionType { rectangle = 0, ellipse = 1, I = 2, L = 3, cross = 4, custom = 5 };
    public enum StiffAxis { tangent = 0, normal = 1 };

    public partial class LinkageIO : ModelIO
    {
        public RibbonsLayout Layout { get; private set; }
        public SegmentIOCollection Segments { get; private set; }
        public JointIOCollection Joints { get; private set; }
        public int Interleaving { get; private set; }
        public bool CheckTriangles { get; private set; }
        public int FirstJointNodeMap { get; private set; }
        public int[] NodeToJointMaps { get; private set; }
        public TargetSurfaceData TargetSurface { get; private set; }

        // TODO
        public LinkageIO(string fileName) : base(ElasticModelType.RodLinkage)
        {
            JObject data = JObject.Parse(File.ReadAllText(fileName));
        }
   
        public LinkageIO(IEnumerable<SegmentIO> edges, IEnumerable<NormalIO> normals, IEnumerable<MaterialIO> materials, IEnumerable<SupportIO> supports, InterleavingType interleavingType = InterleavingType.noOffset, bool checkTriangles=false) : base(ElasticModelType.RodLinkage)
        {
            Graph = new EdgeGraph(edges, normals, TOLERANCE_CLOSEST_POINT);
            Segments = new SegmentIOCollection(edges);
            TargetSurface = null;
            Interleaving = (int) interleavingType;
            CheckTriangles = checkTriangles;
            FirstJointNodeMap = -1;

            // Materials
            if (materials.Count() == 0) throw new Exception("Linkage should contain at least one material.");
            foreach (var mat in materials) Materials.Add(mat);

            // Supports
            if (supports.Count() == 0) Supports.Add(new SupportIO(Graph.GetAveragePoint()));
            else foreach (var sp in supports) Supports.Add(sp);

            Init();
        }

        public LinkageIO(LinkageIO modelIO) : base(modelIO.ModelType)
        {
            Graph = (EdgeGraph) modelIO.Graph.Clone();
            Joints = (JointIOCollection) modelIO.Joints.Clone();
            Segments = (SegmentIOCollection) modelIO.Segments.Clone();
            Materials = (MaterialIOCollection) modelIO.Materials.Clone();
            Supports = (SupportIOCollection) modelIO.Supports.Clone();
            Layout = (RibbonsLayout) modelIO.Layout.Clone();
            Forces = (ForceIOCollection)modelIO.Forces.Clone();
            Interleaving = modelIO.Interleaving;
            CheckTriangles = modelIO.CheckTriangles;
            FirstJointNodeMap = modelIO.FirstJointNodeMap;
            NodeToJointMaps = modelIO.NodeToJointMaps.ToArray();
            TargetSurface = modelIO.TargetSurface==null ? null : (TargetSurfaceData)modelIO.TargetSurface.Clone();
        }

        public override object Clone()
        {
            return new LinkageIO(this); ;
        }

        public bool ContainsTargetSurface()
        {
            return TargetSurface == null ? false : true;
        }

        public void WriteJsonFile(string path, string filename)
        {
            // Serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(@path + filename + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }
    }
}
