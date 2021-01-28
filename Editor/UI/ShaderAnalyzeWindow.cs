using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UTJ.MaliocPlugin.Result;
using UTJ.MaliocPlugin.DB;

namespace UTJ.MaliocPlugin.UI
{
    public class ShaderAnalyzeWindow : EditorWindow
    {
        private Shader shader;
        string res;
        [MenuItem("Tools/ShaderAnalyze")]
        public static void Create()
        {
            EditorWindow.GetWindow<ShaderAnalyzeWindow>();
        }

        private void OnGUI()
        {
            shader = EditorGUILayout.ObjectField(shader, typeof(Shader), true) as Shader;

            if (GUILayout.Button("Compile And Analyze"))
            {
                var compiled = CompileShaderUtil.GetCompileShaderText(shader);
                var parser = new CompiledShaderParser(compiled);

                ShaderDbUtil.Create(shader, parser);
            }
            if (!string.IsNullOrEmpty(res))
            {
                EditorGUILayout.TextArea(res);
            }
        }
    }
}
