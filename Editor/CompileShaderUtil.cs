using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace UTJ.MaliocPlugin
{
    public class CompileShaderUtil
    {
       
        public static string GetCompileShaderText(Shader shader)
        {
            CompiledShader(shader);
            var path = GetCompiledShaderPath(shader);

            return File.ReadAllText(path);
        }
        private static void CompiledShader(Shader s)
        {
            var utilType = typeof(ShaderUtil);
            var flag = BindingFlags.NonPublic | BindingFlags.Static;
            var method = utilType.GetMethod("OpenCompiledShader", flag);
            int mode = 3;
            int externPlatformsMask = 0;
            bool includeAllVariant = true;

            externPlatformsMask = (1 << (int)ShaderCompilerPlatform.GLES3x);
            //        externPlatformsMask = (int)ShaderCompilerPlatform.Vulkan;
            method.Invoke(null,
                new object[] { s, mode, externPlatformsMask, includeAllVariant });
        }
        private static string GetCompiledShaderPath(Shader s)
        {
            string path = "Temp/Compiled-" + s.name.Replace('/', '-') + ".shader";
            return path;
        }

    }
}
