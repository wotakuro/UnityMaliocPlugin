using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using UTJ.MaliocPlugin.Result;

namespace UTJ.MaliocPlugin
{
    public class ProcessUtil
    {

        public static string CallMaliShaderOfflineCompiler(string file,bool jsonFormat) {
            string output = null;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "malioc";
                if (jsonFormat)
                {
                    process.StartInfo.Arguments = file + " --format json";
                }
                else
                {
                    process.StartInfo.Arguments = file + " --format text";
                }
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                // Synchronously read the standard output of the spawned process.
                StreamReader reader = process.StandardOutput;

                process.WaitForExit();
                output = reader.ReadToEnd();
            }
            return output;
        }
    }
}