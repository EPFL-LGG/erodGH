using System;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
	public class TargetSurfaceData : IGH_Goo
    {
        public double[][] Vertices { get; private set; }
        public int[][] Trias { get; private set; }
        public double TargetJointWeight { get; set; }

        public TargetSurfaceData()
        {
            Vertices = new double[0][];
            Trias = new int[0][];
            TargetJointWeight = 1e-4;
        }

        public TargetSurfaceData(Mesh mesh, double weight= 1e-4)
		{
            if(!mesh.IsManifold()) throw new Exception("Non-manifold mesh.");
            if (mesh.Faces.QuadCount > 0) throw new Exception("Mesh contains quad faces.");

            Vertices = new double[mesh.Vertices.Count][];
            for (int i = 0; i < Vertices.Length; i++)
            {
                var v = mesh.Vertices[i];
                Vertices[i] = new double[] { v.X, v.Y, v.Z };
            }

            Trias = new int[mesh.Faces.Count][];
            for (int i=0; i<Trias.Length; i++) {
                var f = mesh.Faces[i];
                Trias[i] = new int[]{ f.A, f.B, f.C };
            }

            TargetJointWeight = weight;
		}

        public TargetSurfaceData(TargetSurfaceData data)
        {
            Vertices = data.Vertices.ToArray();
            Trias = data.Trias.ToArray();
            TargetJointWeight = data.TargetJointWeight;
        }

        public object Clone()
        {
            return new TargetSurfaceData(this);
        }

        public override string ToString()
        {
            return "TargetSurfaceMesh [V: " + Vertices.Length + " ; F: " + Trias.Length + "]\n";
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "Not enough data has been provided";

        public string TypeName => "TargetSurfaceMesh";

        public string TypeDescription => ToString();

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

