using System.Web.Mvc;

namespace CVScreeningWeb.Helpers
{
    public static class ActionHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="evaluation"></param>
        /// <param name="falseValue"></param>
        /// <returns></returns>
        public static MvcHtmlString If(
            this MvcHtmlString value,
            bool evaluation,
            MvcHtmlString falseValue = default(MvcHtmlString))
        {
            return evaluation ? value : falseValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="evaluation"></param>
        /// <param name="falseValue"></param>
        /// <returns></returns>
        public static MvcHtmlString IfLi(
            this MvcHtmlString value,
            bool evaluation,
            MvcHtmlString falseValue = default(MvcHtmlString))
        {
            if (evaluation)
            {
                return new MvcHtmlString("<li>" + value + "</li>");
            }
            return falseValue;
        }

        
    }
}