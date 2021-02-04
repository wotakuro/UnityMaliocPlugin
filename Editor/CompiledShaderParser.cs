using ICSharpCode.NRefactory.Ast;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace UTJ.MaliocPlugin
{
    public class CompiledShaderParser
    {
        static readonly string GLOBAL_KEYWORD = "Global Keywords: ";
        static readonly string LOCAL_KEYWORD = "Local Keywords: ";
        static readonly string VERTEX_START = "#ifdef VERTEX";
        static readonly string FRAGMENT_START = "#ifdef FRAGMENT";


        // Shader
        public struct ShaderProgram
        {
            public string globalKeyword;
            public string localKeyword;
            public string vertShader;
            public string fragShader;
            public int passInfoIdx;
        }
        // PassInfo
        public struct PassInfo
        {
            public int index;
            public int strPos;
            public string name;
            public string tags;

            public PassInfo( int idx , int pos)
            {
                this.index = idx;
                this.strPos = pos;
                this.name = null;
                this.tags = null;
            }
        }

        private List<PassInfo> passInfos = new List<PassInfo>();

        private List<ShaderProgram> programs = new List<ShaderProgram>();


        public CompiledShaderParser(string str)
        {
            this.ConstructPassInfos(str);
            this.ConstructShaderPrograms(str);
        }


        public List<ShaderProgram> GetShaderPrograms()
        {
            return this.programs;
        }

        public List<PassInfo> GetPassInfos()
        {
            return this.passInfos;
        }

        private void ConstructPassInfos(string str)
        {
            string passStr = "Pass {";
            int idx = str.IndexOf(passStr) ;
            int currentPassIdx = 0;
            while(idx < str.Length){
                var passInfo = CreatePassInfo(str, idx, currentPassIdx);
                this.passInfos.Add(passInfo);
                ++currentPassIdx;
                idx = str.IndexOf(passStr,idx+passStr.Length);
                if(idx < 0) { break; }
            }
        }

        private PassInfo CreatePassInfo(string str,int strPos,int currentPassIdx)
        {
            PassInfo passInfo = new PassInfo(currentPassIdx,strPos);
            int length = str.Length;
            // skip first line
            strPos = GetLineEndIdx(str, strPos);
            strPos = SkipTrimChars(str, strPos);
            while( strPos < length)
            {
                int nextStrPos = strPos;
                if( IsMatch(str, strPos, "////", out nextStrPos) ){
                    strPos = nextStrPos;
                    break;
                }
                if (IsMatch(str, strPos, "Name", out nextStrPos))
                {
                    strPos = nextStrPos;
                    nextStrPos = GetLineEndIdx(str, strPos);
                    passInfo.name = str.Substring(strPos, nextStrPos - strPos);
                    strPos = SkipTrimChars(str, nextStrPos);
                }
                else if (IsMatch(str, strPos, "Tags", out nextStrPos))
                {
                    strPos = nextStrPos;
                    nextStrPos = GetLineEndIdx(str, strPos);
                    passInfo.tags = str.Substring(strPos, nextStrPos - strPos);
                    strPos = SkipTrimChars(str, nextStrPos);
                }
                else
                {
                    // next 
                    strPos = GetLineEndIdx(str, strPos);
                    strPos = SkipTrimChars(str, strPos);
                }
            }
            return passInfo;
        }



        private int GetPassIndex(int strPos,int previewIdx)
        {
            int length = this.passInfos.Count;
            for( int i = 0; i<length; ++i)
            {
                if( passInfos[i].strPos > strPos)
                {
                    return i-1;
                }
            }
            return length-1;
        }

        private void ConstructShaderPrograms(string str)
        {
            int strPos = str.IndexOf(GLOBAL_KEYWORD, 0);
            int length = str.Length;
            int previewIdx = 0;
            while (strPos < length)
            {
                int nextStrPos = str.IndexOf(GLOBAL_KEYWORD, strPos + 1);
                if (nextStrPos < 0) { nextStrPos = length; }

                var shaderProgram = CreateShaderProgram(str, strPos, nextStrPos);
                shaderProgram.passInfoIdx = GetPassIndex(strPos, previewIdx);
                this.programs.Add(shaderProgram);
                previewIdx = shaderProgram.passInfoIdx;

                strPos = nextStrPos;
            }

        }

        private ShaderProgram CreateShaderProgram(string str,int startIdx,int nextIdx)
        {
            string vertProgram = null;
            string fragProgram = null;

            int vertStart = str.IndexOf(VERTEX_START,startIdx);
            if ( 0 < vertStart && vertStart < nextIdx)
            {
                int vertEnd = GetEndifEndIndex(str, vertStart);
                vertProgram = str.Substring(vertStart, vertEnd - vertStart);

                // workaround...
                vertProgram = vertProgram.Replace("#version 300 es", "#version 310 es");
            }
            int fragStart = str.IndexOf(FRAGMENT_START, startIdx);
            if (0 < fragStart && fragStart < nextIdx)
            {
                int fragEnd = GetEndifEndIndex(str, fragStart);
                fragProgram = str.Substring(fragStart, fragEnd - fragStart);
                // workaround...
                fragProgram = fragProgram.Replace("#version 300 es", "#version 310 es");
            }

            ShaderProgram shaderProgram = new ShaderProgram()
            {
                globalKeyword = GetKeyword(str, GLOBAL_KEYWORD, startIdx, nextIdx),
                localKeyword = GetKeyword(str, LOCAL_KEYWORD, startIdx, nextIdx),
                vertShader = RemoveTopIfdefBlock(vertProgram),
                fragShader = RemoveTopIfdefBlock(fragProgram),
            };
            return shaderProgram;
        }
        private string GetKeyword(string str,string keywordType, int startIdx, int nextIdx)
        {
            int index = str.IndexOf(keywordType, startIdx);
            if( index < 0) { return null; }
            int typeLength = keywordType.Length;
            int lineEnd = str.IndexOf("\n",index);
            return str.Substring(index + typeLength, lineEnd - index - typeLength);
        }

        private static string RemoveTopIfdefBlock(string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return null;
            }
            int start = str.IndexOf("\n");
            int end = str.LastIndexOf("\n");
            return str.Substring(start+1,end-start-1);
        }



        // todo skip comment out
        private static int GetEndifEndIndex(string str, int ifdefIdx)
        {
            int length = str.Length;
            int currentIdx = ifdefIdx;
            int ifLevel = 0;

            while (currentIdx < length)
            {
                int nextIdx = currentIdx;
                if (IsMatch(str, currentIdx, "#if", out nextIdx))
                {
                    ++ifLevel;
                }
                else if (IsMatch(str, currentIdx, "#endif", out nextIdx))
                {
                    --ifLevel;
                    if( ifLevel <= 0)
                    {
                        return nextIdx;
                    }
                }
                currentIdx = nextIdx;
            }

            return currentIdx;
        }

        private int GetLineEndIdx(string str, int strPos)
        {
            int length = str.Length;
            int startIdx = strPos;
            for (; strPos < length; ++strPos)
            {
                if (str[strPos] == '\n' || str[strPos] == '\r')
                {
                    break;
                }
            }
            return strPos;
        }


        private int SkipTrimChars(string str, int strPos)
        {
            int length = str.Length;
            for (; strPos < length; ++strPos)
            {
                if (str[strPos] != '\n' &&
                    str[strPos] != '\r' &&
                    str[strPos] != '\t' &&
                    str[strPos] != ' ')
                {
                    break;
                }
            }
            return strPos;

        }

        private static bool IsMatch(string src, int idx, string search, out int nextIdx)
        {
            int searchLength = search.Length;
            int srcLenght = src.Length;
            for (int i = 0; i < searchLength; ++i)
            {
                if (idx + i >= srcLenght)
                {
                    nextIdx = srcLenght - 1;
                    return false;
                }

                if (src[idx + i] != search[i])
                {
                    nextIdx = idx + 1;
                    return false;
                }
            }
            nextIdx = idx + searchLength;
            return true;
        }
    }
}
