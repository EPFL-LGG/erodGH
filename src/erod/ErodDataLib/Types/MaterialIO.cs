using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace ErodDataLib.Types
{
    public class MaterialIO
    {
        public Point3d ReferencePosition { get; private set; }
        public double E { get; private set; }
        public double PoisonsRatio { get; private set; }
        public int CrossSectionType { get; private set; }
        public int Orientation { get; private set; }
        public int IndexMap { get; set; }
        public double[] Parameters { get; private set; }
        public double[] ContourProfile { get; private set; }

        public MaterialIO(JToken data)
        {
            var token = data["Position"];
            ReferencePosition = new Point3d((double)token[0], (double)token[1], (double)token[2]);
            E = (double) data["E"];
            PoisonsRatio = (double) data["PoisonsRatio"];
            CrossSectionType = (int) data["CrossSectionType"];
            Orientation = (int)data["Orientation"];
            IndexMap = (int)data["JointIndex"];

            token = data["Parameters"];
            int count = token.Count();
            Parameters = new double[count];
            for (int i = 0; i < count; i++) Parameters[i] = (double)token[i];

            token = data["ContourProfile"];
            count = token.Count();
            ContourProfile = new double[count];
            for (int i = 0; i < count; i++) ContourProfile[i] = (double)token[i];
        }

        public MaterialIO(CrossSectionType section, StiffAxis orientation, double width, double height, double youngModulus, double poissonRatio)
        {
            ReferencePosition = Point3d.Unset;
            E = youngModulus ;
            PoisonsRatio = poissonRatio ;
            CrossSectionType = (int)section;
            Orientation = (int)orientation;
            Parameters = new double[] { width, height };
            IndexMap = -1;
            ContourProfile = new double[0];
        }

        public MaterialIO(CrossSectionType section, StiffAxis orientation, double[] sectionParams, double youngModulus, double poissonRatio)
        {
            ReferencePosition = Point3d.Unset;
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = (int)section;
            Orientation = (int)orientation;
            Parameters = sectionParams;
            IndexMap = -1;
            ContourProfile = new double[0];
        }

        public MaterialIO(MaterialIO mat)
        {
            ReferencePosition = mat.ReferencePosition;
            E = mat.E;
            PoisonsRatio = mat.PoisonsRatio;
            CrossSectionType = mat.CrossSectionType;
            Orientation = mat.Orientation;
            Parameters = (double[])mat.Parameters.Clone();
            ContourProfile = (double[])mat.ContourProfile.Clone();
            IndexMap = mat.IndexMap; 
        }

        public MaterialIO(int section, int orientation, double width, double height, double youngModulus, double poissonRatio)
        {
            ReferencePosition = Point3d.Unset;
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = section;
            Orientation = orientation;
            Parameters = new double[] { width, height };
            IndexMap = -1;
            ContourProfile = new double[0];
        }

        public MaterialIO(int section, int orientation, double[] sectionParams, double youngModulus, double poissonRatio)
        {
            ReferencePosition = Point3d.Unset;
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = section;
            Orientation = orientation;
            Parameters = sectionParams;
            IndexMap = -1;
            ContourProfile = new double[0];
        }

        public MaterialIO(Point3d point, CrossSectionType section, StiffAxis orientation, double width, double height, double youngModulus, double poissonRatio)
        {
            ReferencePosition = point;
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = (int)section;
            Orientation = (int)orientation;
            Parameters = new double[] { width, height };
            IndexMap = -1 ;
            ContourProfile = new double[0];
        }

        public MaterialIO(Point3d point, int section, int orientation, double width, double height, double youngModulus, double poissonRatio)
        {
            ReferencePosition = point;
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = section;
            Orientation = orientation;
            Parameters = new double[] { width, height };
            IndexMap = -1 ;
            ContourProfile = new double[0];
        }

        public MaterialIO(Point3d point, int section, int orientation, double[] sectionParams, double youngModulus, double poissonRatio)
        {
            ReferencePosition = point;
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            CrossSectionType = section;
            Orientation = orientation;
            Parameters = sectionParams;
            IndexMap = -1;
            ContourProfile = new double[0];
        }

        public MaterialIO(Point3d point, Polyline contour, double youngModulus, double poissonRatio, double scalingFactor)
        {
            ReferencePosition = point;
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            IndexMap = -1;
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

        public MaterialIO(Polyline contour, double youngModulus, double poissonRatio, double scalingFactor)
        {
            ReferencePosition = Point3d.Unset;
            E = youngModulus;
            PoisonsRatio = poissonRatio;
            IndexMap = -1;
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

        public bool IsGradientMaterial()
        {
            if (ReferencePosition == Point3d.Unset) return false;
            else return true;
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
