using System;
using ErodModelLib.Types;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using System.Collections.Generic;
using System.Linq;
using ErodDataLib.Types;
using Rhino.Geometry;

namespace ErodModel.Model
{
	public class SliderActuationGH : GH_Component
	{
        private bool run;
        private RodLinkage copy;
        private ConvergenceReport report;
        private NewtonSolverOpts options;
        double[] refT;
        int[][] incidentSegments;
        Curve[] refCrv;
        private readonly double THRESHOLD_SLIDING = 0.05;
        private List<int> refJointIndices;
        private List<bool> refIsFamiliesAs;
        double minWidth;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SliderActuationGH()
          : base("SlidingActuation", "SlidingActuation",
            "Linkage deployment via sliding actuation at selected joints.",
            "Erod", "Solvers")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Linkage model to deploy.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Opts", "Opts", "Newton solver options.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Compute equilibrium.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Restart computation.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("JointIndices", "JointIds", "Indices of the joints to slide.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("SlideAlongA", "SlideAlongA", "Sets whether to slide the joint along ribbons of family A or B.", GH_ParamAccess.list);
            pManager.AddNumberParameter("SlidingStep","SlidingStep", "Sets the signed step to modify the position of the joint. A joint can be positioned within at most two segments of the same family. The value should be between -1.0 and 1.0.", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Linkage", "Linkage", "Deployed linkage model.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Report", "Report", "Convergence report", GH_ParamAccess.item);
            pManager.AddNumberParameter("RestLengths", "RestLengths", "Updated rest-length for each segment", GH_ParamAccess.item);
        }

        protected override void AfterSolveInstance()
        {
            if (run)
            {
                GH_Document document = base.OnPingDocument();
                if (document != null)
                {
                    GH_Document.GH_ScheduleDelegate callback = new GH_Document.GH_ScheduleDelegate(this.ScheduleCallback);
                    document.ScheduleSolution(1, callback);
                }
            }
            else this.Message = "Stop";
        }

        private void ScheduleCallback(GH_Document doc)
        {
            this.ExpireSolution(false);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RodLinkage model = null;
            bool reset = false;
            run = false;
            options = new NewtonSolverOpts(20, 1);
            List<int> jointIndices = new List<int>();
            List<bool> isFamiliesAs = new List<bool>();
            List<double> slidingStep = new List<double>();

            DA.GetData(0, ref model);
            DA.GetData(1, ref options);
            DA.GetData(2, ref run);
            DA.GetData(3, ref reset);
            DA.GetDataList(4, jointIndices);
            DA.GetDataList(5, isFamiliesAs);
            DA.GetDataList(6, slidingStep);

            if (model.ContainsTemporarySupports() && !model.ContainsRollingSupports()) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Temporary supports detected. This solver only operates with permanent supports. Temporary supports will be disabled.");
            if (model.ContainsRollingSupports() && !model.ContainsTemporarySupports()) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Rolling supports detected. This solver only operates with fixed supports. Rolling supports will be fixed.");
            if (model.ContainsTemporarySupports() && model.ContainsRollingSupports()) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Rolling and temporary supports detected. This solver only operates with fixed and permanent supports. Rolling supports will be fixed and temporary supports will be disabled.");
            if (!model.ModelIO.Layout.ContainsLayoutData()) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The linkage should contain a ribbon layout to identify families.");

            if (reset || copy == null ||
                refJointIndices==null || !refJointIndices.SequenceEqual(jointIndices) ||
                refIsFamiliesAs==null || !refIsFamiliesAs.SequenceEqual(isFamiliesAs) )
            {
                this.Message = "Reset";
                copy = (RodLinkage)model.Clone();
                report = new ConvergenceReport();

                // Store reference paremeters for joints and incident segments to modify
                minWidth = copy.ModelIO.Materials.GetMaxWidth()*1.5;
                int numJoints = jointIndices.Count;
                refJointIndices = jointIndices;
                refIsFamiliesAs = isFamiliesAs;

                refT = new double[numJoints];
                incidentSegments = new int[numJoints][];
                refCrv = new Curve[numJoints];

                for (int i = 0; i < jointIndices.Count; i++)
                {
                    // Get the joint
                    int jIdx = jointIndices[i];
                    var jt = copy.Joints[jIdx];

                    // Get the incident segments
                    int nIdx = copy.ModelIO.Graph.GetClosestNode(jt.Position);
                    bool isFamilyA = i < isFamiliesAs.Count() ? isFamiliesAs[i] : isFamiliesAs.Last();
                    incidentSegments[i] = copy.ModelIO.Graph.GetSortedIncidentEdges(nIdx).Where(idx => copy.ModelIO.Segments[idx].EdgeLabel == (isFamilyA ? SegmentLabels.RodA : SegmentLabels.RodB)).ToArray();

                    // Build one single curve and calculate the parameter on the curve where the joint is located
                    Curve crv = Curve.JoinCurves(incidentSegments[i].Select(idx => copy.ModelIO.Segments[idx].GetUnderlyingCurve()))[0];
                    crv.Domain = new Interval(0, 1);
                    refCrv[i] = crv;

                    double t;
                    crv.ClosestPoint(jt.Position, out t);
                    refT[i] = t;
                }
            }

            if (run)
            {
                this.Message = "Computing";
                // Get the rest lengths per segment
                double[] dofs = copy.GetRestLenghtsSolveDoFs();
                int offsetIndex = dofs.Length - copy.Segments.Count;

                for (int i = 0; i < jointIndices.Count; i++)
                {
                    Curve crv = refCrv[i];

                    // Signed sliding step
                    double tStep = i < slidingStep.Count() ? slidingStep[i] : slidingStep.Last();

                    int[] segments = incidentSegments[i];
                    var pos = crv.PointAt(refT[i]);

                    if (segments.Length > 2) throw new Exception("Joint " + jointIndices[i] + " has a valence higher than 4");
                    if(segments.Length < 1) throw new Exception("Joint " + jointIndices[i] + " has a valence lower than 1");

                    // Manage different cases
                    if (segments.Length == 1)
                    {
                        double t0 = refT[i];
                        double tStepAbs = Math.Abs(tStep);
                        if (t0 <= 1e-6) t0 = tStepAbs > 1.0 - THRESHOLD_SLIDING ? 1.0 - THRESHOLD_SLIDING : tStepAbs;
                        else if (t0 >= 1.0 - 1e-6) t0 = tStepAbs < THRESHOLD_SLIDING ? THRESHOLD_SLIDING : 1-tStepAbs;

                        int sIdx = segments[0];
                        var c = copy.ModelIO.Segments[sIdx].GetUnderlyingCurve();
                        double t1;
                        crv.ClosestPoint(c.PointAtStart.DistanceTo(pos) < c.PointAtEnd.DistanceTo(pos) ? c.PointAtEnd : c.PointAtStart, out t1);
                        double l = crv.GetLength(t1 > t0 ? new Interval(t0, t1) : new Interval(t1, t0));
                        dofs[offsetIndex + sIdx] = l < minWidth ? minWidth : l;
                    }

                    if (segments.Length == 2)
                    {
                        double t0 = refT[i] + (tStep*0.5);
                        // To avoid collisions with other joints set a minimum value
                        if (t0 < THRESHOLD_SLIDING) t0 = THRESHOLD_SLIDING;
                        if (t0 > 1.0 - THRESHOLD_SLIDING) t0 = 1.0 - THRESHOLD_SLIDING;

                        int sIdx = segments[0];
                        var c = copy.ModelIO.Segments[sIdx].GetUnderlyingCurve();
                        double t1;
                        crv.ClosestPoint(c.PointAtStart.DistanceTo(pos) < c.PointAtEnd.DistanceTo(pos) ? c.PointAtEnd : c.PointAtStart, out t1);
                        double l = crv.GetLength(t1 > t0 ? new Interval(t0, t1) : new Interval(t1, t0));
                        if (l < minWidth) l = minWidth;
                        else if (l > crv.GetLength() - minWidth) l = crv.GetLength() - minWidth;
                        dofs[offsetIndex + sIdx] = l;

                        sIdx = segments[1];
                        dofs[offsetIndex + sIdx] = crv.GetLength()- l;
                    }
                }

                // Set the new rest lengths per segment
                copy.SetRestLenghtsSolveDoFs(dofs);

                double[] forces = copy.GetForceVars(options.IncludeForces);
                int[] supports = copy.GetFixedVars(false, 0.0);

                NewtonSolver.Optimize(copy, supports, forces, options, out report);
            }

            DA.SetData(0, copy);
            DA.SetData(1, report);
            DA.SetDataList(2, copy.GetPerSegmentRestLenghts());
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.Resources.slider_actuation;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("77d5fd50-ac43-467f-a4ae-ccfdfec8f074"); }
        }
    }
}

