using System;
using ErodDataLib.Types;

namespace ErodDataLib.Utils
{
	public struct JsonWeaving
	{
		public EdgeGraph Graph { get; private set; }
		public SupportIOCollection Supports { get; private set;}
		public MaterialIOCollection Materials { get; private set; }
		public WeavingOptimizationOptions OptimizationOptions { get; private set; }
		public int Interleaving { get; private set; }
		public TargetSurfaceIO TargetSurface { get; private set; }

        public JsonWeaving(LinkageIO linkage, TargetSurfaceIO surface, WeavingOptimizationOptions options)
		{
			Graph = linkage.Graph;
			OptimizationOptions = options;
			Supports = linkage.Supports;
			Materials = linkage.Materials;
			Interleaving = linkage.Interleaving;
			TargetSurface = surface;
		}
	}
}

