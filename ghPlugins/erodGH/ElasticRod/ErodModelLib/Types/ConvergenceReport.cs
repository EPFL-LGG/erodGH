using System;
using System.Runtime.InteropServices;
using ErodModelLib.Creators;

namespace ErodModelLib.Types
{
    public struct ConvergenceReport
    {
        public int OpeningStep { get; set; }
        public bool Success { get; private set; }
        public bool BacktrackingFailure { get; private set; }
        public int Iterations { get; private set; }
        public double[] Energy { get; private set; }
        public double[] GradientNorm { get; private set; }
        public double[] FreeGradientNorm { get; private set; }
        public double[] StepLength { get; private set; }
        public bool[] Indefinite { get; private set; }

        public ConvergenceReport(double[] data, int numIterations)
        {
            OpeningStep = 0;
            Iterations = numIterations;
            Success = Convert.ToBoolean(data[0]);
            BacktrackingFailure = Convert.ToBoolean(data[1]);

            int idx = 2;
            Energy = new double[Iterations];
            Array.Copy(data, idx, Energy, 0, Iterations);

            idx += Iterations;
            GradientNorm = new double[Iterations];
            Array.Copy(data, idx, GradientNorm, 0, Iterations);

            idx += Iterations;
            FreeGradientNorm = new double[Iterations];
            Array.Copy(data, idx, FreeGradientNorm, 0, Iterations);

            idx += Iterations;
            StepLength = new double[Iterations];
            Array.Copy(data, idx, StepLength, 0, Iterations);

            idx += Iterations;
            Indefinite = new bool[Iterations];
            for (int i=0; i<Iterations; i++)
            {
                Indefinite[i] = Convert.ToBoolean(data[idx + i]);
            }
        }

        public override string ToString()
        {
            if(Iterations>0)
            {
                string txt = "--- Convergence Report ---\nOpening Step: " + OpeningStep + " :: Success: " + Success + " :: Backtracking Failure: " + BacktrackingFailure;
                for (int i = 0; i < Iterations; i++)
                {
                    txt += "\nIter(" + i + ") Energy: " + Energy[i] + " -- GradientNorm: " + GradientNorm[i] + " -- FreeGradientNorm: " + FreeGradientNorm[i] + " -- StepLength: " + StepLength[i] + " -- Indefinite: " + Indefinite[i];
                }
                return txt;
            }else return "--- Empty Convergence Report --- ";
        }
    }
}
