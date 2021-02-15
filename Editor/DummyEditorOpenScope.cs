using System.IO;
using UnityEditor;

namespace UTJ.MaliocPlugin
{
    public class DummyEditorOpenScope : System.IDisposable
    {
        private string str;
        private static readonly string DummyAppPath = "Library/com.utj.malioc.plugin/DummyApplication.exe";
        public DummyEditorOpenScope()
        {
            string dummyExePath = InitDummyExe();
            str = EditorPrefs.GetString("kScriptsDefaultApp");
            EditorPrefs.SetString("kScriptsDefaultApp.backup", str);
            EditorPrefs.SetString("kScriptsDefaultApp", dummyExePath);
        }

        public void Dispose()
        {
            EditorPrefs.SetString("kScriptsDefaultApp", str);
        }


        private string InitDummyExe()
        {
            if (!File.Exists(DummyAppPath))
            {
                AssetDatabase.CopyAsset("Packages/com.utj.malioc.plugin/Editor/DummyApplication.exe", DummyAppPath);
            }

            return Path.Combine(Directory.GetCurrentDirectory(), DummyAppPath);
        }
    }
}