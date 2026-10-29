using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ErodModel.Solver;

class Program
{
    static void Wait(Func<bool> condition, string failure) {
        if (!SpinWait.SpinUntil(condition, 10000)) throw new Exception(failure);
    }
    static string ReadLog(string path) {
        if(!File.Exists(path))return String.Empty;
        using(var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
        using(var reader=new StreamReader(stream))return reader.ReadToEnd();
    }
    static void MacBackend() {
        string launched=null;
        using(var console=new MacSolverConsole("ERod mac backend test", script=>launched=script)) {
            console.Write("0\t1.25\n");
            Wait(()=>launched!=null && ReadLog(console.LogPath).Contains("1.25"),"Mac log did not stream before disposal");
            if(!File.ReadAllText(launched).StartsWith("#!/bin/sh\n"))throw new Exception("Invalid Terminal script encoding");
            for(int i=0;i<1500;i++)console.Write("row "+i+"\n");
            console.Dispose();
            Wait(()=>File.Exists(console.StopPath),"Mac viewer stop signal missing");
            string text=ReadLog(console.LogPath);
            if(!text.Contains("row 1499") || !text.Contains("Report stream closed."))throw new Exception("Pending rows lost on disposal");
            if(console.Failure!=null)throw new Exception(console.Failure);
        }
        using(var console=new MacSolverConsole("Failure test", script=> { throw new IOException("Controlled launch failure"); })) {
            Wait(()=>console.IsClosed,"Failed Terminal launch was not contained");
            if(console.Failure==null || !console.Failure.Contains("Controlled"))throw new Exception("Missing launch diagnostics");
        }
        using(var console=new MacSolverConsole("Bounded history",script=>{})) {
            for(int i=0;i<1500;i++)console.Write(new string('x',4096)+"\n");
            console.Dispose();
            Wait(()=>File.Exists(console.StopPath),"History test did not finish");
            if(new FileInfo(console.LogPath).Length>2*1024*1024+8192)throw new Exception("Mac history was not bounded");
        }
        if(MacSolverConsole.ShellQuote("a'b $HOME")!="'a'\\''b $HOME'")throw new Exception("Unsafe shell quoting");
        Console.WriteLine("PASS Mac backend streams, drains on disposal, bounds history, signals reader shutdown and contains launch failures");
    }
    static int Main(string[] args) {
        if(args.Length==3 && args[0]=="--reader-script") {
            Console.Write(MacSolverConsole.ReaderScript(args[1],args[2],0));return 0;
        }
        try {
            MacBackend();
            const string title = "ERod live report validation";
            using(var console = new LiveSolverConsole(title)) {
                console.Write("Iteration\tEnergy\n0\t1.25\n");
                // This thread does not pump UI messages. The separate window must
                // still initialize and consume progress on its own STA thread.
                Thread.Sleep(350);
                Form window = null;
                Wait(()=> { window=Application.OpenForms.Cast<Form>().FirstOrDefault(f=>f.Text==title);return window!=null && window.IsHandleCreated; }, "Report window did not open independently");
                Wait(()=> (bool)window.Invoke(new Func<bool>(()=>window.Controls.OfType<TextBox>().Single().Text.Contains("1.25"))), "Live text was not displayed");
                Console.WriteLine("PASS live report paints while caller does not pump UI messages");
                for(int i=0;i<12000;i++)console.Write(i+"\t2.5\n");
                window.Invoke(new Action(()=>window.Close()));
                Wait(()=>console.IsClosed,"Closing report did not stop its display thread");
                console.Write("Ignored after closing\n");
                Console.WriteLine("PASS closing the window leaves caller running and rejects further display text");
            }
            using(var console = new LiveSolverConsole(title)) { console.Dispose(); }
            Wait(()=>Application.OpenForms.Count==0,"Report window remained open after disposal");
            Console.WriteLine("PASS early disposal and window cleanup");
            return 0;
        } catch(Exception e) { Console.Error.WriteLine(e);return 1; }
    }
}
