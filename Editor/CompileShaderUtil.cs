using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace UTJ.MaliocPlugin
{
    public class CompileShaderUtil
    {
        [Flags]
        public enum GraphicsAPI:int
        {
            None = 0,
            GLES3 = 1,
            Vulkan = 2,
        }
       
        public static string GetCompileShaderText(Shader shader,bool includeAllVariant=false)
        {
            string path;
            using (var scope = new DummyEditorOpenScope())
            {
                CompiledShader(shader, includeAllVariant);
                path = GetCompiledShaderPath(shader);
            }

            return File.ReadAllText(path);
        }
        private static void CompiledShader(Shader s, bool includeAllVariant)
        {
            var utilType = typeof(ShaderUtil);
            var flag = BindingFlags.NonPublic | BindingFlags.Static;
            var method = utilType.GetMethod("OpenCompiledShader", flag);
            int mode = 3;
            int externPlatformsMask = 0;

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
