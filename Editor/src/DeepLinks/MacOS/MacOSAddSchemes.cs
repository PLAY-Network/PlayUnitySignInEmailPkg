#if UNITY_EDITOR && PLATFORM_STANDALONE_OSX
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Build;

namespace RGN.MyEditor
{
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
            if (!PlayerSettings.macOS.urlSchemes.Contains("unitydl"))
            {
                List<string> schemes = new List<string>();
                schemes = PlayerSettings.macOS.urlSchemes.ToList();
                schemes.Add("unitydl");
                PlayerSettings.macOS.urlSchemes = schemes.ToArray();
                Debug.Log("New URL schemes added : unitydl");
            }
        }
    }
}
#endif
