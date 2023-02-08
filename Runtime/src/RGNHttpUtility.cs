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

        //public static string GetSanitizedRGNProjecId()
        //{
        //    ApplicationStore applicationStore = ApplicationStore.LoadFromResources();
        //    string projectId = applicationStore.RGNProjectId;
        //    return projectId.
        //        ToLower().
        //        Replace(".", string.Empty).
        //        Replace("-", string.Empty).
        //        Replace("_", string.Empty);
        //}

        public static string GetDeepLinkRedirectScheme()
        {
            return (Application.companyName + Application.productName).ToLower().Replace(".", string.Empty);
        }
    }
}
