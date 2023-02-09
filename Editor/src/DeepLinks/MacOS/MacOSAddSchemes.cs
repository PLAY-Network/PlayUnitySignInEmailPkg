#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER && PLATFORM_STANDALONE_OSX
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using RGN.Modules.SignIn;

namespace RGN.MyEditor
{
    public class MacOSAddSchemes
    {
        [InitializeOnLoadMethod]
        private static void AddSchemes()
        {
            string deepLinkRedirectScheme = RGNHttpUtility.GetDeepLinkRedirectScheme();

            if (!PlayerSettings.macOS.urlSchemes.Contains(deepLinkRedirectScheme))
            {
                List<string> schemes = new List<string>();
                //schemes = PlayerSettings.macOS.urlSchemes.ToList();
                schemes.Add(deepLinkRedirectScheme);
                PlayerSettings.macOS.urlSchemes = schemes.ToArray();
                Debug.Log("New URL schemes added : " + deepLinkRedirectScheme);
            }
        }
    }
}
#endif
