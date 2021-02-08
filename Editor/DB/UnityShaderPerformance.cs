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
        public List<PassInformation> passInfos;
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
            if (performanceInfoByKeyword == null) { performanceInfoByKeyword = new Dictionary<ShaderKeywordInfo, ShaderProgramInfo>(); }

            keywordInfos.Add(info);
            programInfos.Add(programInfo);

            performanceInfoByKeyword.Add(info, programInfo);
        }

        public void AddPassInfo(string name , string tags)
        {
            if(passInfos == null) { passInfos = new List<PassInformation>(); }
            string tagsSearch = "\"LIGHTMODE\"=\"";
            int tagSearchLength = tagsSearch.Length;
            var info = new PassInformation();
            if (name != null)
            {
                info.name = name.Replace("\"", "").Trim();
            }
            else
            {
                info.name = null;
            }
            if (tags != null)
            {
                int lightModeIdx = tags.IndexOf(tagsSearch);

                if (lightModeIdx >= 0)
                {
                    int closePoint = tags.IndexOf('\"', lightModeIdx + tagSearchLength + 1);
                    if (closePoint > 0)
                    {
                        info.lightMode = tags.Substring(lightModeIdx + tagSearchLength,
                            closePoint - lightModeIdx - tagSearchLength );
                    }
                }
                else
                {
                    info.lightMode = "";
                }
            }
            passInfos.Add(info);
        }

        private void CreateDictionary()
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
            var obj =  JsonUtility.FromJson<ShaderInfo>(str);
            obj.CreateDictionary();
            return obj;
        }


        public void GetShaderMatchPrograms(List<ShaderKeywordInfo>  result,
            List<string> keywords)
        {
            foreach (var kvs in performanceInfoByKeyword) {
                var keyInfo = kvs.Key;
                var isMatch = MaliocPluginUtility.IsMatchKeyword(keyInfo.globalKeyword, keyInfo.localKeyword, keywords);
                if(isMatch)
                {
                    result.Add(kvs.Key);
                }
            }
        }
        public ShaderProgramInfo GetProgramInfo(ShaderKeywordInfo key)
        {
            ShaderProgramInfo val;
            if(this.performanceInfoByKeyword.TryGetValue(key,out val)){
                return val;
            }
            return null;
        }

        public PassInformation GetPassInfo(int idx)
        {
            if (passInfos != null) { return null; }
            if (idx < 0 || this.passInfos.Count >= idx) { return null; }
            return passInfos[idx];
        }
        public List<PassInformation> GetPassInfos()
        {
            return passInfos;
        }

    }

    [Serializable]
    public class PassInformation
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public string lightMode;
    }


    [Serializable]
    public class ShaderKeywordInfo
    {
        [SerializeField]
        public string globalKeyword;
        [SerializeField]
        public string localKeyword;
        [SerializeField]
        public int passIndex;

        public override bool Equals(object obj)
        {
            ShaderKeywordInfo keyInfo = obj as ShaderKeywordInfo;
            if (keyInfo == null)
            {
                return false;
            }
            return (this.globalKeyword == keyInfo.globalKeyword) &&
                (this.localKeyword == keyInfo.localKeyword) &&
                (this.passIndex == keyInfo.passIndex);
        }
        public override int GetHashCode()
        {
            return globalKeyword.GetHashCode() + localKeyword.GetHashCode() + passIndex;
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