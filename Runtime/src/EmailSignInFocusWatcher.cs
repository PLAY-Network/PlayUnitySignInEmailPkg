using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RGN.Modules.SignIn
{
    internal class EmailSignInFocusWatcher : MonoBehaviour
    {
        public event Action<EmailSignInFocusWatcher, bool> OnFocusChanged;
        
        public static EmailSignInFocusWatcher Create()
        {
            GameObject watcherGO = new GameObject("EmailSignInFocusWatcher");
            return watcherGO.AddComponent<EmailSignInFocusWatcher>();
        }

        public void Destroy() => Object.Destroy(gameObject);

        private void OnApplicationFocus(bool hasFocus)
        {
            OnFocusChanged?.Invoke(this, hasFocus);
        }
    }
}
