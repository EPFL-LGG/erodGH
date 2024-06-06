using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErodDataLib.Types;
using ErodDataLib.Utils;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
    public enum ModelTypes { Undefined=0, ElasticRod = 1, PeriodicRod = 2, RodLinkage = 3, AttractedSurfaceRodLinkage = 4 };

    public abstract partial class ElasticModel : IGH_Goo, ICloneable
    {
        protected double oldEnergy = 0;
        public Mesh MeshVis { get; protected set; }

        public IntPtr Error;
        public IntPtr Model { get;  protected set; }
        public ModelTypes ModelType { get; protected set; }
        public SupportCollection Supports { get; set; }
        public ForceCollection Forces { get; set; }

        protected abstract void Init();

        public abstract void SetMaterial(int sectionType, double E, double poisonRatio, double[] sectionParams, int axisType);

        public abstract void InitMesh();

        protected abstract void GetMeshData(out double[] outCoords, out int[] outQuads);

        public abstract void UpdateMesh();

        public abstract int GetDoFCount();

        public abstract void AddSupports(SupportData anchor);

        public abstract void AddForces(CableForceData force);

        public abstract void AddForces(UnaryForceData force);

        public abstract object Clone();

        public abstract double GetEnergy();

        public abstract int[] GetCentralSupportVars();

        public abstract double[] ComputeForceVars();

        #region GH_Methods
        public bool IsValid
        {
            get
            {
                if (Model != null || Model != IntPtr.Zero) return true;
                else return false;
            }
        }

        public string IsValidWhyNot => "Missing pointer";

        public string TypeName => ToString();

        public string TypeDescription => ModelType.ToString();

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
            if (typeof(T).Equals(typeof(GH_Mesh)))
            {
                double[] outCoords;
                int[] outQuads;
                GetMeshData(out outCoords, out outQuads);

                Mesh m = Helpers.GetQuadMesh(outCoords, outQuads);
                target = (T)(object)new GH_Mesh(m);
                return true;
            }

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
