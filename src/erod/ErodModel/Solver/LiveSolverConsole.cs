using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ErodModel.Solver
{
    // This thread owns only WinForms controls and copied log text, never Rhino
    // geometry or Grasshopper data. Solver callbacks only enqueue bounded text.
    internal sealed class LiveSolverConsole : IDisposable
    {
        private readonly MacSolverConsole mac;
        internal static bool IsSupported => Environment.OSVersion.Platform == PlatformID.Win32NT || File.Exists("/System/Library/CoreServices/SystemVersion.plist");
        internal string Failure => mac?.Failure;
        private readonly ConcurrentQueue<string> pending = new ConcurrentQueue<string>();
        private int queued;
        private volatile bool closeRequested, closed;
        internal bool IsClosed => mac != null ? mac.IsClosed : closed || closeRequested;
        internal LiveSolverConsole(string title)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) { mac = new MacSolverConsole(title); return; }
            var thread = new Thread(() => Run(title)) { IsBackground = true, Name = "ERod live solver report" };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        internal void Write(string text)
        {
            if (mac != null) { mac.Write(text); return; }
            if (IsClosed || String.IsNullOrEmpty(text)) return;
            pending.Enqueue(text);
            if (Interlocked.Increment(ref queued) > 10000) {
                string ignored;
                while (Volatile.Read(ref queued) > 10000 && pending.TryDequeue(out ignored)) Interlocked.Decrement(ref queued);
            }
        }
        public void Dispose() { if (mac != null) mac.Dispose(); else closeRequested = true; }
        private void Run(string title)
        {
            try {
                using (var font = new Font(FontFamily.GenericMonospace, 9f))
                using (var window = new Form { Text = title, Width = 1000, Height = 480, StartPosition = FormStartPosition.CenterScreen })
                using (var text = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Dock = DockStyle.Fill, Font = font, MaxLength = 2000000 })
                using (var timer = new System.Windows.Forms.Timer { Interval = 75 }) {
                    window.Controls.Add(text);
                    timer.Tick += (sender, args) => {
                        if (closeRequested) { window.Close(); return; }
                        var batch = new StringBuilder(); string line;
                        for (int i = 0; i < 500 && pending.TryDequeue(out line); ++i) {
                            Interlocked.Decrement(ref queued);
                            batch.Append(line.Replace("\r\n", "\n").Replace("\n", Environment.NewLine));
                        }
                        if (batch.Length == 0) return;
                        if (text.TextLength + batch.Length > 1000000) {
                            text.Select(0, Math.Min(text.TextLength, 250000));
                            text.SelectedText = String.Empty;
                        }
                        text.AppendText(batch.ToString());
                        text.SelectionStart = text.TextLength;
                        text.ScrollToCaret();
                    };
                    if (closeRequested) return;
                    timer.Start();
                    Application.Run(window);
                }
            }
            catch { /* A display failure must not terminate the host process. */ }
            finally {
                closed = true;
                string ignored;
                while (pending.TryDequeue(out ignored)) Interlocked.Decrement(ref queued);
            }
        }
    }
}
