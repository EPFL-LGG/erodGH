using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ErodModel.Solver
{
    // Terminal is a separate viewer process: no Cocoa/Eto controls are created
    // on a worker thread and no Rhino message pumping or reentrancy is required.
    internal sealed class MacSolverConsole : IDisposable
    {
        private readonly ConcurrentQueue<string> pending = new ConcurrentQueue<string>();
        private readonly AutoResetEvent wake = new AutoResetEvent(false);
        private readonly Action<string> launch;
        private readonly string title;
        private int queued;
        private volatile bool closeRequested, closed;
        internal bool IsClosed => closeRequested || closed;
        private volatile string failure;
        internal string Failure => failure;
        internal string LogPath { get; }
        internal string ScriptPath { get; }
        internal string StopPath { get; }
        internal MacSolverConsole(string title, Action<string> launcher = null)
        {
            this.title = title;
            launch = launcher ?? OpenTerminal;
            string directory = Path.Combine(Path.GetTempPath(), "ERod-reports", Guid.NewGuid().ToString("N"));
            LogPath = Path.Combine(directory, "report.log");
            ScriptPath = Path.Combine(directory, "ERod-report.command");
            StopPath = Path.Combine(directory, "closed");
            new Thread(Run) { IsBackground = true, Name = "ERod macOS report writer" }.Start();
        }
        internal void Write(string text)
        {
            if (IsClosed || String.IsNullOrEmpty(text)) return;
            pending.Enqueue(text);
            if (Interlocked.Increment(ref queued) > 10000) {
                string ignored;
                while (Volatile.Read(ref queued) > 10000 && pending.TryDequeue(out ignored)) Interlocked.Decrement(ref queued);
            }
            try { wake.Set(); } catch (ObjectDisposedException) { }
        }
        public void Dispose()
        {
            closeRequested = true;
            try { wake.Set(); } catch (ObjectDisposedException) { }
        }
        private void Run()
        {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
                using (var stream = new FileStream(LogPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true }) {
                    writer.WriteLine(title);
                    writer.WriteLine("Closing Terminal stops viewing the report, not the solver.");
                    File.WriteAllText(ScriptPath, ReaderScript(LogPath, StopPath, Process.GetCurrentProcess().Id), new UTF8Encoding(false));
                    if (!closeRequested) launch(ScriptPath);
                    while (!closeRequested) {
                        Drain(writer, stream);
                        wake.WaitOne(75);
                    }
                    while (!pending.IsEmpty) Drain(writer, stream);
                    writer.WriteLine("Report stream closed.");
                }
            }
            catch (Exception error) { failure = error.Message; }
            finally {
                // The reader stops without closing any unrelated Terminal tab.
                try { if (Directory.Exists(Path.GetDirectoryName(StopPath))) File.WriteAllText(StopPath, String.Empty); } catch { }
                closed = true;
                string ignored;
                while (pending.TryDequeue(out ignored)) Interlocked.Decrement(ref queued);
                wake.Dispose();
            }
        }
        private void Drain(StreamWriter writer, FileStream stream)
        {
            string text;
            for (int i = 0; i < 500 && pending.TryDequeue(out text); ++i) {
                Interlocked.Decrement(ref queued);
                if (stream.Length > 2 * 1024 * 1024) {
                    writer.Flush(); stream.SetLength(0); stream.Position = 0;
                    writer.WriteLine("Earlier log history discarded.");
                }
                writer.Write(text);
            }
        }
        internal static string ShellQuote(string value) { return "'" + value.Replace("'", "'\\''") + "'"; }
        internal static string ReaderScript(string log, string stop, int owner)
        {
            if (owner < 0) throw new ArgumentOutOfRangeException(nameof(owner));
            return "#!/bin/sh\n" +
                "log=" + ShellQuote(log) + "\nstop=" + ShellQuote(stop) + "\nowner=" + owner + "\n" +
                "/usr/bin/tail -n +1 -f \"$log\" &\ntail_pid=$!\n" +
                "trap 'kill \"$tail_pid\" 2>/dev/null || :; wait \"$tail_pid\" 2>/dev/null || :' EXIT\ntrap 'exit 0' HUP INT TERM\n" +
                "while kill -0 \"$owner\" 2>/dev/null && kill -0 \"$tail_pid\" 2>/dev/null; do\n" +
                "  if [ -e \"$stop\" ]; then /bin/sleep 1.2; break; fi\n  /bin/sleep 0.2\ndone\n";
        }
        private static void OpenTerminal(string script)
        {
            // Feed code via stdin, quoting paths as literal shell arguments.
            // LaunchServices opens the local executable .command in Terminal;
            // no AppleScript automation permission or Python runtime is needed.
            using (var process = new Process { StartInfo = new ProcessStartInfo("/bin/sh") {
                UseShellExecute = false, RedirectStandardInput = true, RedirectStandardError = true, CreateNoWindow = true } }) {
                process.Start();
                process.StandardInput.Write("/bin/chmod 700 " + ShellQuote(script) + " && exec /usr/bin/open -a Terminal " + ShellQuote(script) + "\n");
                process.StandardInput.Close();
                if (!process.WaitForExit(5000)) { process.Kill(); throw new IOException("Terminal launch timed out. Report: " + script); }
                if (process.ExitCode != 0) throw new IOException("Could not open Terminal: " + process.StandardError.ReadToEnd());
            }
        }
    }
}
