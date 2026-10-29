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
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergy(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetEnergy(Handle);
        }

        public override double GetBendingEnergy()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergyBend(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetEnergyBend(Handle);
        }

        public override double GetStretchingEnergy()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergyStretch(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetEnergyStretch(Handle);
        }

        public override double GetTwistingEnergy()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergyTwist(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetEnergyTwist(Handle);
        }

        public override double GetMaxStrain()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetMaxStrain(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetMaxStrain(Handle);
        }

        public double[] GetStretchingStiffnesses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr ptrData;
            int numData;
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetStretchingStiffness(Handle, out ptrData, out numData);
            else Kernel.ElasticRod.ErodElasticRodGetStretchingStiffness(Handle, out ptrData, out numData);

            double[] data = new double[numData];
            NativeBufferScope.Copy(ptrData, data, 0, numData);
            Kernel.FreeNativeBuffer(ptrData);
            return data;

            }
        }

        public double[] GetTwistingStiffnesses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr ptrData;
            int numData;
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetTwistingStiffness(Handle, out ptrData, out numData);
            else Kernel.ElasticRod.ErodElasticRodGetTwistingStiffness(Handle, out ptrData, out numData);

            double[] data = new double[numData];
            NativeBufferScope.Copy(ptrData, data, 0, numData);
            Kernel.FreeNativeBuffer(ptrData);
            return data;

            }
        }

        public void GetBendingStiffnesses(out double[] lambda1, out double[] lambda2)
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr ptrData1, ptrData2;
            int numData1, numData2;
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetBendingStiffness(Handle, out ptrData1, out ptrData2, out numData1, out numData2);
            else Kernel.ElasticRod.ErodElasticRodGetBendingStiffness(Handle, out ptrData1, out ptrData2, out numData1, out numData2);

            lambda1 = new double[numData1];
            NativeBufferScope.Copy(ptrData1, lambda1, 0, numData1);
            Kernel.FreeNativeBuffer(ptrData1);

            lambda2 = new double[numData2];
            NativeBufferScope.Copy(ptrData2, lambda2, 0, numData2);
            Kernel.FreeNativeBuffer(ptrData2);

            }
        }

        public double[] GetVonMisesStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr outData;
            int outDataCount;

            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetVonMisesStresses(Handle, out outData, out outDataCount);
            else Kernel.ElasticRod.ErodElasticRodGetVonMisesStresses(Handle, out outData, out outDataCount);

            double[] stresses = new double[outDataCount];
            NativeBufferScope.Copy(outData, stresses, 0, outDataCount);
            Kernel.FreeNativeBuffer(outData);

            return stresses;

            }
        }

        #endregion
    }
}

