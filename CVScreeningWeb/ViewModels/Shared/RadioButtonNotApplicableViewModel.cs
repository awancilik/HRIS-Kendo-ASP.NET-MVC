using System.Collections.Generic;
using System.Web.Mvc;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class RadioButtonNotApplicableViewModel
    {
        /// <summary>
        /// Value to define if the field is applicable or not
        /// </summary>
        public bool IsNotApplicable { get; set; }

        /// <summary>
        /// Id of the radio box, should be unique by page
        /// </summary>
        public string Id { get; set; }
    }
}