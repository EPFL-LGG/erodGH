using System;
using System.Runtime.InteropServices;
using ErodModelLib.Creators;
using System.Linq;
using System.Collections.Generic;
using ErodDataLib.Types;

namespace ErodModelLib.Types
{
    public static class NewtonSolver
    {
        public static bool Optimize(ElasticModel model, int[] supports, double[] forces, NewtonSolverOpts options, out ConvergenceReport report, bool updateMesh = true, double deployedAngle=0, bool lastStep=false)
        {
            return Optimize(model, supports, forces, options, out report, null, updateMesh, deployedAngle, lastStep);
        }

        public static bool Optimize(ElasticModel model, int[] supports, double[] forces, NewtonSolverOpts options, out ConvergenceReport report, Action<string> log, bool updateMesh = true, double deployedAngle=0, bool lastStep=false)
        {
            using (var liveLog = new NativeSolverLog(log))
            using (var nativeBuffers = new NativeBufferScope())
            {

            if (model == null) throw new ArgumentNullException(nameof(model));
            if (!model.IsValid) throw new ObjectDisposedException(nameof(model));
            if (supports == null) throw new ArgumentNullException(nameof(supports));
            if (forces == null) throw new ArgumentNullException(nameof(forces));
            var dofCount=model.GetDoFCount();
            NativeChecks.Indices(supports,dofCount,nameof(supports));
            if (forces.Length != 0 && forces.Length != dofCount) throw new ArgumentException("Force array must match the model DOFs",nameof(forces));
            int numIterations = options.NumIterations;
            bool writeReport = false;
            if (options.WriteConvergenceReport == 2) writeReport = true;
            if (lastStep)
            {
                numIterations = checked(options.NumIterations * options.IterationMultiplier);
                if (options.WriteConvergenceReport == 1) writeReport = true;
            }

            IntPtr ptrReport;
            int errorCode;
            int includeForces = Convert.ToInt32(true);

            switch (model.ModelType)
            {
                case ElasticModelType.ElasticRod:
                    errorCode = Kernel.Solvers.ErodElasticRodNewtonSolver(model.Handle, numIterations, supports.Length, forces.Length, supports, forces, options.GradTol, options.Beta, includeForces,
                                                                    Convert.ToInt32(options.Verbose), Convert.ToInt32(options.UseIdentityMetric), Convert.ToInt32(options.UseNegativeCurvatureDirection), Convert.ToInt32(options.FeasibilitySolve), Convert.ToInt32(options.VerboseNonPosDef), Convert.ToInt32(writeReport), out ptrReport, out model.Error);
                    break;
                case ElasticModelType.PeriodicRod:
                    errorCode = Kernel.Solvers.ErodPeriodicElasticRodNewtonSolver(model.Handle, numIterations, supports.Length, forces.Length, supports, forces, options.GradTol, options.Beta, includeForces,
                                                                    Convert.ToInt32(options.Verbose), Convert.ToInt32(options.UseIdentityMetric), Convert.ToInt32(options.UseNegativeCurvatureDirection), Convert.ToInt32(options.FeasibilitySolve), Convert.ToInt32(options.VerboseNonPosDef), Convert.ToInt32(writeReport), out ptrReport, out model.Error);
                    break;
                case ElasticModelType.RodLinkage:
                    errorCode = Kernel.Solvers.ErodXShellNewtonSolver(model.Handle, numIterations, deployedAngle, supports.Length, forces.Length, supports, forces, options.GradTol, options.Beta, includeForces,
                                                                    Convert.ToInt32(options.Verbose), Convert.ToInt32(options.UseIdentityMetric), Convert.ToInt32(options.UseNegativeCurvatureDirection), Convert.ToInt32(options.FeasibilitySolve), Convert.ToInt32(options.VerboseNonPosDef), Convert.ToInt32(writeReport), out ptrReport, out model.Error);
                    break;
                case ElasticModelType.AttractedSurfaceRodLinkage:
                    errorCode = Kernel.Solvers.ErodXShellAttractedLinkageNewtonSolver(model.Handle, numIterations, deployedAngle, supports.Length, forces.Length, supports, forces, options.GradTol, options.Beta, includeForces,
                                                                    Convert.ToInt32(options.Verbose), Convert.ToInt32(options.UseIdentityMetric), Convert.ToInt32(options.UseNegativeCurvatureDirection), Convert.ToInt32(options.FeasibilitySolve), Convert.ToInt32(options.VerboseNonPosDef), Convert.ToInt32(writeReport), out ptrReport, out model.Error);
                    break;
                default:
                    ptrReport = IntPtr.Zero;
                    errorCode = -1;
                    break;
            }

            if (errorCode == -1)
            {
                string errorMsg = "Invalid construction";// model.Error.ToString();
                throw new Exception(errorMsg);
            }
            else
            {
                report = new ConvergenceReport();
                if (writeReport)
                {
                    int size = NativeBufferScope.Length(ptrReport, sizeof(double));
                    if (size < 2 || (size - 2) % 5 != 0) throw new InvalidOperationException("Malformed native convergence report");
                    double[] data = new double[size];
                    NativeBufferScope.Copy(ptrReport, data, 0, size);

                    report = new ConvergenceReport(data, (size - 2) / 5);
                    Kernel.FreeNativeBuffer(ptrReport);
                }

                if (updateMesh) model.Update();

                // Return true if the model converged
                if (errorCode == 1) return true;
                else return false;
            }

            }
        }
    }
}
