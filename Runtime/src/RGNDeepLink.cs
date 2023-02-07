using System;
using UnityEngine;

namespace RGN.Modules.SignIn
{
    internal sealed class RGNDeepLink : System.IDisposable
    {
        internal event Action<string> TokenReceived;
        const string signInURL = "https://rgn-auth.web.app?redirect_url=";
        public string deeplinkURL;

        private bool _initialized;

        internal void Init(IRGNRolesCore rGNCore)
        {
#if UNITY_STANDALONE_WIN
            WindowsDeepLinks.StartHandling();
            WindowsDeepLinks.DeepLinkActivated += onDeepLinkActivated;
            rGNCore.UpdateEvent += () => WindowsDeepLinks.Tick();
#endif

            Application.deepLinkActivated += OnDeepLinkActivated;

            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }


        private void OnApplicationQuit()
        {
#if UNITY_STANDALONE_WIN
        WindowsDeepLinks.Dispose();
        WindowsDeepLinks.DeepLinkActivated -= onDeepLinkActivated;
#endif
        }

        public void Dispose()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
#if UNITY_STANDALONE_WIN
            WindowsDeepLinks.DeepLinkActivated -= onDeepLinkActivated;
            WindowsDeepLinks.Dispose();
#endif
            TokenReceived = null;
        }

        internal void OpenURL()
        {
            string redirectUrl = Application.identifier.ToLower().Replace(".", string.Empty);
            Application.OpenURL(signInURL + redirectUrl); // Send the deeplink redirect url
        }

        private void OnDeepLinkActivated(string url)
        {
            Debug.Log("OnDeepLinkActivated with url: " + url);
            // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
            deeplinkURL = url;

            // Decode the URL
            string parameters = url.Split("?"[0])[1];
            var parsedParameters = RGNHttpUtility.ParseQueryString(parameters);

            string token = parsedParameters["token"];

            TokenReceived?.Invoke(token);
        }
    }
}
