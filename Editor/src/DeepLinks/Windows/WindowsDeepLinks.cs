using System.Diagnostics;
using System.IO;
using System.Security;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_STANDALONE_WIN
using System;
using Microsoft.Win32;
#endif

namespace RGN.MyEditor
{
    public class WindowsDeepLinks
    {
        public static void StartHandling()
        {
#if UNITY_STANDALONE_WIN
        try
        {
            if (!IsCustomUrlRegistered())
            {
                RegisterCustomURL();
            }
        }
        catch (Exception exception) when (exception is SecurityException or UnauthorizedAccessException)
        {
            using (Process appRunasProcess = new Process())
            {
                string appExecutablePath = GetAppExecutablePath();
                appRunasProcess.StartInfo.FileName = appExecutablePath;
                appRunasProcess.StartInfo.Verb = "runas";
                appRunasProcess.Start();
            }

            Application.Quit();
        }
#endif
        }

        private static void RegisterCustomURL()
        {
#if UNITY_STANDALONE_WIN
        string appExecutablePath = GetAppExecutablePath();
        
        RegistryKey customUrlSubKey = Registry.ClassesRoot.CreateSubKey("unitydl");
        customUrlSubKey.SetValue("", "");
        customUrlSubKey.SetValue("URL Protocol", "");
        
        RegistryKey shellSubKey = customUrlSubKey.CreateSubKey("shell");
        shellSubKey.SetValue("", "open");

        RegistryKey openSubKey = shellSubKey.CreateSubKey("open");
        customUrlSubKey.SetValue("", "");

        RegistryKey commandSubKey = openSubKey.CreateSubKey("command");
        commandSubKey.SetValue("", $"{appExecutablePath} \"%1\"");
        
        customUrlSubKey.Close();
        
        PlayerPrefs.SetInt("IsCustomUrlRegistered", 1);
#endif
        }

        private static string GetAppExecutablePath()
        {
            string dataPath = Application.dataPath;
            string appPath = Path.Combine(dataPath, "../");
            string appExecutablePath = Path.GetFullPath(Path.Combine(appPath, Application.productName + ".exe"));
            return appExecutablePath;
        }

        private static bool IsCustomUrlRegistered()
        {
            return PlayerPrefs.GetInt("IsCustomUrlRegistered", 0) == 1;
        }
    }
}
