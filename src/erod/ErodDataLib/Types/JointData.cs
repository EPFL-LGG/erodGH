using System;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public struct JointData
    {
        public Point3d Position { get; set; }
        public Vector3d Normal { get; set; }
        public Vector3d EdgeA { get; set; }
        public Vector3d EdgeB { get; set; }
        public int[] SegmentsA { get; set; }
        public int[] SegmentsB { get; set; }
        public bool[] IsStartA { get; set; }
        public bool[] IsStartB { get; set; }
        public int NumA { get; set; }
        public int NumB { get; set; }
        public double LenA => EdgeA.Length;
        public double LenB => EdgeB.Length;

        public JointData(JToken data)
        {
            // Position
            var token = data["Position"];
            Position = new Point3d((double)token["X"], (double)token["Y"], (double)token["Z"]);

            // Normal
            token = data["Normal"];
            Normal = new Vector3d((double)token["X"], (double)token["Y"], (double)token["Z"]);

            // Edge-vectors
            token = data["EdgeA"];
            EdgeA = new Vector3d((double)token["X"], (double)token["Y"], (double)token["Z"]);
            token = data["EdgeB"];
            EdgeB = new Vector3d((double)token["X"], (double)token["Y"], (double)token["Z"]);

            // Segments
            token = data["SegmentsA"];
            SegmentsA = new int[] { (int)token[0], (int)token[1] };
            token = data["SegmentsB"];
            SegmentsB = new int[] { (int)token[0], (int)token[1] };

            // Start conditions
            token = data["IsStartA"];
            IsStartA = new bool[] { (bool)token[0], (bool)token[1] };
            token = data["IsStartB"];
            IsStartB = new bool[] { (bool)token[0], (bool)token[1] };

            // Number
            NumA = (int)data["NumA"];
            NumB = (int)data["NumB"];
        }

        public JointData(Point3d position)
        {
            Position = position;
            EdgeA = Vector3d.Unset;
            EdgeB = Vector3d.Unset;
            Normal = Vector3d.Unset;
            SegmentsA = new int[] { -1, -1 };
            SegmentsB = new int[] { -1, -1 };
            IsStartA = new bool[] { false, false };
            IsStartB = new bool[] { false, false };
            NumA = 0;
            NumB = 0;
        }

        public JointData(Point3d position, Vector3d normal, Vector3d edgeA, Vector3d edgeB, int[] segmentsA, int[] segmentsB, bool[] isStartA, bool[] isStartB, int numA, int numB)
        {
            Position = position;
            EdgeA = edgeA;
            EdgeB = edgeB;
            SegmentsA = segmentsA;
            SegmentsB = segmentsB;
            IsStartA = isStartA;
            IsStartB = isStartB;
            NumA = numA;
            NumB = numB;
            Normal = normal;

            // Adjuts the sign of edge vector B after initializing all values
            EdgeB = AdjustVectorBToAcuteAngle(edgeA, edgeB);
            // Check the cross product of both vectors
            Vector3d sourceNormal = Vector3d.CrossProduct(EdgeA,EdgeB);

            // Flip data if sourceNormal and input normals don't match
            if (sourceNormal * normal < 0)
            {
                EdgeA = EdgeB;
                EdgeB = edgeA;
                SegmentsA = segmentsB;
                SegmentsB = segmentsA;
                IsStartA = isStartB;
                IsStartB = isStartA;
                NumA = numB;
                NumB = numA;
            }
        }

        // Function to pick the sign of vectorB so that angle "alpha" between vectors A and B is acute, not obtuse.
        private Vector3d AdjustVectorBToAcuteAngle(Vector3d vecA, Vector3d vecB)
        {
            // Calculate dot product of A and B
            double dotProduct = vecA * vecB;

            // Calculate magnitudes of A and B
            double magnitudeA = vecA.Length;
            double magnitudeB = vecB.Length;

            // Calculate the cosine of the angle between A and B
            double cosAlpha = dotProduct / (magnitudeA * magnitudeB);

            // If the angle is obtuse, reverse the sign of vector B
            Vector3d signVecB = new Vector3d(vecB);
            if (cosAlpha < 0) signVecB.Reverse();

            return signVecB;
        }

        // The index of the segment that segment "si" connects with at this joint.
        public int ContinuationSegment(int si)
        {
            if (si == SegmentsA[0]) return SegmentsA[1];
            if (si == SegmentsA[1]) return SegmentsA[0];
            if (si == SegmentsB[0]) return SegmentsB[1];
            if (si == SegmentsB[1]) return SegmentsB[0];
            throw new Exception("Segment " + si + " is not incident");
        }

        // Figure out whether segment "si" is part of rod A or rod B at this joint.
        // 0 ==> A, 1 ==> B, NONE ==> si not incident this joint.
        public int SegmentABOffset(int si)
        {
            if ((si == SegmentsA[0]) || (si == SegmentsA[1])) return 0;
            if ((si == SegmentsB[0]) || (si == SegmentsB[1])) return 1;
            return -1;
        }

        // Is this joint normal best aligned with a given normal?
        public bool IsNormalConsistent(Vector3d normal)
        {
            return Normal * normal >= 0;
        }

        public override string ToString()
        {
            return "JointData";
        }
    }
}
