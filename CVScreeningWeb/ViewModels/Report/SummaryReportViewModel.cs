using System;
using System.Collections.Generic;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Report
{
    [Serializable]
    public class SummaryReportViewModel
    {
        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Report))]
        public string Name { get; set; }

        [LocalizedDisplayName("PlaceOfBirth", NameResourceType = typeof (Resources.Report))]
        public string PlaceOfBirth { get; set; }

        [LocalizedDisplayName("DateOfBirth", NameResourceType = typeof(Resources.Report))]
        public string DateOfBirth { get; set; }

        [LocalizedDisplayName("IDCardNumber", NameResourceType = typeof(Resources.Report))]
        public string IDCardNumber { get; set; }

        [LocalizedDisplayName("PassportNumber", NameResourceType = typeof(Resources.Report))]
        public string PassportNumber { get; set; }

        [LocalizedDisplayName("MaritalStatus", NameResourceType = typeof(Resources.Report))]
        public string MaritalStatus { get; set; }

        [LocalizedDisplayName("CVAddress", NameResourceType = typeof (Resources.Report))]
        public string CVAddress { get; set; }

        [LocalizedDisplayName("CurrentAddress", NameResourceType = typeof(Resources.Report))]
        public string CurrentAddress { get; set; }

        [LocalizedDisplayName("IDCardAddress", NameResourceType = typeof(Resources.Report))]
        public string IDCardAddress { get; set; }

        [LocalizedDisplayName("MobilePhoneNumber", NameResourceType = typeof(Resources.Report))]
        public string MobilePhoneNumber { get; set; }

        [LocalizedDisplayName("HomePhoneNumber", NameResourceType = typeof (Resources.Report))]
        public string HomePhoneNumber { get; set; }

        [LocalizedDisplayName("EmergencyContactName", NameResourceType = typeof(Resources.Report))]
        public string EmergencyContactName { get; set; }

        [LocalizedDisplayName("EmergencyContactNumber", NameResourceType = typeof (Resources.Report))]
        public string EmergencyContactNumber { get; set; }

        public IEnumerable<TypeOfCheckVerificationViewModel> TypeOfChecks { get; set; }

        [LocalizedDisplayName("Remarks", NameResourceType = typeof(Resources.Common))]
        public string AdditionnalRemarks { get; set; }
    }
}