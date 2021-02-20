using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;
using UTJ.MaliocPlugin.DB;
using UnityEditor.UIElements;

namespace UTJ.MaliocPlugin.UI
{
    public class FrameDebugAnalyzeWindow : EditorWindow
    {
        private int currentPosition;
        private int resultPosition;

        private ScrollView resultArea;
        private Button analyzeBtn;
        private ObjectField shaderField;


        private Dictionary<Shader, AnalyzedShaderInfo> shaderInfos;
        private List<ShaderKeywordInfo> programKeyInfo = new List<ShaderKeywordInfo>();


        [MenuItem("Tools/MaliocPlugin/FrameDebugPos")]
        public static void Create()
        {
            EditorWindow.GetWindow<FrameDebugAnalyzeWindow>();
        }

        private void OnEnable()
        {
            resultPosition = -1;
            currentPosition = -1;

            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.utj.malioc.plugin/Editor/UI/UXML/FrameDebuggerAttach.uxml");
            var ve = asset.CloneTree();


            this.resultArea = ve.Q<ScrollView>("ResultArea");
            this.analyzeBtn = ve.Q<Button>("AnalyzeBtn");
            this.shaderField = ve.Q<ObjectField>("ShaderProp");

            this.analyzeBtn.clicked += OnClickAnalyzeBtn;
            this.rootVisualElement.Add(ve);


            var selectable = this.shaderField.Q<VisualElement>(null, "unity-object-field__selector");
            selectable.parent.Remove(selectable);
        }

        private void Update()
        {
            this.currentPosition = FrameDebuggerUtility.GetCurrentFramePosition();
            this.UpdateResultArea();
        }

        private void OnClickAnalyzeBtn()
        {
            var data = FrameDebuggerUtility.GetCurrentData();
            if (data == null)
            {
                return;
            }
            Shader shader = Shader.Find(data.shaderName);
            var info = CreateAnalyzedInfo(shader);
        }


        private void UpdateResultArea() {
            if ( this.resultPosition == this.currentPosition)
            {
                return;
            }
            this.resultArea.Clear();
            var data = FrameDebuggerUtility.GetCurrentData();
            if(data == null)
            {
                return;
            }
            if(data.frameEventIndex != this.currentPosition-1)
            {
                return;
            }
            Shader shader = Shader.Find(data.shaderName);
            if(shader == null) { return; }
            var info = GetAnalyzedShaderInfo(shader);
            AppendResult(info, data);

            Debug.Log("this.shaderField. " + this.shaderField.objectType);
            this.shaderField.objectType = typeof(Shader);
            this.shaderField.value = shader;
            //data.shaderKeywords
        }

        private void AppendResult(AnalyzedShaderInfo info, FrameDebuggerEventData data)
        {
            if (info == null)
            {
                return;
            }
            var keywords = MaliocPluginUtility.KeywordStrToList(data.shaderKeywords);
            info.GetShaderMatchPrograms(programKeyInfo, keywords);
            foreach (var key in programKeyInfo)
            {
                var shaderProgramInfo = info.GetProgramInfo(key);

                if (key.passIndex == data.shaderPassIndex)
                {
                    var ve = ShaderInfolElement.Create(key, shaderProgramInfo, info.GetPassInfos());
                    resultArea.Add(ve);
                    this.resultPosition = this.currentPosition;
                    this.Repaint();
                    break;
                }
            }

        }

        private AnalyzedShaderInfo GetAnalyzedShaderInfo(Shader shader)
        {
            AnalyzedShaderInfo info = null;
            if (this.shaderInfos == null)
            {
                this.shaderInfos = new Dictionary<Shader, AnalyzedShaderInfo>();
            }
            if( this.shaderInfos.TryGetValue(shader,out info)) {
                return info;
            }
            info = ShaderDbUtil.LoadShaderData(shader);
            if (info != null)
            {
                this.shaderInfos[shader] = info;
            }
            return info;
        }
        private AnalyzedShaderInfo CreateAnalyzedInfo(Shader shader)
        {
            AnalyzedShaderInfo info = null;
            var compiled = CompileShaderUtil.GetCompileShaderText(shader);
            var parser = new CompiledShaderParser(compiled);
            info = ShaderDbUtil.Create(shader, parser);
            if (info != null )
            {
                this.shaderInfos[shader] = info;
            }
            return info;
        }
    }

    public class FrameDebuggerUtility
    {
        private static PropertyInfo framePositionProperty;
        private static ReflectionType frameDebuggeUtil;
        private static ReflectionType frameEventData;
        private static ReflectionCache reflectionCache;

        static FrameDebuggerUtility()
        {
            reflectionCache = new ReflectionCache();
            frameDebuggeUtil = reflectionCache.GetTypeObject("UnityEditorInternal.FrameDebuggerUtility");
            framePositionProperty = frameDebuggeUtil.GetPropertyInfo("limit");
            frameEventData = reflectionCache.GetTypeObject("UnityEditorInternal.FrameDebuggerEventData");

        }

        public static int GetCurrentFramePosition()
        {
            return (int)framePositionProperty.GetValue(null);
        }
        public static FrameDebuggerEventData GetCurrentData()
        {
            FrameDebuggerEventData data = null;
            ReflectionClassWithObject obj = null;
            int currentPos = GetCurrentFramePosition() - 1;
            if (GetFrameEventData(currentPos, out obj))
            {
                data = new FrameDebuggerEventData();
                obj.CopyFieldsToObjectByVarName<FrameDebuggerEventData>(ref data);
            }
            return data;
        }

        private static bool GetFrameEventData(int frameIdx, out ReflectionClassWithObject ret)
        {
            if (frameIdx < 0)
            {
                ret = null;
                return false;
            }
            object[] args = null;

            args = new object[] { frameIdx, frameEventData.CreateInstance() };
            bool result = frameDebuggeUtil.CallMethod<bool>("GetFrameEventData", null, args);
            if (result)
            {
                ret = new ReflectionClassWithObject(frameEventData, args[1]);
            }
            else
            {
                ret = null;
            }
            return result;
        }

    }

    public class FrameDebuggerEventData
    {
        public int frameEventIndex;
        public string shaderName;
        public string passName;
        public string passLightMode;
        public int shaderInstanceID;
        public int subShaderIndex;
        public int shaderPassIndex;
        public string shaderKeywords;

    }


}