using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;

namespace RGN.Modules.SignIn
{
    public static class RGNHttpUtility
    {
        public const string EMAIL_SIGN_IN_PATH = "emailSignIn";

        public static NameValueCollection ParseQueryString(string query)
        {
            var ret = new NameValueCollection();
            foreach (string pair in query.Split('&'))
            {
                string[] kv = pair.Split('=');

                string key = kv.Length == 1
                    ? null : Uri.UnescapeDataString(kv[0]).Replace('+', ' ');

                string[] values = Uri.UnescapeDataString(
                    kv.Length == 1 ? kv[0] : kv[1]).Replace('+', ' ').Split(',');

                foreach (string value in values)
                {
                    ret.Add(key, value);
                }
            }
            return ret;
        }

        public static string GetDeepLinkRedirectScheme()
        {
            #if UNITY_EDITOR
            return $"http://{IPAddress.Loopback}:{GetRandomUnusedPort()}/";
#else
            ApplicationStore applicationStore = ApplicationStore.LoadFromResources();
            string projectId = "rgn" + applicationStore.RGNProjectId;
            return projectId.
                ToLower().
                Replace(".", string.Empty).
                Replace("-", string.Empty).
                Replace("_", string.Empty);
#endif
        }
        
        private static int GetRandomUnusedPort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
