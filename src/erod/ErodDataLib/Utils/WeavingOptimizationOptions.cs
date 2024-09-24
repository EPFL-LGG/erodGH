using System;
using System.Collections.Generic;
using System.IO;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;

namespace ErodDataLib.Utils
{
	public struct WeavingOptimizationOptions : IGH_Goo
    {
		public enum OptimizationStages { OneStep = 1, TwoSteps = 2, ThreeSteps = 3 }

		public bool AutomaticVariedCrossSection { get; set; }
		public int NumOptimizationStages { get; private set;}
        public int NumberOfUpdates { get; set; }
        public double MinWidthScalingFactor { get; set; }
        public double MaxWidthScalingFactor { get; set; }
        public double UpdateAttractionWeight { get; set; }

        public WeavingOptimizationOptions(bool autoVariedCS, double minFactor, double maxFactor, OptimizationStages stages)
        {
            AutomaticVariedCrossSection = autoVariedCS;
            NumOptimizationStages = (int) stages;
            MinWidthScalingFactor = minFactor;
            MaxWidthScalingFactor = maxFactor;
            NumberOfUpdates = 3;
            UpdateAttractionWeight = -5;
        }

        public WeavingOptimizationOptions(JToken data)
        {
            // Joints
            AutomaticVariedCrossSection = (bool)data["AutomaticVariedCrossSection"];
            var stages = (int)data["NumOptimizationStages"];
            switch (stages)
            {
                case 1:
                    NumOptimizationStages = 1;
                    break;
                case 2:
                    NumOptimizationStages = 2;
                    break;
                case 3:
                    NumOptimizationStages = 3;
                    break;
                default:
                    NumOptimizationStages = 1;
                    break;

            }
            MinWidthScalingFactor = (double)data["MinWidthScalingFactor"];
            MaxWidthScalingFactor = (double)data["MaxWidthScalingFactor"];
            NumberOfUpdates = (int)data["NumberOfUpdates"];
            UpdateAttractionWeight = (double)data["UpdateAttractionWeight"];
        }

        public void SetNumOptimizationStages(OptimizationStages stages)
        {
            NumOptimizationStages = (int)stages;
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "";

        public string TypeName => ToString();

        public string TypeDescription => "Optimization options.";

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

