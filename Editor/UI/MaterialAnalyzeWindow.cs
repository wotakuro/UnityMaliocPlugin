using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UTJ.MaliocPlugin.DB;
using ShaderInfo = UTJ.MaliocPlugin.DB.ShaderInfo;

namespace UTJ.MaliocPlugin.UI
{
    public class MaterialAnalyzeWindow : EditorWindow
    {

        private Material mat;
        private List<ShaderKeywordInfo> programKeyInfo = new List<ShaderKeywordInfo>();
        private ScrollView resultArea;

        [MenuItem("Tools/MaterialAnalyze")]
        public static void Create()
        {
            EditorWindow.GetWindow<MaterialAnalyzeWindow>();
        }

        private void OnEnable()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.utj.malioc.plugin/Editor/UI/UXML/MaterialView.uxml");
            var ve = asset.CloneTree();
            var objectFiled = ve.Q<ObjectField>("SelectMaterial");
            objectFiled.objectType = typeof(Material);
            objectFiled.RegisterValueChangedCallback(OnSetMaterial);
            var btn = ve.Q<Button>("AnalyzeBtn");
            btn.clickable.clicked += OnClickAnalyzeBtn;

            this.resultArea = ve.Q<ScrollView>("ResultArea");

            this.rootVisualElement.Add(ve);
        }
        private void OnSetMaterial(ChangeEvent<Object> evt)
        {
            this.mat = evt.newValue as Material;
        }

        private void OnClickAnalyzeBtn()
        {
            this.resultArea.Clear();
            if( mat == null || mat.shader == null)
            {
                return;
            }
            var data = ShaderDbUtil.LoadShaderData(mat.shader);
            if( data == null)
            {
                var compiled = CompileShaderUtil.GetCompileShaderText(mat.shader);
                var parser = new CompiledShaderParser(compiled);
                data = ShaderDbUtil.Create(mat.shader, parser);
            }
            SetResult(data);
        }

        private void SetResult(ShaderInfo data)
        {
            if (data != null)
            {
                var keywords = MaliocPluginUtility.GetMaterialCurrentKeyword(mat);
                programKeyInfo.Clear();
                data.GetShaderMatchPrograms(programKeyInfo, keywords);

                foreach (var key in programKeyInfo)
                {
                    var info = data.GetProgramInfo(key);
                    var ve = ShaderInfolElement.Create(key, info, data.GetPassInfos());
                    this.resultArea.Add(ve);
                }
            }
         }
    }
}