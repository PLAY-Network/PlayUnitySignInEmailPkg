using System;
using System.Collections.Specialized;
using UnityEngine;

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
        public static string GetDeepLinkRedirectUrlForEmailSignIn()
        {
            string appIdentifier = GetSanitizedApplicationIdentifier();
#if UNITY_ANDROID
            // <scheme>://<host>:<port>/<path>
            string redirectUrl = appIdentifier + "://localhost/" + EMAIL_SIGN_IN_PATH;
#else
            // myphotoapp:albumname?name="albumname"
            // myphotoapp:albumname?index=1
            string redirectUrl = appIdentifier + ":" + EMAIL_SIGN_IN_PATH;
#endif
            return redirectUrl;
        }
        public static string GetSanitizedApplicationIdentifier()
        {
            return Application.identifier.
                ToLower().
                Replace(".", string.Empty).
                Replace("-", string.Empty).
                Replace("_", string.Empty);
        }
    }
}
