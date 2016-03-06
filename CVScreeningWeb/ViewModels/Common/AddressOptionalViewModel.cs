using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using Foolproof;

namespace CVScreeningWeb.ViewModels.Common
{
    public class AddressOptionalViewModel
    {
        /// <summary>
        /// Name of the variable, it is used in editorfor template to perform cascading
        /// </summary>
        public string Name;

        [LocalizedDisplayName("Street", NameResourceType = typeof(Resources.Common))]
        public string Street { get; set; }

        [RegularExpression(@"[0-9]+", ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "IncorrectPostalCode")]
        [LocalizedDisplayName("PostalCode", NameResourceType = typeof(Resources.Common))]
        public string PostalCode { get; set; }

        [LocalizedDisplayName("FullAddress", NameResourceType = typeof(Resources.Common))]
        public string FullAddress { get; set; }

        [UIHint("LocationOptionalViewModel")]
        public LocationOptionalViewModel LocationViewModel { get; set; }

    }
}