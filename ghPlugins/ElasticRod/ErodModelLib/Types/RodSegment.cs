using System;
using System.Runtime.InteropServices;
using ErodModelLib.Creators;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
    public class RodSegment : IGH_Goo
    {
        private IntPtr _ptr;
        public int EdgeCount { get; private set; }
        public int VerticesCount { get; private set; }
        public int EdgeMaterialCount { get; private set; }

        public RodSegment(IntPtr linkage, int index)
        {
            _ptr = Kernel.RodSegment.ErodRodSegmentBuild(linkage, index);
            EdgeCount = Kernel.RodSegment.ErodRodSegmentGetEdgesCount(_ptr);
            VerticesCount = Kernel.RodSegment.ErodRodSegmentGetVerticesCount(_ptr);
            EdgeMaterialCount = Kernel.RodSegment.ErodRodSegmentGetEdgeMaterialCount(_ptr);
        }

        public int GetStartJoint()
        {
            return Kernel.RodSegment.ErodRodSegmentGetStartJointIndex(_ptr);
        }

        public int GetEndJoint()
        {
            return Kernel.RodSegment.ErodRodSegmentGetEndJointIndex(_ptr);
        }

        public double[][] GetCenterLinePositions()
        {
            double[] coords = GetCenterLineCoordinates();

            int count = (int)coords.Length / 3;
            double[][] pts = new double[count][];
            for (int i = 0; i < count; i++)
            {
                pts[i] = new double[] { coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2] };
            }
            return pts;
        }

        public double[] GetCenterLineCoordinates()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetCenterLinePositions(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public Point3d[] GetCenterLinePositionsAsPoint3d()
        {
            double[] coords = GetCenterLineCoordinates();

            int count = (int)coords.Length / 3;
            Point3d[] pts = new Point3d[count];
            for (int i = 0; i < count; i++)
            {
                pts[i] = new Point3d(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
            }
            return pts;
        }

        public Curve GetInterpolatedCurve(int degree=3)
        {
            return Curve.CreateInterpolatedCurve(GetCenterLinePositionsAsPoint3d(), degree);
        }

        public double[] GetRestLengths()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetRestLengths(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetRestKappas()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetRestKappas(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetStretchingStresses()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetStretchingStresses(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetTwistingStresses()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetTwistingStresses(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetMaxBendingStresses()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetMaxBendingStresses(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetMinBendingStresses()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetMinBendingStresses(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetSqrtBendingEnergies()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetSqrtBendingEnergies(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double GetEnergy() {
            return Kernel.RodSegment.ErodRodSegmentGetEnergy(_ptr);
        }

        public double GetBendingEnergy() {
            return Kernel.RodSegment.ErodRodSegmentGetEnergyBend(_ptr);
        }

        public double GetStretchingEnergy() {
            return Kernel.RodSegment.ErodRodSegmentGetEnergyStretch(_ptr);
        }

        public double GetTwistingEnergy()
        {
            return Kernel.RodSegment.ErodRodSegmentGetEnergyTwist(_ptr);
        }

        public void GetMeshData(out double[] outCoords, out int[] outQuads)
        {
            int numCoords, numQuads;
            IntPtr cPtr, qPtr;
            Kernel.RodSegment.ErodRodSegmentGetMeshData(_ptr, out cPtr, out qPtr, out numCoords, out numQuads);

            outCoords = new double[numCoords];
            outQuads = new int[numQuads];
            Marshal.Copy(cPtr, outCoords, 0, numCoords);
            Marshal.Copy(qPtr, outQuads, 0, numQuads);
            Marshal.FreeCoTaskMem(cPtr);
            Marshal.FreeCoTaskMem(qPtr);
        }

        public double[] GetStretchingStiffnesses()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetStretchingStiffness(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetTwistingStiffnesses()
        {
            IntPtr ptrData;
            int numData;
            Kernel.RodSegment.ErodRodSegmentGetTwistingStiffness(_ptr, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public void GetBendingStiffnesses(out double[] lambda1, out double[] lambda2)
        {
            IntPtr ptrData1, ptrData2;
            int numData1, numData2;
            Kernel.RodSegment.ErodRodSegmentGetBendingStiffness(_ptr, out ptrData1, out ptrData2, out numData1, out numData2);

            lambda1 = new double[numData1];
            Marshal.Copy(ptrData1, lambda1, 0, numData1);
            Marshal.FreeCoTaskMem(ptrData1);

            lambda2 = new double[numData2];
            Marshal.Copy(ptrData2, lambda2, 0, numData2);
            Marshal.FreeCoTaskMem(ptrData2);
        }

        public Plane[] GetMaterialFames()
        {
            IntPtr outCoordsD1, outCoordsD2;
            int outCoordsCount;
            Kernel.RodSegment.ErodRodSegmentGetMaterialFrame(_ptr, out outCoordsCount, out outCoordsD1, out outCoordsD2);

            double[] coordsD1 = new double[outCoordsCount];
            double[] coordsD2 = new double[outCoordsCount];
            Marshal.Copy(outCoordsD1, coordsD1, 0, outCoordsCount);
            Marshal.Copy(outCoordsD2, coordsD2, 0, outCoordsCount);
            Marshal.FreeCoTaskMem(outCoordsD1);
            Marshal.FreeCoTaskMem(outCoordsD2);

            int numFrames = outCoordsCount / 3;
            Plane[] frames = new Plane[numFrames];
            Point3d[] pts = GetCenterLinePositionsAsPoint3d();
            for (int i = 0; i < numFrames; i++)
            {
                Vector3d x = new Vector3d(coordsD1[i * 3], coordsD1[i * 3 + 1], coordsD1[i * 3 + 2]);
                Vector3d y = new Vector3d(coordsD2[i * 3], coordsD2[i * 3 + 1], coordsD2[i * 3 + 2]);
                Point3d orig = (pts[i+1] + pts[i]) * 0.5;
                frames[i] = new Plane(orig, x, y);
            }

            return frames;
        }


        public double[][] GetRestPoints()
        {
            IntPtr outCoords;
            int outCoordsCount;

            Kernel.RodSegment.ErodRodSegmentGetRestPoints(_ptr, out outCoords, out outCoordsCount);

            double[] coords = new double[outCoordsCount];
            Marshal.Copy(outCoords, coords, 0, outCoordsCount);
            Marshal.FreeCoTaskMem(outCoords);

            int count = outCoordsCount / 3;
            double[][] pts = new double[count][];
            for (int i = 0; i < count; i++)
            {
                pts[i] = new double[] { coords[i*3], coords[i*3+1], coords[i*3+2] };
            }

            return pts;
        }

        public double[][][] GetRestDirectors()
        {
            IntPtr outCoords;
            int outCoordsCount;

            Kernel.RodSegment.ErodRodSegmentGetRestDirectors(_ptr, out outCoords, out outCoordsCount);

            double[] coords = new double[outCoordsCount];
            Marshal.Copy(outCoords, coords, 0, outCoordsCount);
            Marshal.FreeCoTaskMem(outCoords);

            int count = outCoordsCount / 6;
            double[][][] dir = new double[count][][];
            for (int i = 0; i < count; i++)
            {
                dir[i] = new double[][] { new double[] { coords[i * 6], coords[i * 6 + 1], coords[i * 6 + 2] }, new double[] { coords[i * 6 + 3], coords[i * 6 + 4], coords[i * 6 + 5] } };
            }

            return dir;
        }

        public double[] GetRestTwists()
        {
            IntPtr outData;
            int outDataCount;

            Kernel.RodSegment.ErodRodSegmentGetRestTwists(_ptr, out outData, out outDataCount);

            double[] data = new double[outDataCount];
            Marshal.Copy(outData, data, 0, outDataCount);
            Marshal.FreeCoTaskMem(outData);

            return data;
        }

        public int GetBendingEnergyType()
        {
            return Kernel.RodSegment.ErodRodSegmentGetBendingEnergyType(_ptr);
        }

        public double[] GetDensities()
        {
            IntPtr outData;
            int outDataCount;

            Kernel.RodSegment.ErodRodSegmentGetDensities(_ptr, out outData, out outDataCount);

            double[] data = new double[outDataCount];
            Marshal.Copy(outData, data, 0, outDataCount);
            Marshal.FreeCoTaskMem(outData);

            return data;
        }

        public double GetInitialMinRestLength()
        {
            return Kernel.RodSegment.ErodRodSegmentGetInitialMinRestLength(_ptr);
        }

        public void GetEdgeMaterial(int idx, out double[] matData, out double[][] boundaryPts, out int[][] boundaryEdges)
        {
            if (idx >= EdgeMaterialCount) throw new Exception("Edge index out of range.");

            IntPtr outMatData, outCoords, outEdges;
            int numMatData, numCoords, numEdges;

            Kernel.RodSegment.ErodRodSegmentGetEdgeMaterial(_ptr, idx, out outMatData, out outCoords, out outEdges, out numMatData, out numCoords, out numEdges);

            // Material Data
            matData = new double[numMatData];
            Marshal.Copy(outMatData, matData, 0, numMatData);
            Marshal.FreeCoTaskMem(outMatData);

            // CrossSection Boundary Points2D
            double[] coords = new double[numCoords];
            Marshal.Copy(outCoords, coords, 0, numCoords);
            Marshal.FreeCoTaskMem(outCoords);

            int count = numCoords / 2;
            boundaryPts = new double[count][];
            for (int i = 0; i < count; i++)
            {
                boundaryPts[i] = new double[] { coords[i * 2], coords[i * 2 + 1] };
            }

            // CrossSection Boundary Edges
            int[] edges = new int[numEdges];
            Marshal.Copy(outEdges, edges, 0, numEdges);
            Marshal.FreeCoTaskMem(outEdges);

            count = numEdges / 2;
            boundaryEdges = new int[count][];
            for (int i = 0; i < count; i++)
            {
                boundaryEdges[i] = new int[] { edges[i * 2], edges[i * 2 + 1] };
            }
        }

        public void GetDeformedState(out double[][] points, out double[] thetas, out double[][] sourceTangent, out double[][] sourceReferenceDirectors, out double[] sourceTheta, out double[] sourceReferenceTwist)
        {
            IntPtr ptrPts, ptrThetas, ptrSrcTangents, ptrDir, ptrSrcThetas, ptrSrcTwist;
            int numPts, numThetas, numSrcTangents, numDir, numSrcThetas, numSrcTwist;

            Kernel.RodSegment.ErodRodSegmentGetDeformedState(_ptr, out ptrPts, out ptrThetas, out ptrSrcTangents, out ptrDir, out ptrSrcThetas, out ptrSrcTwist,
                                                             out numPts, out numThetas, out numSrcTangents, out numDir, out numSrcThetas, out numSrcTwist);

            // Points
            double[] pts = new double[numPts];
            Marshal.Copy(ptrPts, pts, 0, numPts);
            Marshal.FreeCoTaskMem(ptrPts);

            int count = numPts / 3;
            points = new double[count][];
            for (int i = 0; i < count; i++)
            {
                points[i] = new double[] { pts[i * 3], pts[i * 3 + 1], pts[i*3+2] };
            }

            // Thetas
            thetas = new double[numThetas];
            Marshal.Copy(ptrThetas, thetas, 0, numThetas);
            Marshal.FreeCoTaskMem(ptrThetas);

            //SourceTangent
            double[] tangents = new double[numSrcTangents];
            Marshal.Copy(ptrSrcTangents, tangents, 0, numSrcTangents);
            Marshal.FreeCoTaskMem(ptrSrcTangents);

            count = numSrcTangents / 3;
            sourceTangent = new double[count][];
            for (int i = 0; i < count; i++)
            {
                sourceTangent[i] = new double[] { tangents[i * 3], tangents[i * 3 + 1], tangents[i * 3 + 2] };
            }

            //Reference Directors
            double[] directors = new double[numDir];
            Marshal.Copy(ptrDir, directors, 0, numDir);
            Marshal.FreeCoTaskMem(ptrDir);

            count = numDir / 3;
            sourceReferenceDirectors = new double[count][];
            for (int i = 0; i < count; i++)
            {
                sourceReferenceDirectors[i] = new double[] { directors[i * 3], directors[i * 3 + 1], directors[i * 3 + 2] };
            }

            //Source Thetas
            sourceTheta = new double[numSrcThetas];
            Marshal.Copy(ptrSrcThetas, sourceTheta, 0, numSrcThetas);
            Marshal.FreeCoTaskMem(ptrSrcThetas);

            //Source twist
            sourceReferenceTwist = new double[numSrcTwist];
            Marshal.Copy(ptrSrcTwist, sourceReferenceTwist, 0, numSrcTwist);
            Marshal.FreeCoTaskMem(ptrSrcTwist);
        }

        #region GH_Methods
        public bool IsValid
        {
            get
            {
                if (_ptr != null || _ptr != IntPtr.Zero) return true;
                else return false;
            }
        }

        public string IsValidWhyNot => "Missing pointer";

        public string TypeName => "RodSegment";

        public string TypeDescription => "RodSegment model.";

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
