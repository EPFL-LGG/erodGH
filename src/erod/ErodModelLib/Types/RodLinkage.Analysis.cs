using System;
using ErodModelLib.Creators;

namespace ErodModelLib.Types
{
    public partial class RodLinkage
    {
        public double GetMaxRodEnergy()
        {
            return Kernel.Analysis.ErodXShellGetMaxRodEnergy(Model);
        }

        public double GetTotalRestLengths()
        {
            return Kernel.RodLinkage.ErodXShellGetTotalRestLength(Model);
        }

        public double GetMinJointAngle()
        {
            return Kernel.RodLinkage.ErodXShellGetMinJointAngle(Model);
        }

        public override int GetDoFCount()
        {
            return Kernel.RodLinkage.ErodXShellGetDoFCount(Model);
        }

        public double GetMaxStrain()
        {
            return Kernel.Analysis.ErodXShellGetMaxStrain(Model);
        }

        public double GetAverageJointAngle()
        {
            return Kernel.RodLinkage.ErodXShellGetAverageJointAngle(Model);
        }

        public override double GetEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergy(Model);
        }

        public double GetBendingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyBend(Model);
        }

        public double GetStretchingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyStretch(Model);
        }

        public double GetTwistingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyTwist(Model);
        }
    }
}
