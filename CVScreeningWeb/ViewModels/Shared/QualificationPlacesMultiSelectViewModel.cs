using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CVScreeningCore.Models;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class QualificationPlacesMultiSelectViewModel
    {
        [LocalizedDisplayName("NotApplicable", NameResourceType = typeof(Resources.Qualification))]
        public RadioButtonNotApplicableViewModel NotApplicable { get; set; }

        [LocalizedDisplayName("WrongQualification", NameResourceType = typeof(Resources.Qualification))]
        public string WrongQualification { get; set; }

        public IEnumerable<int> WrongQualificationIds { get; set; }

        [UIHint("BoolCheckBox")]
        [LocalizedDisplayName("HasBeenRequalified", NameResourceType = typeof(Resources.Qualification))]
        public bool HasBeenRequalified { get; set; }

        [UIHint("StringTextArea")]
        [LocalizedDisplayName("AlreadyQualified", NameResourceType = typeof(Resources.Qualification))]
        public string AlreadyQualified { get; set; }

        [UIHint("MultiSelectKendoUiViewModel")]
        [LocalizedDisplayName("Empty", NameResourceType = typeof(Resources.Common))]
        public MultiSelectKendoUiViewModel MultiSelectKendoUiViewModel { get; set; }

        public TypeOfCheckEnum TypeOfCheckCodesCompatible { get; set; }

        public string AtomicCheckType { get; set; }

    }
}