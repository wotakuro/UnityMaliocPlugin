using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UTJ.MaliocPlugin.UI
{
    public class FrameDebugAnalyzeWindow : EditorWindow
    {
        [MenuItem("Tools/FrameDebugPos")]
        public static void Create()
        {
            EditorWindow.GetWindow<FrameDebugAnalyzeWindow>();
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Frame" + FrameDebuggerUtility.GetCurrentFramePosition());
            var data = FrameDebuggerUtility.GetCurrentData();
            if(data != null)
            {
                EditorGUILayout.LabelField(data.shaderName);
                EditorGUILayout.LabelField(data.passLightMode);
                EditorGUILayout.LabelField(data.shaderKeywords);
            }
        }
    }

    class FrameDebuggerUtility
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