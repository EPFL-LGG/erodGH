using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ErodDataLib.Utils;
using Rhino.Geometry;
using ErodModelLib.Creators;
using ErodDataLib.Types;
using System.Linq;


namespace ErodModelLib.Types
{
    public partial class ElasticRod : ElasticModel
    {
        protected PointCloud _nodesCloud;
        public bool IsPeriodicRod { get; private set; }
        public bool RemoveRestCurvature { get; private set; }
        public int EdgeCount { get; private set; }
        public int VerticesCount { get; private set; }

        public ElasticRod(double[] coords, bool removeRestCurvature, bool periodicRod)
        {
            // Initialize vertices
            int numPoints = coords.Length / 3;

            // Initialize RodLinkage
            if (coords.Length == 0 || coords == null)
            {
                throw new MissingFieldException("Missing vertex data.");
            }
            else
            {
                IsPeriodicRod = periodicRod;
                RemoveRestCurvature = removeRestCurvature;

                if (periodicRod)
                {
                    Model = Kernel.PeriodicRod.ErodPeriodicElasticRodBuild(numPoints, coords, Convert.ToInt32(RemoveRestCurvature), out Error);
                    EdgeCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetEdgesCount(Model);
                    VerticesCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetVerticesCount(Model);
                }
                else
                {
                    Model = Kernel.ElasticRod.ErodElasticRodBuild(numPoints, coords, out Error);
                    EdgeCount = Kernel.ElasticRod.ErodElasticRodGetEdgesCount(Model);
                    VerticesCount = Kernel.ElasticRod.ErodElasticRodGetVerticesCount(Model);
                    if (RemoveRestCurvature)
                    {
                        Kernel.ElasticRod.ErodElasticRodRemoveRestCurvatures(Model);
                    }

                }

                if (Model != IntPtr.Zero)
                {
                    Init();
                    _nodesCloud = new PointCloud();
                    for(int i=0; i<numPoints; i++)
                    {
                        _nodesCloud.Add(new Point3d(coords[i*3], coords[i * 3+1], coords[i * 3+2]));
                    }
                }
                else
                {
                    string errorMsg = Marshal.PtrToStringAnsi(Error);
                    throw new Exception(errorMsg);
                }
            }
        }

        public ElasticRod(ElasticRod model)
        {
            int numPoints = model._nodesCloud.Count;
            double[] coords = new double[numPoints*3];
            for(int i=0; i < numPoints; i++)
            {
                Point3d p = model._nodesCloud[i].Location;
                coords[i*3] = p.X;
                coords[i * 3+1] = p.Y;
                coords[i * 3+2] = p.Z;
            }

            IsPeriodicRod = model.IsPeriodicRod;
            RemoveRestCurvature = model.RemoveRestCurvature;
            if (model.IsPeriodicRod)
            {
                Model = Kernel.PeriodicRod.ErodPeriodicElasticRodCopy(model.Model, out Error);
                EdgeCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetEdgesCount(Model);
                VerticesCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetVerticesCount(Model);
            }
            else
            {
                Model = Kernel.ElasticRod.ErodElasticRodCopy(model.Model, out Error);
                EdgeCount = Kernel.ElasticRod.ErodElasticRodGetEdgesCount(Model);
                VerticesCount = Kernel.ElasticRod.ErodElasticRodGetVerticesCount(Model);
            }

            if (Model != IntPtr.Zero)
            {
                Init();
                Supports = (SupportCollection)model.Supports.Clone();
                Forces = (ForceCollection)model.Forces.Clone();

                _nodesCloud = new PointCloud(model._nodesCloud);
                InitMesh();
            }
            else
            {
                string errorMsg = Marshal.PtrToStringAnsi(Error);
                throw new Exception(errorMsg);
            }
        }

        public override string ToString()
        {
            return ModelType.ToString();
        }

        public override object Clone()
        {
            return new ElasticRod(this);
        }

        protected override void Init()
        {
            Supports = new SupportCollection();
            Forces = new ForceCollection();

            if (IsPeriodicRod) ModelType = ModelTypes.PeriodicRod;
            else ModelType = ModelTypes.ElasticRod;
        }

        public override void SetMaterial(int sectionType, double E, double poisonRatio, double[] sectionParams, int axisType)
        {
            if (IsPeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodSetMaterial(Model, sectionType, E, poisonRatio, sectionParams, sectionParams.Length, axisType);
            else Kernel.ElasticRod.ErodElasticRodSetMaterial(Model, sectionType, E, poisonRatio, sectionParams, sectionParams.Length, axisType);
        }

        public override void InitMesh()
        {
            // Build visualization mesh
            double[] outCoords;
            int[] outQuads;
            GetMeshData(out outCoords, out outQuads);

            MeshVis = Helpers.GetQuadMesh(outCoords, outQuads);
            if (!MeshVis.IsValid) throw new Exception("Errors during the construction of the xshell. There might be some incompatibilities with the joint normals.");
        }

        protected override void GetMeshData(out double[] outCoords, out int[] outQuads)
        {
            int numCoords, numQuads;
            IntPtr cPtr, qPtr;
            if (IsPeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetMeshData(Model, out cPtr, out qPtr, out numCoords, out numQuads);
            else Kernel.ElasticRod.ErodElasticRodGetMeshData(Model, out cPtr, out qPtr, out numCoords, out numQuads);

            outCoords = new double[numCoords];
            outQuads = new int[numQuads];
            Marshal.Copy(cPtr, outCoords, 0, numCoords);
            Marshal.Copy(qPtr, outQuads, 0, numQuads);
            Marshal.FreeCoTaskMem(cPtr);
            Marshal.FreeCoTaskMem(qPtr);
        }

        public override void UpdateMesh()
        {
            double[] outCoords;
            int[] outQuads;
            GetMeshData(out outCoords, out outQuads);

            int vCount = outCoords.Length / 3;
            for (int i = 0; i < vCount; i++)
            {
                MeshVis.Vertices.SetVertex(i, outCoords[i * 3], outCoords[i * 3 + 1], outCoords[i * 3 + 2]);
            }
        }

        public override int GetDoFCount()
        {
            if (IsPeriodicRod)
            {
                return Kernel.PeriodicRod.ErodPeriodicElasticRodGetDoFCount(Model);
            }
            else
            {
                return Kernel.ElasticRod.ErodElasticRodGetDoFCount(Model);
            }
        }

        public override void AddSupports(SupportData anchor)
        {
            Point3d p = anchor.GetPoint(0);

            int[] dof = anchor.LockedDOF;

            int idx = _nodesCloud.ClosestPoint(p);

            int[] outVars = new int[dof.Length];
            for (int i = 0; i < dof.Length; i++) outVars[i] = idx * 3 + dof[i];

            Support sp = new Support(p, outVars, anchor.IsTemporary);
            Supports.Add(sp);
        }

        public override void AddForces(UnaryForceData force)
        {
            int idx = _nodesCloud.ClosestPoint(force.GetPoint(0));
            Force f = new UnaryForce(idx, force.Vector, false);
            Forces.Add(f);
        }

        // TODO: implement cable forces for nodes
        public override void AddForces(CableForceData force)
        {
            throw new NotImplementedException();
        }

        public override int[] GetCentralSupportVars()
        {
            int jdo = _nodesCloud.Count/2;
            return new int[] { jdo, jdo + 1, jdo + 2 };
        }

        public override double GetEnergy()
        {
            if (IsPeriodicRod)
            {
                return Kernel.PeriodicRod.ErodPeriodicElasticRodGetEnergy(Model);
            }
            else
            {
                return Kernel.ElasticRod.ErodElasticRodGetEnergy(Model);
            }
        }

        public double[] GetVertexCoordinates()
        {
            double[] coords = new double[VerticesCount * 3];
            if (IsPeriodicRod)
            {
                Kernel.PeriodicRod.ErodPeriodicElasticRodGetCenterLinePositions(Model, coords, VerticesCount);
            }
            else
            {
                Kernel.ElasticRod.ErodElasticRodGetCenterLinePositions(Model, coords, VerticesCount);
            }
            return coords;
        }

        public double[] GetRestLengths()
        {
            double[] restLengths = new double[EdgeCount];
            if (IsPeriodicRod)
            {
                Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestLengths(Model, restLengths, EdgeCount);
            }
            else
            {
                Kernel.ElasticRod.ErodElasticRodGetRestLengths(Model, restLengths, EdgeCount);
            }
            return restLengths;
        }

        public double[] GetStretchingStresses()
        {
            double[] stresses = new double[EdgeCount];
            if (IsPeriodicRod)
            {
                Kernel.PeriodicRod.ErodPeriodicElasticRodGetStretchingStresses(Model, stresses, EdgeCount);
            }
            else
            {
                Kernel.ElasticRod.ErodElasticRodGetStretchingStresses(Model, stresses, EdgeCount);
            }
            return stresses;
        }

        public double[] GetTwistingStresses()
        {
            double[] stresses = new double[VerticesCount];
            if (IsPeriodicRod)
            {
                Kernel.PeriodicRod.ErodPeriodicElasticRodGetTwistingStresses(Model, stresses, VerticesCount);
            }
            else
            {
                Kernel.ElasticRod.ErodElasticRodGetTwistingStresses(Model, stresses, VerticesCount);
            }
            return stresses;
        }

        public double[] GetMaxBendingStresses()
        {
            double[] stresses = new double[VerticesCount];
            if (IsPeriodicRod)
            {
                Kernel.PeriodicRod.ErodPeriodicElasticRodGetMaxBendingStresses(Model, stresses, VerticesCount);
            }
            else
            {
                Kernel.ElasticRod.ErodElasticRodGetMaxBendingStresses(Model, stresses, VerticesCount);
            }
            return stresses;
        }

        public double[] GetMinBendingStresses()
        {
            double[] stresses = new double[VerticesCount];
            if (IsPeriodicRod)
            {
                Kernel.PeriodicRod.ErodPeriodicElasticRodGetMinBendingStresses(Model, stresses, VerticesCount);
            }
            else
            {
                Kernel.ElasticRod.ErodElasticRodGetMinBendingStresses(Model, stresses, VerticesCount);
            }
            return stresses;
        }

        public double[] GetSqrtBendingEnergies()
        {
            double[] stresses = new double[VerticesCount];
            if (IsPeriodicRod)
            {
                Kernel.PeriodicRod.ErodPeriodicElasticRodGetSqrtBendingEnergies(Model, stresses, VerticesCount);
            }
            else
            {
                Kernel.ElasticRod.ErodElasticRodGetSqrtBendingEnergies(Model, stresses, VerticesCount);
            }
            return stresses;
        }

        //TODO: Implemet compute forces
        public override double[] ComputeForceVars()
        {
            return new double[0];
        }
    }
}
