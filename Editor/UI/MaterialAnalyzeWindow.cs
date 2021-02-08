using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UTJ.MaliocPlugin.DB;
using UnityEngine.UIElements;

namespace UTJ.MaliocPlugin.UI
{
    public class MaterialAnalyzeWindow : EditorWindow
    {

        private Material mat;
        private string res;
        private List<ShaderKeywordInfo> programKeyInfo = new List<ShaderKeywordInfo>();

        [MenuItem("Tools/MaterialAnalyze")]
        public static void Create()
        {
            EditorWindow.GetWindow<MaterialAnalyzeWindow>();
        }

        private void OnEnable()
        {
            this.rootVisualElement.Add(new IMGUIContainer(this.OnGUITest));
        }

        private void OnGUITest()
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
            if (GUILayout.Button("Visual"))
            {
                var data = ShaderDbUtil.LoadShaderData(mat.shader);
                if( data != null)
                {
                    var keywords = MaliocPluginUtility.GetMaterialCurrentKeyword(mat);
                    programKeyInfo.Clear();
                    data.GetShaderMatchPrograms(programKeyInfo, keywords);

                    foreach( var key in programKeyInfo)
                    {
                        var info = data.GetProgramInfo(key);
                        var ve = ShaderInfolElement.Create(key,info,data.GetPassInfos());
                        this.rootVisualElement.Add(ve);
                       // info.positionVertPerf);
                    }
                }
            }
         }
    }
}