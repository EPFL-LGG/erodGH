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
        public int SampleCount => Energy == null ? 0 : Energy.Length;
        public double[] Energy { get; private set; }
        public double[] GradientNorm { get; private set; }
        public double[] FreeGradientNorm { get; private set; }
        public double[] StepLength { get; private set; }
        public bool[] Indefinite { get; private set; }

        public ConvergenceReport(double[] data, int sampleCount)
        {
            if (data == null || sampleCount < 0 || data.Length != checked(2 + sampleCount * 5)) throw new ArgumentException("Invalid convergence report length", nameof(data));
            OpeningStep = 0;
            Iterations = Math.Max(0, sampleCount - 1);
            Success = Convert.ToBoolean(data[0]);
            BacktrackingFailure = Convert.ToBoolean(data[1]);

            int idx = 2;
            Energy = new double[sampleCount];
            Array.Copy(data, idx, Energy, 0, sampleCount);

            idx += sampleCount;
            GradientNorm = new double[sampleCount];
            Array.Copy(data, idx, GradientNorm, 0, sampleCount);

            idx += sampleCount;
            FreeGradientNorm = new double[sampleCount];
            Array.Copy(data, idx, FreeGradientNorm, 0, sampleCount);

            idx += sampleCount;
            StepLength = new double[sampleCount];
            Array.Copy(data, idx, StepLength, 0, sampleCount);

            idx += sampleCount;
            Indefinite = new bool[sampleCount];
            for (int i=0; i<sampleCount; i++)
            {
                Indefinite[i] = Convert.ToBoolean(data[idx + i]);
            }
        }

        public override string ToString()
        {
            if(SampleCount>0)
            {
                string txt = "--- Convergence Report ---\nOpening Step: " + OpeningStep + " :: Success: " + Success + " :: Backtracking Failure: " + BacktrackingFailure;
                for (int i = 0; i < SampleCount; i++)
                {
                    txt += "\nIter(" + i + ") Energy: " + Energy[i] + " -- GradientNorm: " + GradientNorm[i] + " -- FreeGradientNorm: " + FreeGradientNorm[i] + " -- StepLength: " + StepLength[i] + " -- Indefinite: " + Indefinite[i];
                }
                return txt;
            }else return "--- Empty Convergence Report --- ";
        }
    }
}
