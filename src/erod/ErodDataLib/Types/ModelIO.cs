using System;
using System.Collections.Generic;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public enum ElasticModelType { Undefined = 0, ElasticRod = 1, PeriodicRod = 2, RodLinkage = 3, AttractedSurfaceRodLinkage = 4 };

    public abstract class ModelIO : IGH_Goo, ICloneable
	{
        public MaterialIOCollection Materials { get; protected set; }
        public SupportIOCollection Supports { get; protected set; }
        public ForceIOCollection Forces { get; protected set; }
        public ElasticModelType ModelType { get; private set; }
        public EdgeGraph Graph { get; protected set; }

        protected readonly double TOLERANCE_CLOSEST_POINT = 1e-3;
        protected readonly int NONE = -1;

		public ModelIO(ElasticModelType modelType)
        {
            Materials = new MaterialIOCollection();
            Supports = new SupportIOCollection();
            Forces = new ForceIOCollection();

            switch (modelType)
            {
                case ElasticModelType.ElasticRod:
                    ModelType = ElasticModelType.ElasticRod;
                    break;
                case ElasticModelType.PeriodicRod:
                    ModelType = ElasticModelType.PeriodicRod;
                    break;
                case ElasticModelType.RodLinkage:
                    ModelType = ElasticModelType.RodLinkage;
                    break;
                case ElasticModelType.AttractedSurfaceRodLinkage:
                    ModelType = ElasticModelType.AttractedSurfaceRodLinkage;
                    break;
                default:
                    ModelType = ElasticModelType.Undefined;
                    break;
            }
        }

        public abstract object Clone();

        public override string ToString()
        {
            return ModelType.ToString() + "_IO";
        }

        public void AddSupports(IEnumerable<SupportIO> supports)
        {
            foreach (var sp in supports) AddSupport(sp);
        }

        public void AddMaterials(IEnumerable<MaterialIO> materials)
        {
            foreach (var mt in materials) AddMaterial(mt);
        }

        public void AddForces(IEnumerable<ForceIO> forces)
        {
            foreach (var fr in forces) AddForce(fr);
        }

        public void AddSupport(SupportIO support)
        {
            Supports.Add(support);
        }

        public void AddCentralSupport()
        {
            Supports.Add(new SupportIO(Graph.GetAveragePoint()));
        }

        public void AddMaterial(MaterialIO material)
        {
            Materials.Add(material);
        }

        public void AddForce(ForceIO force)
        {
            Forces.Add(force);
        }

        public void SetForces(IEnumerable<ForceIO> forces)
        {
            Forces = new ForceIOCollection(forces);
        }

        public void CleanSupports()
        {
            Supports.Clear();
        }

        public void CleanForces()
        {
            Forces.Clear();
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "";

        public string TypeName => ToString();

        public string TypeDescription => "Initialization data.";

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

