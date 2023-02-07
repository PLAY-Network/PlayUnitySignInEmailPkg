using System;
using UnityEngine;

namespace RGN.Modules.SignIn
{
    internal sealed class RGNDeepLink : System.IDisposable
    {
        internal event Action<string> TokenReceived;
        const string signInURL = "https://rgn-auth.web.app";
        public string deeplinkURL;

        private bool _initialized;

        internal void Init()
        {
#if UNITY_STANDALONE_WIN
            WindowsDeepLinks.StartHandling();
#endif
            Application.deepLinkActivated += OnDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }
        public void Dispose()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
            TokenReceived = null;
        }

        internal void OpenURL()
        {
            Application.OpenURL(signInURL);
        }

        private void OnDeepLinkActivated(string url)
        {
            Debug.Log("OnDeepLinkActivated with url: " + url);
            // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
            deeplinkURL = url;

            // Decode the URL
            string parameters = url.Split("?"[0])[1];
            var parsedParameters = RGNHttpUtility.ParseQueryString(parameters);

            string token = parsedParameters["token"]; // TODO Do something with the token

            Debug.Log(token);
            TokenReceived?.Invoke(token);
        }
    }
}
