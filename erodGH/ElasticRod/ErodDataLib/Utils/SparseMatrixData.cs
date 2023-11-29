using System;
using System.Collections.Generic;

namespace ErodDataLib.Utils
{
    public class SparseMatrixData
    {
        public List<long> Ai { get; set; }
        public List<double> Ax { get; set; }
        public List<long> Ap { get; set; }
        public long M { get; private set; }
        public long N { get; private set; }
        public long NZ { get; private set; }

        public SparseMatrixData(long m, long n, long nz)
        {
            Ai = new List<long>();
            Ax = new List<double>();
            Ap = new List<long>();
            M = m;
            N = n;
            NZ = nz;
        }
    }
}
