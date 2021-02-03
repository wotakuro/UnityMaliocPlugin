using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UTJ.MaliocPlugin.UI
{
    public class MaterialAnalyzeWindow : EditorWindow
    {

        private Material mat;
        private string res;

        [MenuItem("Tools/MaterialAnalyze")]
        public static void Create()
        {
            EditorWindow.GetWindow<MaterialAnalyzeWindow>();
        }

        private void OnGUI()
        {
            mat = EditorGUILayout.ObjectField(mat, typeof(Material), true) as Material;

            if (GUILayout.Button("DebugKeyword"))
            {
                var keywords = MaliocPluginUtility.GetMaterialCurrentKeyword(mat);

                res = "Keyword:";
                foreach (var keyword in keywords)
                {
                    res += keyword + "\n";
                }
            }
            if (!string.IsNullOrEmpty(res))
            {
                EditorGUILayout.TextArea(res);
            }
        }
    }
}