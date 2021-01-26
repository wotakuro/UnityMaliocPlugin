using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.MaliocPlugin.Result
{
    public class MaliOcReport
    {
        ProducerReport producer;
        SchemaReport schema;
        object []shaders;
    }

    public class ProducerReport
    {
        public string build;
        public string documentation;
        public string name;
        public int[] version;
    }
    public class SchemaReport
    {
        public string name;
        public int version;
    }
    public class ShaderReport
    {
        public string driver;
        public string filename;
    }
}
