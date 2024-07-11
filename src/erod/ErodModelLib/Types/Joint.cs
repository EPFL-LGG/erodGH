using System;
using System.Runtime.InteropServices;
using ErodModelLib.Creators;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ErodModelLib.Types
{
    public class Joint : IGH_Goo
    {
        private IntPtr _ptr;

        public double RestAlpha { get; private set; }
        public int Index { get; private set; }
        public Point3d Position { get; private set; }

        public Joint(IntPtr linkage, int index)
        {
            _ptr = Kernel.LinkageJoint.ErodJointBuild(linkage, index);
            Index = index;
            RestAlpha = GetAlpha();
            Position = GetPositionAsPoint3d();
        }

        public void UpdatePosition()
        {
            Position = GetPositionAsPoint3d();
        }

        public Point3d GetPositionAsPoint3d()
        {
            double[] coords = GetPosition();
            return new Point3d(coords[0], coords[1], coords[2]);
        }

        public double[] GetPosition()
        {
            IntPtr ptrCoords;
            Kernel.LinkageJoint.ErodJointGetPosition(_ptr, out ptrCoords);

            double[] coords = new double[3];
            Marshal.Copy(ptrCoords, coords, 0, 3);
            Marshal.FreeCoTaskMem(ptrCoords);

            return coords;
        }

        public double[] GetNormal()
        {
            IntPtr ptrCoords;
            Kernel.LinkageJoint.ErodJointGetNormal(_ptr, out ptrCoords);

            double[] coords = new double[3];
            Marshal.Copy(ptrCoords, coords, 0, 3);
            Marshal.FreeCoTaskMem(ptrCoords);

            return coords;
        }

        public double[] GetEdgeVecA()
        {
            IntPtr ptrCoords;
            Kernel.LinkageJoint.ErodJointGetEdgeVecA(_ptr, out ptrCoords);

            double[] coords = new double[3];
            Marshal.Copy(ptrCoords, coords, 0, 3);
            Marshal.FreeCoTaskMem(ptrCoords);

            return coords;
        }

        public double[] GetEdgeVecB()
        {
            IntPtr ptrCoords;
            Kernel.LinkageJoint.ErodJointGetEdgeVecB(_ptr, out ptrCoords);

            double[] coords = new double[3];
            Marshal.Copy(ptrCoords, coords, 0, 3);
            Marshal.FreeCoTaskMem(ptrCoords);

            return coords;
        }

        public bool[] GetIsStartA()
        {
            IntPtr ptrData;
            Kernel.LinkageJoint.ErodJointGetIsStartA(_ptr, out ptrData);

            int[] data = new int[2];
            Marshal.Copy(ptrData, data, 0, 2);
            Marshal.FreeCoTaskMem(ptrData);

            bool[] s = new bool[] { Convert.ToBoolean(data[0]), Convert.ToBoolean(data[1]) };
            return s;
        }

        public bool[] GetIsStartB()
        {
            IntPtr ptrData;
            Kernel.LinkageJoint.ErodJointGetIsStartB(_ptr, out ptrData);

            int[] data = new int[2];
            Marshal.Copy(ptrData, data, 0, 2);
            Marshal.FreeCoTaskMem(ptrData);

            bool[] s = new bool[] { Convert.ToBoolean(data[0]), Convert.ToBoolean(data[1]) };
            return s;
        }

        public void GetConnectedSegments(out int[] segmentsA, out int[] segmentsB)
        {
            IntPtr ptrA, ptrB;
            Kernel.LinkageJoint.ErodJointGetConnectedSegments(_ptr, out ptrA, out ptrB);

            segmentsA = new int[2];
            segmentsB = new int[2];
            Marshal.Copy(ptrA, segmentsA, 0, 2);
            Marshal.Copy(ptrB, segmentsB, 0, 2);
            Marshal.FreeCoTaskMem(ptrA);
            Marshal.FreeCoTaskMem(ptrB);
        }

        public double[] GetOmega()
        {
            IntPtr ptrData;
            Kernel.LinkageJoint.ErodJointGetOmega(_ptr, out ptrData);
            double[] omega = new double[3];
            Marshal.Copy(ptrData, omega, 0, 3);
            Marshal.FreeCoTaskMem(ptrData);
            return omega;
        }

        public double[] GetSourceTangent()
        {
            IntPtr ptrCoords;
            Kernel.LinkageJoint.ErodJointGetSourceTangent(_ptr, out ptrCoords);

            double[] coords = new double[3];
            Marshal.Copy(ptrCoords, coords, 0, 3);
            Marshal.FreeCoTaskMem(ptrCoords);

            return coords;
        }

        public double[] GetSourceNormal()
        {
            IntPtr ptrCoords;
            Kernel.LinkageJoint.ErodJointGetSourceNormal(_ptr, out ptrCoords);

            double[] coords = new double[3];
            Marshal.Copy(ptrCoords, coords, 0, 3);
            Marshal.FreeCoTaskMem(ptrCoords);

            return coords;
        }

        public double GetAlpha()
        {
            return Kernel.LinkageJoint.ErodJointGetAlpha(_ptr);
        }

        public int[] GetNormalSigns()
        {
            IntPtr ptrSigns;
            Kernel.LinkageJoint.ErodJointGetNormalSigns(_ptr, out ptrSigns);

            int[] signs = new int[4];
            Marshal.Copy(ptrSigns, signs, 0, 4);
            Marshal.FreeCoTaskMem(ptrSigns);

            return signs;
        }

        public double GetSignB()
        {
            return Kernel.LinkageJoint.ErodJointGetSignB(_ptr);
        }

        public double GetLenA()
        {
            return Kernel.LinkageJoint.ErodJointGetLenA(_ptr);
        }

        public double GetLenB()
        {
            return Kernel.LinkageJoint.ErodJointGetLenB(_ptr);
        }

        public int[] GetSegmentsA()
        {
            IntPtr ptrData;
            Kernel.LinkageJoint.ErodJointGetSegmentsA(_ptr, out ptrData);

            int[] segmentsA = new int[2];
            Marshal.Copy(ptrData, segmentsA, 0, 2);
            Marshal.FreeCoTaskMem(ptrData);

            return segmentsA;
        }

        public int[] GetSegmentsB()
        {
            IntPtr ptrData;
            Kernel.LinkageJoint.ErodJointGetSegmentsB(_ptr, out ptrData);

            int[] segmentsB = new int[2];
            Marshal.Copy(ptrData, segmentsB, 0, 2);
            Marshal.FreeCoTaskMem(ptrData);

            return segmentsB;
        }

        public int GetJointType()
        {
            return Kernel.LinkageJoint.ErodJointGetType(_ptr);
        }


        #region GH_Methods
        public bool IsValid {
            get
            {
                if (_ptr != null || _ptr != IntPtr.Zero) return true;
                else return false;
            }
        }

        public string IsValidWhyNot => "Missing pointer";

        public string TypeName => "JointLinkage";

        public string TypeDescription => "JointLinkage model.";

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
