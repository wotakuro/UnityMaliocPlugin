﻿using ICSharpCode.NRefactory.Ast;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

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
        }
        // PassInfo
        public struct PassInfo
        {
            public int index;
            public int strPos;
            public string name;
            public string tags;
        }

        private List<PassInfo> passInfos = new List<PassInfo>();

        private List<ShaderProgram> programs = new List<ShaderProgram>();


        public CompiledShaderParser(string str)
        {
            this.CreatePassInfos(str);
            int index = str.IndexOf(GLOBAL_KEYWORD, 0);
            int length = str.Length;
            while( index < length)
            {
                int nextIdx = str.IndexOf(GLOBAL_KEYWORD, index + 1);
                if( nextIdx < 0) { nextIdx = length; }
                var shaderProgram = CreateShaderProgram(str, index, nextIdx);
                this.programs.Add(shaderProgram);
                index = nextIdx;
            }
        }

        private void CreatePassInfos(string str)
        {
            string passStr = "Pass {";
            int idx = str.IndexOf(passStr) ;
            while(idx < str.Length){
                idx = CreatePassInfos(str, idx);
                idx = str.IndexOf(passStr,idx);
                if(idx < 0) { break; }
            }
        }
        private int CreatePassInfos(string str,int idx)
        {
            int length = str.Length;
            // skip first line
            idx = GetLineEndIdx(str, idx);
            idx = SkipTrimChars(str, idx);
            while( idx < length)
            {
                int nextIdx = idx;
                if( IsMatch(str, idx, "////", out nextIdx) ){
                    idx = nextIdx;
                    break;
                }
                if (IsMatch(str, idx, "Name", out nextIdx))
                {
                    idx = nextIdx;
                    nextIdx = GetLineEndIdx(str, idx);
                    Debug.Log("Name::::" + str.Substring(idx, nextIdx - idx));

                    idx = SkipTrimChars(str, nextIdx);
                }
                else if (IsMatch(str, idx, "Tags", out nextIdx))
                {
                    idx = nextIdx;
                    nextIdx = GetLineEndIdx(str, idx);
                    Debug.Log("Tags::::" + str.Substring(idx, nextIdx - idx));
                    idx = SkipTrimChars(str, nextIdx);
                }
                else
                {
                    // next 
                    idx = GetLineEndIdx(str, idx);
                    idx = SkipTrimChars(str, idx);
                }
            }
            return idx;
        }
        private int GetLineEndIdx(string str, int idx)
        {
            int length = str.Length;
            int startIdx = idx;
            for(;idx < length; ++idx)
            {
                if(str[idx] == '\n' || str[idx] == '\r')
                {
                    break;
                }
            }
            return idx;
        }
        private int SkipTrimChars(string str,int idx)
        {
            int length = str.Length;
            for (; idx < length; ++idx)
            {
                if (str[idx] != '\n' &&
                    str[idx] != '\r' &&
                    str[idx] != '\t' &&
                    str[idx] != ' ' )
                {
                    break;
                }
            }
            return idx;

        }


        public List<ShaderProgram> GetShaderPrograms()
        {
            return programs;
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
                localKeyword = GetKeyword(str, GLOBAL_KEYWORD, startIdx, nextIdx),
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
