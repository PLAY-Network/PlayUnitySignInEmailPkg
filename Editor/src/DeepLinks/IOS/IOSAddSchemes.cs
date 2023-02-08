#if UNITY_EDITOR && PLATFORM_IOS
using System.Collections.Generic;
using System.Linq;
using RGN.Modules.SignIn;
using UnityEditor;
using UnityEngine;

namespace RGN.MyEditor
{
    public class IOSAddSchemes
    {
        [InitializeOnLoadMethod]
        public static void AddSchemes()
        {
            string packageNameScheme = RGNHttpUtility.GetDeepLinkRedirectScheme();

            if (!PlayerSettings.iOS.iOSUrlSchemes.Contains(packageNameScheme))
            {
                List<string> schemes = new List<string>();
                schemes = PlayerSettings.iOS.iOSUrlSchemes.ToList();
                schemes.Add(packageNameScheme);
                PlayerSettings.iOS.iOSUrlSchemes = schemes.ToArray();
                Debug.Log("New URL schemes added : " + packageNameScheme);
            }
        }
    }
}
#endif
