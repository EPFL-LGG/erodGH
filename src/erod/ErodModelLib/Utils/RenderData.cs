using System;
using System.Collections.Generic;
using System.IO;
using ErodModelLib.Types;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace ErodModelLib.Utils
{
    public struct RenderData
    {
        public List<RodSegmentRenderingData> RodsA { get; set; }
        public List<RodSegmentRenderingData> RodsB { get; set; }
        public CrossSectionRenderingData CrossSection { get; set; }

        public RenderData(RodLinkage model)
        {
            if (!model.ModelIO.Layout.ContainsLayoutData()) throw new Exception("RodLinkage model doesn't contain layout.");

            CrossSection = new CrossSectionRenderingData(model);

            // Linkages
            RodsA = new List<RodSegmentRenderingData>();
            foreach (int key in model.ModelIO.Layout.RibbonsFamilyA.Keys)
            {
                foreach (int idx in model.ModelIO.Layout.RibbonsFamilyA[key])
                {
                    RodsA.Add(new RodSegmentRenderingData(model.Segments[idx]));
                }
            }

            RodsB = new List<RodSegmentRenderingData>();
            foreach (int key in model.ModelIO.Layout.RibbonsFamilyB.Keys)
            {
                foreach (int idx in model.ModelIO.Layout.RibbonsFamilyB[key])
                {
                    RodsB.Add(new RodSegmentRenderingData(model.Segments[idx]));
                }
            }
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

    public struct CrossSectionRenderingData
    {
        public double[] MatParameters { get; set; }
        public double E { get; set; }
        public double PoisonsRatio { get; set; }
        public string ProfileType { get; set; }

        public CrossSectionRenderingData(RodLinkage model)
        {
            var mat = model.ModelIO.Materials[0];
            MatParameters = mat.Parameters;
            E = mat.E;
            PoisonsRatio = mat.PoisonsRatio;
            ProfileType = mat.ToString();
        }
    }

    public struct RodSegmentRenderingData
    {
        public double[] Stretching { get; set; }
        public double[] Twisting { get; set; }
        public double[] SqrtBending { get; set; }
        public double[][] FrameX { get; set; }
        public double[][] FrameY { get; set; }
        public double[][] FrameZ { get; set; }
        public double[][] Pos { get; set; }

        public RodSegmentRenderingData(RodSegment segment)
        {
            // Positions
            Pos = segment.GetCenterLinePositions();

            // Frames (Conversion is needed)
            // FrameX => align with plane normal vector
            // FrameY => align with plane X axis
            // FrameZ => align with plane Y axis
            var frames = segment.GetMaterialFames();
            FrameX = new double[frames.Length][];
            FrameY = new double[frames.Length][];
            FrameZ = new double[frames.Length][];
            for(int i=0; i<frames.Length; i++)
            {
                Plane f = frames[i];
                FrameX[i] = new double[] { f.ZAxis.X, f.ZAxis.Y, f.ZAxis.Z };
                FrameY[i] = new double[] { f.XAxis.X, f.XAxis.Y, f.XAxis.Z };
                FrameZ[i] = new double[] { f.YAxis.X, f.YAxis.Y, f.YAxis.Z };
            }

            // Stresses
            Twisting = segment.GetTwistingStresses();
            Stretching = segment.GetStretchingStresses();
            SqrtBending = segment.GetSqrtBendingEnergies();
        }
    }
}


//if (PosA.BranchCount != FramesA.BranchCount):
//    raise ValueError("Invalid data for family A")

//if (PosB.BranchCount != FramesB.BranchCount):
//    raise ValueError("Invalid data for family B")


//data = { 'Flat_FamilyA' : [], 'Flat_FamilyB' : [], 'Deploy_FamilyA' : [], 'Deploy_FamilyB' : [], 'CrossSection' : []}
//for i in range(PosA.BranchCount):
//    rodData = generateRodSegmentData(PosA.Branch(i), FramesA.Branch(i), SqrtBendA.Branch(i), TwistingA.Branch(i), StretchingA.Branch(i))
//    data['Flat_FamilyA'].append(rodData)


//for i in range(PosB.BranchCount):
//    rodData = generateRodSegmentData(PosB.Branch(i), FramesB.Branch(i), SqrtBendB.Branch(i), TwistingB.Branch(i), StretchingB.Branch(i))
//    data['Flat_FamilyB'].append(rodData)


//for i in range(D_PosA.BranchCount):
//    rodData = generateRodSegmentData(D_PosA.Branch(i), D_FramesA.Branch(i), D_SqrtBendA.Branch(i), D_TwistingA.Branch(i), D_StretchingA.Branch(i))
//    data['Deploy_FamilyA'].append(rodData)


//for i in range(D_PosB.BranchCount):
//    rodData = generateRodSegmentData(D_PosB.Branch(i), D_FramesB.Branch(i), D_SqrtBendB.Branch(i), D_TwistingB.Branch(i), D_StretchingB.Branch(i))
//    data['Deploy_FamilyB'].append(rodData)

//data['CrossSection'].append([Width, Height])


//# Save file
//                if (write):
//    file = open(path + filename + ".json", 'w')
//    json.dump(data, file)
//    file.close()


//Json = data
