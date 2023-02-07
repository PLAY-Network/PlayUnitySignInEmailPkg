using System;
using System.Collections.Specialized;

namespace RGN.Modules.SignIn
{
    internal static class RGNHttpUtility
    {
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
    }
}
