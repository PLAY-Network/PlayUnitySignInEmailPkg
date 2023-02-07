#if UNITY_EDITOR && PLATFORM_IOS
using System.Collections.Generic;
using System.Linq;
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
            string packageNameScheme = PlayerSettings.applicationIdentifier.ToLower().Replace(".", string.Empty);

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
