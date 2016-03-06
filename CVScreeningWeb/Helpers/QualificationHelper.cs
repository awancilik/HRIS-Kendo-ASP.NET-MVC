using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningWeb.ViewModels.Qualification;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.Helpers
{
    public class QualificationHelper
    {
        /// <summary>
        ///     Retrieve which education type of check is used for this screening (education check standard or with evidence)
        /// </summary>
        /// <param name="atomicChecksDTO"></param>
        /// <returns></returns>
        private static TypeOfCheckEnum GetEducationTypeOfCheck(IEnumerable<AtomicCheckBaseDTO> atomicChecksDTO)
        {
            if (
                atomicChecksDTO.Any(
                    u => u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EDUCATION_CHECK_STANDARD))
                return TypeOfCheckEnum.EDUCATION_CHECK_STANDARD;
            if (atomicChecksDTO.Any(
                u => u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EDUCATION_CHECK_WITH_EVIDENCE))
                return TypeOfCheckEnum.EDUCATION_CHECK_WITH_EVIDENCE;
            return TypeOfCheckEnum.END_OF_ENUM;
        }

        /// <summary>
        ///     Retrieve which employment type of check is used for this screening (education check standard or with evidence)
        /// </summary>
        /// <param name="atomicChecksDTO"></param>
        /// <returns></returns>
        private static TypeOfCheckEnum GetEmploymentTypeOfCheck(IEnumerable<AtomicCheckBaseDTO> atomicChecksDTO)
        {
            if (atomicChecksDTO.Any(
                    u => u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD))
                return TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD;
            
            if (atomicChecksDTO.Any(
                u => u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_PERFORMANCE))
                return TypeOfCheckEnum.EMPLOYMENT_CHECK_PERFORMANCE;

            return TypeOfCheckEnum.END_OF_ENUM;
        }

        /// <summary>
        ///     Retrieve which litigation type of check is used for this screening (civil, criminal or civil and criminal)
        /// </summary>
        /// <param name="atomicChecksDTO"></param>
        /// <returns></returns>
        private static TypeOfCheckEnum GetLitigationTypeOfCheck(IEnumerable<AtomicCheckBaseDTO> atomicChecksDTO)
        {
            if (atomicChecksDTO.Any(u => u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.LITIGATION_CHECK_CIVIL))
                return TypeOfCheckEnum.LITIGATION_CHECK_CIVIL;
            if (atomicChecksDTO.Any(
                u => u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.LITIGATION_CHECK_CRIMINAL))
                return TypeOfCheckEnum.LITIGATION_CHECK_CRIMINAL;
            if (atomicChecksDTO.Any(
                u => u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.LITIGATION_CHECK_CIVIL_CRIMINAL))
                return TypeOfCheckEnum.LITIGATION_CHECK_CIVIL_CRIMINAL;
            return TypeOfCheckEnum.END_OF_ENUM;
        }

        private static void BuildAddressQualificationFormViewModel(
            ref QualificationFormViewModel qualificationFormViewModel,
            ScreeningQualificationDTO screeningQualificationDTO)
        {
            if (qualificationFormViewModel == null)
                return;

            qualificationFormViewModel.CurrentAddressWrongQualification =
                screeningQualificationDTO.IsCurrentAddressWronglyQualified;
            qualificationFormViewModel.CurrentAddressNotApplicable = new RadioButtonNotApplicableViewModel
            {
                IsNotApplicable = false,
                Id = "CurrentAddress"
            };
            qualificationFormViewModel.CurrentAddressViewModel =
                AddressHelper.BuildAddressOptionalViewModel(
                    screeningQualificationDTO.CurrentAddress, "CurrentAddress");

            qualificationFormViewModel.IDAddressWrongQualification =
                screeningQualificationDTO.IsIDCardAddressWronglyQualified;
            qualificationFormViewModel.IDAddressNotApplicable = new RadioButtonNotApplicableViewModel
            {
                IsNotApplicable = false,
                Id = "IDAddress"
            };
            qualificationFormViewModel.IDCardAddressViewModel =
                AddressHelper.BuildAddressOptionalViewModel(screeningQualificationDTO.IDCardAddress, "IDCardAddress");

            qualificationFormViewModel.CVAddressWrongQualification =
                screeningQualificationDTO.IsCVAddressWronglyQualified;
            qualificationFormViewModel.CVAddressNotApplicable = new RadioButtonNotApplicableViewModel
            {
                IsNotApplicable = false,
                Id = "CVAddress"
            };
            qualificationFormViewModel.CVAddressViewModel =
                AddressHelper.BuildAddressOptionalViewModel(screeningQualificationDTO.CVAddress, "CVAddress");
        }

        /// <summary>
        ///     Generate QualificationFormViewModel for qualification action
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="atomicCheckDTO"></param>
        /// <param name="screeningQualificationDTO"></param>
        /// <param name="qualificationPlacesDTO"></param>
        /// <returns></returns>
        public static QualificationFormViewModel BuildQualificationFormViewModel(
            ScreeningBaseDTO screeningDTO,
            IEnumerable<AtomicCheckBaseDTO> atomicChecksDTO,
            ScreeningQualificationDTO screeningQualificationDTO,
            IEnumerable<BaseQualificationPlaceDTO> qualificationPlacesDTO,
            IEnumerable<BaseQualificationPlaceDTO> wrongQualificationPlacesDTO)
        {
            TypeOfCheckEnum educationTypeOfCheck = GetEducationTypeOfCheck(atomicChecksDTO);
            TypeOfCheckEnum employmentTypeOfCheck = GetEmploymentTypeOfCheck(atomicChecksDTO);
            TypeOfCheckEnum litigationTypeOfCheck = GetLitigationTypeOfCheck(atomicChecksDTO);

            IList<bool> typeOfChecks = TypeOfCheck.InitializeTypeOfChecksCollection(false);
            foreach (AtomicCheckBaseDTO atomicCheckDTO in atomicChecksDTO)
            {
                typeOfChecks[atomicCheckDTO.TypeOfCheckCode] = true;
            }

            var qualificationFormVm = new QualificationFormViewModel
            {
                TypeOfChecksPresent = typeOfChecks,
                ScreeningId = screeningDTO.ScreeningId,
                FullName = screeningDTO.ScreeningFullName,
                Gender = FormHelper.BuildDropDownListViewModel(GetGender(), null,
                    screeningQualificationDTO.ScreeningQualificationGender),
                MartialStatus = FormHelper.BuildDropDownListViewModel(
                    GetMaritalStatus(), null, screeningQualificationDTO.ScreeningQualificationMaritalStatus),
                BirthDate = screeningQualificationDTO.ScreeningQualificationBirthDate ?? DateTime.Now,
                BirthPlace = screeningQualificationDTO.ScreeningQualificationBirthPlace,
                IdCardNumber = screeningQualificationDTO.ScreeningQualificationIDCardNumber,
                PassportNumber = screeningQualificationDTO.ScreeningQualificationPassportNumber,
                HomePhoneNumber = screeningQualificationDTO.PersonalContactInfo == null
                    ? ContactHelper.BuildPhoneNumberViewModel(false)
                    : ContactHelper.BuildPhoneNumberViewModel(false,
                        screeningQualificationDTO.PersonalContactInfo.HomePhoneNumber),
                HomePhoneNumberIsNotApplicable = new RadioButtonNotApplicableViewModel
                {
                    IsNotApplicable = false,
                    Id = "HomePhoneNumber"
                },
                MobilePhoneNumberIsNotApplicable = new RadioButtonNotApplicableViewModel
                {
                    IsNotApplicable = false,
                    Id = "MobilePhoneNumber"
                },
                MobilePhoneNumber = screeningQualificationDTO.PersonalContactInfo == null
                    ? ContactHelper.BuildPhoneNumberViewModel(false)
                    : ContactHelper.BuildPhoneNumberViewModel(false,
                        screeningQualificationDTO.PersonalContactInfo.MobilePhoneNumber),
                EmergencyPhoneNumber = screeningQualificationDTO.EmergencyContactPerson != null
                                       && screeningQualificationDTO.EmergencyContactPerson.ContactInfo != null
                    ? ContactHelper.BuildPhoneNumberViewModel(false,
                        screeningQualificationDTO.EmergencyContactPerson.ContactInfo.HomePhoneNumber)
                    : ContactHelper.BuildPhoneNumberViewModel(false),
                EmergencyContactName = screeningQualificationDTO.EmergencyContactPerson != null
                    ? screeningQualificationDTO.EmergencyContactPerson.ContactPersonName
                    : "",
                EmergencyContactRelationship = FormHelper.BuildDropDownListViewModel(
                    GetRelationship(), null, screeningQualificationDTO.ScreeningQualificationRelationshipWithCandidate),
                EmergencyContactIsNotApplicable = new RadioButtonNotApplicableViewModel
                {
                    IsNotApplicable = false,
                    Id = "EmergencyContact"
                },
                // Current model drop down
                CurrentCompany = new QualificationPlacesDropDownListViewModel
                {
                    DropDownListKendoUiViewModel = new DropDownListKendoUiViewModel
                    {
                        SelectedItem = "",
                        DataTextField = "CompanyName",
                        DataValueField = "CompanyId",
                        Placeholder = "Select model...",
                        Controller = "Company",
                        Action = "GetCompanies"
                    },
                    AtomicCheckType = AtomicCheck.kCurrentCompanyType,
                    AlreadyQualified =
                        BuildQualificationEnumAsString(
                            qualificationPlacesDTO.Where(u => u is CompanyDTO 
                                && CheckIsCurrentCompany(u, screeningDTO))),
                    WrongQualification =
                        BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(u => u is CompanyDTO
                            && CheckIsCurrentCompany(u, screeningDTO))),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is CompanyDTO 
                            && CheckIsCurrentCompany(u, screeningDTO))
                                .Select(u => u.QualificationPlaceId),
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "CurrentCompany"
                    },
                    TypeOfCheckCodesCompatible = employmentTypeOfCheck
                },
                
                // Company multiselect
                Company = new QualificationPlacesMultiSelectViewModel
                {
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "Companies"
                    },
                    AtomicCheckType = AtomicCheck.kOtherCompanyType,
                    WrongQualification =
                        BuildQualificationEnumAsString(wrongQualificationPlacesDTO.
                            Where(u => u is CompanyDTO && !CheckIsCurrentCompany(u, screeningDTO))),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is CompanyDTO 
                            && !CheckIsCurrentCompany(u, screeningDTO)).Select(u => u.QualificationPlaceId),
                    AlreadyQualified =
                        BuildQualificationEnumAsString(qualificationPlacesDTO
                            .Where(u => u is CompanyDTO 
                                && !CheckIsCurrentCompany(u, screeningDTO))),
                    MultiSelectKendoUiViewModel = new MultiSelectKendoUiViewModel
                    {
                        SelectedItems = new List<string>(),
                        DataTextField = "CompanyName",
                        DataValueField = "CompanyId",
                        Placeholder = "Select model...",
                        Controller = "Company",
                        Action = "GetCompanies",
                    },
                    TypeOfCheckCodesCompatible = employmentTypeOfCheck
                },
                // Police multiselect
                Police = new QualificationPlacesMultiSelectViewModel
                {
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "Polices"
                    },
                    AlreadyQualified = BuildQualificationEnumAsString(qualificationPlacesDTO.Where(u => u is PoliceDTO)),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is PoliceDTO).Select(u => u.QualificationPlaceId),
                    WrongQualification =
                        BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(u => u is PoliceDTO)),
                    MultiSelectKendoUiViewModel = new MultiSelectKendoUiViewModel
                    {
                        SelectedItems = new List<string>(),
                        DataTextField = "PoliceName",
                        DataValueField = "PoliceId",
                        Placeholder = "Select polices...",
                        Controller = "Police",
                        Action = "GetPolices"
                    },
                    TypeOfCheckCodesCompatible = TypeOfCheckEnum.POLICE_CHECK,
                },

                // High school multiselect
                HighSchool = new QualificationPlacesMultiSelectViewModel
                {
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "HighSchools"
                    },
                    AlreadyQualified =
                        BuildQualificationEnumAsString(qualificationPlacesDTO.Where(u => u is HighSchoolDTO)),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is HighSchoolDTO).Select(u => u.QualificationPlaceId),
                    WrongQualification =
                        BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(u => u is HighSchoolDTO)),
                    MultiSelectKendoUiViewModel = new MultiSelectKendoUiViewModel
                    {
                        SelectedItems = new List<string>(),
                        DataTextField = "HighSchoolName",
                        DataValueField = "HighSchoolId",
                        Placeholder = "Select high school...",
                        Controller = "HighSchool",
                        Action = "GetHighSchools",
                    },
                    TypeOfCheckCodesCompatible = educationTypeOfCheck,
                    AtomicCheckType = AtomicCheck.kHighSchoolType
                },
                // District courts multiselect
                DistrictCourts = new QualificationPlacesMultiSelectViewModel
                {
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "DistrictCourts"
                    },
                    AlreadyQualified = BuildQualificationEnumAsString(qualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kDistrictCategory && u is CourtDTO)),
                    WrongQualificationIds = wrongQualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kDistrictCategory && u is CourtDTO)
                        .Select(u => u.QualificationPlaceId),
                    WrongQualification = BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kDistrictCategory && u is CourtDTO)),
                    MultiSelectKendoUiViewModel = new MultiSelectKendoUiViewModel
                    {
                        SelectedItems = new List<string>(),
                        DataTextField = "DistrictCourtName",
                        DataValueField = "DistrictCourtId",
                        Placeholder = "Select district court...",
                        Controller = "Court",
                        Action = "GetDistrictCourts",
                    },
                    TypeOfCheckCodesCompatible = litigationTypeOfCheck
                },
                // Commercial courts multiselect
                CommercialCourts = new QualificationPlacesMultiSelectViewModel
                {
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "CommercialCourts"
                    },
                    AlreadyQualified = BuildQualificationEnumAsString(qualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kCommercialCategory && u is CourtDTO)),
                    WrongQualification = BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kCommercialCategory && u is CourtDTO)),
                    WrongQualificationIds = wrongQualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kCommercialCategory && u is CourtDTO)
                        .Select(u => u.QualificationPlaceId),
                    MultiSelectKendoUiViewModel = new MultiSelectKendoUiViewModel
                    {
                        SelectedItems = new List<string>(),
                        DataTextField = "CommercialCourtName",
                        DataValueField = "CommercialCourtId",
                        Placeholder = "Select commercial court...",
                        Controller = "Court",
                        Action = "GetCommercialCourts",
                    },
                    TypeOfCheckCodesCompatible = TypeOfCheckEnum.BANKRUPTY_CHECK,
                },
                // Industrial courts multiselect
                IndustrialCourts = new QualificationPlacesMultiSelectViewModel
                {
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "IndustrialCourts"
                    },
                    AlreadyQualified = BuildQualificationEnumAsString(qualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kIndustrialCategory && u is CourtDTO)),
                    WrongQualification = BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kIndustrialCategory && u is CourtDTO)),
                    WrongQualificationIds = wrongQualificationPlacesDTO.Where(
                        u => u.QualificationPlaceCategory == CourtDTO.kIndustrialCategory && u is CourtDTO)
                        .Select(u => u.QualificationPlaceId),
                    MultiSelectKendoUiViewModel = new MultiSelectKendoUiViewModel
                    {
                        SelectedItems = new List<string>(),
                        DataTextField = "IndustrialCourtName",
                        DataValueField = "IndustrialCourtId",
                        Placeholder = "Select industrial court...",
                        Controller = "Court",
                        Action = "GetIndustrialCourts",
                    },
                    TypeOfCheckCodesCompatible = TypeOfCheckEnum.INDUSTRIAL_CHECK
                },
                // Immigration office multiselect
                ImmigrationOffice = new QualificationPlacesDropDownListViewModel
                {
                    DropDownListKendoUiViewModel = new DropDownListKendoUiViewModel
                    {
                        SelectedItem = "",
                        DataTextField = "ImmigrationOfficeName",
                        DataValueField = "ImmigrationOfficeId",
                        Placeholder = "Select immigration office...",
                        Controller = "ImmigrationOffice",
                        Action = "GetImmigrationOffices",
                    },
                    AlreadyQualified =
                        BuildQualificationEnumAsString(qualificationPlacesDTO.Where(u => u is ImmigrationOfficeDTO)),
                    WrongQualification =
                        BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(u => u is ImmigrationOfficeDTO)),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is ImmigrationOfficeDTO)
                            .Select(u => u.QualificationPlaceId),
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "ImmigrationOffice"
                    },
                    TypeOfCheckCodesCompatible = TypeOfCheckEnum.PASSPORT_CHECK
                },
                // Driving license office multiselect
                DrivingLicenseOffice = new QualificationPlacesDropDownListViewModel
                {
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "DrivingLicenseOffice"
                    },
                    AlreadyQualified =
                        BuildQualificationEnumAsString(qualificationPlacesDTO.Where(u => u is DrivingLicenseOfficeDTO)),
                    WrongQualification =
                        BuildQualificationEnumAsString(
                            wrongQualificationPlacesDTO.Where(u => u is DrivingLicenseOfficeDTO)),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is DrivingLicenseOfficeDTO)
                            .Select(u => u.QualificationPlaceId),
                    DropDownListKendoUiViewModel = new DropDownListKendoUiViewModel
                    {
                        SelectedItem = "",
                        DataTextField = "DrivingLicenseOfficeName",
                        DataValueField = "DrivingLicenseOfficeId",
                        Placeholder = "Select driving license office...",
                        Controller = "DrivingLicenseOffice",
                        Action = "GetDrivingLicenseOffices",
                    },
                    TypeOfCheckCodesCompatible = TypeOfCheckEnum.DRIVING_LICENSE_CHECK,
                },
                // Faculties multiselect
                Faculty = new QualificationPlacesCascadingViewModel
                {
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "Faculties"
                    },
                    AlreadyQualified =
                        BuildQualificationEnumAsString(qualificationPlacesDTO.Where(u => u is FacultyDTO)),
                    WrongQualification =
                        BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(u => u is FacultyDTO)),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is FacultyDTO).Select(u => u.QualificationPlaceId),
                    MultiSelectKendoUiViewModel = new MultiSelectKendoUiViewModel
                    {
                        SelectedItems = new List<string>(),
                        DataTextField = "FacultyName",
                        DataValueField = "FacultyId",
                        Placeholder = "Select faculties...",
                        Controller = "University",
                        Action = "GetFaculties",
                    },
                    // University drop down list
                    DropDownListKendoUiViewModel = new DropDownListKendoUiViewModel
                    {
                        SelectedItem = "",
                        DataTextField = "UniversityName",
                        DataValueField = "UniversityId",
                        Placeholder = "Select an university...",
                        Controller = "University",
                        Action = "GetUniversities",
                        ChangeEvent = "dropdownlist_change_university"
                    },
                    TypeOfCheckCodesCompatible = educationTypeOfCheck,
                    AtomicCheckType = AtomicCheck.kFacultyType
                },
                CertificationPlace = new QualificationPlacesCascadingViewModel
                {
                    // Faculties multiselect
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "CertificationPlace"
                    },
                    AlreadyQualified =
                        BuildQualificationEnumAsString(qualificationPlacesDTO.Where(u => u is CertificationPlaceDTO)),
                    WrongQualification =
                        BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(u => u is CertificationPlaceDTO)),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is CertificationPlaceDTO)
                            .Select(u => u.QualificationPlaceId),
                    MultiSelectKendoUiViewModel = new MultiSelectKendoUiViewModel
                    {
                        SelectedItems = new List<string>(),
                        DataTextField = "CertificationPlaceName",
                        DataValueField = "CertificationPlaceId",
                        Placeholder = "Select certification places...",
                        Controller = "CertificationPlace",
                        Action = "GetCertificationPlaces",
                    },
                    // Professionnal qualification drop down list
                    DropDownListKendoUiViewModel = new DropDownListKendoUiViewModel
                    {
                        SelectedItem = "",
                        DataTextField = "ProfessionnalQualificationName",
                        DataValueField = "ProfessionnalQualificationId",
                        Placeholder = "Select a professionnal qualification...",
                        Controller = "ProfessionalQualification",
                        Action = "GetProfessionnalQualifications",
                        ChangeEvent = "dropdownlist_change_professionnalqualification"
                    },
                    TypeOfCheckCodesCompatible = TypeOfCheckEnum.PROFESSIONNAL_QUALIFICATION_CHECK
                },
                // Immigration office multiselect
                PopulationOffice = new QualificationPlacesDropDownListViewModel
                {
                    DropDownListKendoUiViewModel = new DropDownListKendoUiViewModel
                    {
                        SelectedItem = "",
                        DataTextField = "PopulationOfficeName",
                        DataValueField = "PopulationOfficeId",
                        Placeholder = "Select population office...",
                        Controller = "PopulationOffice",
                        Action = "GetPopulationOffices",
                    },
                    AlreadyQualified =
                        BuildQualificationEnumAsString(qualificationPlacesDTO.Where(u => u is PopulationOfficeDTO)),
                    WrongQualification =
                        BuildQualificationEnumAsString(wrongQualificationPlacesDTO.Where(u => u is PopulationOfficeDTO)),
                    WrongQualificationIds =
                        wrongQualificationPlacesDTO.Where(u => u is PopulationOfficeDTO)
                            .Select(u => u.QualificationPlaceId),
                    NotApplicable = new RadioButtonNotApplicableViewModel
                    {
                        IsNotApplicable = false,
                        Id = "PopulationOffice"
                    },
                    TypeOfCheckCodesCompatible = TypeOfCheckEnum.ID_CHECK
                }
            };


            BuildAddressQualificationFormViewModel(ref qualificationFormVm, screeningQualificationDTO);
            // Set not applicable field on the view model
            SetNotApplicableViewModel(ref qualificationFormVm, atomicChecksDTO);
            return qualificationFormVm;
        }

        private static bool CheckIsCurrentCompany(BaseQualificationPlaceDTO baseQualificationPlaceDTO,
            ScreeningBaseDTO screeningDTO)
        {
            ScreeningQualificationPlaceMetaDTO meta =
                baseQualificationPlaceDTO.ScreeningQualificationPlaceMeta.FirstOrDefault(
                    e =>
                        e.Screening.ScreeningId == screeningDTO.ScreeningId &&
                        e.ScreeningQualificationMetaKey == ScreeningQualificationPlaceMeta.kIsCurrentCompany);

            return meta != null && meta.ScreeningQualificationMetaValue == ScreeningQualificationPlaceMeta.kYesValue;
        }

        /// <summary>
        ///     Set the view model field not applicable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="atomicChecksDTO"></param>
        private static void SetTypeOfCheckNotApplicable(
            ref QualificationPlacesDropDownListViewModel model,
            IEnumerable<AtomicCheckBaseDTO> atomicChecksDTO)
        {
            TypeOfCheckEnum typeOfCheckCompatible = model.TypeOfCheckCodesCompatible;
            IList<AtomicCheckBaseDTO> atomicCheckListDTO = atomicChecksDTO as IList<AtomicCheckBaseDTO> ??
                                                       atomicChecksDTO.ToList();
            if (atomicCheckListDTO.All(u => u.TypeOfCheckCode != (Byte) typeOfCheckCompatible))
            {
                model.NotApplicable.IsNotApplicable = false;
                return;
            }
            AtomicCheckBaseDTO atomicCheck =
                atomicCheckListDTO.First(u => u.TypeOfCheckCode == (Byte) typeOfCheckCompatible);
            model.NotApplicable.IsNotApplicable = atomicCheck.AtomicCheckState ==
                                                  (Byte) AtomicCheckStateType.NOT_APPLICABLE;
        }

        /// <summary>
        ///     Set the view model field not applicable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="atomicChecksDTO"></param>
        private static void SetTypeOfCheckNotApplicable<T>(
            ref T model,
            IEnumerable<AtomicCheckBaseDTO> atomicChecksDTO)
            where T : QualificationPlacesMultiSelectViewModel
        {
            TypeOfCheckEnum typeOfCheckCompatible = model.TypeOfCheckCodesCompatible;
            string atomicCheckType = model.AtomicCheckType;

            IList<AtomicCheckBaseDTO> atomicCheckListDTO = atomicChecksDTO as IList<AtomicCheckBaseDTO> ??
                                                       atomicChecksDTO.ToList();
            if (atomicCheckListDTO.All(u => u.TypeOfCheckCode != (Byte) typeOfCheckCompatible))
            {
                model.NotApplicable.IsNotApplicable = false;
                return;
            }

            AtomicCheckBaseDTO atomicCheck = atomicCheckListDTO.First(
                u =>
                    u.TypeOfCheckCode == (Byte) typeOfCheckCompatible &&
                    u.AtomicCheckType == atomicCheckType);
            model.NotApplicable.IsNotApplicable = atomicCheck.AtomicCheckState ==
                                                  (Byte) AtomicCheckStateType.NOT_APPLICABLE;
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="atomicChecksDTO"></param>
        private static void SetNotApplicableViewModel(ref QualificationFormViewModel model,
            IEnumerable<AtomicCheckBaseDTO> atomicChecksDTO)
        {
            IList<AtomicCheckBaseDTO> atomicCheckListDTO = atomicChecksDTO as IList<AtomicCheckBaseDTO> ??
                                                       atomicChecksDTO.ToList();
            QualificationPlacesMultiSelectViewModel companies = model.Company;
            QualificationPlacesMultiSelectViewModel districtCourts = model.DistrictCourts;
            QualificationPlacesMultiSelectViewModel commercialCourts = model.CommercialCourts;
            QualificationPlacesMultiSelectViewModel industrialCourts = model.IndustrialCourts;
            QualificationPlacesMultiSelectViewModel polices = model.Police;
            QualificationPlacesMultiSelectViewModel highSchools = model.HighSchool;
            QualificationPlacesDropDownListViewModel drivingLicense = model.DrivingLicenseOffice;
            QualificationPlacesDropDownListViewModel immigrationOffice = model.ImmigrationOffice;
            QualificationPlacesDropDownListViewModel populationOffice = model.PopulationOffice;
            QualificationPlacesCascadingViewModel faculties = model.Faculty;
            QualificationPlacesCascadingViewModel certificationPlace = model.CertificationPlace;

            SetTypeOfCheckNotApplicable(ref companies, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref districtCourts, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref commercialCourts, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref industrialCourts, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref polices, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref highSchools, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref faculties, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref certificationPlace, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref drivingLicense, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref immigrationOffice, atomicCheckListDTO);
            SetTypeOfCheckNotApplicable(ref populationOffice, atomicCheckListDTO);

            // Contact number
            if (atomicCheckListDTO.All(u => u.TypeOfCheckCode != (Byte) TypeOfCheckEnum.CONTACT_NUMBER_CHECK))
            {
                model.HomePhoneNumberIsNotApplicable.IsNotApplicable = false;
                model.MobilePhoneNumberIsNotApplicable.IsNotApplicable = false;
                model.EmergencyContactIsNotApplicable.IsNotApplicable = false;
            }
            else
            {
                AtomicCheckBaseDTO homePhoneNumberAtomicCheck = atomicCheckListDTO.First(
                    u =>
                        u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.CONTACT_NUMBER_CHECK &&
                        u.AtomicCheckType == AtomicCheck.kHomePhoneNumberType);
                model.HomePhoneNumberIsNotApplicable.IsNotApplicable = homePhoneNumberAtomicCheck.AtomicCheckState ==
                                                                       (Byte) AtomicCheckStateType.NOT_APPLICABLE;

                AtomicCheckBaseDTO mobilePhoneNumberAtomicCheck = atomicCheckListDTO.First(
                    u =>
                        u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.CONTACT_NUMBER_CHECK &&
                        u.AtomicCheckType == AtomicCheck.kMobilePhoneNumberType);
                model.MobilePhoneNumberIsNotApplicable.IsNotApplicable =
                    mobilePhoneNumberAtomicCheck.AtomicCheckState == (Byte) AtomicCheckStateType.NOT_APPLICABLE;

                AtomicCheckBaseDTO emergencyContactAtomicCheck = atomicCheckListDTO.First(
                    u =>
                        u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.CONTACT_NUMBER_CHECK &&
                        u.AtomicCheckType == AtomicCheck.kEmergencyContactType);
                model.EmergencyContactIsNotApplicable.IsNotApplicable = emergencyContactAtomicCheck.AtomicCheckState ==
                                                                        (Byte) AtomicCheckStateType.NOT_APPLICABLE;
            }

            // Type of check neighborhood
            if (atomicCheckListDTO.All(u => u.TypeOfCheckCode != (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK))
            {
                model.CurrentAddressNotApplicable.IsNotApplicable = false;
                model.CVAddressNotApplicable.IsNotApplicable = false;
                model.IDAddressNotApplicable.IsNotApplicable = false;
            }
            else
            {
                AtomicCheckBaseDTO currentAtomicCheck = atomicCheckListDTO.First(
                    u =>
                        u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK &&
                        u.AtomicCheckType == AtomicCheck.kCurrentAddressType);
                model.CurrentAddressNotApplicable.IsNotApplicable = currentAtomicCheck.AtomicCheckState ==
                                                                    (Byte) AtomicCheckStateType.NOT_APPLICABLE;

                AtomicCheckBaseDTO idCardAtomicCheck = atomicCheckListDTO.First(
                    u =>
                        u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK &&
                        u.AtomicCheckType == AtomicCheck.kIDCardAddressType);
                model.IDAddressNotApplicable.IsNotApplicable = idCardAtomicCheck.AtomicCheckState ==
                                                               (Byte) AtomicCheckStateType.NOT_APPLICABLE;

                AtomicCheckBaseDTO cvAtomicCheck = atomicCheckListDTO.First(
                    u =>
                        u.TypeOfCheckCode == (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK &&
                        u.AtomicCheckType == AtomicCheck.kCVAddressType);
                model.CVAddressNotApplicable.IsNotApplicable = cvAtomicCheck.AtomicCheckState ==
                                                               (Byte) AtomicCheckStateType.NOT_APPLICABLE;
            }
        }


        /// <summary>
        ///     Returns a string contains the list of qualification places concatenated and separated by a comma
        /// </summary>
        /// <param name="qualifcationPlacesDTO"></param>
        public static string BuildQualificationEnumAsString(IEnumerable<BaseQualificationPlaceDTO> qualifcationPlacesDTO)
        {
            string ret = "";
            bool first = true;
            foreach (BaseQualificationPlaceDTO baseQualificationPlaceDTO in qualifcationPlacesDTO)
            {
                if (first)
                {
                    ret = baseQualificationPlaceDTO.QualificationPlaceName;
                    first = false;
                }
                else
                {
                    ret = ret + ", " + baseQualificationPlaceDTO.QualificationPlaceName;
                }
            }
            return ret;
        }


        /// <summary>
        ///     Extract screening qualification information from the view model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ScreeningQualificationDTO ExtractScreeningQualificationDTOFromViewModel(
            QualificationFormViewModel model)
        {
            var screeningQualificationDTO = new ScreeningQualificationDTO
            {
                ScreeningQualificationMaritalStatus =
                    GetMaritalStatus()[FormHelper.ExtractDropDownListViewModel(model.MartialStatus)],
                ScreeningQualificationBirthDate = model.BirthDate,
                ScreeningQualificationBirthPlace = model.BirthPlace,
                ScreeningQualificationGender = GetGender()[FormHelper.ExtractDropDownListViewModel(model.Gender)],
                ScreeningQualificationIDCardNumber = model.IdCardNumber,
                ScreeningQualificationPassportNumber = model.PassportNumber,
                ScreeningQualificationRelationshipWithCandidate = model.EmergencyContactRelationship != null &&
                                                                  model.EmergencyContactRelationship.PostData != "" &&
                                                                  model.EmergencyContactRelationship.PostData != "0"
                    ? GetRelationship()[FormHelper.ExtractDropDownListViewModel(model.EmergencyContactRelationship)]
                    : "",
                PersonalContactInfo = new ContactInfoDTO
                {
                    HomePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(model.HomePhoneNumber),
                    MobilePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(model.MobilePhoneNumber),
                },
                EmergencyContactPerson = new ContactPersonDTO
                {
                    ContactPersonName = model.EmergencyContactName,
                    ContactInfo = new ContactInfoDTO
                    {
                        HomePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(model.EmergencyPhoneNumber)
                    }
                },
                CurrentAddress = AddressHelper.ExtractAddressOptionalViewModel(model.CurrentAddressViewModel),
                IsCurrentAddressReQualified = model.CurrentAddressHasBeenRequalified,
                IDCardAddress = AddressHelper.ExtractAddressOptionalViewModel(model.IDCardAddressViewModel),
                IsIDCardAddressReQualified = model.IDAddressHasBeenRequalified,
                CVAddress = AddressHelper.ExtractAddressOptionalViewModel(model.CVAddressViewModel),
                IsCVAddressReQualified = model.CVAddressHasBeenRequalified,
            };
            return screeningQualificationDTO;
        }

        

        

        /// <summary>
        ///     Extract screening qualification information from the view model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public new static IEnumerable<BaseQualificationPlaceDTO> ExtractScreeningQualificationPlacesDTOFromViewModel(
            QualificationFormViewModel model, out List<int> requalifiedQualifications)
        {
            // Mutiselect view model
            var screeningQualificationPlacesDTO = new List<BaseQualificationPlaceDTO>();
            requalifiedQualifications = new List<int>();

            AddScreeningQualificationFromViewModel<PoliceDTO, QualificationPlacesMultiSelectViewModel>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.Police);
            AddScreeningQualificationFromViewModel<CourtDTO, QualificationPlacesMultiSelectViewModel>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.CommercialCourts);
            AddScreeningQualificationFromViewModel<CourtDTO, QualificationPlacesMultiSelectViewModel>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.DistrictCourts);
            AddScreeningQualificationFromViewModel<CourtDTO, QualificationPlacesMultiSelectViewModel>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.IndustrialCourts);
            AddScreeningQualificationFromViewModelForCompany(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.Company, model.ScreeningId);
            AddScreeningQualificationFromViewModel<HighSchoolDTO, QualificationPlacesMultiSelectViewModel>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.HighSchool);

            // Multiselect and drop down list view model
            AddScreeningQualificationFromViewModel<FacultyDTO, QualificationPlacesCascadingViewModel>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.Faculty);
            AddScreeningQualificationFromViewModel<CertificationPlaceDTO, QualificationPlacesCascadingViewModel>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.CertificationPlace);

            // Drop down list view model
            AddScreeningQualificationFromViewModel<DrivingLicenseOfficeDTO>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.DrivingLicenseOffice);
            AddScreeningQualificationFromViewModel<ImmigrationOfficeDTO>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.ImmigrationOffice);
            AddScreeningQualificationFromViewModel<PopulationOfficeDTO>(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.PopulationOffice);
            AddScreeningQualificationFromViewModelForCurrentCompany(
                ref screeningQualificationPlacesDTO, ref requalifiedQualifications, model.CurrentCompany, model.ScreeningId);
            return screeningQualificationPlacesDTO;
        }

        private static void AddScreeningQualificationFromViewModelForCompany(
            ref List<BaseQualificationPlaceDTO> qualificationPlacesDTO, 
            ref List<int> requalifiedQualifications, 
            QualificationPlacesMultiSelectViewModel model, 
            int screeningID)
        {
            if (model == null)
                return;

            if (model.NotApplicable.IsNotApplicable == false
                && model.MultiSelectKendoUiViewModel != null
                && model.MultiSelectKendoUiViewModel.SelectedItems != null)
            {
                qualificationPlacesDTO.AddRange(
                    model.MultiSelectKendoUiViewModel.SelectedItems.Select(id =>
                    new CompanyDTO()
                    {
                        QualificationPlaceId = Convert.ToInt32(id),
                        ScreeningQualificationPlaceMeta = new Collection<ScreeningQualificationPlaceMetaDTO>
                        {
                            new ScreeningQualificationPlaceMetaDTO
                            {
                                ScreeningQualificationMetaKey = ScreeningQualificationPlaceMeta.kIsCurrentCompany,
                                ScreeningQualificationMetaValue = ScreeningQualificationPlaceMeta.kNoValue,
                                ScreeningId = screeningID
                            }
                        }
                    }));
            }

            if (model.HasBeenRequalified)
            {
                requalifiedQualifications.AddRange(model.WrongQualificationIds);
            }
        }

        /// <summary>
        /// Extract model from view model of current company
        /// </summary>
        /// <param name="qualificationsDTO"></param>
        /// <param name="requalifiedQualifications"></param>
        /// <param name="model"></param>
        private static void AddScreeningQualificationFromViewModelForCurrentCompany(
            ref List<BaseQualificationPlaceDTO> qualificationsDTO, 
            ref List<int> requalifiedQualifications, 
            QualificationPlacesDropDownListViewModel model,
            int screeningID)
        {
            if (model == null)
                return;

            if (model.NotApplicable.IsNotApplicable == false && model != null &&
                model.DropDownListKendoUiViewModel != null
                && model.DropDownListKendoUiViewModel.SelectedItem != null &&
                model.DropDownListKendoUiViewModel.SelectedItem != "0")
            {
                qualificationsDTO.Add(new CompanyDTO
                {
                    QualificationPlaceId = Convert.ToInt32(model.DropDownListKendoUiViewModel.SelectedItem),
                    QualificationReQualified = model.HasBeenRequalified,
                    ScreeningQualificationPlaceMeta = new Collection<ScreeningQualificationPlaceMetaDTO>
                    {
                        new ScreeningQualificationPlaceMetaDTO
                        {
                            ScreeningQualificationMetaKey = ScreeningQualificationPlaceMeta.kIsCurrentCompany,
                            ScreeningQualificationMetaValue = ScreeningQualificationPlaceMeta.kYesValue,
                            ScreeningId = screeningID
                        }
                    }
                });
            }

            if (model.HasBeenRequalified)
            {
                requalifiedQualifications.AddRange(model.WrongQualificationIds);
            }
        }


        /// <summary>
        ///     Extract qualification place from view model and create qualification place DTO accordingly. Data re
        ///     retrieve from a multiselect
        /// </summary>
        /// <typeparam name="T">Generic template that should be inherited from BaseQualificationPlaceDTO</typeparam>
        /// <param name="qualificationsDTO"></param>
        /// <param name="model"></param>
        private static void AddScreeningQualificationFromViewModel<T, U>(
            ref List<BaseQualificationPlaceDTO> qualificationsDTO,
            ref List<int> requalifiedQualifications,
            QualificationPlacesMultiSelectViewModel model)
            where T : BaseQualificationPlaceDTO, new()
            where U : QualificationPlacesMultiSelectViewModel
        {
            if (model == null)
                return;

            if (model.NotApplicable.IsNotApplicable == false
                && model.MultiSelectKendoUiViewModel != null
                && model.MultiSelectKendoUiViewModel.SelectedItems != null)
            {
                qualificationsDTO.AddRange(
                    model.MultiSelectKendoUiViewModel.SelectedItems.Select(id =>
                    new T
                    {
                        QualificationPlaceId = Convert.ToInt32(id),
                    }));
            }

            if (model.HasBeenRequalified)
            {
                requalifiedQualifications.AddRange(model.WrongQualificationIds);
            }
        }

        /// <summary>
        ///     Extract qualification place from view model and create qualification place DTO accordingly. Data are retrieved
        ///     from a dropdown list
        /// </summary>
        /// <typeparam name="T">Generic template that should be inherited from BaseQualificationPlaceDTO</typeparam>
        /// <param name="qualificationsDTO"></param>
        /// <param name="model"></param>
        private static void AddScreeningQualificationFromViewModel<T>(
            ref List<BaseQualificationPlaceDTO> qualificationsDTO,
            ref List<int> requalifiedQualifications,
            QualificationPlacesDropDownListViewModel model)
            where T : BaseQualificationPlaceDTO, new()
        {
            if (model == null)
                return;

            if (model.NotApplicable.IsNotApplicable == false &&
                model.DropDownListKendoUiViewModel != null
                && model.DropDownListKendoUiViewModel.SelectedItem != null &&
                model.DropDownListKendoUiViewModel.SelectedItem != "0")
            {
                qualificationsDTO.Add(new T
                {
                    QualificationPlaceId = Convert.ToInt32(model.DropDownListKendoUiViewModel.SelectedItem),
                    QualificationReQualified = model.HasBeenRequalified
                });
            }

            if (model.HasBeenRequalified)
            {
                requalifiedQualifications.AddRange(model.WrongQualificationIds);
            }
        }


        /// <summary>
        ///     Get all the type of check that are not applicable. A dictionnary with atomic check type and type of check code is
        ///     returned
        ///     with the value not applicable equal to true or false
        /// </summary>
        /// <param name="atomicCheckDTODictionnary"></param>
        /// <param name="model"></param>
        private static void ExtractNotApplicableQualification(
            ref IDictionary<AtomicCheckDTO, bool> atomicCheckDTODictionnary,
            QualificationPlacesMultiSelectViewModel model)
        {
            if (model == null)
                return;

            var atomicCheckDTO = new AtomicCheckDTO
            {
                AtomicCheckType = model.AtomicCheckType,
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckCode = (Byte) model.TypeOfCheckCodesCompatible
                }
            };

            if (model.NotApplicable.IsNotApplicable == false)
            {
                atomicCheckDTODictionnary[atomicCheckDTO] = false;
                return;
            }
            atomicCheckDTODictionnary[atomicCheckDTO] = true;
        }

        private static void ExtractNotApplicableQualification(
            ref IDictionary<AtomicCheckDTO, bool> atomicCheckDTODictionnary,
            QualificationPlacesDropDownListViewModel model)
        {
            if (model == null)
                return;

            var atomicCheckDTO = new AtomicCheckDTO
            {
                AtomicCheckType = model.AtomicCheckType,
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckCode = (Byte) model.TypeOfCheckCodesCompatible
                }
            };

            if (model == null || model.NotApplicable.IsNotApplicable == false)
            {
                atomicCheckDTODictionnary[atomicCheckDTO] = false;
                return;
            }
            atomicCheckDTODictionnary[atomicCheckDTO] = true;
        }

        /// <summary>
        ///     Extract screening qualification information from the view model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IDictionary<AtomicCheckDTO, bool> ExtractNotApplicableQualificationFromViewModel(
            QualificationFormViewModel model)
        {
            IDictionary<AtomicCheckDTO, bool> notApplicablesTypeOfChecks = new Dictionary<AtomicCheckDTO, bool>();
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.Company);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.CurrentCompany);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.PopulationOffice);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.DistrictCourts);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.CommercialCourts);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.IndustrialCourts);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.Police);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.HighSchool);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.DrivingLicenseOffice);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.ImmigrationOffice);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.Faculty);
            ExtractNotApplicableQualification(ref notApplicablesTypeOfChecks, model.CertificationPlace);

            // Contact number atomic check
            notApplicablesTypeOfChecks[new AtomicCheckDTO
            {
                AtomicCheckType = AtomicCheck.kHomePhoneNumberType,
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckCode = (Byte) TypeOfCheckEnum.CONTACT_NUMBER_CHECK
                }
            }] = model.HomePhoneNumberIsNotApplicable != null && model.HomePhoneNumberIsNotApplicable.IsNotApplicable;

            notApplicablesTypeOfChecks[new AtomicCheckDTO
            {
                AtomicCheckType = AtomicCheck.kMobilePhoneNumberType,
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckCode = (Byte) TypeOfCheckEnum.CONTACT_NUMBER_CHECK
                }
            }] = model.MobilePhoneNumberIsNotApplicable != null &&
                 model.MobilePhoneNumberIsNotApplicable.IsNotApplicable;

            notApplicablesTypeOfChecks[new AtomicCheckDTO
            {
                AtomicCheckType = AtomicCheck.kEmergencyContactType,
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckCode = (Byte) TypeOfCheckEnum.CONTACT_NUMBER_CHECK
                }
            }] = model.EmergencyContactIsNotApplicable != null && model.EmergencyContactIsNotApplicable.IsNotApplicable;

            // Current address atomic check
            notApplicablesTypeOfChecks[new AtomicCheckDTO
            {
                AtomicCheckType = AtomicCheck.kCurrentAddressType,
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckCode = (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK
                }
            }] = model.CurrentAddressNotApplicable != null && model.CurrentAddressNotApplicable.IsNotApplicable;


            // ID address atomic check
            notApplicablesTypeOfChecks[new AtomicCheckDTO
            {
                AtomicCheckType = AtomicCheck.kIDCardAddressType,
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckCode = (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK
                }
            }] = model.IDAddressNotApplicable != null && model.IDAddressNotApplicable.IsNotApplicable;


            // CV address atomic check
            notApplicablesTypeOfChecks[new AtomicCheckDTO
            {
                AtomicCheckType = AtomicCheck.kCVAddressType,
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckCode = (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK
                }
            }] = model.CVAddressNotApplicable != null && model.CVAddressNotApplicable.IsNotApplicable;

            return notApplicablesTypeOfChecks;
        }

        private static IDictionary<int, string> GetRelationship()
        {
            return new Dictionary<int, string>
            {
                {1, "Mother"},
                {2, "Father"},
                {3, "Brother"},
                {4, "Sister"},
                {5, "Cousin"},
                {6, "Friend"},
                {7, "Uncle"},
                {8, "Aunt"},
                {9, "Father/Mother-in-law"},
                {10, "Son/Daughter-in-law"},
                {11, "Brother/Sister-in-law"},
                {12, "Other"}
            };
        }

        private static IDictionary<int, string> GetMaritalStatus()
        {
            return new Dictionary<int, string>
            {
                {1, "Single"},
                {2, "Married"},
                {3, "Separated"},
                {4, "Divorced"},
                {5, "Widow"},
                {6, "Other"}
            };
        }

        private static IDictionary<int, string> GetGender()
        {
            return new Dictionary<int, string>
            {
                {1, "Male"},
                {2, "Female"}
            };
        }
    }
}