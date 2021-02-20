using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UTJ.MaliocPlugin.DB;


namespace UTJ.MaliocPlugin.UI
{
    public class TestWindow : EditorWindow
    {
        [MenuItem("Tools/Test")]
        public static void Create()
        {
            TestWindow.GetWindow<TestWindow>();
        }

        private void OnEnable()
        {
            this.rootVisualElement.Add(ShaderProgramInfoElement.Create(null));
        }
    }

    public class ShaderProgramInfoElement : VisualElement
    {
        [Flags]
        private enum BorderFlag
        {
            None = 0,
            Left = 1,
            Right = 2,
            Top = 4,
            Bottom = 8,
            All = 1 + 2 + 4 + 8
        }
        public static ShaderProgramInfoElement Create(ShaderProgramPerfInfo info)
        {
            return new ShaderProgramInfoElement(info);
        }

        private ShaderProgramInfoElement(ShaderProgramPerfInfo info)
        {
            this.Add(CreateCycleTable(info));
            this.Add(CreateMainShaderInfoTable(info));
            this.Add(CreateShaderProperties(info));
        }
        private VisualElement CreateCycleTable(ShaderProgramPerfInfo info)
        {
            Foldout fold = new Foldout();
            fold.text = "Cycle Info";
            fold.name = "CycleInfo";
            var val = new string[4, info.pipelines.Length + 1];
            for (int i = 0; i < info.pipelines.Length; ++i)
            {
                val[0, 1 + i] = info.pipelines[i];
                val[1, 1 + i] = info.totalCycles[i].ToString();
                val[2, 1 + i] = info.longestCycles[i].ToString();
                val[3, 1 + i] = info.shortestCycles[i].ToString();
            }
            val[0, 0] = " ";
            val[1, 0] = "total";
            val[2, 0] = "longest";
            val[3, 0] = "shortest";
            fold.Add(CreateTableView(val));
            return fold;
        }

        private VisualElement CreateMainShaderInfoTable(ShaderProgramPerfInfo info)
        {
            Foldout fold = new Foldout();
            fold.text = "Main Info";
            fold.name = "MainInfo";
            var val = new string[2, 4];
            val[0, 0] = "Work register";
            val[0, 1] = "uniform register";
            val[0, 2] = "Stack spilling";
            val[0, 3] = "16-bit arithmetic";
            val[1, 0] = info.workRegisters.ToString();
            val[1, 1] = info.uniformRegister.ToString();
            val[1, 2] = info.hasStackSpilling.ToString();
            val[1, 3] = info.bit16Arithmetic + "%";
            fold.Add(CreateTableView(val));
            return fold;
        }
        private VisualElement CreateShaderProperties(ShaderProgramPerfInfo info)
        {
            Foldout fold = new Foldout();
            fold.text = "Shader Property";
            fold.name = "ShaderInfo";

            var val = new string[2, 6];
            val[0, 0] = "Has uniform computation";
            val[0, 1] = "Has side-effects";
            val[0, 2] = "Modifies coverage";
            val[0, 3] = "Use late ZS test";
            val[0, 4] = "Use late ZS update";
            val[0, 5] = "Read color buffer";


            val[1, 0] = info.hasUniformComputation.ToString();
            val[1, 1] = info.hasSideEffects.ToString();
            val[1, 2] = info.modifisCoverage.ToString();
            val[1, 3] = info.useLateZSTest.ToString();
            val[1, 4] = info.useLateZSUpdate.ToString();
            val[1, 5] = info.readColorBuffer.ToString();
            fold.Add(CreateTableView(val));
            return fold;
        }

        VisualElement CreateTableView(string[,] val)
        {

            VisualElement table = new VisualElement();
            table.style.flexDirection = FlexDirection.Row;

            for (int i = 0; i < val.GetLength(0); ++i)
            {
                VisualElement column = new VisualElement();
                var borderFlag = BorderFlag.Bottom | BorderFlag.Top | BorderFlag.Right;
                if(i == 0)
                {
                    borderFlag |= BorderFlag.Left;
                }
                SetVisualBorder(column, 2, Color.white, borderFlag);
                for (int j = 0; j < val.GetLength(1); ++j)
                {
                    var label = new Label(val[i, j]);
                    label.style.paddingRight = 5;
                    label.style.paddingLeft = 5;
                    label.style.minWidth = 40;
                    column.Add(label);
                    if (j != 0) {
                        SetVisualBorder(label, 1, Color.white, BorderFlag.Top);
                    }
                }
                table.Add(column);
            }
            return table;
        }


        private void SetVisualBorder(VisualElement ve, int size, Color col, BorderFlag borderFlag)
        {
            if (borderFlag.HasFlag(BorderFlag.Top))
            {
                ve.style.borderTopWidth = size;
                ve.style.borderTopColor = col;
            }
            if (borderFlag.HasFlag(BorderFlag.Bottom))
            {
                ve.style.borderBottomWidth = size;
                ve.style.borderBottomColor = col;
            }
            if (borderFlag.HasFlag(BorderFlag.Left))
            {
                ve.style.borderLeftWidth = size;
                ve.style.borderLeftColor = col;
            }
            if (borderFlag.HasFlag(BorderFlag.Right))
            {
                ve.style.borderRightWidth = size;
                ve.style.borderRightColor = col;
            }
        }
    }
}