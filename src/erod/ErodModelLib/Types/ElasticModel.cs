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
    public abstract partial class ElasticModel : IGH_Goo, ICloneable
    {
        public Mesh MeshVis { get; protected set; }
        public IntPtr Error;
        public IntPtr Model { get;  protected set; }
        protected ModelIO _modelIO { get; set; }

        public ElasticModelType ModelType => _modelIO.ModelType;

        public ElasticModel(ModelIO modelIO) { _modelIO = modelIO; }

        public bool ContainsTemporarySupports()
        {
            if (_modelIO.Supports.GetNumberTemporarySupport() > 0) return true;
            else return false;
        }

        public bool ContainsRollingSupports()
        {
            if (_modelIO.Supports.GetNumberRollingSupport() > 0) return true;
            else return false;
        }

        public override string ToString()
        {
            return _modelIO.ModelType.ToString();
        }

        protected abstract void Init();

        public abstract void SetMaterial(int sectionType, double E, double poisonRatio, double[] sectionParams, int axisType);

        public abstract void InitMesh();

        protected abstract void GetMeshData(out double[] outCoords, out int[] outQuads);

        public abstract void Update();

        public abstract int GetDoFCount();

        public abstract object Clone();

        public abstract double GetEnergy();

        public abstract double GetBendingEnergy();

        public abstract double GetStretchingEnergy();

        public abstract double GetTwistingEnergy();

        public abstract double GetMaxStrain();

        public abstract double[] GetForceVars(bool includeExternalForces = true, bool includeCables=false);

        public abstract int[] GetFixedVars(int numDeploymentSteps, int deploymentStep, double step = 1.0);

        public abstract int[] GetFixedVars(bool includeTemporarySupports, double step = 1.0);

        public abstract Line[] GetCablesAsLines();

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

        public string TypeDescription => _modelIO.ModelType.ToString();

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
