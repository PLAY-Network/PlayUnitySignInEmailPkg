using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace RGN.Modules.SignIn
{
    internal sealed class RGNDeepLink : System.IDisposable
    {
        internal event Action<bool, string> TokenReceived;

        private HttpListener _editorHttpListener;
        private string _redirectUrl;
        private string _baseSignInUrl;
        private string _finalSignInUrl;
        private bool _initialized;

        internal void Init(IRGNRolesCore rGNCore)
        {
            if (_initialized)
            {
                return;
            }
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            WindowsDeepLinks.StartHandling();
            WindowsDeepLinks.DeepLinkActivated += OnDeepLinkActivated;
            rGNCore.UpdateEvent += WindowsDeepLinks.Tick;
#endif
            
#if UNITY_EDITOR
            _redirectUrl = RGNHttpUtility.GetEditorRedirectScheme();
#else
            _redirectUrl = RGNHttpUtility.GetDeepLinkRedirectScheme();
#endif
#if UNITY_IOS && !UNITY_EDITOR
            iOS.WebViewPlugin.ChangeURLScheme(_redirectUrl);
            iOS.WebViewPlugin.SetBackButtonText("Back");
#endif

            _baseSignInUrl = GetEmailSignInURL();
            _finalSignInUrl =
                _baseSignInUrl +
                _redirectUrl +
                "&returnSecureToken=false" +
                "&appId=" + RGNCore.I.AppIDForRequests +
                "&lang=" + Utility.LanguageUtility.GetISO631Dash1CodeFromSystemLanguage();
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
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            WindowsDeepLinks.Dispose();
            WindowsDeepLinks.DeepLinkActivated -= OnDeepLinkActivated;
#endif
        }

        public void Dispose()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
            
#if UNITY_EDITOR
            _editorHttpListener?.Stop();
            _editorHttpListener?.Close();
#endif
            
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            WindowsDeepLinks.DeepLinkActivated -= OnDeepLinkActivated;
            WindowsDeepLinks.Dispose();
#endif
            
            TokenReceived = null;
        }

        internal void OpenURL()
        {
#if UNITY_EDITOR
            HandleDeepLinkInEditorAsync();
#endif
#if UNITY_IOS && !UNITY_EDITOR
            iOS.WebViewPlugin.OpenURL(_finalSignInUrl);
#else
            Application.OpenURL(_finalSignInUrl); // Send the deeplink redirect url
#endif
        }

        private async void HandleDeepLinkInEditorAsync()
        {
#if UNITY_EDITOR
            _editorHttpListener ??= new HttpListener();
            _editorHttpListener.Prefixes.Add(_redirectUrl);
            _editorHttpListener.Start();

            HttpListenerContext context;
            try { context = await _editorHttpListener.GetContextAsync(); }
            catch (ObjectDisposedException) { return; }
            HttpListenerResponse response = context.Response;
            
            string url = context.Request.Url.ToString();
            
            response.Close();
            _editorHttpListener.Stop();
            
            OnDeepLinkActivated(url);
#endif
        }

        private void OnDeepLinkActivated(string url)
        {
            Debug.Log("OnDeepLinkActivated with url: " + url);
            
            // we need sync here to the dashboard to have same vision for cancelled tokens
            if (url.Contains("cancelled"))
            {
                TokenReceived?.Invoke(true, "");
                return;
            }
            
            string parameters = url.Split("?"[0])[1];
            var parsedParameters = RGNHttpUtility.ParseQueryString(parameters);
            string token = parsedParameters["token"];

            TokenReceived?.Invoke(false, token);
        }

        private string GetEmailSignInURL()
        {
            ApplicationStore applicationStore = ApplicationStore.LoadFromResources();
            string baseURL = applicationStore.GetRGNStagingEmailSignInURL;
            if (applicationStore.GetRGNEnvironment == EnumRGNEnvironment.Production)
            {
                baseURL = applicationStore.GetRGNProductionEmailSignInURL;
            }
            else if (applicationStore.GetRGNEnvironment == EnumRGNEnvironment.Development)
            {
                baseURL = applicationStore.GetRGNDevelopmentEmailSignInURL;
            }
            return baseURL;
        }
    }
}
