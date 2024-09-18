using System;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;

namespace ErodModelLib.Types
{
    public struct NewtonSolverOpts : IGH_Goo
    {
        public bool Verbose { get; set; }
        public double GradTol { get; set; }
        public double Beta { get; set; }
        public int NumDeploymentSteps { get; private set; }
        // Maximum number of newton iterations per opening step
        public int NumIterations { get; set; }
        // Whether to force the use of the identity matrix for Hessian modification (instead of the problem's custom metric)
        public bool UseIdentityMetric { get; set; }
        public bool HessianScaledBeta { get; set; }
        // Whether to compute and move in negative curvature directions to escape from saddle points.
        public bool UseNegativeCurvatureDirection { get; set; }
        // Whether to solve for a feasible starting point or rely on the problem to jump to feasible parameters.
        public bool FeasibilitySolve { get; set; }
        // Print CHOLMOD warning for non-pos-def matrices
        public bool VerboseNonPosDef { get; set; }
        // Report types: (0)"No Report", (1) "Last-Step Report", (3) "Step-By-Step Report"
        public int WriteConvergenceReport { get; set; }
        public bool IncludeForces { get; set; }
        public int IterationMultiplier { get; set; }
        // Specify the step at which temporary supports are released; if no step is provided, the middle of the total number of steps is used
        public int ReleaseStep { get; private set; }

        public NewtonSolverOpts(int iterationsPerStep=20, int openingSteps=1)
        {
            Verbose = true;
            GradTol = 1e-8;
            Beta = 1e-8;
            NumIterations = iterationsPerStep;
            UseIdentityMetric = false;
            HessianScaledBeta = true;
            UseNegativeCurvatureDirection = true;
            FeasibilitySolve = true;
            VerboseNonPosDef = false;
            WriteConvergenceReport = 0;
            IncludeForces = false;
            IterationMultiplier = 1;
            NumDeploymentSteps = openingSteps;
            ReleaseStep = (int) Math.Floor(openingSteps * 0.5);
        }

        public void SetReleaseStep(int step)
        {
            if (step < 0) ReleaseStep = 0;
            if (step>NumDeploymentSteps) ReleaseStep = NumDeploymentSteps-1;
            ReleaseStep = step;
        }

        public override string ToString()
        {
            return "NewtonSolverOptions";
        }

        #region GH methods
        public bool IsValid => true;

        public string IsValidWhyNot => "Not enough data has been provided";

        public string TypeName => ToString();

        public string TypeDescription => ToString();

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
