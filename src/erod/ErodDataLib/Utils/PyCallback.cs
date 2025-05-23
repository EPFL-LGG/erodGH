﻿using System;
using System.Diagnostics;
using System.IO;

namespace ErodDataLib.Utils
{
    public static class PyCallback
    {
        public static bool CallScript(string environment, string scriptName, string inputFile)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            //python interpreter location
            start.FileName = @environment;

            //argument with file name and input parameters
            start.Arguments = string.Format("{0} {1}", scriptName, inputFile);
            start.UseShellExecute = false;// Do not use OS shell
            start.CreateNoWindow = true; // We don't need new window
            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)
            start.LoadUserProfile = true;

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string errMsg = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                    if (errMsg.Length > 0) throw new Exception(errMsg);
                    else return true;
                }
            }
        }
    }
}
