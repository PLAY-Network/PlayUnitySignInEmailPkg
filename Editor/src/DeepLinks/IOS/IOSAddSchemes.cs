#if UNITY_EDITOR && PLATFORM_IOS
using System.Collections.Generic;
using System.Linq;
using RGN.Modules.SignIn;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;


namespace RGN.MyEditor
{
    public class IOSAddSchemes : MonoBehaviour, IActiveBuildTargetChanged
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

            if (!PlayerSettings.iOS.iOSUrlSchemes.Contains(urlScheme))
            {
                List<string> schemes = PlayerSettings.iOS.iOSUrlSchemes.ToList();
                schemes.Add(urlScheme);
                PlayerSettings.iOS.iOSUrlSchemes = schemes.ToArray();
                Debug.Log("New URL schemes added : " + urlScheme);
            }
        }
    }
}
#endif
