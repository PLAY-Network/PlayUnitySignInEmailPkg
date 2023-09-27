#if UNITY_IOS
using System;
using System.Runtime.InteropServices;

namespace RGN.Modules.SignIn.iOS
{
    public sealed class WebViewPlugin
    {
        [DllImport("__Internal")]
        private static extern void _RGNWebViewPlugin_OpenURL(string url);
        [DllImport("__Internal")]
        private static extern void _RGNWebViewPlugin_SetBackButtonText(string urlScheme);
        [DllImport("__Internal")]
        private static extern void _RGNWebViewPlugin_SetURLScheme(string urlScheme);

        public static void OpenURL(string url)
        {
            _RGNWebViewPlugin_OpenURL(url);
        }
        public static void ChangeURLScheme(string newScheme)
        {
            _RGNWebViewPlugin_SetURLScheme(newScheme);
        }
        public static void SetBackButtonText(string backButtonText)
        {
            _RGNWebViewPlugin_SetBackButtonText(backButtonText);
        }
    }
}
#endif
