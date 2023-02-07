#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace RGN.MyEditor
{
    public class PlayerSettingsForceSingleInstance : MonoBehaviour, IActiveBuildTargetChanged
    {
        public int callbackOrder { get { return 0; } }
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            SetForceSingleInstance();
        }

        private void OnValidate()
        {
            SetForceSingleInstance();
        }

        private void SetForceSingleInstance()
        {
            PlayerSettings.forceSingleInstance = true;
        }
    }
}
#endif
