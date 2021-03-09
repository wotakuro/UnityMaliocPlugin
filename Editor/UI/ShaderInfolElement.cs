using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UTJ.MaliocPlugin.DB;

namespace UTJ.MaliocPlugin.UI
{
    public class ShaderInfolElement : VisualElement
    {
        private static readonly int FoldMarginLeft = 20;

        public static ShaderInfolElement Create( ShaderKeywordInfo keywordInfo,
            ShaderProgramInfo programinfo,
            List<PassInformation> passInfoms)
        {
            var obj = new ShaderInfolElement(keywordInfo,programinfo, passInfoms);
            return obj;
        }

        private ShaderInfolElement(ShaderKeywordInfo keywordInfo,
            ShaderProgramInfo programinfo,
            List<PassInformation> passInfoms)
        {
            var fold = new Foldout();
            var str = CreateSummaryString(keywordInfo, passInfoms);
            fold.text = str;

            var keywordVe = CreateKeywordInfoElement(keywordInfo);
            var positionVe = CreateProgramElement("positionVertPerf", programinfo.positionVertPerf);
            var varyingVe = CreateProgramElement("varyingVertPerf", programinfo.varyingVertPerf);
            var fragVe = CreateProgramElement("fragPerf", programinfo.fragPerf);

            fold.Add(keywordVe);
            fold.Add(positionVe);
            fold.Add(varyingVe);
            fold.Add(fragVe);
            this.Add(fold);
        }

        private string CreateSummaryString(ShaderKeywordInfo keywordInfo, List<PassInformation> passInfoms)
        {
            var sb = new StringBuilder(128);
            var passInfo = passInfoms[keywordInfo.passIndex];
            sb.Append("Pass:").Append(passInfo.name);
            sb.Append("(").Append(passInfo.lightMode).Append(")");
            /*
            sb.Append("\nGlobalKeyword:");
            sb.Append(keywordInfo.globalKeyword);
            sb.Append("\nLocalKeyword:");
            sb.Append(keywordInfo.localKeyword);
            */
            return sb.ToString();
        }

        private VisualElement CreateKeywordInfoElement(ShaderKeywordInfo keywordInfo)
        {
            var fold = new Foldout();
            fold.text = "Keywords";
            Append(keywordInfo.globalKeyword, fold);
            Append(keywordInfo.localKeyword, fold);

            fold.style.marginLeft = FoldMarginLeft;
            return fold;
        }
        private void Append(string keywords,VisualElement parent)
        {
            if( string.IsNullOrEmpty(keywords))
            {
                return;
            }
            var keywordArray = keywords.Split(' ');
            foreach( var keyword in keywordArray)
            {
                if(keyword == "<none>" || keyword == "")
                {
                    continue;
                }
                parent.Add(new Label(keyword));
            }
        }


        private VisualElement CreateProgramElement(string name,ShaderProgramPerfInfo perfInfo)
        {
            var fold = new Foldout();
            fold.text = name;
            VisualElement ve = null;
            if (perfInfo != null)
            {
                ve = ShaderProgramInfoElement.Create(perfInfo);
            }
            else
            {
                ve = new Label("No data");
            }
            ve.style.marginLeft = FoldMarginLeft;
            fold.Add(ve);
            fold.style.marginLeft = FoldMarginLeft;
            return fold;
        }
    }
}
