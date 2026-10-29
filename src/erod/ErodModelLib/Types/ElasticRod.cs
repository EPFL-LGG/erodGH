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
                    Handle = Kernel.PeriodicRod.ErodPeriodicElasticRodBuild(numPoints, coords, Convert.ToInt32(ModelIO.RemoveRestCurvature), out Error);
                    if (Model == IntPtr.Zero) throw new InvalidOperationException("Native construction failed");
                    EdgeCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetEdgesCount(Handle);
                    NodesCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetVerticesCount(Handle);
                }
                else
                {
                    Handle = Kernel.ElasticRod.ErodElasticRodBuild(numPoints, coords, out Error);
                    if (Model == IntPtr.Zero) throw new InvalidOperationException("Native construction failed");
                    EdgeCount = Kernel.ElasticRod.ErodElasticRodGetEdgesCount(Handle);
                    NodesCount = Kernel.ElasticRod.ErodElasticRodGetVerticesCount(Handle);
                    if (ModelIO.RemoveRestCurvature) Kernel.ElasticRod.ErodElasticRodRemoveRestCurvatures(Handle);
                }

                if (Model != IntPtr.Zero) Init();
                else throw new Exception(Marshal.PtrToStringAnsi(Error));
            }
            catch { Dispose(); throw; }
        }

        public ElasticRod(ElasticRod model) : base((RodIO)model.ModelIO.Clone())
        {
            try
            {
                if (ModelIO.IsPeriodic)
                {
                    Handle = Kernel.PeriodicRod.ErodPeriodicElasticRodCopy(model.Handle, out Error);
                    if (Model == IntPtr.Zero) throw new InvalidOperationException("Native construction failed");
                    EdgeCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetEdgesCount(Handle);
                    NodesCount = Kernel.PeriodicRod.ErodPeriodicElasticRodGetVerticesCount(Handle);
                }
                else
                {
                    Handle = Kernel.ElasticRod.ErodElasticRodCopy(model.Handle, out Error);
                    if (Model == IntPtr.Zero) throw new InvalidOperationException("Native construction failed");
                    EdgeCount = Kernel.ElasticRod.ErodElasticRodGetEdgesCount(Handle);
                    NodesCount = Kernel.ElasticRod.ErodElasticRodGetVerticesCount(Handle);
                }

                if (Model != IntPtr.Zero) Init();
                else throw new Exception(Marshal.PtrToStringAnsi(Error));
            }
            catch { Dispose(); throw; }
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
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodSetMaterial(Handle, sectionType, E, poisonRatio, sectionParams, sectionParams.Length, axisType);
            else Kernel.ElasticRod.ErodElasticRodSetMaterial(Handle, sectionType, E, poisonRatio, sectionParams, sectionParams.Length, axisType);
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
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numCoords, numQuads;
            IntPtr cPtr, qPtr;
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetMeshData(Handle, out cPtr, out qPtr, out numCoords, out numQuads);
            else Kernel.ElasticRod.ErodElasticRodGetMeshData(Handle, out cPtr, out qPtr, out numCoords, out numQuads);

            outCoords = new double[numCoords];
            outQuads = new int[numQuads];
            NativeBufferScope.Copy(cPtr, outCoords, 0, numCoords);
            NativeBufferScope.Copy(qPtr, outQuads, 0, numQuads);
            Kernel.FreeNativeBuffer(cPtr);
            Kernel.FreeNativeBuffer(qPtr);

            }
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
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetDoFCount(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetDoFCount(Handle);
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
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetThetaOffset(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetThetaOffset(Handle);
        }

        public int GetRestLengthOffset()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestLengthOffset(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetRestLengthOffset(Handle);
        }

        public int GetRestKappaOffset()
        {
            if (ModelIO.IsPeriodic) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestKappaOffset(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetRestKappaOffset(Handle);
        }

        public double[] GetCenterLineCoordinates()
        {
            double[] coords = new double[NodesCount * 3];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetCenterLinePositions(Handle, coords, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetCenterLinePositions(Handle, coords, NodesCount);
            return coords;
        }

        public double[] GetRestLengths()
        {
            double[] restLengths = new double[EdgeCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestLengths(Handle, restLengths, EdgeCount);
            else Kernel.ElasticRod.ErodElasticRodGetRestLengths(Handle, restLengths, EdgeCount);
            return restLengths;
        }

        public double[] GetStretchingStresses()
        {
            double[] stresses = new double[EdgeCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetStretchingStresses(Handle, stresses, EdgeCount);
            else Kernel.ElasticRod.ErodElasticRodGetStretchingStresses(Handle, stresses, EdgeCount);
            return stresses;
        }

        public double[] GetTwistingStresses()
        {
            double[] stresses = new double[NodesCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetTwistingStresses(Handle, stresses, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetTwistingStresses(Handle, stresses, NodesCount);
            return stresses;
        }

        public double[] GetMaxBendingStresses()
        {
            double[] stresses = new double[NodesCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetMaxBendingStresses(Handle, stresses, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetMaxBendingStresses(Handle, stresses, NodesCount);
            return stresses;
        }

        public double[] GetMinBendingStresses()
        {
            double[] stresses = new double[NodesCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetMinBendingStresses(Handle, stresses, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetMinBendingStresses(Handle, stresses, NodesCount);
            return stresses;
        }

        public double[] GetSqrtBendingEnergies()
        {
            double[] stresses = new double[NodesCount];
            if (ModelIO.IsPeriodic) Kernel.PeriodicRod.ErodPeriodicElasticRodGetSqrtBendingEnergies(Handle, stresses, NodesCount);
            else Kernel.ElasticRod.ErodElasticRodGetSqrtBendingEnergies(Handle, stresses, NodesCount);
            return stresses;
        }

        public double[] GetDoFs()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numDoFs;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.ElasticRod) Kernel.ElasticRod.ErodElasticRodGetDoFs(Handle, out cPtr, out numDoFs);
            else Kernel.PeriodicRod.ErodPeriodicElasticRodGetDoFs(Handle, out cPtr, out numDoFs);

            double[] outDoFs = new double[numDoFs];
            NativeBufferScope.Copy(cPtr, outDoFs, 0, numDoFs);
            Kernel.FreeNativeBuffer(cPtr);
            return outDoFs;

            }
        }

        public void SetDoFs(double[] dofs)
        {
            if (ModelType == ElasticModelType.ElasticRod) Kernel.ElasticRod.ErodElasticRodSetDoFs(Handle, dofs, dofs.Length);
            else Kernel.PeriodicRod.ErodPeriodicElasticRodSetDoFs(Handle, dofs, dofs.Length);
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
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr outCoordsD1, outCoordsD2;
            int outCoordsCount;

            if (ModelType == ElasticModelType.ElasticRod) Kernel.ElasticRod.ErodElasticRodGetMaterialFrame(Handle, out outCoordsCount, out outCoordsD1, out outCoordsD2);
            else Kernel.PeriodicRod.ErodPeriodicElasticRodGetMaterialFrame(Handle, out outCoordsCount, out outCoordsD1, out outCoordsD2);

            double[] coordsD1 = new double[outCoordsCount];
            double[] coordsD2 = new double[outCoordsCount];
            NativeBufferScope.Copy(outCoordsD1, coordsD1, 0, outCoordsCount);
            NativeBufferScope.Copy(outCoordsD2, coordsD2, 0, outCoordsCount);
            Kernel.FreeNativeBuffer(outCoordsD1);
            Kernel.FreeNativeBuffer(outCoordsD2);

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
            if (ModelType == ElasticModelType.PeriodicRod) return Kernel.PeriodicRod.ErodPeriodicElasticRodGetInitialMinRestLength(Handle);
            else return Kernel.ElasticRod.ErodElasticRodGetInitialMinRestLength(Handle);
        }

        public Curve GetInterpolatedCurve(int degree = 3)
        {
            return Curve.CreateInterpolatedCurve(GetCenterLinePositionsAsPoint3d(), degree);
        }

        public double[] GetRestKappas()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            IntPtr ptrData;
            int numData;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetRestKappas(Handle, out ptrData, out numData);
            else Kernel.ElasticRod.ErodElasticRodGetRestKappas(Handle, out ptrData, out numData);

            double[] data = new double[numData];
            NativeBufferScope.Copy(ptrData, data, 0, numData);
            Kernel.FreeNativeBuffer(ptrData);
            return data;

            }
        }

        public double[] GetScalarFieldSqrtBendingEnergies()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldSqrtBendingEnergies(Handle, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldSqrtBendingEnergies(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldMaxBendingStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldMaxBendingStresses(Handle, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldMaxBendingStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        #region Stresses
        public double[] GetScalarFieldMinBendingStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldMinBendingStresses(Handle, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldMinBendingStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldTwistingStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldTwistingStresses(Handle, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldTwistingStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldStretchingStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldStretchingStresses(Handle, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldStretchingStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }

        public double[] GetScalarFieldVonMisesStresses()
        {
            using (var nativeBuffers = new NativeBufferScope())
            {

            int numField;
            IntPtr cPtr;
            if (ModelType == ElasticModelType.PeriodicRod) Kernel.PeriodicRod.ErodPeriodicElasticRodGetScalarFieldVonMisesStresses(Handle, out cPtr, out numField);
            else Kernel.ElasticRod.ErodElasticRodGetScalarFieldVonMisesStresses(Handle, out cPtr, out numField);

            double[] outField = new double[numField];
            NativeBufferScope.Copy(cPtr, outField, 0, numField);
            Kernel.FreeNativeBuffer(cPtr);
            return outField;

            }
        }
        #endregion
    }
}
