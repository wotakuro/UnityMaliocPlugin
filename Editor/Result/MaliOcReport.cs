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

        public int GetValueAsInt()
        {
            int val = 0;
            int.TryParse(this.value, out val);
            return val;
        }
        public bool GetValueAsBool()
        {
            if(this.value == "true")
            {
                return true;
            }
            return false;
        }

        public static PropertyInfo GetByName(PropertyInfo[] props,string name)
        {
            if(props == null) { return null; }
            foreach( var prop in props)
            {
                if(prop != null && prop.name == name)
                {
                    return prop;
                }
            }
            return null;
        }
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

        public static int GetIndexByName(ShaderVariantsReport[] variants,string name)
        {
            if(variants == null) { return -1; }
            int length = variants.Length;
            for (int i = 0; i < length; ++i)
            {
                if(variants[i] != null && 
                    variants[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }
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
