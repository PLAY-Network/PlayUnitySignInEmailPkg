using System;
using UnityEngine;

namespace RGN.Modules.SignIn
{
    internal sealed class RGNDeepLink : System.IDisposable
    {
        private const string SIGN_IN_URL = "https://rgn-auth.web.app/?url_redirect=";

        internal event Action<string> TokenReceived;

        private string _finalSignInUrl;
        private bool _initialized;

        internal void Init(IRGNRolesCore rGNCore)
        {
            if (_initialized)
            {
                return;
            }
#if UNITY_STANDALONE_WIN
            WindowsDeepLinks.StartHandling();
            WindowsDeepLinks.DeepLinkActivated += OnDeepLinkActivated;
            rGNCore.UpdateEvent += WindowsDeepLinks.Tick;
#endif
            string redirectUrl = RGNHttpUtility.GetDeepLinkRedirectScheme();
            _finalSignInUrl = SIGN_IN_URL + redirectUrl;
            Application.deepLinkActivated += OnDeepLinkActivated;

            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                OnDeepLinkActivated(Application.absoluteURL);
            }
            _initialized = true;
        }


        private void OnApplicationQuit()
        {
#if UNITY_STANDALONE_WIN
            WindowsDeepLinks.Dispose();
            WindowsDeepLinks.DeepLinkActivated -= OnDeepLinkActivated;
#endif
        }

        public void Dispose()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
#if UNITY_STANDALONE_WIN
            WindowsDeepLinks.DeepLinkActivated -= OnDeepLinkActivated;
            WindowsDeepLinks.Dispose();
#endif
            TokenReceived = null;
        }

        internal void OpenURL()
        {
            Application.OpenURL(_finalSignInUrl); // Send the deeplink redirect url
        }

        private void OnDeepLinkActivated(string url)
        {
            Debug.Log("OnDeepLinkActivated with url: " + url);
            string parameters = url.Split("?"[0])[1];
            var parsedParameters = RGNHttpUtility.ParseQueryString(parameters);

            string token = parsedParameters["token"];

            TokenReceived?.Invoke(token);
        }
    }
}
