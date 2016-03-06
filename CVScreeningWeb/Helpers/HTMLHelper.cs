using System.Text.RegularExpressions;
using System.Web;

namespace CVScreeningWeb.Helpers
{
    public static class HTMLHelper
    {
        public static string BaseUrl()
        {
            var request = HttpContext.Current.Request;
            var baseUrl = string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);
            return baseUrl;
        }

        public static string CleanHtmlReport(string original)
        {
            original = original.Replace(@"&rdquo;", "\"");  // need to replace with double quotes
            original = original.Replace(@"&ldquo;", "\"");  // need to replace with double quotes
            original = original.Replace(@"&rsquo;", "'");   // need to replace with single quotes
            original = original.Replace(@"&lsquo;", "'");   // need to replace with single quotes
            original = original.Replace(@"&lsquo;", "'");   // need to replace with single quotes
            original = original.Replace(@"&lsaquo;", "<");   // need to replace with <
            original = original.Replace(@"&rsaquo;", ">");   // need to replace with >
            original = original.Replace(@"&hellip;", "...");   // need to replace with >

            return Regex.Replace(original.Replace(@"&nbsp;", " "), @"\s{2,}", " ");
        }
        
    }
}