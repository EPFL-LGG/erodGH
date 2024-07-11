using System;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using ErodModelLib.Creators;

namespace ErodModelLib.Types
{
	public partial class ElasticRod
	{
        #region analysis
        public override double GetEnergy()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergy(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetEnergy(Model);
        }

        public override double GetBendingEnergy()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergyBend(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetEnergyBend(Model);
        }

        public override double GetStretchingEnergy()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergyStretch(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetEnergyStretch(Model);
        }

        public override double GetTwistingEnergy()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergyTwist(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetEnergyTwist(Model);
        }

        public override double GetMaxStrain()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetMaxStrain(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetMaxStrain(Model);
        }

        public double[] GetStretchingStiffnesses()
        {
            IntPtr ptrData;
            int numData;
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetStretchingStiffness(Model, out ptrData, out numData);
            else Kernel.ElasticRod.ErodElasticRodGetStretchingStiffness(Model, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetTwistingStiffnesses()
        {
            IntPtr ptrData;
            int numData;
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetTwistingStiffness(Model, out ptrData, out numData);
            else Kernel.ElasticRod.ErodElasticRodGetTwistingStiffness(Model, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public void GetBendingStiffnesses(out double[] lambda1, out double[] lambda2)
        {
            IntPtr ptrData1, ptrData2;
            int numData1, numData2;
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetBendingStiffness(Model, out ptrData1, out ptrData2, out numData1, out numData2);
            else Kernel.ElasticRod.ErodElasticRodGetBendingStiffness(Model, out ptrData1, out ptrData2, out numData1, out numData2);

            lambda1 = new double[numData1];
            Marshal.Copy(ptrData1, lambda1, 0, numData1);
            Marshal.FreeCoTaskMem(ptrData1);

            lambda2 = new double[numData2];
            Marshal.Copy(ptrData2, lambda2, 0, numData2);
            Marshal.FreeCoTaskMem(ptrData2);
        }

        public double[] GetVonMisesStresses()
        {
            IntPtr outData;
            int outDataCount;

            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetVonMisesStresses(Model, out outData, out outDataCount);
            else Kernel.ElasticRod.ErodElasticRodGetVonMisesStresses(Model, out outData, out outDataCount);

            double[] stresses = new double[outDataCount];
            Marshal.Copy(outData, stresses, 0, outDataCount);
            Marshal.FreeCoTaskMem(outData);

            return stresses;
        }

        #endregion
    }
}

