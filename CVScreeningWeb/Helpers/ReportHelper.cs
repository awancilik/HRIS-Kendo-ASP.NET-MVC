using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Report;
using Kendo.Mvc.Extensions;
using AtomicCheck = CVScreeningCore.Models.AtomicCheck;
using System.Security.Cryptography;
using System.Text;

namespace CVScreeningWeb.Helpers
{
    public class ReportHelper
    {
        /// <summary>
        /// Generate report view model from screening
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static ReportViewModel GenerateReportViewModel(ScreeningDTO screeningDTO)
        {
            return new ReportViewModel
            {
                ScreeningId = screeningDTO.ScreeningId,
                Title = string.Format("Pre-Employment Screening for {0} - {1} - {2}",
                    screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyName,
                    screeningDTO.ScreeningFullName,
                    screeningDTO.ScreeningDeliveryDate),
                Summary = GenerateSummaryReport(screeningDTO),
                TypeOfCheckReports = GenerateTypeOfCheckReports(screeningDTO),
                Appendices = GenerateAppendixReports(screeningDTO)
            };
        }

        /// <summary>
        /// Get address, postal, and location information from given AddressDTO
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string GetFullAddress(AddressDTO address)
        {
            string fullAddress = string.Empty;
            fullAddress += address.Street ?? string.Empty;
            fullAddress += address.PostalCode != null ? ", " + address.PostalCode : string.Empty;
            fullAddress += address.Location != null
                ? address.Location.LocationLevel == 1 //    if level is country
                    ? ", " + address.Location.LocationName //    then concat country name directly
                    : ", " + string.Format("{0}, {1}, {2}, {3}, {4}",
                        address.Location.LocationName,
                        address.Location.LocationParent.LocationName,
                        address.Location.LocationParent.LocationParent.LocationName,
                        address.Location.LocationParent.LocationParent.LocationParent.LocationName,
                        address.Location.LocationParent.LocationParent.LocationParent.LocationParent.LocationName
                        )
                : string.Empty;
            return fullAddress;
        }

        /// <summary>
        /// To generate cover
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static CoverPageViewModel GenerateCoverReportViewModel(ScreeningDTO screeningDTO)
        {
            return new CoverPageViewModel
            {
                FullName = screeningDTO.ScreeningFullName,
                ScreeningReference = screeningDTO.ScreeningReference,
                DateOfIssuance = DateTime.Now.ToString("dd MMMM yyyy")
            };
        }

        /// <summary>
        /// To generate part of report which displays type of check
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static IEnumerable<TypeOfCheckReportViewModel> GenerateTypeOfCheckReports(ScreeningDTO screeningDTO)
        {
            var atomicChecks = screeningDTO.AtomicCheck.Where(u => u.AtomicCheckState != (Byte)AtomicCheckStateType.NOT_APPLICABLE
                && u.AtomicCheckState != (Byte)AtomicCheckStateType.WRONGLY_QUALIFIED).ToList();

            atomicChecks = GetSortedAtomicChecks(atomicChecks);

            var typeOfChecks =
                screeningDTO.ScreeningLevelVersion.TypeOfCheckScreeningLevelVersion.Select(e => e.TypeOfCheck).ToList();
            var qualification = screeningDTO.ScreeningQualification;

            return (from typeOfCheckDTO in typeOfChecks
                    let verificationReports = GenerateVerificationReportsByTypeOfCheck(atomicChecks, typeOfCheckDTO, qualification)
                    where verificationReports.Any()
                    select new TypeOfCheckReportViewModel
                    {
                        TypeOfCheckName = typeOfCheckDTO.CheckName,
                        VerificationReports = GenerateVerificationReportsByTypeOfCheck(atomicChecks, typeOfCheckDTO, qualification)
                    }).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="atomicChecks"></param>
        /// <param name="typeOfCheck"></param>
        /// <param name="qualification"></param>
        /// <returns></returns>
        public static IEnumerable<VerificationReportViewModel> GenerateVerificationReportsByTypeOfCheck(
            IEnumerable<AtomicCheckDTO> atomicChecks, TypeOfCheckDTO typeOfCheck,
            ScreeningQualificationDTO qualification)
        {
            return (
                from atomicCheck in atomicChecks
                where atomicCheck.TypeOfCheck.TypeOfCheckId == typeOfCheck.TypeOfCheckId
                select new VerificationReportViewModel
                {
                    Type = atomicCheck.AtomicCheckType,
                    Lines = GenerateVerificationLines(atomicCheck, qualification),
                    Passage =
                        (atomicCheck.AtomicCheckReport != null
                            ? ConvertImageRelativePathToImageAbsolutePath(atomicCheck.AtomicCheckReport)
                            : "<p></p>")
                }).ToList();
        }

        /// <summary>
        /// General method to do sorting
        /// </summary>
        /// <param name="atomicChecks"></param>
        /// <returns></returns>
        private static List<AtomicCheckDTO> GetSortedAtomicChecks(List<AtomicCheckDTO> atomicChecks)
        {
            var atomicCheckTemp = atomicChecks;

            atomicCheckTemp = GetSortedChecksForCurrentCompany(atomicCheckTemp);

            return atomicCheckTemp;
        }

        /// <summary>
        /// Specific method to sort based on current company first
        /// </summary>
        /// <param name="atomicChecks"></param>
        /// <returns></returns>
        private static List<AtomicCheckDTO> GetSortedChecksForCurrentCompany(List<AtomicCheckDTO> atomicChecks)
        {
            var firstCompany = atomicChecks.FirstOrDefault(e => e.QualificationPlace is CompanyDTO);
            var currentCompany = atomicChecks.FirstOrDefault(e => IsACurrentCompany(e.QualificationPlace));

            if (firstCompany == null || currentCompany == null ||
                firstCompany.AtomicCheckId == currentCompany.AtomicCheckId)
                return atomicChecks;

            // Swap
            var atomicCheckTemp = new List<AtomicCheckDTO>();
            foreach (var atomicCheckDTO in atomicChecks)
            {
                if (atomicCheckDTO.AtomicCheckId == firstCompany.AtomicCheckId)
                {
                    atomicCheckTemp.Add(currentCompany);
                }
                else if (atomicCheckDTO.AtomicCheckId == currentCompany.AtomicCheckId)
                {
                    atomicCheckTemp.Add(firstCompany);
                }
                else
                {
                    atomicCheckTemp.Add(atomicCheckDTO);
                }
            }
            return atomicCheckTemp;
        }

        /// <summary>
        /// Check if it is not allowed to contact 
        /// </summary>
        /// <param name="atomicCheck"></param>
        /// <returns></returns>
        private static bool IsNotAllowedToContactCurrentCompany(AtomicCheckDTO atomicCheck)
        {
            //make sure that it is current company
            var meta = atomicCheck.QualificationPlace.ScreeningQualificationPlaceMeta.FirstOrDefault(e
                => e.ScreeningQualificationMetaKey == ScreeningQualificationPlaceMeta.kIsCurrentCompany);

            if (meta == null || meta.ScreeningQualificationMetaValue == ScreeningQualificationPlaceMeta.kNoValue)
                return false;

            //check screening level version 
            // return true if it does not allow to contact current company
            return atomicCheck.Screening.ScreeningLevelVersion.ScreeningLevelVersionAllowedToContactCurrentCompany ==
                   ScreeningLevelVersion.kNotAllowedContactCurrentCompany;
        }

        /// <summary>
        /// To compare latest submitted automatic report with generated automatic report
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static bool IsSameContent(ScreeningDTO screeningDTO)
        {
            var latestAutomaticReport = screeningDTO.ScreeningReport.Where(r =>
                r.ScreeningReportGenerationType.Equals(ScreeningReport.kAutomaticGenerationType)).LastOrDefault();

            if (latestAutomaticReport != null)
            {
                var notSubmittedReport = GenerateReportViewModel(screeningDTO);
                var latestReportContent = (ReportViewModel)FileHelper.ByteArrayToObject(latestAutomaticReport.ScreeningReportContent);

                latestReportContent.Title = notSubmittedReport.Title; //Ignoring content title

                if (GetHashString(FileHelper.ObjectToByteArray(latestReportContent)).Equals(GetHashString(FileHelper.ObjectToByteArray(notSubmittedReport))))
                    return true;
            }

            return false;
        }

        private static string GetHashString(byte[] reportContent)
        {
            byte[] hash;
            StringBuilder sb = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(reportContent);
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
            }
            return sb.ToString();
        }

        private static string ConvertImageRelativePathToImageAbsolutePath(string htmlString)
        {
            const string pattern = @"<(img)\b[^>]*>";
            var rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            var matches = rgx.Matches(htmlString);
            for (int i = 0; i < matches.Count; i++)
            {
                var insideSrcString = matches[i].Value.Split(new[] { "src=" }, StringSplitOptions.None);
                var relativePath = insideSrcString[1].Split('\"')[1];
                htmlString = htmlString.Replace(relativePath, HttpContext.Current.Server.MapPath(relativePath));
            }
            return HTMLHelper.CleanHtmlReport(htmlString);
        }

        /// <summary>
        /// Generate collection of string containing information to display in PDF report concerning address
        /// </summary>
        /// <param name="addressDTO"></param>
        /// <returns></returns>
        private static IEnumerable<string> BuildAddress(AddressDTO addressDTO)
        {
            ICollection<string> addressList = new Collection<string>();

            // Indonesian address
            if (addressDTO.Location.LocationLevel > 1)
            {
                addressList.Add(addressDTO.Street + ", " + addressDTO.PostalCode);

                //district and sub district
                addressList.Add(addressDTO.Location.LocationName + ", " + addressDTO.Location.LocationParent.LocationName);
                addressList.Add(
                    //city
                    addressDTO.Location.LocationParent.LocationParent.LocationName +
                    ", " +
                    //province
                    addressDTO.Location.LocationParent.LocationParent.LocationParent
                        .LocationName + ", " +
                    //country
                    addressDTO.Location.LocationParent.LocationParent.LocationParent
                        .LocationParent.LocationName);
            }
            // Other address
            else
            {
                addressList.Add(addressDTO.Street);
                addressList.Add(addressDTO.Location.LocationName);
            }
            return addressList;
        }

        public static IEnumerable<string> GenerateVerificationLines(AtomicCheckDTO atomicCheck,
            ScreeningQualificationDTO qualification)
        {
            var lines = new Collection<string>();

            // if qualification place Type of Check
            if (atomicCheck.QualificationPlace != null)
            {
                lines.Add(
                    IsACurrentCompany(atomicCheck.QualificationPlace) ?
                    atomicCheck.QualificationPlace.QualificationPlaceName +
                    string.Format(" ({0})", Report.CurrentCompany) :
                    atomicCheck.QualificationPlace.QualificationPlaceName);
                lines.AddRange(BuildAddress(atomicCheck.QualificationPlace.Address));
                if (IsNotAllowedToContactCurrentCompany(atomicCheck))
                    lines.Add("Not Allowed to Contact Current Company");
            }
            // if Contact Type of Check
            else if (atomicCheck.AtomicCheckType != null && atomicCheck.TypeOfCheck.TypeOfCheckCode
                == (Byte)TypeOfCheckEnum.CONTACT_NUMBER_CHECK && qualification.PersonalContactInfo != null)
            {
                if (atomicCheck.AtomicCheckType.Equals(AtomicCheck.kHomePhoneNumberType))
                {
                    lines.Add(Report.HomePhoneNumber);
                    lines.Add(qualification.PersonalContactInfo.HomePhoneNumber);
                }
                if (atomicCheck.AtomicCheckType.Equals(AtomicCheck.kMobilePhoneNumberType))
                {
                    lines.Add(Report.MobilePhoneNumber);
                    lines.Add(qualification.PersonalContactInfo.MobilePhoneNumber);
                }
                if (atomicCheck.AtomicCheckType.Equals(AtomicCheck.kEmergencyContactType))
                {
                    lines.Add(Report.EmergencyContactName);
                    lines.Add(qualification.EmergencyContactPerson.ContactPersonName);
                    lines.Add(qualification.EmergencyContactPerson.ContactInfo.HomePhoneNumber);
                }
            }
            else if (atomicCheck.AtomicCheckType != null &&
                atomicCheck.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.NEIGHBOURHOOD_CHECK)
            {
                if (atomicCheck.AtomicCheckType.Equals(AtomicCheck.kCurrentAddressType))
                {
                    lines.AddRange(BuildAddress(qualification.CurrentAddress));
                }
                if (atomicCheck.AtomicCheckType.Equals(AtomicCheck.kCVAddressType))
                {
                    lines.AddRange(BuildAddress(qualification.CVAddress));
                }
                if (atomicCheck.AtomicCheckType.Equals(AtomicCheck.kIDCardAddressType))
                {
                    lines.AddRange(BuildAddress(qualification.IDCardAddress));
                }
            }
            return lines;
        }

        private static bool IsACurrentCompany(BaseQualificationPlaceDTO qualificationPlace)
        {
            if (qualificationPlace == null)
                return false;

            var meta = qualificationPlace.ScreeningQualificationPlaceMeta.FirstOrDefault(e =>
                e.ScreeningQualificationMetaKey == ScreeningQualificationPlaceMeta.kIsCurrentCompany);

            return qualificationPlace is CompanyDTO
                   && meta != null &&
                   meta.ScreeningQualificationMetaValue == ScreeningQualificationPlaceMeta.kYesValue;
        }

        public static SummaryReportViewModel GenerateSummaryReport(ScreeningDTO screeningDTO)
        {
            var qualificationDTO = screeningDTO.ScreeningQualification;
            var atomicChecks = screeningDTO.AtomicCheck;
            var summaryViewModel = new SummaryReportViewModel();
            summaryViewModel.TypeOfChecks = screeningDTO.ScreeningLevelVersion.TypeOfCheckScreeningLevelVersion.Select(
                e => new TypeOfCheckVerificationViewModel
                {
                    Name = e.TypeOfCheck.CheckName,
                    Verifications = GenerateVerificationByTypeOfCheck(atomicChecks, e.TypeOfCheck)
                }).ToList();
            summaryViewModel.AdditionnalRemarks = String.IsNullOrEmpty(screeningDTO.ScreeningAdditionalRemarks) ?
                Report.AdditionnalRemarksDefault : screeningDTO.ScreeningAdditionalRemarks;
            summaryViewModel.MaritalStatus = qualificationDTO.ScreeningQualificationMaritalStatus ?? "";
            summaryViewModel.PassportNumber = qualificationDTO.ScreeningQualificationPassportNumber ?? "";
            summaryViewModel.IDCardNumber = qualificationDTO.ScreeningQualificationIDCardNumber ?? "";
            summaryViewModel.MobilePhoneNumber = qualificationDTO.PersonalContactInfo.MobilePhoneNumber ?? "";
            summaryViewModel.HomePhoneNumber = qualificationDTO.PersonalContactInfo.HomePhoneNumber ?? "";
            summaryViewModel.EmergencyContactNumber = qualificationDTO.EmergencyContactPerson.ContactInfo.HomePhoneNumber ?? "";
            summaryViewModel.EmergencyContactName = qualificationDTO.EmergencyContactPerson.ContactPersonName ?? "";
            summaryViewModel.PlaceOfBirth = qualificationDTO.ScreeningQualificationBirthPlace ?? "";
            summaryViewModel.DateOfBirth = qualificationDTO.ScreeningQualificationBirthDate != null
                ? ((DateTime)qualificationDTO.ScreeningQualificationBirthDate).ToShortDateString()
                : "";
            summaryViewModel.IDCardAddress = qualificationDTO.IDCardAddress != null
                ? GetFullAddress(qualificationDTO.IDCardAddress)
                : "";
            summaryViewModel.CurrentAddress = qualificationDTO.CurrentAddress != null
                ? GetFullAddress(qualificationDTO.CurrentAddress)
                : "";
            summaryViewModel.CVAddress = qualificationDTO.CVAddress != null
                ? GetFullAddress(qualificationDTO.CVAddress)
                : "";
            summaryViewModel.Name = screeningDTO.ScreeningFullName;
            return summaryViewModel;
        }

        public static IEnumerable<VerificationViewModel> GenerateVerificationByTypeOfCheck(IEnumerable<AtomicCheckDTO>
            atomicChecks, TypeOfCheckDTO typeOfCheck)
        {
            return (
                from atomicCheck in atomicChecks
                where atomicCheck.TypeOfCheck.TypeOfCheckId == typeOfCheck.TypeOfCheckId && atomicCheck.AtomicCheckState
                    != (Byte)AtomicCheckStateType.WRONGLY_QUALIFIED
                select new VerificationViewModel
                {
                    Name =
                        atomicCheck.QualificationPlace != null
                            ? atomicCheck.QualificationPlace.QualificationPlaceName
                            : atomicCheck.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.NEIGHBOURHOOD_CHECK
                            || atomicCheck.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.CONTACT_NUMBER_CHECK
                            || atomicCheck.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD
                            || atomicCheck.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EDUCATION_CHECK_WITH_EVIDENCE
                            ? atomicCheck.AtomicCheckType
                            : Resources.Screening.Done,
                    Note = atomicCheck.AtomicCheckVerificationSummary,
                    Status = Convert.ToString((AtomicCheckStateType)atomicCheck.AtomicCheckState)
                }).ToList();
        }

        /// <summary>
        /// Generate list of appendix from given screening
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static IEnumerable<AppendixReportViewModel> GenerateAppendixReports(ScreeningDTO screeningDTO)
        {
            var appendixVms = new Collection<AppendixReportViewModel>();
            var appendixNumber = 0;

            foreach (var atomicCheck in screeningDTO.AtomicCheck.Where(u => u.AtomicCheckState != (Byte)AtomicCheckStateType.NOT_APPLICABLE))
            {
                AtomicCheckDTO atomicCheckWithAppendix = null;
                if (IsOnlyOneAppendix(screeningDTO, out atomicCheckWithAppendix))
                {
                    appendixVms.Add(GenerateOneAppendixReport(atomicCheckWithAppendix));
                    break;
                }
                if (atomicCheck.Attachment.Count > 0)
                    appendixVms.Add(GenerateAppendixReport(atomicCheck, ++appendixNumber));
            }

            return appendixVms;
        }

        private static bool IsOnlyOneAppendix(ScreeningDTO screeningDTO, out AtomicCheckDTO atomicCheckWithAppendix)
        {
            var number = 0;
            atomicCheckWithAppendix = null;
            foreach (var atomicCheckDTO in screeningDTO.AtomicCheck)
            {
                if (atomicCheckDTO.Attachment.Count > 0)
                {
                    number++;
                    atomicCheckWithAppendix = atomicCheckDTO;
                }
                if (number > 1)
                {
                    atomicCheckWithAppendix = null;
                    return false;
                }
            }
            return number != 0;
        }

        private static AppendixReportViewModel GenerateOneAppendixReport(AtomicCheckDTO atomicCheck)
        {
            var viewModel = new AppendixReportViewModel
            {
                AppendixTypeOfCheck = atomicCheck.TypeOfCheck.CheckName,
                AppendixTitle = "Appendix",
                Attachments = atomicCheck.Attachment.Select(attachmentDTO => new AppendixAttachmentViewModel()
                {
                    FileName = FileHelper.GetFileNameWithoutExtension(attachmentDTO.AttachmentName),
                    ImagePath = attachmentDTO.AttachmentFilePath
                }).ToList()
            };
            return viewModel;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="atomicCheck"></param>
        /// <returns></returns>
        private static AppendixReportViewModel GenerateAppendixReport(AtomicCheckDTO atomicCheck, int appendixNumber)
        {
            var viewModel = new AppendixReportViewModel
            {
                AppendixTypeOfCheck = atomicCheck.TypeOfCheck.CheckName,
                AppendixTitle = string.Format("Appendix {0} - {1}",
                    appendixNumber, atomicCheck.TypeOfCheck.CheckName),
                Attachments = atomicCheck.Attachment.Select(attachmentDTO => new AppendixAttachmentViewModel()
                {
                    FileName = FileHelper.GetFileNameWithoutExtension(attachmentDTO.AttachmentName),
                    ImagePath = attachmentDTO.AttachmentFilePath
                }).ToList()
            };
            return viewModel;
        }
    }
}