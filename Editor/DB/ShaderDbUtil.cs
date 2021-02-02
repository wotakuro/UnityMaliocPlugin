using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UTJ.MaliocPlugin.Result;

namespace UTJ.MaliocPlugin.DB
{
    public class ShaderDbUtil : MonoBehaviour
    {
        private static readonly string COMPILED_FILE_PATH = "Library/com.utj.malioc.plugin/compiled/";
        private static readonly string DB_FILE_PATH = "Library/com.utj.malioc.plugin/db/";

        public static ShaderInfo Create(Shader shader,CompiledShaderParser compiledShaderParser)
        {
            ShaderInfo info = new ShaderInfo();
            info.shaderName = shader.name;
            var programs = compiledShaderParser.GetShaderPrograms();

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
                info.AddProgramInfo(key,shaderProgramInfo);
            }

            InitDirectory(DB_FILE_PATH);
            info.SaveToFile( Path.Combine(DB_FILE_PATH, 
                shader.name.Replace('/', '_')+".json") );
            return info;
        }

        private static void CreateCompiledFiles(string dir,CompiledShaderParser compiledShaderParser)
        {
            int idx = 0;
            var programs = compiledShaderParser.GetShaderPrograms();
            InitDirectory(dir);
            StringBuilder pathSb = new StringBuilder(128);
            StringBuilder infoSb = new StringBuilder(1024);
            int count = programs.Count;
            infoSb.Append("{\"keywords\":[\n");
            foreach (var program in programs)
            {
                infoSb.Append("  {\n    \"global\":\"").Append(program.globalKeyword).Append("\",\n");
                infoSb.Append("    \"local\":\"").Append(program.globalKeyword).Append("\"\n  }");
                if( idx == count - 1)
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

        private static void InitDirectory(string dir)
        {
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}