using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UTJ.MaliocPlugin.UI
{
    public class MaterialAnalyzeWindow : EditorWindow
    {

        private Material mat;

        [MenuItem("Tools/MaterialAnalyze")]
        public static void Create()
        {
            EditorWindow.GetWindow<MaterialAnalyzeWindow>();
        }

        private void OnGUI()
        {
            mat = EditorGUILayout.ObjectField(mat, typeof(Material), true) as Material;

        }
    }
}