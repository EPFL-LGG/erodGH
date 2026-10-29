using System;
using ErodModelLib.Types;
using Grasshopper.Kernel;

namespace ErodModel.Solver
{
    /// <summary>Shared live-report lifetime and solve forwarding for ERod solver components.</summary>
    public abstract class LiveReportSolverComponent : GH_Component
    {
        private LiveSolverConsole console;
        private bool liveReport;
        protected LiveReportSolverComponent(string name, string nickname, string description, string category, string subcategory)
            : base(name, nickname, description, category, subcategory) { }

        protected void AddLiveReportInput(GH_InputParamManager manager)
        {
            manager.AddBooleanParameter("Live report", "Log", "Show live iteration rows in a separate report window (Windows) or Terminal (macOS). Closing the window does not stop the solver.", GH_ParamAccess.item, false);
        }
        protected void ConfigureLiveReport(IGH_DataAccess data, int index, bool run, bool reset)
        {
            liveReport = false;
            data.GetData(index, ref liveReport);
            if (!liveReport || reset) CloseReport();
            if (liveReport && !LiveSolverConsole.IsSupported) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Live reports require Windows or macOS. Existing solver outputs remain available.");
                liveReport = false;
            }
            if (!run && console != null && console.IsClosed) CloseReport();
        }
        protected bool OptimizeWithLiveReport(ElasticModel model, int[] supports, double[] forces, NewtonSolverOpts options,
            out ConvergenceReport report, bool updateMesh = true, double deployedAngle = 0, bool lastStep = false, string context = null)
        {
            if (liveReport && console == null) {
                console = new LiveSolverConsole("ERod - " + Name + " report (" + InstanceGuid.ToString("N").Substring(0, 6) + ")");
                console.Write("Iteration\tEnergy\tGradient norm\tFree gradient norm\tStep length\tIndefinite\n");
            }
            Action<string> log = liveReport && console != null && !console.IsClosed ? (Action<string>)console.Write : null;
            try {
                console?.Write("--- " + (context ?? Name + " batch") + " ---\n");
                bool converged = NewtonSolver.Optimize(model, supports, forces, options, out report, log,
                    updateMesh, deployedAngle, lastStep);
                if (liveReport && !String.IsNullOrEmpty(console?.Failure)) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Live report: " + console.Failure);
                console?.Write(converged ? "Equilibrium reached.\n" : "Batch finished; equilibrium not yet reached.\n");
                return converged;
            }
            catch (Exception error) { console?.Write("ERROR: " + error.Message + "\n"); throw; }
        }
        private void CloseReport() { console?.Dispose(); console = null; }
        public override void DocumentContextChanged(GH_Document document, GH_DocumentContext context)
        {
            if (context == GH_DocumentContext.Close || context == GH_DocumentContext.Unloaded) CloseReport();
            base.DocumentContextChanged(document, context);
        }
        public override void RemovedFromDocument(GH_Document document)
        {
            CloseReport();
            base.RemovedFromDocument(document);
        }
    }
}
