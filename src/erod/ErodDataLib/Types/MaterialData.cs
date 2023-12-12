using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class MaterialData : ElementData
    {
        public double E { get; set; }
        public double PoisonsRatio { get; set; }
        public int[] Indexes { get; set; }
        public int CrossSectionType { get; set; }
        public int Orientation { get; set; }
        public double[] Parameters { get; set; }
        public double[] ContourProfile { get; set; }

        public MaterialData(JToken data) : base(0)
        {
            E = (double) data["E"];
            PoisonsRatio = (double) data["PoisonsRatio"];
            CrossSectionType = (int) data["CrossSectionType"];
            Orientation = (int)data["Orientation"];
            Indexes = new int[0];

            var token = data["Parameters"];
            int count = token.Count();
            Parameters = new double[count];
            for (int i = 0; i < count; i++) Parameters[i] = (double)token[i];

            token = data["ContourProfile "];
            count = token.Count();
            ContourProfile = new double[count];
            for (int i = 0; i < count; i++) ContourProfile[i] = (double)token[i];
        }

        public MaterialData(CrossSectionType section, StiffAxis orientation, double width, double height, double youngModulus, double poissonRatio) : base(0)
        {
            E = youngModulus ;
            PoisonsRatio = poissonRatio ;
            CrossSectionType = (int)section;
            Orientation = (int)orientation;
            Parameters = new double[] { width, height };
            Indexes = new int[0];
            ContourProfile = new double[0];
        }

        public MaterialData(CrossSectionType section, StiffAxis orientation, double[] sectionParams, double youngModulus, double poissonRatio) : base(0)
        {
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = (int)section;
            Orientation = (int)orientation;
            Parameters = sectionParams;
            Indexes = new int[0];
            ContourProfile = new double[0];
        }

        public MaterialData(MaterialData mat) : base(0)
        {
            E = mat.E;
            PoisonsRatio = mat.PoisonsRatio;
            CrossSectionType = mat.CrossSectionType;
            Orientation = mat.Orientation;
            Parameters = (double[])mat.Parameters.Clone();
            ContourProfile = (double[])mat.ContourProfile.Clone();
            Indexes = mat.Indexes;
            
        }

        public MaterialData(int section, int orientation, double width, double height, double youngModulus, double poissonRatio) : base(0)
        {
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = section;
            Orientation = orientation;
            Parameters = new double[] { width, height };
            Indexes = new int[0];
            ContourProfile = new double[0];
        }

        public MaterialData(int section, int orientation, double[] sectionParams, double youngModulus, double poissonRatio) : base(0)
        {
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = section;
            Orientation = orientation;
            Parameters = sectionParams;
            Indexes = new int[0];
            ContourProfile = new double[0];
        }

        public MaterialData(Point3d point, CrossSectionType section, StiffAxis orientation, double width, double height, double youngModulus, double poissonRatio) : base(point)
        {

            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = (int)section;
            Orientation = (int)orientation;
            Parameters = new double[] { width, height };
            Indexes = new int[] { -1 };
            ContourProfile = new double[0];
        }

        public MaterialData(Point3d point, int section, int orientation, double width, double height, double youngModulus, double poissonRatio) : base(point)
        {

            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = section;
            Orientation = orientation;
            Parameters = new double[] { width, height };
            Indexes = new int[] { -1 };
            ContourProfile = new double[0];
        }

        public MaterialData(Point3d point, int section, int orientation, double[] sectionParams, double youngModulus, double poissonRatio) : base(point)
        {
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = section;
            Orientation = orientation;
            Parameters = sectionParams;
            Indexes = new int[] { -1 };
            ContourProfile = new double[0];
        }

        public MaterialData(Point3d point, Polyline contour, double youngModulus, double poissonRatio, double scalingFactor) : base(point)
        {
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            Indexes = new int[] { -1 };
            Parameters = new double[0];
            CrossSectionType = 5;

            if (!contour.IsClosed) throw new Exception("Contour should be a closed polyline.");
            ContourProfile = new double[(contour.Count-1)*3];
            for(int i=0; i<contour.Count-1; i++)
            {
                var p = contour[i];
                ContourProfile[i * 3] = p.X * scalingFactor;
                ContourProfile[i * 3 + 1] = p.Y * scalingFactor;
                ContourProfile[i * 3 + 2] = p.Z * scalingFactor;
            }

        }

        public MaterialData(Polyline contour, double youngModulus, double poissonRatio, double scalingFactor) : base(0)
        {
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            Indexes = new int[0];
            Parameters = new double[0];
            CrossSectionType = 5;

            if (!contour.IsClosed) throw new Exception("Contour should be a closed polyline.");
            ContourProfile = new double[(contour.Count - 1) * 3];
            for (int i = 0; i < contour.Count - 1; i++)
            {
                var p = contour[i];
                ContourProfile[i * 3] = p.X * scalingFactor;
                ContourProfile[i * 3 + 1] = p.Y * scalingFactor;
                ContourProfile[i * 3 + 2] = p.Z * scalingFactor;
            }

        }

        public bool HasCustomProfile()
        {
            return CrossSectionType == 5 ? true : false; 
        }

        public override string ToString()
        {
            return Enum.GetName(typeof(CrossSectionType), CrossSectionType);
        }
    }
}
