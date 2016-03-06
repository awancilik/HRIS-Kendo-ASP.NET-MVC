using System;

namespace CVScreeningWeb.Helpers
{
    public class TenantHelper
    {

        public static Byte GetTenantId(string hostname)
        {
            return hostname.Contains("tenant2") ? (Byte) 2 : (Byte) 1;
        }

    }
}