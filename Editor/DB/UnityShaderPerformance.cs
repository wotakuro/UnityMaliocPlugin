using ICSharpCode.NRefactory.Ast;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UTJ.MaliocPlugin.Result;


namespace UTJ.MaliocPlugin.DB
{
    [Serializable]
    public class ShaderInfo
    {
        [SerializeField]
        public string shaderName;
        [SerializeField]
        public List<ShaderKeywordInfo> keywordInfos;
        [SerializeField]
        public List<ShaderProgramInfo> programInfos;
        [NonSerialized]
        public Dictionary<ShaderKeywordInfo, ShaderProgramInfo> performanceInfoByKeyword;

        public void AddProgramInfo(ShaderKeywordInfo info, ShaderProgramInfo programInfo)
        {
            if (keywordInfos == null) { keywordInfos = new List<ShaderKeywordInfo>(); }
            if (programInfos == null) { programInfos = new List<ShaderProgramInfo>(); }
            keywordInfos.Add(info);
            programInfos.Add(programInfo);
        }

        public void CreateDictionary()
        {
            if(keywordInfos == null) { return; }
            if(performanceInfoByKeyword == null) { performanceInfoByKeyword = new Dictionary<ShaderKeywordInfo, ShaderProgramInfo>(); }
            performanceInfoByKeyword.Clear();
            int length = keywordInfos.Count;
            for(int i = 0; i < length; ++i)
            {
                performanceInfoByKeyword.Add(keywordInfos[i], programInfos[i]);
            }
        }

        public void SaveToFile(string path)
        {
            string str = JsonUtility.ToJson(this);
            File.WriteAllText(path, str);
        }
        public static ShaderInfo LoadFromFile(string path) { 
            if(!File.Exists(path))
            {
                return null;
            }
            string str = File.ReadAllText(path);
            return JsonUtility.FromJson<ShaderInfo>(str);
        }
    }

    [Serializable]
    public class ShaderKeywordInfo
    {
        [SerializeField]
        public string globalKeyword;
        [SerializeField]
        public string localKeyword;

        public override bool Equals(object obj)
        {
            ShaderKeywordInfo keyInfo = obj as ShaderKeywordInfo;
            if (keyInfo == null)
            {
                return false;
            }
            return (this.globalKeyword == keyInfo.globalKeyword) &&
                (this.localKeyword == keyInfo.localKeyword );
        }
        public override int GetHashCode()
        {
            return globalKeyword.GetHashCode() + localKeyword.GetHashCode();
        }

    }

    [Serializable]
    public class ShaderProgramInfo
    {
        [SerializeField]
        public ShaderProgramPerfInfo positionVertPerf;
        [SerializeField]
        public ShaderProgramPerfInfo varyingVertPerf;
        [SerializeField]
        public ShaderProgramPerfInfo fragPerf;

        public static ShaderProgramInfo Create(MaliOcReport vertReport, MaliOcReport fragReport)
        {
            ShaderProgramInfo info = new ShaderProgramInfo();
            info.positionVertPerf = ShaderProgramPerfInfo.Create(vertReport, "Position");
            info.varyingVertPerf = ShaderProgramPerfInfo.Create(vertReport, "Varying");
            info.fragPerf = ShaderProgramPerfInfo.Create(fragReport, 0);
            return info;
        }
    }

    [Serializable]
    public class ShaderProgramPerfInfo
    {
        [SerializeField]
        public int workRegisters;
        [SerializeField]
        public int uniformRegister;
        [SerializeField]
        public int bit16Arithmetic;
        [SerializeField]
        public string[] pipelines;
        [SerializeField]
        public float[] totalCycles;
        [SerializeField]
        public float[] longestCycles;
        [SerializeField]
        public float[] shortestCycles;
        [SerializeField]
        public bool hasUniformComputation;
        [SerializeField]
        public bool hasSideEffects;
        [SerializeField]
        public bool modifisCoverage;
        [SerializeField]
        public bool useLateZSUpdate;
        [SerializeField]
        public bool useLateZSTest;
        [SerializeField]
        public bool readColorBuffer;
        [SerializeField]
        public bool hasStackSpilling;

        public static ShaderProgramPerfInfo Create(MaliOcReport report, string variantName)
        {
            if(report == null || report.shaders == null || 
                report.shaders.Length <= 0 || report.shaders[0] == null) {
                return null; 
            }
            int variantIdx = ShaderVariantsReport.GetIndexByName(report.shaders[0].variants, variantName);
            return ShaderProgramPerfInfo.Create(report , variantIdx);
        }

        public static ShaderProgramPerfInfo Create(MaliOcReport report,int variantIdx)
        {
            if(variantIdx < 0) { return null; }
            if( report == null) { return null; }
            if(report.shaders == null || report.shaders.Length <= 0)
            {
                return null;
            }
            var shader = report.shaders[0];
            var variants = shader.variants;
            if (variants == null || variants.Length <= variantIdx)
            {
                return null;
            }
            if (shader == null) { return null; }

            var info = new ShaderProgramPerfInfo();
            info.SetVariantProperties(variants[variantIdx].properties);
            info.SetPerformanceInfo(variants[variantIdx].performance);
            info.SetShaderProperties(shader.properties);
            return info;
        }

        private void SetPerformanceInfo(ShaderPerformanceReport report)
        {
            this.pipelines = report.pipelines;
            this.totalCycles = report.total_cycles.cycle_count;
            this.longestCycles = report.longest_path_cycles.cycle_count;
            this.shortestCycles = report.shortest_path_cycles.cycle_count;
        }
        private void SetVariantProperties(PropertyInfo[] props)
        {
            SetProperty(props, "work_registers_used",out workRegisters);
            SetProperty(props, "uniform_registers_used", out uniformRegister);
            SetProperty(props, "has_stack_spilling", out hasStackSpilling);
            SetProperty(props, "fp16_arithmetic", out bit16Arithmetic);
        }
        private void SetShaderProperties(PropertyInfo[] props)
        {
            SetProperty(props, "has_uniform_computation", out hasUniformComputation);
            SetProperty(props, "has_side_effects", out hasSideEffects);
            SetProperty(props, "modifies_coverage", out modifisCoverage);
            SetProperty(props, "uses_late_zs_test", out useLateZSTest);
            SetProperty(props, "uses_late_zs_update", out useLateZSUpdate);
            SetProperty(props, "reads_color_buffer", out readColorBuffer);
        }

        private void SetProperty(PropertyInfo[] props,string name , out int val,int defaultVal = 0)
        {
            var workProp = PropertyInfo.GetByName(props, name);
            if(workProp == null) {
                val = defaultVal;
                return;
            }
            val = workProp.GetValueAsInt();
        }
        private void SetProperty(PropertyInfo[] props, string name, out bool val, bool defaultVal = false)
        {
            var workProp = PropertyInfo.GetByName(props, name);
            if (workProp == null)
            {
                val = defaultVal;
                return;
            }
            val = workProp.GetValueAsBool();
        }
    }

}