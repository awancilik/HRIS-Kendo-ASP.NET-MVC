using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class MultiSelectViewModel
    {
        /// <summary>
        /// Selected items
        /// </summary>
        public IEnumerable<string> SelectedItems { get; set; }

        /// <summary>
        /// Text field retrieved from the action method
        /// </summary>
        public string DataTextField { get; set; }

        /// <summary>
        /// Value field retrieved from the action method
        /// </summary>
        public string DataValueField { get; set; }

        /// <summary>
        /// Message to display on the top of the dropdown list
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// Controller invoked to retrieve all the items of the list
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Action invoked to retrieve all the items of the list
        /// </summary>
        public string Action { get; set; }
    }
}