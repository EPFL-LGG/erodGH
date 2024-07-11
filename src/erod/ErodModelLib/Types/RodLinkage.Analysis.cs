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

        public override double GetMaxStrain()
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

        public override double GetBendingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyBend(Model);
        }

        public override double GetStretchingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyStretch(Model);
        }

        public override double GetTwistingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyTwist(Model);
        }
    }
}
