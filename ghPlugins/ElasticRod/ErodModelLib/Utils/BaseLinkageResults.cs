using System;
using System.Collections.Generic;
using System.Linq;
using ErodModelLib.Types;
using Rhino.Geometry;
using Speckle.Core.Models;

namespace ErodModelLib.Utils
{
    public class BaseLinkageResults : Base
    {
        public BaseLinkageResults(RodLinkage linkage)
        {
            this["E"] = linkage.HomogenousMaterial.E;
            this["PoisonsRatio"] = linkage.HomogenousMaterial.PoisonsRatio;
            this["CrossSectionType"] = linkage.HomogenousMaterial.CrossSectionType;
            this["Orientation"] = linkage.HomogenousMaterial.Orientation;
            this["MatParameters"] = linkage.HomogenousMaterial.Parameters;
            this["AverageAngle"] = linkage.GetAverageJointAngle();

            this["Mesh"] = BaseTargetSurface.BuildSpeckleMesh(linkage.MeshVis.DuplicateMesh());
            this["ScalarFieldMaxBendingStresses"] = linkage.GetScalarFieldMaxBendingStresses();
            this["ScalarFieldMinBendingStresses"] = linkage.GetScalarFieldMinBendingStresses();
            this["ScalarFieldStretchingStresses"] = linkage.GetScalarFieldStretchingStresses();
            this["ScalarFieldSqrtBendingEnergies"] = linkage.GetScalarFieldSqrtBendingEnergies();
            this["ScalarFieldTwistingStresses"] = linkage.GetScalarFieldTwistingStresses();

            //Per rod segment data
            int count = linkage.Segments.Length;
            RodSegmentResults[] rodResults = new RodSegmentResults[count];
            for(int i=0;i< count; i++)
            {
                rodResults[i] = new RodSegmentResults(linkage.Segments[i]);
            }
            this["RodSegments"] = rodResults;
        }
    }

    public class RodSegmentResults : Base
    {
        public RodSegmentResults(RodSegment segment)
        {
            this["Nodes"] = BuildSpecklePoints(segment.GetCenterLinePositionsAsPoint3d());
            this["MaxBendingStresses"] = segment.GetMaxBendingStresses();
            this["MinBendingStresses"] = segment.GetMinBendingStresses();
            this["TwistingStresses"] = segment.GetTwistingStresses();
            this["StretchingStresses"] = segment.GetStretchingStresses();
            this["SqrtBendingEnergies"] = segment.GetSqrtBendingEnergies();
            this["StretchingEnergy"] = segment.GetStretchingEnergy();
            this["TwistingEnergy"] = segment.GetTwistingEnergy();
            this["BendingEnergies"] = segment.GetBendingEnergy();
            this["MaterialFrames"] = BuildSpecklePlanes(segment.GetMaterialFames());
        }

        public static Objects.Geometry.Point[] BuildSpecklePoints(IEnumerable<Point3d> pts)
        {
            Objects.Geometry.Point[] sPts = new Objects.Geometry.Point[pts.Count()];

            for (int i = 0; i < sPts.Length; i++)
            {
                Point3d p = pts.ElementAt(i);
                sPts[i] = new Objects.Geometry.Point(p.X, p.Y, p.Z);
            }

            return sPts;
        }

        public static Objects.Geometry.Plane[] BuildSpecklePlanes(IEnumerable<Plane> planes)
        {
            Objects.Geometry.Plane[] sPlanes = new Objects.Geometry.Plane[planes.Count()];

            for (int i = 0; i < sPlanes.Length; i++)
            {
                Plane p = planes.ElementAt(i);
                var orig = new Objects.Geometry.Point(p.OriginX, p.OriginY, p.OriginZ);
                var norm = new Objects.Geometry.Vector(p.Normal.X, p.Normal.Y, p.Normal.Z);
                var xDir = new Objects.Geometry.Vector(p.XAxis.X, p.XAxis.Y, p.XAxis.Z);
                var yDir = new Objects.Geometry.Vector(p.YAxis.X, p.YAxis.Y, p.YAxis.Z);
                sPlanes[i] = new Objects.Geometry.Plane(orig, norm, xDir, yDir);
            }

            return sPlanes;
        }
    }
}
