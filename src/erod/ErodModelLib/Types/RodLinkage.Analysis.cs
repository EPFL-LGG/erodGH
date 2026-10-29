using System;
using ErodModelLib.Creators;

namespace ErodModelLib.Types
{
    public partial class RodLinkage
    {
        public double GetMaxRodEnergy()
        {
            return Kernel.Analysis.ErodXShellGetMaxRodEnergy(Handle);
        }

        public double GetTotalRestLengths()
        {
            return Kernel.RodLinkage.ErodXShellGetTotalRestLength(Handle);
        }

        public double GetMinJointAngle()
        {
            return Kernel.RodLinkage.ErodXShellGetMinJointAngle(Handle);
        }

        public override int GetDoFCount()
        {
            return Kernel.RodLinkage.ErodXShellGetDoFCount(Handle);
        }

        public override double GetMaxStrain()
        {
            return Kernel.Analysis.ErodXShellGetMaxStrain(Handle);
        }

        public double GetAverageJointAngle()
        {
            return Kernel.RodLinkage.ErodXShellGetAverageJointAngle(Handle);
        }

        public override double GetEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergy(Handle);
        }

        public override double GetBendingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyBend(Handle);
        }

        public override double GetStretchingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyStretch(Handle);
        }

        public override double GetTwistingEnergy()
        {
            return Kernel.Analysis.ErodXShellGetEnergyTwist(Handle);
        }
    }
}
