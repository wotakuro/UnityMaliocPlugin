using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.MaliocPlugin.Result
{
    [Serializable]
    public class MaliOcReport
    {
        [SerializeField]
        public ProducerReport producer;
        [SerializeField]
        public SchemaReport schema;
        [SerializeField]
        public ShaderReport[]shaders;

        public static MaliOcReport CreateFromJson(string json)
        {
            var obj = JsonUtility.FromJson<MaliOcReport>(json);
            return obj;
        }
    }

    [Serializable]
    public class ProducerReport
    {
        [SerializeField]
        public string build;
        [SerializeField]
        public string documentation;
        [SerializeField]
        public string name;
        [SerializeField]
        public int[] version;
    }
    [Serializable]
    public class SchemaReport
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public int version;
    }
    [Serializable]
    public class ShaderReport
    {
        [SerializeField]
        public string driver;
        [SerializeField]
        public string filename;
        [SerializeField]
        public HardwareInfo hardware;
        [SerializeField]
        public PropertyInfo[] properties;
        [SerializeField]
        public ShaderAPIReport shader;
        [SerializeField]
        public ShaderVariantsReport[] variants;
    }
    [Serializable]
    public class HardwareInfo
    {
        [SerializeField]
        public string architecture;
        [SerializeField]
        public string core;
        [SerializeField]
        public PropertyInfo[] pipelines;
        [SerializeField]
        public string revision;
    }
    [Serializable]
    public class PropertyInfo
    {
        [SerializeField]
        public string description;
        [SerializeField]
        public string display_name;
        [SerializeField]
        public string name;
        [SerializeField]
        public string value;
    }
    [Serializable]
    public class ShaderAPIReport
    {
        [SerializeField]
        public string api;
        [SerializeField]
        public string type;
    }

    [Serializable]
    public class CycleReport
    {
        [SerializeField]
        public string bound_pipelines;
        [SerializeField]
        public float[] cycle_count;
    }
    [Serializable]
    public class ShaderVariantsReport
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public ShaderPerformanceReport performance;
        [SerializeField]
        public PropertyInfo[] properties;
    }

    [Serializable]
    public class ShaderPerformanceReport
    {
        [SerializeField]
        public CycleReport longest_path_cycles;
        [SerializeField]
        public string[] pipelines;
        [SerializeField]
        public CycleReport shortest_path_cycles;
        [SerializeField]
        public CycleReport total_cycles;
    }
}
