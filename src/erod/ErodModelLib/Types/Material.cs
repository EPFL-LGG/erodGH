using System;
using ErodDataLib.Types;
using ErodModelLib.Creators;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
    public struct Material : IGH_Goo
    {
        public double E { get; set; }
        public double PoisonsRatio { get; set; }
        public string CrossSection { get; set; }
        public string Orientation { get; set; }
        public double Area { get; set; }
        public double G { get; set; }
        public double StretchingStiffness { get; set; }
        public double TwistingStiffness { get; set; }
        public double CrossSectionHeight { get; set; }
        public (double Lambda1, double Lambda2) BendingStiffness { get; set; }
        public (double Lambda1, double Lambda2) MomentOfInertia { get; set; }
        public IntPtr Model { get; private set; }

        public Material(MaterialIO matData)
        {
            if (matData.HasCustomProfile()) Model = Kernel.Material.ErodMaterialCustomBuild(matData.E, matData.PoisonsRatio, matData.ContourProfile, matData.ContourProfile.Length / 3, new double[0], 0, matData.Orientation);
            else Model = Kernel.Material.ErodMaterialBuild(matData.CrossSectionType, matData.E, matData.PoisonsRatio, matData.Parameters, matData.Parameters.Length, matData.Orientation);

            E = matData.E;
            PoisonsRatio = matData.PoisonsRatio;
            Orientation = Enum.GetName(typeof(StiffAxis), matData.Orientation);
            CrossSection = Enum.GetName(typeof(CrossSectionType), matData.CrossSectionType);

            Area = Kernel.Material.ErodMaterialGetArea(Model);
            G = Kernel.Material.ErodMaterialGetSherModulus(Model);
            StretchingStiffness = Kernel.Material.ErodMaterialGetStretchingStiffness(Model);
            TwistingStiffness = Kernel.Material.ErodMaterialGetTwistingStiffness(Model);
            CrossSectionHeight = Kernel.Material.ErodMaterialGetCrossSectionHeight(Model);

            double lambda1, lambda2;
            Kernel.Material.ErodMaterialGetBendingStiffness(Model, out lambda1, out lambda2);
            BendingStiffness = (lambda1, lambda2);

            Kernel.Material.ErodMaterialGetMomentOfInertia(Model, out lambda1, out lambda2);
            MomentOfInertia = (lambda1, lambda2);
        }

        public override string ToString()
        {
            return "[" + CrossSection + "-section :: " + Orientation + "-orientation]";
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "Missing pointer";

        public string TypeName => "RodMaterial";

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

