using System;
using System.Runtime.InteropServices;
using ErodModelLib.Creators;
using System.Linq;
using System.Collections.Generic;

namespace ErodModelLib.Types
{
    public static class NewtonSolver
    {
        public static bool Optimize(ElasticModel model, NewtonSolverOpts options, out ConvergenceReport report, bool updateMesh = true, double deployedAngle=0, bool lastStep=false)
        {
            int numIterations = options.NumIterations;
            bool writeReport = false;
            if (options.WriteConvergenceReport == 2) writeReport = true;
            if (lastStep)
            {
                numIterations = options.NumIterations * options.IterationMultiplier;
                if (options.WriteConvergenceReport == 1) writeReport = true;
            }

            // Supports
            HashSet<int> allSupports = new HashSet<int>(model.Supports);
            int[] supports = allSupports.Concat(model.TemporarySupports).ToArray();

            IntPtr ptrReport;
            int errorCode;

            switch (model.ModelType)
            {
                case ModelTypes.ElasticRod:
                    errorCode = Kernel.Solvers.ErodElasticRodNewtonSolver(model.Model, numIterations, supports.Length, model.Forces.Length, supports, model.Forces, options.GradTol, options.Beta, Convert.ToInt32(options.IncludeForces),
                                                                    Convert.ToInt32(options.Verbose), Convert.ToInt32(options.UseIdentityMetric), Convert.ToInt32(options.UseNegativeCurvatureDirection), Convert.ToInt32(options.FeasibilitySolve), Convert.ToInt32(options.VerboseNonPosDef), Convert.ToInt32(writeReport), out ptrReport, out model.Error);
                    break;
                case ModelTypes.PeriodicRod:
                    errorCode = Kernel.Solvers.ErodPeriodicElasticRodNewtonSolver(model.Model, numIterations, supports.Length, model.Forces.Length, supports, model.Forces, options.GradTol, options.Beta, Convert.ToInt32(options.IncludeForces),
                                                                    Convert.ToInt32(options.Verbose), Convert.ToInt32(options.UseIdentityMetric), Convert.ToInt32(options.UseNegativeCurvatureDirection), Convert.ToInt32(options.FeasibilitySolve), Convert.ToInt32(options.VerboseNonPosDef), Convert.ToInt32(writeReport), out ptrReport, out model.Error);
                    break;
                case ModelTypes.RodLinkage:
                    errorCode = Kernel.Solvers.ErodXShellNewtonSolver(model.Model, numIterations, deployedAngle, supports.Length, model.Forces.Length, supports, model.Forces, options.GradTol, options.Beta, Convert.ToInt32(options.IncludeForces),
                                                                    Convert.ToInt32(options.Verbose), Convert.ToInt32(options.UseIdentityMetric), Convert.ToInt32(options.UseNegativeCurvatureDirection), Convert.ToInt32(options.FeasibilitySolve), Convert.ToInt32(options.VerboseNonPosDef), Convert.ToInt32(writeReport), out ptrReport, out model.Error);
                    break;
                case ModelTypes.AttractedSurfaceRodLinkage:
                    errorCode = Kernel.Solvers.ErodXShellAttractedLinkageNewtonSolver(model.Model, numIterations, deployedAngle, supports.Length, model.Forces.Length, supports, model.Forces, options.GradTol, options.Beta, Convert.ToInt32(options.IncludeForces),
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
                if (updateMesh) model.UpdateMesh();

                report = new ConvergenceReport();
                if (writeReport)
                {
                    int size = numIterations * 5 + 2;
                    double[] data = new double[size];
                    Marshal.Copy(ptrReport, data, 0, size);

                    report = new ConvergenceReport(data, numIterations);
                    Marshal.FreeCoTaskMem(ptrReport);
                }

                // Return true if the model converged
                if (errorCode == 1) return true;
                else return false;
            }
        }
    }
}
