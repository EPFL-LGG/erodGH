using System;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using Grasshopper.Kernel.Types;
using Speckle.Core.Api;
using ErodDataLib.Types;
using Rhino;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Forms;
using Eto.Forms;
using Renci.SshNet;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Grasshopper.Kernel;

namespace ErodDataLib.Utils
{
    public class WeavingOpt
    {
        private SSHConnector _sshclient { get; set; }

        // Flat data
        public GH_Structure<GH_Point> flat_joint_positions { get; set; }
        public GH_Structure<GH_Integer> flat_joint_indexes { get; set; }
        public GH_Structure<GH_Integer> joints_topo { get; set; }
        public List<Polyline> flat_ribbons { get; set; }
        public List<Curve> flat_center_line { get; set; }
        public GH_Structure<GH_Point> flat_center_line_positions { get; set; }
        public Point3d[] deformed_all_joints { get; set; }
        public List<Mesh> deformed_mesh_ribbons { get; set; }
        public GH_Structure<GH_Point> deformed_joints_per_ribbon { get; set; }
        public string Log { get; set; }

        private string HOST = "gcmsrv1.epfl.ch";
        private int PORT = 22;
        private string USERNAME = "cs458";
        private string PASSWORD = "weavingplease";
        private string RUN_FOLDER = "Weaving/weaving/";
        private string INPUT_FOLDER = "Weaving/weaving/grasshopper/inputs/";
        private string OUTPUT_FOLDER = "Weaving/weaving/grasshopper/outputs/";
        private string PICKLE_FOLDER = "Weaving/weaving/optimization_diagram_results/";
        public RodLinkageData Data { get; set; }

        public WeavingOpt()
        {
            _sshclient = new SSHConnector(HOST, PORT, USERNAME, PASSWORD);
            Log = "";

            // Init data
            flat_center_line_positions = new GH_Structure<GH_Point>();
            flat_joint_positions = new GH_Structure<GH_Point>();
            flat_joint_indexes = new GH_Structure<GH_Integer>();
            joints_topo = new GH_Structure<GH_Integer>();
            flat_ribbons = new List<Polyline>();
            deformed_all_joints = new Point3d[0];
            deformed_mesh_ribbons = new List<Mesh>();
            deformed_joints_per_ribbon = new GH_Structure<GH_Point>();
        }

        public string RunOptimization(string filename, bool deleteCache=false)
        {
            var tempFileName = Path.GetTempFileName();
            tempFileName = tempFileName.Split(new char[] { '.' })[0] + ".json";

            _sshclient.Connect();

            // Generate input file
            string inputFile = INPUT_FOLDER + filename + ".json";
            _sshclient.SendJsonObject(Data, inputFile);

            // Run optimization
            if (deleteCache)
            {
                string removeFolder = "rm -rf " + PICKLE_FOLDER + filename;
                Log = _sshclient.ExecuteCommand(removeFolder);
            }
            string activateEnvironment = "source miniconda3/bin/activate && conda activate weave && ";
            string runPython = "cd " + RUN_FOLDER + " && python gcmaker_gh.py " + filename;
            Log = _sshclient.ExecuteCommand(activateEnvironment + runPython);

            _sshclient.ReceiveJsonFile(OUTPUT_FOLDER + filename + "_optimization.json", tempFileName);
            _sshclient.Disconnect();

            ParseOptimizationDataFromJson(tempFileName);
            // Delete the temporary file
            File.Delete(tempFileName);

            return Log;
        }

        public void RunAsyncOptimization(GH_Component component, string filename, bool deleteCache = false)
        {
            var form = new System.Windows.Forms.Form();
            form.Text = "Weaving Optimization";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Size = new System.Drawing.Size(600, 400);
            form.TopMost = true;

            var textBox = new System.Windows.Forms.TextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Text = "Starting weaving optimization...\n\n";

            // Add the text box to the window
            form.Controls.Add(textBox);
            form.Show();
            form.BringToFront();

            var outputThread = new Thread(() =>
            {
                var outputBuilder = new StringBuilder();
                ConnectionInfo connectionInfo = new ConnectionInfo(HOST, USERNAME,
                new AuthenticationMethod[]
                {
                        new PasswordAuthenticationMethod(USERNAME, PASSWORD),
                });

                using (var sshclient = new SshClient(connectionInfo))
                {
                    var tempFileName = Path.GetTempFileName();
                    tempFileName = tempFileName.Split(new char[] { '.' })[0] + ".json";

                    sshclient.Connect();

                    // Generate input file
                    string inputFile = INPUT_FOLDER + filename + ".json";
                    SSHConnector.SendJsonObjectToClient(connectionInfo, Data, inputFile);

                    // Run optimization
                    if (deleteCache)
                    {
                        string removeFolder = "rm -rf " + PICKLE_FOLDER + filename;
                        sshclient.RunCommand(removeFolder);
                    }
                    string activateEnvironment = "source miniconda3/bin/activate && conda activate weave && ";
                    string runPython = "cd " + RUN_FOLDER + " && python gcmaker_gh.py " + filename;

                    ///////////////////////////////////////////////////////////////
                    ///////////////////////////////////////////////////////////////
                    var command = sshclient.CreateCommand(activateEnvironment + runPython);
                    //The CommandTimeout property sets a timeout for the execution of the SSH command, after which the command will be aborted and an exception will be thrown.
                    command.CommandTimeout = TimeSpan.FromMinutes(10);
                    var asyncResult = command.BeginExecute();

                    // Read and print the output of the command as it becomes available
                    var buffer = new byte[8192];
                    while (!asyncResult.IsCompleted)
                    {
                        var count = command.OutputStream.Read(buffer, 0, buffer.Length);
                        if (count > 0)
                        {
                            var output = System.Text.Encoding.UTF8.GetString(buffer, 0, count);
                            outputBuilder.Append(output);
                            textBox.Invoke(new Action(() =>
                            {
                                textBox.AppendText(output);
                                textBox.SelectionStart = textBox.Text.Length;
                                textBox.ScrollToCaret();
                            }));
                        }
                        Thread.Sleep(10); // Adjust the delay as needed
                    }

                    ///////////////////////////////////////////////////////////////
                    ///////////////////////////////////////////////////////////////

                    // Get the result of the command as a string and append it to the outputBuilder
                    var result = command.Result;
                    outputBuilder.Append(result);
                    textBox.Invoke(new Action(() =>
                    {
                        textBox.AppendText(result);
                        textBox.SelectionStart = textBox.Text.Length;
                        textBox.ScrollToCaret();
                    }));
                    Log = outputBuilder.ToString();

                    SSHConnector.ReceiveJsonFileToClient(connectionInfo, OUTPUT_FOLDER + filename + "_optimization.json", tempFileName);
                    ParseOptimizationDataFromJson(tempFileName);
                    // Delete the temporary file
                    File.Delete(tempFileName);

                    // Close the form
                    form.Invoke(new Action(() => form.Close()));

                    sshclient.Disconnect();

                    component.Message = "Done!";
                    component.ExpireSolution(true);
                }
            });

            outputThread.Start();
        }

        private void ParseOptimizationDataFromJson(string filename)
        {
            JObject data = JObject.Parse(File.ReadAllText(filename));

            // Init data
            flat_joint_positions = new GH_Structure<GH_Point>();
            flat_joint_indexes = new GH_Structure<GH_Integer>();
            joints_topo = new GH_Structure<GH_Integer>();
            flat_ribbons = new List<Polyline>();
            deformed_all_joints = new Point3d[(int)data["number_joints"]];
            deformed_mesh_ribbons = new List<Mesh>();
            deformed_joints_per_ribbon = new GH_Structure<GH_Point>();
            flat_center_line = new List<Curve>();

            // Intersections
            var vData = data["crossings"];
            for (int i = 0; i < vData.Count(); i++)
            {
                var index_list = vData[i]["ribbons"];
                foreach (var idx in index_list) joints_topo.Append(new GH_Integer((int)idx), new GH_Path(i));
            }

            // Flat ribbons outlines
            vData = data["flat_ribbon_outlines"];
            foreach (var seg in vData)
            {
                List<Point3d> pts = new List<Point3d>();
                foreach (var pos in seg) pts.Add(new Point3d((double)pos[0], (double)pos[1], (double)pos[2]));
                pts.Add(pts[0]);
                flat_ribbons.Add(new Polyline(pts));
            }

            vData = data["flat_center_line"];
            for(int i=0; i< vData.Count(); i++)
            {
                var seg = vData[i];
                List<Point3d> pts = new List<Point3d>();
                foreach (var pos in seg)
                {
                    Point3d p = new Point3d((double)pos[0], (double)pos[1], (double)pos[2]);
                    pts.Add(p);
                    flat_center_line_positions.Append(new GH_Point(p), new GH_Path(i));
                }
                flat_center_line.Add(Curve.CreateInterpolatedCurve(pts,3));
            }

            // Flat joint positions
            vData = data["flat_joint_positions"];
            for (int i = 0; i < vData.Count(); i++)
            {
                List<Point3d> pts = new List<Point3d>();
                var joint_list = vData[i];
                var index_list = data["flat_joint_indexes"][i];
                GH_Path path = new GH_Path(i);
                for (int j = 0; j < joint_list.Count(); j++)
                {
                    var p = joint_list[j];
                    flat_joint_positions.Append(new GH_Point(new Point3d((double)p[0], (double)p[1], (double)p[2])), path);
                    flat_joint_indexes.Append(new GH_Integer((int)index_list[j]), path);
                }
            }

            // Deformed ribbons
            int numRibbons = data["deformed_rod_vertices"].Count();
            for (int i = 0; i < numRibbons; i++)
            {
                Mesh ribbon_mesh = new Mesh();
                GH_Path path = new GH_Path(i);

                int numSegments = data["deformed_rod_vertices"][i].Count();
                for (int j = 0; j < numSegments; j++)
                {

                    Mesh segment_mesh = new Mesh();

                    // Vertices
                    var vertices = data["deformed_rod_vertices"][i][j];
                    for (int k = 0; k < vertices.Count(); k++)
                    {
                        var p = vertices[k];
                        segment_mesh.Vertices.Add(new Point3d((double)p[0], (double)p[1], (double)p[2]));
                    }
                    // Faces
                    var faces = data["deformed_rod_quad_faces"][i][j];
                    for (int k = 0; k < faces.Count(); k++)
                    {
                        var f = faces[k];
                        segment_mesh.Faces.AddFace(new MeshFace((int)f[0], (int)f[1], (int)f[2], (int)f[3]));
                    }
                    ribbon_mesh.Append(segment_mesh);

                    // Joints
                    var jt = data["deformed_joints"][i][j];
                    Point3d jt_pos = new Point3d((double)jt[0], (double)jt[1], (double)jt[2]);
                    int jt_idx = (int)data["flat_joint_indexes"][i][j];
                    deformed_joints_per_ribbon.Append(new GH_Point(jt_pos), path);
                    deformed_all_joints[jt_idx] = jt_pos;
                }

                deformed_mesh_ribbons.Add(ribbon_mesh);
            }
        }
    }
}

