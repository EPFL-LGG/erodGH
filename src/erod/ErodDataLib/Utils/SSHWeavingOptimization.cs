using System;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using Grasshopper.Kernel.Types;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using Renci.SshNet;
using Grasshopper.Kernel;

namespace ErodDataLib.Utils
{
    public class SSHWeavingOptimization
    {
        private SSHServerID _server { get; set; }

        public string Log { get; set; }
        public JsonWeaving DataIO { get; set; }
        public GH_Structure<GH_Integer> JointIndices { get; set; }
        public GH_Structure<GH_Point> Joints2D { get; set; }
        public GH_Structure<GH_Point> Joints3D { get; set; }
        public Point3d[] AllJoints2D { get; set; }
        public GH_Structure<GH_Integer> JointsTopology { get; set; }
        public List<Mesh> Ribbons { get; set; }
        public List<Polyline> RibbonContours { get; set; }

        public List<Curve> flat_center_line { get; set; }
        public GH_Structure<GH_Point> flat_center_line_positions { get; set; }
        public GH_Structure<GH_Curve> DeformedCenterlines { get; set; }


        public SSHWeavingOptimization()
        {
            Log = "";

            // Init data
            flat_center_line_positions = new GH_Structure<GH_Point>();
            Joints2D = new GH_Structure<GH_Point>();
            JointIndices = new GH_Structure<GH_Integer>();
            JointsTopology = new GH_Structure<GH_Integer>();
            RibbonContours = new List<Polyline>();
            AllJoints2D = new Point3d[0];
            Ribbons = new List<Mesh>();
            Joints3D = new GH_Structure<GH_Point>();
            DeformedCenterlines = new GH_Structure<GH_Curve>();
        }

        public int RunAsyncOptimization(GH_Component component, SSHServerID server, string filename, bool deleteCache = false)
        {
            try
            {
                _server = server;
                string cleanFilename = filename.Replace(" ", "");
                Log = "";

                // This information should be consistent with the information in the Python script running on the server. 
                string inputFolder = _server.RunFolder + "grasshopper/inputs/";
                string outputFolder = _server.RunFolder + "grasshopper/outputs/";
                string pickleFolder = _server.RunFolder + "grasshopper/optimization_diagram_results/";

                int error = 0;
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
                textBox.AppendText("Input Folder: " + inputFolder + "\n");
                textBox.AppendText("Ouput Folder: " + outputFolder + "\n");
                textBox.AppendText("Pickle Folder: " + pickleFolder + "\n\n");

                // Add the text box to the window
                form.Controls.Add(textBox);
                form.Show();
                form.BringToFront();

                var outputThread = new Thread(() =>
                {
                    var outputBuilder = new StringBuilder();
                    outputBuilder.Append("[PATHS]\n");
                    outputBuilder.Append("Input Folder: " + inputFolder + "\n");
                    outputBuilder.Append("Ouput Folder: " + outputFolder + "\n");
                    outputBuilder.Append("Pickle Folder: " + pickleFolder + "\n\n");

                    ConnectionInfo connectionInfo = new ConnectionInfo(_server.Host, _server.Username,
                    new AuthenticationMethod[]
                    {
                        new PasswordAuthenticationMethod(_server.Username, _server.Password),
                    });

                    using (var sshclient = new SshClient(connectionInfo))
                    {
                        var tempFileName = Path.GetTempFileName();
                        tempFileName = tempFileName.Split(new char[] { '.' })[0] + ".json";

                        ///////////////////////////////////////////////////////////////
                        /// Establishing server connection
                        ///////////////////////////////////////////////////////////////
                        try
                        {
                            sshclient.Connect();
                            outputBuilder.Append("Server connection established!\n");
                            outputBuilder.Append("Sending weaving data...\n");
                        }
                        catch (Renci.SshNet.Common.SshOperationTimeoutException e)
                        {
                            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.ToString());
                            form.Invoke(new Action(() => form.Close()));
                            error = 1;
                            return;
                        }
                        catch (Renci.SshNet.Common.SshAuthenticationException e)
                        {
                            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.ToString());
                            form.Invoke(new Action(() => form.Close()));
                            error = 1;
                            return;
                        }

                        // Generate input file
                        string inputFile = inputFolder + cleanFilename + ".json";
                        if (!SSHConnector.SendJsonObjectToClient(connectionInfo, DataIO, inputFile, ref outputBuilder))
                        {
                            Log = outputBuilder.ToString();

                            sshclient.Disconnect();

                            component.Message = "Done with errors!";
                            component.ExpireSolution(true);
                            error = 1;

                            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error sending information.");
                            return;
                        }

                        // Optional: Deleting cache
                        if (deleteCache)
                        {
                            string removeFolder = "rm -rf " + pickleFolder + cleanFilename;
                            sshclient.RunCommand(removeFolder);
                            outputBuilder.Append("Cache deleted.\n");
                        }

                        ///////////////////////////////////////////////////////////////
                        /// Calling optimization
                        ///////////////////////////////////////////////////////////////
                        SshCommand command; 
                        try
                        {
                            string activateEnvironment = "source miniconda3/bin/activate && conda activate " + _server.CondaEnvironment + " && ";
                            string runPython = "cd " + _server.RunFolder + " && python gh_optimizer.py " + cleanFilename;

                            command = sshclient.CreateCommand(activateEnvironment + runPython);
                            outputBuilder.Append("Starting optimization: Initializing weaving model.\n");
                        }
                        catch(Renci.SshNet.Common.SshException e)
                        {
                            Log = outputBuilder.ToString();

                            sshclient.Disconnect();

                            component.Message = "Done with errors!";
                            component.ExpireSolution(true);
                            error = 1;

                            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.ToString());
                            return;
                        }

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
                        bool succeed = SSHConnector.ReceiveJsonFileToClient(connectionInfo, outputFolder + cleanFilename + "_optimization.json", tempFileName);

                        if (succeed)
                        {
                            ParseOptimizationDataFromJson(tempFileName);
                            File.Delete(tempFileName);
                        }

                        // Close the form
                        form.Invoke(new Action(() => form.Close()));

                        sshclient.Disconnect();

                        component.Message = "Done!";
                        component.ExpireSolution(true);

                        if (!succeed) component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Failed to run optimization!");
                    }
                });

                outputThread.Start();
                return error;
            }
            catch (Exception e)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.ToString());
                return 1;
            }
        }

        private void ParseOptimizationDataFromJson(string filename)
        {
            try
            {
                JObject data = JObject.Parse(File.ReadAllText(filename));

                // Init data
                Joints2D = new GH_Structure<GH_Point>();
                JointIndices = new GH_Structure<GH_Integer>();
                JointsTopology = new GH_Structure<GH_Integer>();
                RibbonContours = new List<Polyline>();
                //deformed_all_joints = new Point3d[(int)data["number_joints"]];
                Ribbons = new List<Mesh>();
                Joints3D = new GH_Structure<GH_Point>();
                flat_center_line = new List<Curve>();

                // Intersections
                var vData = data["crossings"];
                for (int i = 0; i < vData.Count(); i++)
                {
                    var index_list = vData[i]["ribbons"];
                    foreach (var idx in index_list) JointsTopology.Append(new GH_Integer((int)idx), new GH_Path(i));
                }

                // Flat ribbons outlines
                vData = data["flat_ribbon_outlines"];
                foreach (var seg in vData)
                {
                    List<Point3d> pts = new List<Point3d>();
                    foreach (var pos in seg) pts.Add(new Point3d((double)pos[0], (double)pos[1], (double)pos[2]));
                    pts.Add(pts[0]);
                    RibbonContours.Add(new Polyline(pts));
                }

                vData = data["flat_center_line"];
                for (int i = 0; i < vData.Count(); i++)
                {
                    var seg = vData[i];
                    List<Point3d> pts = new List<Point3d>();
                    foreach (var pos in seg)
                    {
                        Point3d p = new Point3d((double)pos[0], (double)pos[1], (double)pos[2]);
                        pts.Add(p);
                        flat_center_line_positions.Append(new GH_Point(p), new GH_Path(i));
                    }
                    flat_center_line.Add(Curve.CreateInterpolatedCurve(pts, 3));
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
                        Joints2D.Append(new GH_Point(new Point3d((double)p[0], (double)p[1], (double)p[2])), path);
                        JointIndices.Append(new GH_Integer((int)index_list[j]), path);
                    }
                }

                // Deformed ribbons
                AllJoints2D = new Point3d[ (int)data["number_joints"] ];
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
                    }

                    int numJoints = data["deformed_joints"][i].Count();
                    for (int j = 0; j < numJoints; j++)
                    {
                        // Joints
                        var jt = data["deformed_joints"][i][j];
                        Point3d jt_pos = new Point3d((double)jt[0], (double)jt[1], (double)jt[2]);
                        int jt_idx = (int)data["flat_joint_indexes"][i][j];
                        Joints3D.Append(new GH_Point(jt_pos), path);
                        AllJoints2D[jt_idx] = jt_pos;
                    }

                    Ribbons.Add(ribbon_mesh);
                }

                // Deformed centerlines
                DeformedCenterlines = new GH_Structure<GH_Curve>();
                numRibbons = data["deformed_center_line"].Count();
                for (int i = 0; i < numRibbons; i++)
                {
                    var seg_data = data["deformed_center_line"][i];
                    int numSegments = seg_data.Count();
                    GH_Curve[] segments = new GH_Curve[numSegments];

                    for (int j = 0; j < numSegments; j++)
                    {
                        var pts_data = seg_data[j];
                        int numPts = pts_data.Count();
                        Point3d[] pts = new Point3d[numPts];
                        for (int k = 0; k < numPts; k++) pts[k] = new Point3d((double)pts_data[k][0], (double)pts_data[k][1], (double)pts_data[k][2]);
                        segments[j] = new GH_Curve(new PolylineCurve(pts));
                    }

                    DeformedCenterlines.AppendRange(segments, new GH_Path(i));
                }
            }catch(IndexOutOfRangeException e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}

