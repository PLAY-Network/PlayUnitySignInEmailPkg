using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if UNITY_STANDALONE_WIN
using Microsoft.Win32;
#endif

namespace RGN.Modules.SignIn
{
    internal sealed class WindowsDeepLinks
    {
        private static Queue<string> _events;
        private static Mutex _mutex;
        private static Thread _thread;

        public delegate void DeepLinkActivatedDelegate(string url);
        public static DeepLinkActivatedDelegate DeepLinkActivated;

        public static void StartHandling()
        {
#if UNITY_STANDALONE_WIN
            try
            {
                if (!IsCustomUrlRegistered())
                {
                    RegisterCustomURL();
                    WindowsDeepLinkUtility.RunAsDesktopUser(GetAppExecutablePath());
                    Application.Quit();
                    return;
                }
            }
            catch (Exception exception) when (exception is SecurityException or UnauthorizedAccessException)
            {
                using (Process appRunasProcess = new Process())
                {
                    appRunasProcess.StartInfo.FileName = GetAppExecutablePath();
                    appRunasProcess.StartInfo.Verb = "runas";
                    appRunasProcess.Start();
                }
                Application.Quit();
                return;
            }

            _events = new Queue<string>();
            if (!string.IsNullOrEmpty(Environment.CommandLine))
            {
                _events.Enqueue(Environment.CommandLine);
            }

            _mutex = new Mutex(false, Application.productName);

            try
            {
                if (!_mutex.WaitOne(0))
                {
                    Debug.LogError("Mutex already opened");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Error while to try open mutex: {exception}");
            }

            _thread = new Thread(ListenPipe);
            _thread.Start();
#endif
        }

        public static void Tick()
        {
#if UNITY_STANDALONE_WIN
        if (_events.Count > 0)
        {
            string message = _events.Dequeue();
            try
            {
                message = message.Split(' ')[1].Replace("\"", "");
                DeepLinkActivated?.Invoke(message);
            }
            catch
            {
                // ignore
            }
        }
#endif
        }

        public static void Dispose()
        {
#if UNITY_STANDALONE_WIN
        _mutex?.Close();
        _thread?.Abort();
#endif
        }

        private static void ListenPipe()
        {
            while (true)
            {
                using (var pipeServer = new NamedPipeServerStream(Application.productName, PipeDirection.In, 1))
                {
                    pipeServer.WaitForConnection();

                    using (var sr = new StreamReader(pipeServer))
                    {
                        string message = sr.ReadLine();
                        _events.Enqueue(message);
                    }
                }
            }
        }

        private static void RegisterCustomURL()
        {
#if UNITY_STANDALONE_WIN
            string packageNameScheme = Application.identifier.ToLower().Replace(".", string.Empty);

            RegistryKey customUrlSubKey = Registry.ClassesRoot.CreateSubKey(packageNameScheme);
            customUrlSubKey.SetValue("", "");
            customUrlSubKey.SetValue("URL Protocol", "");

            RegistryKey shellSubKey = customUrlSubKey.CreateSubKey("shell");
            shellSubKey.SetValue("", "open");

            RegistryKey openSubKey = shellSubKey.CreateSubKey("open");
            customUrlSubKey.SetValue("", "");

            RegistryKey commandSubKey = openSubKey.CreateSubKey("command");
            commandSubKey.SetValue("", $"{GetAppReflectorExecutablePath()} \"%1\"");

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

        private static string GetAppReflectorExecutablePath()
        {
            string dataPath = Application.dataPath;
            string appPath = Path.Combine(dataPath, "../");
            string appExecutablePath = Path.GetFullPath(Path.Combine(appPath, "RGNDeepLinkReflector.exe"));
            return appExecutablePath;
        }

        private static bool IsCustomUrlRegistered()
        {
            return PlayerPrefs.GetInt("IsCustomUrlRegistered", 0) == 1;
        }
    }
}