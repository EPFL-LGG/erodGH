using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class Solvers
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodPeriodicElasticRodNewtonSolver")]
            internal static extern int ErodPeriodicElasticRodNewtonSolver(IntPtr pRod, int numIterations, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces,
                                                                    double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                                    int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodElasticRodNewtonSolver")]
            internal static extern int ErodElasticRodNewtonSolver(IntPtr rod, int numIterations, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces,
                                                                    double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                                    int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellNewtonSolver")]
            internal static extern int ErodXShellNewtonSolver(IntPtr linkage, int numIterations, double deployedAngle, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces,
                                                                    double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                                    int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.StdCall, EntryPoint = "erodXShellAttractedLinkageNewtonSolver")]
            internal static extern int ErodXShellAttractedLinkageNewtonSolver(IntPtr linkage, int numIterations, double deployedAngle, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces,
                                                        double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection,
                                                        int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage);

        }
    }
}
