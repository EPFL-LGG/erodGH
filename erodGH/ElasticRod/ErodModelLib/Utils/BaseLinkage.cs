using System;
using System.Collections.Generic;
using System.IO;
using ErodDataLib.Utils;
using ErodModelLib.Types;
using Newtonsoft.Json;
using Rhino.Geometry;
using Speckle.Core.Models;

namespace ErodModelLib.Utils
{
    public class BaseLinkage : Base
    {
        [DetachProperty]
        public JointState[] Joints { get; private set; }
        [DetachProperty]
        public RodSegmentState[] RodSegments { get; private set; }
        public double InitialMinRestLength { get; private set; }
        public double[] PerSegmentRestLength { get; private set; }
        public double E { get; set; }
        public double PoisonsRatio { get; set; }
        public int CrossSectionType { get; set; }
        public int Orientation { get; set; }
        public double[] MatParameters { get; set; }
        public SparseMatrixState SegmentRestLenToEdgeRestLenMapTranspose { get; set; }
        public DesignParamState DesignParamConfig { get; set; }
        public BaseTargetSurface TargetSurface { get; set; }

        public BaseLinkage() { }

        public BaseLinkage(RodLinkage linkage, BaseTargetSurface target =null, bool includeMesh=true)
        {
            Joints = new JointState[linkage.Joints.Length];
            for (int i = 0; i < Joints.Length; i++)
            {
                Joints[i] = new JointState(linkage.Joints[i]);
            }

            RodSegments = new RodSegmentState[linkage.Segments.Length];
            for (int i = 0; i < RodSegments.Length; i++)
            {
                RodSegments[i] = new RodSegmentState(linkage.Segments[i]);
            }

            InitialMinRestLength = linkage.GetInitialMinRestLength();

            E = linkage.HomogenousMaterial.E;
            PoisonsRatio = linkage.HomogenousMaterial.PoisonsRatio;
            CrossSectionType = linkage.HomogenousMaterial.CrossSectionType;
            Orientation = linkage.HomogenousMaterial.Orientation;
            MatParameters = linkage.HomogenousMaterial.Parameters;

            PerSegmentRestLength = linkage.GetPerSegmentRestLength();

            if(includeMesh) this["LinkageMesh"] = BaseTargetSurface.BuildSpeckleMesh(linkage.MeshVis.DuplicateMesh());

            SegmentRestLenToEdgeRestLenMapTranspose = new SparseMatrixState(linkage.GetSegmentRestLenToEdgeRestLenMapTranspose());

            bool restLength, restKappa;
            linkage.GetDesignParametersConfig(out restLength, out restKappa);
            DesignParamConfig = new DesignParamState(restLength, restKappa);

            if (target != null) TargetSurface = target;
        }

        public override string ToString()
        {
            return "BaseLinkage";
        }

        public void WriteJsonFile(string path, string filename)
        {
            // Serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(@path + filename + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, this);
            }
        }
    }

    public class JointState : Base
    {
        public double[] Omega { get; set; }
        public double[] Position { get; set; }
        public double[] SourceTangent { get; set; }
        public double[] SourceNormal { get; set; }

        public double Alpha { get; set; }
        public int[] NormalSigns { get; set; }
        public double SignB { get; set; }
        public double LenA { get; set; }
        public double LenB { get; set; }
        public int[] SegmentsA { get; set; }
        public int[] SegmentsB { get; set; }
        public bool[] IsStartA { get; set; }
        public bool[] IsStartB { get; set; }
        public int JointType { get; set; }

        public JointState(Joint j)
        {
            Omega = j.GetOmega();
            var tgt = j.GetSourceTangent();
            var nrm = j.GetSourceNormal();
            var p = j.GetPosition();
            SourceTangent = new double[] { tgt[0], tgt[1], tgt[2] };
            SourceNormal = new double[] { nrm[0], nrm[1], nrm[2] };
            Position = new double[] { p[0], p[1], p[2] };

            Alpha = j.GetAlpha();
            NormalSigns = j.GetNormalSigns();
            SignB = j.GetSignB();
            LenA = j.GetLenA();
            LenB = j.GetLenB();
            SegmentsA = j.GetSegmentsA();
            SegmentsB = j.GetSegmentsB();
            IsStartA = j.GetIsStartA();
            IsStartB = j.GetIsStartB();
           
            JointType = j.GetJointType();
        }
    }

    public class RodSegmentState : Base
    {
        public int StartJoint { get; set; }
        public int EndJoint { get; set; }
        public double[] RestKappas { get; set; }
        public double[][] RestPoints { get; set; }
        public double[][][] RestDirectors { get; set; }
        public double[] RestTwists { get; set; }
        public double[] RestLengths { get; set; }
        public double[] StretchingStiffnesses { get; set; }
        public double[] TwistingStiffnesses { get; set; }
        public double[][] BendingStiffnesses { get; set; }
        public int BendingEnergyType { get; set; }
        public double[] Densities { get; set; }
        public double InitialMinRestLength { get; set; }
        public EdgeMaterialState[] EdgeMaterials { get; set; }
        public DeformedState DeformedConfiguration { get; set; }

        public RodSegmentState(RodSegment segment)
        {
            StartJoint = segment.GetStartJoint();
            EndJoint = segment.GetEndJoint();
            RestPoints = segment.GetRestPoints();
            RestDirectors = segment.GetRestDirectors();
            RestTwists = segment.GetRestTwists();
            BendingEnergyType = segment.GetBendingEnergyType();
            Densities = segment.GetDensities();
            InitialMinRestLength = segment.GetInitialMinRestLength();
            RestKappas = segment.GetRestKappas();
            RestLengths = segment.GetRestLengths();
            StretchingStiffnesses = segment.GetStretchingStiffnesses();
            TwistingStiffnesses = segment.GetTwistingStiffnesses();

            double[] lambda1, lambda2;
            segment.GetBendingStiffnesses(out lambda1, out lambda2);
            BendingStiffnesses = new double[][] { lambda1, lambda2 };

            int count = segment.EdgeMaterialCount;
            EdgeMaterials = new EdgeMaterialState[count];
            for (int i = 0; i < count; i++)
            {
                double[] matData;
                double[][] pts;
                int[][] edges;
                segment.GetEdgeMaterial(i, out matData, out pts, out edges);

                EdgeMaterials[i] = new EdgeMaterialState(matData, pts, edges);
            }

            DeformedConfiguration = new DeformedState(segment);

        }
    }

    public class DeformedState : Base
    {
        public double[] Thetas { get; set; }
        public double[][] Points { get; set; }
        public double[][] SourceTangent { get; set; }
        public double[][] SourceReferenceDirectors { get; set; }
        public double[] SourceTheta { get; set; }
        public double[] SourceReferenceTwist { get; set; }

        public DeformedState(RodSegment segment)
        {
            double[][] pts, tgt, dir;
            double[] thetas, srcThetas, srcTwist;
            segment.GetDeformedState(out pts, out thetas, out tgt, out dir, out srcThetas, out srcTwist);

            Points = pts;
            SourceTangent = tgt;
            SourceReferenceDirectors = dir;

            Thetas = thetas;
            SourceTheta = srcThetas;
            SourceReferenceTwist = srcTwist;
        }
    }

    public class EdgeMaterialState : Base
    {
        public double Area { get; set; }
        public double StretchingStiffness { get; set; }
        public double TwistingStiffness { get; set; }
        public double[] BendingStiffness { get; set; }
        public double[] MomentOfInertia { get; set; }
        public double TorsionStressCoefficient { get; set; }
        public double YoungModulus { get; set; }
        public double ShearModulus { get; set; }
        public double CrossSectionHeight { get; set; }
        public double[][] CrossSectionBoundaryPts { get; set; }
        public int[][] CrossSectionBoundaryEdges { get; set; }

        public EdgeMaterialState(double[] matData, double[][] pts, int[][] edges)
        {
            Area = matData[0];
            StretchingStiffness = matData[1];
            TwistingStiffness = matData[2];
            BendingStiffness = new double[] { matData[3], matData[4] };
            MomentOfInertia = new double[] { matData[5], matData[6] };
            TorsionStressCoefficient = matData[7];
            YoungModulus = matData[8];
            ShearModulus = matData[9];
            CrossSectionHeight = matData[10];

            CrossSectionBoundaryPts =  pts;
            CrossSectionBoundaryEdges = edges;
        }
    }

    public class SparseMatrixState : Base
    {
        public List<long> Ai { get; set; }
        public List<double> Ax { get; set; }
        public List<long> Ap { get; set; }
        public long M { get; private set; }
        public long N { get; private set; }
        public long NZ { get; private set; }

        public SparseMatrixState() { }

        public SparseMatrixState(SparseMatrixData matrix)
        {
            Ai = matrix.Ai;
            Ax = matrix.Ax;
            Ap = matrix.Ap;
            M = matrix.M;
            N = matrix.N;
            NZ = matrix.NZ;
        }
    }

    public class DesignParamState : Base
    {
        public bool RestKappa { get; set; }
        public bool RestLength { get; set; }

        public DesignParamState(bool restLength, bool restKappa)
        {
            RestLength = restLength;
            RestKappa = restKappa;
        }
    }
}
