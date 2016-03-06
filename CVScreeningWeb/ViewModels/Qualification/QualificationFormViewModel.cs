using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.ViewModels.Contact;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.Qualification
{
    public class QualificationFormViewModel
    {
        /// <summary>
        /// Screening id of the screening to qualify
        /// </summary>
        public int ScreeningId { get; set; }

        public string PreviousPage { get; set; }

        /// <summary>
        /// Type of check list used to know whether a type of check is defined or not for this screening
        /// </summary>
        public IList<bool> TypeOfChecksPresent;
            
        [UIHint("StringTextBox")]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        [LocalizedDisplayName("FullName", NameResourceType = typeof(Resources.Common))]
        public string FullName { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        [LocalizedDisplayName("MartialStatus", NameResourceType = typeof(Resources.Qualification))]
        public DropDownListViewModel MartialStatus { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        [LocalizedDisplayName("Gender", NameResourceType = typeof(Resources.Qualification))]
        public DropDownListViewModel Gender { get; set; }

        [UIHint("DateTimeCalendar")]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        [LocalizedDisplayName("BirthDate", NameResourceType = typeof(Resources.Qualification))]
        public DateTime? BirthDate { get; set; }

        [UIHint("StringTextBox")]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Validation))]
        [LocalizedDisplayName("BirthPlace", NameResourceType = typeof(Resources.Qualification))]
        public string BirthPlace { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("IdCardNumber", NameResourceType = typeof(Resources.Qualification))]
        public string IdCardNumber { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("PassportNumber", NameResourceType = typeof(Resources.Qualification))]
        public string PassportNumber { get; set; }

        [LocalizedDisplayName("PhoneNumberIsNotApplicable", NameResourceType = typeof(Resources.Qualification))]
        public RadioButtonNotApplicableViewModel HomePhoneNumberIsNotApplicable { get; set; }

        [LocalizedDisplayName("HomePhoneNumber", NameResourceType = typeof(Resources.Qualification))]
        public PhoneViewModel HomePhoneNumber { get; set; }

        [LocalizedDisplayName("PhoneNumberIsNotApplicable", NameResourceType = typeof(Resources.Qualification))]
        public RadioButtonNotApplicableViewModel MobilePhoneNumberIsNotApplicable { get; set; }

        [LocalizedDisplayName("MobilePhoneNumber", NameResourceType = typeof(Resources.Qualification))]
        public PhoneViewModel MobilePhoneNumber { get; set; }

        [LocalizedDisplayName("EmergencyContactIsNotApplicable", NameResourceType = typeof(Resources.Qualification))]
        public RadioButtonNotApplicableViewModel EmergencyContactIsNotApplicable { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("EmergencyContactName", NameResourceType = typeof(Resources.Qualification))]
        public string EmergencyContactName { get; set; }

        [LocalizedDisplayName("EmergencyContactRelationship", NameResourceType = typeof(Resources.Qualification))]
        public DropDownListViewModel EmergencyContactRelationship { get; set; }

        [LocalizedDisplayName("EmergencyPhoneNumber", NameResourceType = typeof(Resources.Qualification))]
        public PhoneViewModel EmergencyPhoneNumber { get; set; }

        [LocalizedDisplayName("CurrentAddressNotApplicable", NameResourceType = typeof(Resources.Qualification))]
        public RadioButtonNotApplicableViewModel CurrentAddressNotApplicable { get; set; }

        public bool CurrentAddressWrongQualification { get; set; }

        [UIHint("BoolCheckBox")]
        [LocalizedDisplayName("HasBeenRequalified", NameResourceType = typeof(Resources.Qualification))]
        public bool CurrentAddressHasBeenRequalified { get; set; }

        [AddressOptional]
        [UIHint("AddressOptionalViewModel")]
        [LocalizedDisplayName("CurrentAddress", NameResourceType = typeof(Resources.Qualification))]
        public AddressOptionalViewModel CurrentAddressViewModel { get; set; }

        [LocalizedDisplayName("IDAddressNotApplicable", NameResourceType = typeof(Resources.Qualification))]
        public RadioButtonNotApplicableViewModel IDAddressNotApplicable { get; set; }

        public bool IDAddressWrongQualification { get; set; }

        [UIHint("BoolCheckBox")]
        [LocalizedDisplayName("HasBeenRequalified", NameResourceType = typeof(Resources.Qualification))]
        public bool IDAddressHasBeenRequalified { get; set; }

        [AddressOptional]
        [UIHint("AddressOptionalViewModel")]
        [LocalizedDisplayName("IDCardAddress", NameResourceType = typeof(Resources.Qualification))]
        public AddressOptionalViewModel IDCardAddressViewModel { get; set; }

        [LocalizedDisplayName("CVAddressNotApplicable", NameResourceType = typeof(Resources.Qualification))]
        public RadioButtonNotApplicableViewModel CVAddressNotApplicable { get; set; }

        public bool CVAddressWrongQualification { get; set; }

        [UIHint("BoolCheckBox")]
        [LocalizedDisplayName("HasBeenRequalified", NameResourceType = typeof(Resources.Qualification))]
        public bool CVAddressHasBeenRequalified { get; set; }

        [AddressOptional]
        [UIHint("AddressOptionalViewModel")]
        [LocalizedDisplayName("CVAddress", NameResourceType = typeof(Resources.Qualification))]
        public AddressOptionalViewModel CVAddressViewModel { get; set; }

        [LocalizedDisplayName("Current Company", NameResourceType = typeof (Resources.Qualification))]
        public QualificationPlacesDropDownListViewModel CurrentCompany { get; set; }

        [LocalizedDisplayName("Company", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesMultiSelectViewModel Company { get; set; }

        [LocalizedDisplayName("DistrictCourts", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesMultiSelectViewModel DistrictCourts { get; set; }

        [LocalizedDisplayName("CommercialCourts", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesMultiSelectViewModel CommercialCourts { get; set; }

        [LocalizedDisplayName("IndustrialCourts", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesMultiSelectViewModel IndustrialCourts { get; set; }

        [LocalizedDisplayName("Police", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesMultiSelectViewModel Police { get; set; }

        [LocalizedDisplayName("HighSchool", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesMultiSelectViewModel HighSchool { get; set; }

        [LocalizedDisplayName("DrivingLicenseOffice", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesDropDownListViewModel DrivingLicenseOffice { get; set; }

        [LocalizedDisplayName("ImmigrationOffice", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesDropDownListViewModel ImmigrationOffice { get; set; }
        
        [LocalizedDisplayName("Faculty", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesCascadingViewModel Faculty { get; set; }

        [LocalizedDisplayName("CertificationPlace", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesCascadingViewModel CertificationPlace { get; set; }

        [LocalizedDisplayName("PopulationOffice", NameResourceType = typeof(Resources.Qualification))]
        public QualificationPlacesDropDownListViewModel PopulationOffice { get; set; }
    }
}