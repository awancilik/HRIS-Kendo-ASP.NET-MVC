using System.Web.Mvc;

namespace CVScreeningWeb.Helpers
{
    public static class LayoutHelper
    {
        /// <summary>
        /// Convert the number of pending days within a string
        /// </summary>
        /// <param name="pendingDays"></param>
        /// <returns></returns>
        public static string GetPendingDaysAsString(int pendingDays)
        {
            return pendingDays > 1 ? pendingDays + " days" : pendingDays + " day";
        }

        /// <summary>
        /// To Draw a grid view based on bootstrap css.
        /// </summary>
        /// <param name="value">Content HTML to render</param>
        /// <param name="panelClass">[Opt] Put class for panel</param>
        /// <param name="gridClass">[Opt] Put class for the grid content</param>
        /// <param name="headerPanelName">[Opt] In case the grid view has a header</param>
        /// <returns></returns>
        public static MvcHtmlString DrawPanel(
            MvcHtmlString value, string headerPanelName = null,
            string panelClass = null, string gridClass = null)
        {
            var startTag =
                new MvcHtmlString(string.Format(@"<div class='{0}'><section class='panel'>", panelClass ?? "col-md-12"));

            if (headerPanelName != null)
            {
                startTag = new MvcHtmlString(
                    startTag.ToHtmlString() + @"<header class='panel-heading'>"
                    + headerPanelName + @"</header>");
            }

            var content = new MvcHtmlString(
                string.Format(@"<div class='panel-body'><div class='{0}'>", gridClass ?? "col-md-12") + value + @"</div>
                            </div>");
            var endTag = new MvcHtmlString(@"
                        </section>
                    </div>");

            return new MvcHtmlString(startTag.ToHtmlString() + content.ToHtmlString() + endTag.ToHtmlString());
        }

        public static MvcHtmlString Image(this HtmlHelper helper, string src, string altText, string height = null)
        {
            var builder = new TagBuilder("img");
            builder.MergeAttribute("src", src);
            builder.MergeAttribute("alt", altText);
            builder.MergeAttribute("height", height);
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }

    }
}