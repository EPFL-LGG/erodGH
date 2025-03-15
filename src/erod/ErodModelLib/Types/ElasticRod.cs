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
        public RodIO ModelIO => (RodIO)_modelIO;
        public int EdgeCount { get; private set; }
        public int NodesCount { get; private set; }

        private PointCloud _nodes { get; set; }

        public ElasticRod(RodIO modelIO) : base(modelIO)
        {
            try
            {
                // Initialize vertices
                double[] coords = modelIO.GetCenterLineCoordinates();
                int numPoints = coords.Length / 3;

                if (ModelIO.IsPeriodic)
                {
                    Model = Kernel.PeriodicRod.ErodPeriodicElasticRodBuild(numPoints, coords, Convert.ToInt32(ModelIO.RemoveRestCurvature), out Error);
                    EdgeCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetEdgesCount(Model);
                    NodesCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetVerticesCount(Model);
                }
                else
                {
                    Model = Kernel.ElasticRod.ErodElasticRodBuild(numPoints, coords, out Error);
                    EdgeCount = Kernel.ElasticRod.ErodElasticRodGetEdgesCount(Model);
                    NodesCount = Kernel.ElasticRod.ErodElasticRodGetVerticesCount(Model);
                    if (ModelIO.RemoveRestCurvature) Kernel.ElasticRod.ErodElasticRodRemoveRestCurvatures(Model);
                }

                if (Model != IntPtr.Zero) Init();
                else throw new Exception(Marshal.PtrToStringAnsi(Error));
            }
            catch (Exception e) { Console.Write(e.ToString()); }
        }

        public ElasticRod(ElasticRod model) : base((RodIO)model.ModelIO.Clone())
        {
            try
            {
                if (ModelIO.IsPeriodic)
                {
                    Model = Kernel.PeriodicRod.ErodPeriodicElasticRodCopy(model.Model, out Error);
                    EdgeCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetEdgesCount(Model);
                    NodesCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetVerticesCount(Model);
                }
                else
                {
                    Model = Kernel.ElasticRod.ErodElasticRodCopy(model.Model, out Error);
                    EdgeCount = Kernel.ElasticRod.ErodElasticRodGetEdgesCount(Model);
                    NodesCount = Kernel.ElasticRod.ErodElasticRodGetVerticesCount(Model);
                }

                if (Model != IntPtr.Zero) Init();
                else throw new Exception(Marshal.PtrToStringAnsi(Error));
            }
            catch(Exception e) { Console.Write(e.ToString()); }
        }

        public override object Clone()
        {
            return new ElasticRod(this);
        }

        protected override void Init()
        {
            InitMaterials();
            InitNodes();
            InitSupports();
            InitForces();
            InitMesh();
        }

        public override void SetMaterial(int sectionType, double E, double poisonRatio, double[] sectionParams, int axisType)
        {
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodSetMaterial(Model, sectionType, E, poisonRatio, sectionParams, sectionParams.Length, axisType);
            else Kernel.ElasticRod.ErodElasticRodSetMaterial(Model, sectionType, E, poisonRatio, sectionParams, sectionParams.Length, axisType);
        }

        private void InitMaterials()
        {
            int numMaterials = ModelIO.Materials.Count;
            if (numMaterials == 0) throw new Exception("No material has been assigned to the rod.");

            var mat = ModelIO.Materials[0];
            SetMaterial(mat.CrossSectionType, mat.E, mat.PoisonsRatio, mat.Parameters, mat.Orientation);
        }

        private void InitNodes()
        {
            _nodes = new PointCloud(GetCenterLinePositionsAsPoint3d());
        }

        private void UpdateNodes()
        {
            var pos = GetCenterLinePositionsAsPoint3d();
            for (int i=0; i<_nodes.Count; i++) _nodes[i].Location = pos[i];
        }

        private void UpdateSupportPositions()
        {
            for (int i = 0; i < ModelIO.Supports.Count; i++)
            {
                var sp = ModelIO.Supports[i];
                if (sp.IndexMap != -1) sp.UpdateReferencePosition(_nodes[sp.IndexMap].Location);
                sp.UpdateReferencePosition(_nodes[sp.IndexMap].Location);
                ModelIO.Supports[i] = sp;
            }
        }

        public override void InitMesh()
        {
            // Build visualization mesh
            double[] outCoords;
            int[] outQuads;
            GetMeshData(out outCoords, out outQuads);

            MeshVis = Helpers.GetQuadMesh(outCoords, outQuads);
        }

        protected override void GetMeshData(out double[] outCoords, out int[] outQuads)
        {
            int numCoords, numQuads;
            IntPtr cPtr, qPtr;
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetMeshData(Model, out cPtr, out qPtr, out numCoords, out numQuads);
            else Kernel.ElasticRod.ErodElasticRodGetMeshData(Model, out cPtr, out qPtr, out numCoords, out numQuads);

            outCoords = new double[numCoords];
            outQuads = new int[numQuads];
            Marshal.Copy(cPtr, outCoords, 0, numCoords);
            Marshal.Copy(qPtr, outQuads, 0, numQuads);
            Marshal.FreeCoTaskMem(cPtr);
            Marshal.FreeCoTaskMem(qPtr);
        }

        public void UpdateMesh()
        {
            double[] outCoords;
            int[] outQuads;
            GetMeshData(out outCoords, out outQuads);

            int vCount = outCoords.Length / 3;
            for (int i = 0; i < vCount; i++) MeshVis.Vertices.SetVertex(i, outCoords[i * 3], outCoords[i * 3 + 1], outCoords[i * 3 + 2]);
        }

        public override void Update()
        {
            UpdateMesh();
            UpdateNodes();
            UpdateSupportPositions();
        }

        public override int GetDoFCount()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetDoFCount(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetDoFCount(Model);
        }

        public void InitSupports()
        {
            // Check that at least one fix support exists
            if (ModelIO.Supports.Count == 0) ModelIO.AddCentralSupport();
            if (ModelIO.Supports.GetNumberFixSupport() == 0)
            {
                var sp = ModelIO.Supports[0];
                sp.IsTemporary = false;
                ModelIO.Supports[0] = sp;
            }


            for(int i= 0; i< ModelIO.Supports.Count; i++)
            {
                var sp = ModelIO.Supports[i];
                if (sp.IndexMap == -1)
                {
                    sp.IndexMap = _nodes.ClosestPoint(sp.ReferencePosition);
                    // Degrees of freedom ordering is:
                    // flattened centerline positions (x1, y1, z1, x2, y2, ...)
                    // followed by thetas (xx1, yy1, zz1, xx2, yy2, zz2, ...)
                    for (int j = 0; j < 3; j++) if(sp.LockedDoFs[j]) sp.SetIndexDoF(j, sp.IndexMap * 3 + j);
                }
                sp.UpdateReferencePosition(_nodes[sp.IndexMap].Location);
                ModelIO.Supports[i] = sp;
            }
        }

        public void InitForces()
        {
            var allForces = ModelIO.Forces;

            // Check for forces with unset reference positions. These will be global forces that are applied to all joints 
            var tempForces = allForces.Where(f => f.Positions[0] == Point3d.Unset);
            List<ForceIO> forces = new List<ForceIO>();
            if (tempForces.Count() != 0)
            {
                int nodeCount = ModelIO.IsPeriodic ? _nodes.Count - 2 : _nodes.Count;

                foreach (var f in tempForces)
                {
                    if (f is ForceExternalIO)
                    {
                        ForceExternalIO extForce = (ForceExternalIO)f;

                        for (int i = 0; i < _nodes.Count; i++)
                        {
                            ForceExternalIO newForce = new ForceExternalIO(_nodes[i].Location, extForce.Force);
                            newForce.SetIndexMap(0, i, false, new int[] { i * 3, i * 3 + 1, i * 3 + 2 });
                            forces.Add(newForce);
                        }
                    }
                }
            }

            tempForces = allForces.Where(f => f.Positions[0] != Point3d.Unset);
            foreach (var f in tempForces)
            {
                for (int i = 0; i < f.Positions.Length; i++)
                {
                    Point3d p = f.Positions[i];
                    int idx = _nodes.ClosestPoint(p);
                    f.SetIndexMap(i, idx, false, new int[] { idx * 3, idx * 3 + 1, idx * 3 + 2 });
                }
                forces.Add((ForceIO) f.Clone());
            }

            ModelIO.SetForces(forces);
        }

        public int GetThetaOffset()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetThetaOffset(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetThetaOffset(Model);
        }

        public int GetRestLengthOffset()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestLengthOffset(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetRestLengthOffset(Model);
        }

        public int GetRestKappaOffset()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestKappaOffset(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetRestKappaOffset(Model);
        }

        public double[] GetCenterLineCoordinates()
        {
            double[] coords = new double[NodesCount * 3];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetCenterLinePositions(Model, coords, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetCenterLinePositions(Model, coords, NodesCount);
            return coords;
        }

        public double[] GetRestLengths()
        {
            double[] restLengths = new double[EdgeCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestLengths(Model, restLengths, EdgeCount);
            else Kernel.ElasticRod.ErodElasticRodGetRestLengths(Model, restLengths, EdgeCount);
            return restLengths;
        }

        public double[] GetStretchingStresses()
        {
            double[] stresses = new double[EdgeCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetStretchingStresses(Model, stresses, EdgeCount);
            else Kernel.ElasticRod.ErodElasticRodGetStretchingStresses(Model, stresses, EdgeCount);
            return stresses;
        }

        public double[] GetTwistingStresses()
        {
            double[] stresses = new double[NodesCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetTwistingStresses(Model, stresses, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetTwistingStresses(Model, stresses, NodesCount);
            return stresses;
        }

        public double[] GetMaxBendingStresses()
        {
            double[] stresses = new double[NodesCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetMaxBendingStresses(Model, stresses, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetMaxBendingStresses(Model, stresses, NodesCount);
            return stresses;
        }

        public double[] GetMinBendingStresses()
        {
            double[] stresses = new double[NodesCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetMinBendingStresses(Model, stresses, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetMinBendingStresses(Model, stresses, NodesCount);
            return stresses;
        }

        public double[] GetSqrtBendingEnergies()
        {
            double[] stresses = new double[NodesCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetSqrtBendingEnergies(Model, stresses, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetSqrtBendingEnergies(Model, stresses, NodesCount);
            return stresses;
        }

        public double[] GetDoFs()
        {
            int numDoFs;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.ElasticRod) Kernel.ElasticRod.ErodElasticRodGetDoFs(Model, out cPtr, out numDoFs);
            else Kernel.PeriodicRod.ErodPeriodicElasticRodGetDoFs(Model, out cPtr, out numDoFs);

            double[] outDoFs = new double[numDoFs];
            Marshal.Copy(cPtr, outDoFs, 0, numDoFs);
            Marshal.FreeCoTaskMem(cPtr);
            return outDoFs;
        }

        public void SetDoFs(double[] dofs)
        {
            if (ModelType == ElasticModelType.ElasticRod) Kernel.ElasticRod.ErodElasticRodSetDoFs(Model, dofs, dofs.Length);
            else Kernel.PeriodicRod.ErodPeriodicElasticRodSetDoFs(Model, dofs, dofs.Length);
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

        public Plane[] GetMaterialFames()
        {
            IntPtr outCoordsD1, outCoordsD2;
            int outCoordsCount;

            if (ModelType == ElasticModelType.ElasticRod) Kernel.ElasticRod.ErodElasticRodGetMaterialFrame(Model, out outCoordsCount, out outCoordsD1, out outCoordsD2);
            else Kernel.PeriodicRod.ErodPeriodicElasticRodGetMaterialFrame(Model, out outCoordsCount, out outCoordsD1, out outCoordsD2);

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
                Point3d orig = (pts[i + 1] + pts[i]) * 0.5;
                frames[i] = new Plane(orig, x, y);
            }

            return frames;
        }

        public override double[] GetForceVars(bool includeExternalForces = true, bool includeCables=false)
        {
            if (ModelIO.Forces.Count == 0) return new double[0];

            double[] forceVars = new double[GetDoFCount()];

            foreach (ForceIO f in ModelIO.Forces)
            {
                if ((includeCables && f.ForceType == ForceIOType.Cable) ||
                    (includeExternalForces && f.ForceType == ForceIOType.External))
                {
                    int numPositions = f.NumPositions;
                    // Calculate forces using updated positions
                    Point3d[] pos = new Point3d[numPositions];
                    for (int i = 0; i < numPositions; i++) pos[i] = _nodes[f.Indices[i]].Location;
                    Vector3d[] forces = f.CalculateForce(pos);

                    // Set vars
                    for (int i = 0; i < numPositions; i++)
                    {
                        int[] indicesDoFs = f.IndicesDoFs[i];
                        for (int j = 0; j < indicesDoFs.Length; j++) forceVars[indicesDoFs[j]] += forces[i][j];
                    }
                }
            }

            return forceVars;
        }

        public override int[] GetFixedVars(bool includeTemporarySupports, double step = 1.0)
        {
            if (step < 0) step = 0;
            if (step > 1) step = 1;

            double[] dofs = GetDoFs();
            // Update positions of supports
            for(int i=0; i<ModelIO.Supports.Count; i++)
            {
                var sp = ModelIO.Supports[i];
                if (!sp.ContainsTarget) continue;

                // Compute linear interpolation between initial position and target position
                Line ln = new Line(sp.ReferencePosition, sp.TargetPosition);
                var pos = ln.PointAt(step);
                sp.VisualizationPosition = pos;

                // Only update dofs linked with the position
                int[] indicesDoFs = sp.IndicesDoFs;
                for (int j = 0; j < 3; j++) dofs[indicesDoFs[j]] = pos[j];
                ModelIO.Supports[i] = sp;
            }
            SetDoFs(dofs);

            return ModelIO.Supports.GetSupportsDoFsIndices(includeTemporarySupports);
        }

        public override int[] GetFixedVars(int numDeploymentSteps, int deploymentStep, double step = 1.0)
        {
            if (step < 0) step = 0.0;
            if (step > 1) step = 1.0;

            double[] dofs = GetDoFs();
            // Update positions of supports
            for(int i = 0; i < ModelIO.Supports.Count; i++)
            {
                var sp = ModelIO.Supports[i];

                if (!sp.ContainsTarget) continue;
                if (sp.IsTemporary && deploymentStep >= (int)Math.Floor(sp.ReleaseCoefficient * (numDeploymentSteps - 1))) continue;

                // Compute linear interpolation between initial position and target position
                Line ln = new Line(sp.ReferencePosition, sp.TargetPosition);
                var pos = ln.PointAt(step);
                sp.VisualizationPosition = pos;

                // Only update dofs linked with the position
                int[] indicesDoFs = sp.IndicesDoFs;
                for (int j = 0; j < 3; j++) dofs[indicesDoFs[j]] = pos[j];
                ModelIO.Supports[i] = sp;
            }
            SetDoFs(dofs);

            return ModelIO.Supports.GetSupportsDoFsIndices(numDeploymentSteps, deploymentStep);
        }

        public Mesh GetMesh()
        {
            double[] coords;
            int[] quads;
            GetMeshData(out coords, out quads);

            return Helpers.GetQuadMesh(coords, quads);
        }

        public override Line[] GetCablesAsLines()
        {
            {
                if (_modelIO.Forces.Count == 0) return new Line[0];

                return _modelIO.Forces.Where(f => f.ForceType == ForceIOType.Cable).Select(f => new Line(_nodes[f.Indices[0]].Location, _nodes[f.Indices[1]].Location)).ToArray();
            }
        }

        public double GetInitialMinRestLength()
        {
            if (ModelType == ElasticModelType.PeriodicRod) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetInitialMinRestLength(Model);
            else return Kernel.ElasticRod.ErodElasticRodGetInitialMinRestLength(Model);
        }

        public Curve GetInterpolatedCurve(int degree = 3)
        {
            return Curve.CreateInterpolatedCurve(GetCenterLinePositionsAsPoint3d(), degree);
        }

        public double[] GetRestKappas()
        {
            IntPtr ptrData;
            int numData;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestKappas(Model, out ptrData, out numData);
            else Kernel.ElasticRod.ErodElasticRodGetRestKappas(Model, out ptrData, out numData);

            double[] data = new double[numData];
            Marshal.Copy(ptrData, data, 0, numData);
            Marshal.FreeCoTaskMem(ptrData);
            return data;
        }

        public double[] GetScalarFieldSqrtBendingEnergies()
        {
            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(Model, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldSqrtBendingEnergies(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldMaxBendingStresses()
        {
            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldMaxBendingStresses(Model, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldMaxBendingStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        #region Stresses
        public double[] GetScalarFieldMinBendingStresses()
        {
            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldMinBendingStresses(Model, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldMinBendingStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldTwistingStresses()
        {
            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldTwistingStresses(Model, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldTwistingStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldStretchingStresses()
        {
            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldStretchingStresses(Model, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldStretchingStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }

        public double[] GetScalarFieldVonMisesStresses()
        {
            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldVonMisesStresses(Model, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldVonMisesStresses(Model, out cPtr, out numField);

            double[] outField = new double[numField];
            Marshal.Copy(cPtr, outField, 0, numField);
            Marshal.FreeCoTaskMem(cPtr);
            return outField;
        }
        #endregion
    }
}
