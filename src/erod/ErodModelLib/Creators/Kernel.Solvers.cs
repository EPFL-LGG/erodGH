using System.Linq;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace ErodModelLib.Creators
{
    public static partial class Kernel
    {
        public static class Solvers
        {
            internal static int ErodPeriodicElasticRodNewtonSolver(NativeHandle pRod, int numIterations, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces, double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage)
            {
                outReport = default(IntPtr);
                errorMessage = default(IntPtr);
                if (numIterations < 0) throw new ArgumentOutOfRangeException(nameof(numIterations));
                if (numSupports < 0) throw new ArgumentOutOfRangeException(nameof(numSupports));
                if (numForces < 0) throw new ArgumentOutOfRangeException(nameof(numForces));
                NativeChecks.Array(supports, NativeChecks.Product(numSupports, 1), nameof(supports));
                NativeChecks.Array(inForces, NativeChecks.Product(numForces, 1), nameof(inForces));
                NativeChecks.Finite(inForces, nameof(inForces));
                using (NativeCall.Enter(pRod))
                {
                    var result = Native_ErodPeriodicElasticRodNewtonSolver(pRod, numIterations, numSupports, numForces, supports, inForces, gradTol, beta, includeForces, verbose, useIdentityMetric, useNegativeCurvatureDirection, feasibilitySolve, verboseNonPosDef, writeReport, out outReport, out errorMessage);
                    try
                    {
                        NativeApi.Check("erodPeriodicElasticRodNewtonSolver");
                        NativeBufferScope.Track(outReport);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outReport); outReport = IntPtr.Zero;
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodPeriodicElasticRodNewtonSolver")]
            private static extern int Native_ErodPeriodicElasticRodNewtonSolver(NativeHandle pRod, int numIterations, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces, double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage);

            internal static int ErodElasticRodNewtonSolver(NativeHandle rod, int numIterations, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces, double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage)
            {
                outReport = default(IntPtr);
                errorMessage = default(IntPtr);
                if (numIterations < 0) throw new ArgumentOutOfRangeException(nameof(numIterations));
                if (numSupports < 0) throw new ArgumentOutOfRangeException(nameof(numSupports));
                if (numForces < 0) throw new ArgumentOutOfRangeException(nameof(numForces));
                NativeChecks.Array(supports, NativeChecks.Product(numSupports, 1), nameof(supports));
                NativeChecks.Array(inForces, NativeChecks.Product(numForces, 1), nameof(inForces));
                NativeChecks.Finite(inForces, nameof(inForces));
                using (NativeCall.Enter(rod))
                {
                    var result = Native_ErodElasticRodNewtonSolver(rod, numIterations, numSupports, numForces, supports, inForces, gradTol, beta, includeForces, verbose, useIdentityMetric, useNegativeCurvatureDirection, feasibilitySolve, verboseNonPosDef, writeReport, out outReport, out errorMessage);
                    try
                    {
                        NativeApi.Check("erodElasticRodNewtonSolver");
                        NativeBufferScope.Track(outReport);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outReport); outReport = IntPtr.Zero;
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodElasticRodNewtonSolver")]
            private static extern int Native_ErodElasticRodNewtonSolver(NativeHandle rod, int numIterations, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces, double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage);

            internal static int ErodXShellNewtonSolver(NativeHandle linkage, int numIterations, double deployedAngle, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces, double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage)
            {
                outReport = default(IntPtr);
                errorMessage = default(IntPtr);
                if (numIterations < 0) throw new ArgumentOutOfRangeException(nameof(numIterations));
                if (numSupports < 0) throw new ArgumentOutOfRangeException(nameof(numSupports));
                if (numForces < 0) throw new ArgumentOutOfRangeException(nameof(numForces));
                NativeChecks.Array(supports, NativeChecks.Product(numSupports, 1), nameof(supports));
                NativeChecks.Array(inForces, NativeChecks.Product(numForces, 1), nameof(inForces));
                NativeChecks.Finite(inForces, nameof(inForces));
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellNewtonSolver(linkage, numIterations, deployedAngle, numSupports, numForces, supports, inForces, gradTol, beta, includeForces, verbose, useIdentityMetric, useNegativeCurvatureDirection, feasibilitySolve, verboseNonPosDef, writeReport, out outReport, out errorMessage);
                    try
                    {
                        NativeApi.Check("erodXShellNewtonSolver");
                        NativeBufferScope.Track(outReport);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outReport); outReport = IntPtr.Zero;
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellNewtonSolver")]
            private static extern int Native_ErodXShellNewtonSolver(NativeHandle linkage, int numIterations, double deployedAngle, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces, double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage);

            internal static int ErodXShellAttractedLinkageNewtonSolver(NativeHandle linkage, int numIterations, double deployedAngle, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces, double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage)
            {
                outReport = default(IntPtr);
                errorMessage = default(IntPtr);
                if (numIterations < 0) throw new ArgumentOutOfRangeException(nameof(numIterations));
                if (numSupports < 0) throw new ArgumentOutOfRangeException(nameof(numSupports));
                if (numForces < 0) throw new ArgumentOutOfRangeException(nameof(numForces));
                NativeChecks.Array(supports, NativeChecks.Product(numSupports, 1), nameof(supports));
                NativeChecks.Array(inForces, NativeChecks.Product(numForces, 1), nameof(inForces));
                NativeChecks.Finite(inForces, nameof(inForces));
                using (NativeCall.Enter(linkage))
                {
                    var result = Native_ErodXShellAttractedLinkageNewtonSolver(linkage, numIterations, deployedAngle, numSupports, numForces, supports, inForces, gradTol, beta, includeForces, verbose, useIdentityMetric, useNegativeCurvatureDirection, feasibilitySolve, verboseNonPosDef, writeReport, out outReport, out errorMessage);
                    try
                    {
                        NativeApi.Check("erodXShellAttractedLinkageNewtonSolver");
                        NativeBufferScope.Track(outReport);
                    }
                    catch
                    {
                        NativeBufferScope.Free(outReport); outReport = IntPtr.Zero;
                        throw;
                    }
                    finally { errorMessage = IntPtr.Zero; }
                    return result;
                }
            }

            [SuppressUnmanagedCodeSecurity]
            [DllImport(erod_dylib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "erodXShellAttractedLinkageNewtonSolver")]
            private static extern int Native_ErodXShellAttractedLinkageNewtonSolver(NativeHandle linkage, int numIterations, double deployedAngle, int numSupports, int numForces, [In] int[] supports, [In] double[] inForces, double gradTol, double beta, int includeForces, int verbose, int useIdentityMetric, int useNegativeCurvatureDirection, int feasibilitySolve, int verboseNonPosDef, int writeReport, out IntPtr outReport, out IntPtr errorMessage);

        }
    }
}
