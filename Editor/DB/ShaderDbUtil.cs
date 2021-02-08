using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UTJ.MaliocPlugin.Result;

namespace UTJ.MaliocPlugin.DB
{
    public class ShaderDbUtil 
    {
        private static readonly string COMPILED_FILE_PATH = "Library/com.utj.malioc.plugin/compiled/";
        private static readonly string DB_FILE_PATH = "Library/com.utj.malioc.plugin/db/";

        public static ShaderInfo Create(Shader shader,CompiledShaderParser compiledShaderParser)
        {
            ShaderInfo info = new ShaderInfo();
            info.shaderName = shader.name;
            var programs = compiledShaderParser.GetShaderPrograms();
            var passInfos = compiledShaderParser.GetPassInfos();

            string dir = Path.Combine(COMPILED_FILE_PATH, shader.name);
            // create compile files
            CreateCompiledFiles(dir, compiledShaderParser);
            for( int i = 0; i < programs.Count; ++i)
            {
                var vertJson = MaliocPluginUtility.CallMaliShaderOfflineCompiler(dir + "/" + i + ".vert", true);
                var fragJson = MaliocPluginUtility.CallMaliShaderOfflineCompiler(dir + "/" + i + ".frag", true);
                var vertResult = MaliOcReport.CreateFromJson(vertJson);
                var fragResult = MaliOcReport.CreateFromJson(fragJson);
                var shaderProgramInfo = ShaderProgramInfo.Create( vertResult , fragResult);

                var key = new ShaderKeywordInfo();
                key.globalKeyword = programs[i].globalKeyword;
                key.localKeyword = programs[i].localKeyword;
                key.passIndex = programs[i].passInfoIdx;
                info.AddProgramInfo(key,shaderProgramInfo);
            }

            for( int i = 0; i < passInfos.Count; ++i)
            {
                info.AddPassInfo(passInfos[i].name, passInfos[i].tags);
            }

            InitDirectory(DB_FILE_PATH);
            info.SaveToFile( Path.Combine(DB_FILE_PATH, 
                shader.name.Replace('/', '_')+".json") );
            return info;
        }

        public static ShaderInfo LoadShaderData(Shader shader)
        {
            string path = Path.Combine(DB_FILE_PATH,
                shader.name.Replace('/', '_') + ".json");
            return ShaderInfo.LoadFromFile(path);
        }

        private static void CreateCompiledFiles(string dir,CompiledShaderParser compiledShaderParser)
        {
            int idx = 0;
            var programs = compiledShaderParser.GetShaderPrograms();
            var passInfos = compiledShaderParser.GetPassInfos();
            InitDirectory(dir);
            StringBuilder pathSb = new StringBuilder(128);
            StringBuilder infoSb = new StringBuilder(1024);
            int programCount = programs.Count;
            int passCount = passInfos.Count;
            infoSb.Append("{");
            infoSb.Append("\"passInfo\":[\n");
            foreach(var passInfo in passInfos)
            {
                infoSb.Append("  {\n    \"name\":");
                AppendJsonStringValue(infoSb, passInfo.name).Append(",\n");

                infoSb.Append("\n    \"tags\":");
                AppendJsonStringValue(infoSb, passInfo.tags).Append("\n");

                infoSb.Append("  }");
                if (idx == passCount - 1)
                {
                    infoSb.Append("\n");
                }
                else
                {
                    infoSb.Append(",\n");
                }
                ++idx;
            }
            infoSb.Append("],\n");

            idx = 0;
            infoSb.Append("\"keywords\":[\n");
            foreach (var program in programs)
            {
                infoSb.Append("  {\n    \"global\":\"").Append(program.globalKeyword).Append("\",\n");
                infoSb.Append("    \"local\":\"").Append(program.globalKeyword).Append("\",\n");
                infoSb.Append("    \"passIdx\":\"").Append(program.passInfoIdx).Append("\"\n  }");
                if ( idx == programCount - 1)
                {
                    infoSb.Append("\n");
                }
                else
                {
                    infoSb.Append(",\n");
                }
                pathSb.Clear().Append(dir).Append('/').Append(idx).Append(".vert");
                File.WriteAllText( pathSb.ToString() , program.vertShader);
                pathSb.Clear().Append(dir).Append('/').Append(idx).Append(".frag");
                File.WriteAllText( pathSb.ToString(), program.fragShader);
                ++idx;
            }
            infoSb.Append("]}");
            pathSb.Clear().Append(dir).Append("/info.json");
            File.WriteAllText(pathSb.ToString(), infoSb.ToString() );
        }

        private static StringBuilder AppendJsonStringValue(StringBuilder sb,string val)
        {
            if (val != null)
            {
                sb.Append('\"').
                    Append(val.Replace("\"", "\\\"")).Append('\"');
            }
            else
            {
                sb.Append("null");
            }
            return sb;
        }

        private static void InitDirectory(string dir)
        {
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}