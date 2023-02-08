#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER && PLATFORM_STANDALONE_OSX
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build;
using RGN.Modules.SignIn;

namespace RGN.MyEditor {
    public class MacOSAddSchemes : MonoBehaviour, IActiveBuildTargetChanged
    {
        public int callbackOrder { get { return 0; } }
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            AddSchemes();
        }

        private void OnValidate()
        {
            AddSchemes();
        }

        private void AddSchemes()
        {
            string packageNameScheme = RGNHttpUtility.GetSanitizedApplicationIdentifier();
            string path = RGNHttpUtility.EMAIL_SIGN_IN_PATH;
            string urlScheme = packageNameScheme + ":" + path;

            if (!PlayerSettings.macOS.urlSchemes.Contains(urlScheme))
            {
                List<string> schemes = new List<string>();
                schemes = PlayerSettings.macOS.urlSchemes.ToList();
                schemes.Add(urlScheme);
                PlayerSettings.macOS.urlSchemes = schemes.ToArray();
                Debug.Log("New URL schemes added : " + urlScheme);
            }
        }
    }
}
#endif
