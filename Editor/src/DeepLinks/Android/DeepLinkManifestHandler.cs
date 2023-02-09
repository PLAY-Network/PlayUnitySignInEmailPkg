using System.IO;
using RGN.Modules.SignIn;
using UnityEditor;
using UnityEngine;

namespace RGN.MyEditor
{
    public class BuildPostprocess
    {
        [InitializeOnLoadMethod]
        public static void UpdateCustomManifest()
        {
            string manifestPath = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";

            // Create manifest
            if (!File.Exists(manifestPath))
            {
                GenerateManifest();
            }

            string deepLinkRedirectScheme = RGNHttpUtility.GetDeepLinkRedirectScheme();

            string manifestContent = File.ReadAllText(manifestPath);

            string newIntent = "<intent-filter>\n" +
                "<action android:name=\"android.intent.action.VIEW\" />\n" +
                "<category android:name=\"android.intent.category.DEFAULT\" />\n" +
                "<category android:name=\"android.intent.category.BROWSABLE\" />\n" +
                "<data android:scheme=\"" + deepLinkRedirectScheme + "\" android:host=\"\" />\n" +
                "</intent-filter>\n";

            if (!manifestContent.Contains(deepLinkRedirectScheme))
            {
                // Add the new intent
                int insertIndex = manifestContent.IndexOf("</activity>");
                manifestContent = manifestContent.Insert(insertIndex, newIntent);

                File.WriteAllText(manifestPath, manifestContent);
                Debug.Log("Android Manifest updated");
            }

        }

        public static void GenerateManifest()
        {
            string path = "Assets/Plugins/Android/AndroidManifest.xml";

            if (!Directory.Exists("Assets/Plugins/Android"))
            {
                Directory.CreateDirectory("Assets/Plugins/Android");
            }

            if (!File.Exists(path))
            {
                using (StreamWriter writer = File.CreateText(path))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\"");
                    writer.WriteLine("    package=\"com.unity3d.player\"");
                    writer.WriteLine("    xmlns:tools=\"http://schemas.android.com/tools\">");
                    writer.WriteLine("    <application>");
                    writer.WriteLine("        <activity android:name=\"com.unity3d.player.UnityPlayerActivity\"");
                    writer.WriteLine("            android:theme=\"@style/UnityThemeSelector\">");
                    writer.WriteLine("            <intent-filter>");
                    writer.WriteLine("                <action android:name=\"android.intent.action.MAIN\" />");
                    writer.WriteLine("                <category android:name=\"android.intent.category.LAUNCHER\" />");
                    writer.WriteLine("            </intent-filter>");
                    writer.WriteLine("            <meta-data android:name=\"unityplayer.UnityActivity\" android:value=\"true\" />");
                    writer.WriteLine("        </activity>");
                    writer.WriteLine("    </application>");
                    writer.WriteLine("");
                    writer.WriteLine("</manifest>");
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
