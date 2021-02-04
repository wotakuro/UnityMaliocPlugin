using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using UTJ.MaliocPlugin.Result;
using System.Reflection;
using System.Text;

namespace UTJ.MaliocPlugin
{
    public class MaliocPluginUtility
    {

        public static string CallMaliShaderOfflineCompiler(string file,bool jsonFormat) {
            string output = null;
            StringBuilder sb = new StringBuilder(file.Length + 32);
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "malioc";
                if (jsonFormat)
                {
                    sb.Append("\"").Append(file).Append("\" --format json");
                    process.StartInfo.Arguments = sb.ToString();
                }
                else
                {
                    sb.Append("\"").Append(file).Append("\" --format text");
                    process.StartInfo.Arguments = sb.ToString();
                }
                process.StartInfo.CreateNoWindow = true;
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


        public bool IsMatchKeyword( string globalKeyword,string localKeyword,List<string> keywords)
        {
            // todo optimize
            var globals = globalKeyword.Split(' ');
            var locals = localKeyword.Split(' ');
            int count = globals.Length + locals.Length;

            if ( count != keywords.Count)
            {
                return false;
            }
            var list = new List<string>(count);
            list.AddRange(globals);
            list.AddRange(locals);

            list.Sort();
            keywords.Sort();

            for(int i = 0; i < count; ++i)
            {
                if(list[i] != keywords[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static List<string> GetMaterialCurrentKeyword(Material mat)
        {
            if (mat.shader == null) { return null; }
            var list = GetShaderCurrentKeyword(mat.shader);
            list.AddRange(mat.shaderKeywords);
            return list;
        }

        public static List<string> GetShaderCurrentKeyword(Shader shader)
        {
            var enableKeywords = GetAllEnableKeywords();
            var result = new List<string>();
            GetShaderCurrentKeyword(result, shader, enableKeywords);
            return result;
        }

        public static void GetShaderCurrentKeyword(List<string> result,
            Shader shader,
            List<string> enableKeywords)
        {
            result.Clear();
            var globalKeywords = GetShaderGlobalKeywords(shader);
            var localKeywords = GetShaderLocalKeywords(shader);
            HashSet<string> shaderKeywords = new HashSet<string>();
            foreach (var keyword in globalKeywords)
            {
                shaderKeywords.Add(keyword);
            }
            foreach (var keyword in localKeywords)
            {
                shaderKeywords.Add(keyword);
            }

            foreach (var keyword in enableKeywords)
            {
                if (shaderKeywords.Contains(keyword))
                {
                    result.Add(keyword);
                }
            }
        }
     
        
        public static List<string> GetAllEnableKeywords()
        {
            var list = new List<string>();
            GetAllEnableKeywords(list,GetAllKeywords() );
            return list;
        }

        public static void GetAllEnableKeywords(List<string> result, string[] allKeywords)
        {
            result.Clear();
            foreach (var keyword in allKeywords)
            {
                if (Shader.IsKeywordEnabled(keyword))
                {
                    result.Add(keyword);
                }
            }
        }

        public static string[] GetAllKeywords()
        {
            var bindFlag = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            var method = typeof(ShaderUtil).GetMethod("GetAllGlobalKeywords", bindFlag);
            if (method == null)
            {
                return null;
            }
            var ret = method.Invoke(null, null);
            return ret as string[];
        }

        public static string[] GetShaderGlobalKeywords(Shader shader)
        {
            var bindFlag = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            var method = typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", bindFlag);
            if (method == null)
            {
                return null;
            }
            var ret = method.Invoke(null, new object[] { shader });
            return ret as string[];
        }
        public static string[] GetShaderLocalKeywords(Shader shader)
        {
            var bindFlag = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            var method = typeof(ShaderUtil).GetMethod("GetShaderLocalKeywords", bindFlag);
            if (method == null)
            {
                return null;
            }
            var ret = method.Invoke(null, new object[] { shader });
            return ret as string[];
        }

    }
}