using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Foolproof;

namespace CVScreeningWeb.ViewModels.Contact
{
    public class PhoneViewModel
    {
        /// <summary>
        /// Used for edit form
        /// </summary>
        public string CountryCode { get; set; }
        public IEnumerable<SelectListItem> SelectListItems { get; set; }

        [RequiredIf("IsMandatory", true, ErrorMessageResourceType = typeof(Resources.Contact), ErrorMessageResourceName = "IncorrectPhoneNumber")]
        [RegularExpression(@"[0-9]{6,16}$", ErrorMessageResourceType = typeof(Resources.Contact), ErrorMessageResourceName = "IncorrectPhoneNumber")]
        public string AreaCode { get; set; }


        /// <summary>
        /// Used for dispay form
        /// </summary>
        public string FullNumber { get; set; }

        public bool IsMandatory { get; set; }
    }
}