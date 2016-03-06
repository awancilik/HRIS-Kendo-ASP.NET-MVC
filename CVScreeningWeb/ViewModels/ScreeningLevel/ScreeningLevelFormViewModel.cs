using System.Collections.Generic;
using System.Web.UI.WebControls;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Shared;
using Foolproof;
using System;
using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Resources;
namespace CVScreeningWeb.ViewModels.ScreeningLevel
{


    public class ScreeningLevelFormViewModel
    {

        /// <summary>
        /// Contract identifier
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Screening level identifier
        /// </summary>
        public int ScreeningLevelId { get; set; }

        /// <summary>
        /// Screening level version identifier
        /// </summary>
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int ScreeningLevelVersionId { get; set; }

        /// <summary>
        /// Contract reference
        /// </summary>
        [LocalizedDisplayName("ReferenceNumber", NameResourceType = typeof(Resources.ClientCompany))]
        public string ContractReference { get; set; }

        /// <summary>
        /// Contract year
        /// </summary>
        [LocalizedDisplayName("Year", NameResourceType = typeof(Resources.Common))]
        public string ContractYear { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("Name", NameResourceType = typeof(Resources.Common))]
        public string ScreeningLevelName { get; set; }

        [LocalizedDisplayName("Description", NameResourceType = typeof(Resources.Common))]
        public string Description { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("StartDate", NameResourceType = typeof(Resources.Common))]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("EndDate", NameResourceType = typeof(Resources.Common))]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("AllowedToContact", NameResourceType = typeof(Resources.ScreeningLevel))]
        public bool AllowedToContact { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("AllowedToContactCurrentCompany", NameResourceType = typeof(Resources.ScreeningLevel))]
        public DropDownListViewModel AllowedToContactCurrentCompany { get; set; }

        [LocalizedDisplayName("VersionNumber", NameResourceType = typeof(Resources.ScreeningLevel))]
        public int VersionNumber { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("TurnaroundTime", NameResourceType = typeof(Resources.ScreeningLevel))]
        public int TurnaroundTime { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Resources.Validation))]
        [LocalizedDisplayName("Language", NameResourceType = typeof(Resources.ScreeningLevel))]
        public string Language { get; set; }
 
        public List<TypeOfCheckForScreeningLevelViewModel> TypeOfChecks { get; set; }
    }

    public class TypeOfCheckForScreeningLevelViewModel
    {

        public CheckBox TypeOfCheckBox { get; set; }

        public int TypeOfCheckId { get; set; }

        public string TypeOfCheckName { get; set; }

        [LocalizedDisplayName("Comment", NameResourceType = typeof(Resources.Common))]
        public string TypeOfCheckScreeningComments { get; set; }


    }
}