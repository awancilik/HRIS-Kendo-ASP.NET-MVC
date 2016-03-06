using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningCore.Models.AtomicCheckValidationState;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.Settings;
using CVScreeningService.Helpers;
using CVScreeningWeb.ViewModels.AtomicCheck;
using CVScreeningWeb.ViewModels.Shared;
using Microsoft.Ajax.Utilities;

namespace CVScreeningWeb.Helpers
{
    public class AtomicCheckHelper
    {


        static private int GetValidationStatusOrder(AtomicCheckBaseDTO atomicCheckDTO)
        {
            switch ((AtomicCheckValidationStateType)atomicCheckDTO.AtomicCheckValidationState)
            {
                case AtomicCheckValidationStateType.REJECTED:
                    return 1;
                case AtomicCheckValidationStateType.NOT_PROCESSED:
                    return 2;
                case AtomicCheckValidationStateType.PROCESSED:
                    return 3;
                case AtomicCheckValidationStateType.VALIDATED:
                    return 4;
            }
            return 0;
        }



        public static IEnumerable<AtomicCheckManageViewModel> BuildAtomicCheckManageViewModels(
            IEnumerable<AtomicCheckBaseDTO> atomicCheckDTO,
            IEnumerable<PublicHolidayDTO> publicHolidaysDTO)
        {
            atomicCheckDTO = atomicCheckDTO.OrderBy(u => GetValidationStatusOrder(u));
            return atomicCheckDTO.Select(e => new AtomicCheckManageViewModel
            {
                Id = e.AtomicCheckId,
                ScreeningId = e.Screening.ScreeningId,
                CanBeProcessed = e.CanBeProcessed,
                CanBeValidated = e.CanBeValidated,
                CanBeRejected = e.CanBeRejected,
                CanBeAssigned = e.CanBeAssigned,
                ScreeningFullName = e.Screening.ScreeningFullName,
                TypeOfCheck = e.TypeOfCheckName.Replace(" check", ""),
                AssignedTo = e.AssignedTo,
                Status = AtomicCheckStateFactory.GetStateAsString((AtomicCheckStateType) e.AtomicCheckState),
                ValidationStatus =
                    AtomicCheckValidationStateFactory.GetStateAsString(
                        (AtomicCheckValidationStateType) e.AtomicCheckValidationState),
                Deadline = ((DateTime) e.Screening.ScreeningDeadlineDate).ToShortDateString(),
                DayPending = LayoutHelper.GetPendingDaysAsString(DateHelper.GetWorkingDaysDifference(
                    DateTime.Now,
                    (DateTime)e.Screening.ScreeningDeadlineDate,
                    publicHolidaysDTO)),
                DayPendingInt = DateHelper.GetWorkingDaysDifference(
                    DateTime.Now,
                    (DateTime)e.Screening.ScreeningDeadlineDate,
                    publicHolidaysDTO),
                Qualified = e.IsQualified,
                InternalDiscussionId =
                    e.InternalDiscussionId
            });
        }

        /// <summary>
        /// Helper used to build atomic check form view model from atomic check data
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        public static AtomicCheckFormViewModel BuildAtomicCheckFormViewModel(AtomicCheckDTO atomicCheckDTO)
        {
            var atomicCheckStates = AtomicCheckState.GetNextStatesAsDictionnary(
                (AtomicCheckValidationStateType) atomicCheckDTO.AtomicCheckValidationState);
            // Atomic check cannot be wrongly qualified
            if (!atomicCheckDTO.CanBeWronglyQualified)
                atomicCheckStates.Remove((int)AtomicCheckStateType.WRONGLY_QUALIFIED);



            var viewModel = new AtomicCheckFormViewModel
            {
                Id = atomicCheckDTO.AtomicCheckId,
                ScreeningId = atomicCheckDTO.Screening.ScreeningId,
                CanBeProcessed = atomicCheckDTO.CanBeProcessed,
                CanBeValidated = atomicCheckDTO.CanBeValidated,
                CanBeRejected = atomicCheckDTO.CanBeRejected,
                IsValidated = atomicCheckDTO.IsValidated,
                ScreeningFullName = atomicCheckDTO.Screening.ScreeningFullName,
                AssignedTo = atomicCheckDTO.Screener.FullName,
                TypeOfCheck = atomicCheckDTO.TypeOfCheck.CheckName,
                Category = atomicCheckDTO.AtomicCheckCategory,
                Type = atomicCheckDTO.AtomicCheckType,
                Remarks = atomicCheckDTO.AtomicCheckRemarks,
                Report = atomicCheckDTO.AtomicCheckReport ?? "" ,
                Attachments = atomicCheckDTO.Attachment.Select(e => new AttachmentViewModel
                {
                    Id = e.AttachmentId,
                    FileName = e.AttachmentName,
                    FilePath = e.AttachmentFilePath,
                    AtomicCheckId = atomicCheckDTO.AtomicCheckId
                }),
                Summary = atomicCheckDTO.AtomicCheckSummary,
                Comments = atomicCheckDTO.TypeOfCheckComments,
                Status = FormHelper.BuildDropDownListViewModel(atomicCheckStates, null, atomicCheckDTO.State),
                ValidationStatus = FormHelper.BuildDropDownListViewModel(
                        AtomicCheckValidationState.GetNextValidationStatesAsDictionnary(
                            (AtomicCheckValidationStateType)atomicCheckDTO.AtomicCheckValidationState), null, atomicCheckDTO.ValidationState),
                ScreeningPhysicalPath = atomicCheckDTO.Screening.ScreeningPhysicalPath,
                AtomicCheckReportPhysicalPath = FileHelper.GetAtomicCheckReportPhysicalPath(atomicCheckDTO.Screening, atomicCheckDTO),
                AtomicCheckPictureReportPhysicalPath = FileHelper.GetAtomicCheckPictureReportPhysicalPath(atomicCheckDTO.Screening, atomicCheckDTO),
                AtomicCheckReportVirtualPath = FileHelper.GetAtomicCheckReportVirtualPath(atomicCheckDTO.Screening, atomicCheckDTO),
                AtomicCheckPictureReportVirtualPath = FileHelper.GetAtomicCheckPictureReportVirtualPath(atomicCheckDTO.Screening, atomicCheckDTO),
            };

            viewModel.Description =
                atomicCheckDTO.Screening.ScreeningLevelVersion.ScreeningLevelVersionAllowedToContactCandidate ==
                true
                    ? "Allowed to contact candidate" + Environment.NewLine + Environment.NewLine
                    : "Not allowed to contact candidates" + Environment.NewLine + Environment.NewLine;

            if (atomicCheckDTO.QualificationPlace != null)
            {
                var currentCompany = "";
                viewModel.QualificationPlaceName = atomicCheckDTO.QualificationPlace.QualificationPlaceName;
                viewModel.QualificationPlaceId = atomicCheckDTO.QualificationPlace.QualificationPlaceId;

                if (atomicCheckDTO.QualificationPlace.ScreeningQualificationPlaceMeta.Any(
                        u => u.ScreeningQualificationMetaKey == ScreeningQualificationPlaceMeta.kIsCurrentCompany
                        && u.ScreeningQualificationMetaValue == ScreeningQualificationPlaceMeta.kYesValue))
                {
                    viewModel.Description = viewModel.Description +
                        atomicCheckDTO.Screening.ScreeningLevelVersion.ScreeningLevelVersionAllowedToContactCurrentCompany +
                        Environment.NewLine + Environment.NewLine;
                    currentCompany = " (Current company)";
                }

                viewModel.Description = viewModel.Description + atomicCheckDTO.QualificationPlace.QualificationPlaceName + currentCompany + Environment.NewLine
                    + AddressHelper.GetAddressAsTextArea(atomicCheckDTO.QualificationPlace.Address);
            }
            if (atomicCheckDTO.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.NEIGHBOURHOOD_CHECK)
            {
                viewModel.Description = atomicCheckDTO.AtomicCheckType + Environment.NewLine;
                if (atomicCheckDTO.AtomicCheckType == AtomicCheck.kCurrentAddressType)
                {
                    viewModel.Description = viewModel.Description + AddressHelper.GetAddressAsTextArea(atomicCheckDTO.Screening.ScreeningQualification.CurrentAddress);
                }
                if (atomicCheckDTO.AtomicCheckType == AtomicCheck.kCVAddressType)
                {
                    viewModel.Description = viewModel.Description + AddressHelper.GetAddressAsTextArea(atomicCheckDTO.Screening.ScreeningQualification.CVAddress);
                } 
                if (atomicCheckDTO.AtomicCheckType == AtomicCheck.kIDCardAddressType)
                {
                    viewModel.Description = viewModel.Description + AddressHelper.GetAddressAsTextArea(atomicCheckDTO.Screening.ScreeningQualification.IDCardAddress);
                }
            }
            if (atomicCheckDTO.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.CONTACT_NUMBER_CHECK)
            {
                viewModel.Description = atomicCheckDTO.AtomicCheckType + Environment.NewLine;
                if (atomicCheckDTO.AtomicCheckType == AtomicCheck.kMobilePhoneNumberType)
                {
                    viewModel.Description = viewModel.Description +
                                            atomicCheckDTO.Screening.ScreeningQualification.PersonalContactInfo
                                                .MobilePhoneNumber;
                }
                if (atomicCheckDTO.AtomicCheckType == AtomicCheck.kHomePhoneNumberType)
                {
                    viewModel.Description = viewModel.Description +
                                            atomicCheckDTO.Screening.ScreeningQualification.PersonalContactInfo
                                                .HomePhoneNumber;
                }
                if (atomicCheckDTO.AtomicCheckType == AtomicCheck.kEmergencyContactType)
                {
                    viewModel.Description = viewModel.Description +
                                            atomicCheckDTO.Screening.ScreeningQualification.EmergencyContactPerson
                                                .ContactPersonName +
                                            Environment.NewLine +
                                            atomicCheckDTO.Screening.ScreeningQualification.EmergencyContactPerson
                                                .ContactInfo.HomePhoneNumber;
                }
            }
            return viewModel;
        }
       
        /// <summary>
        /// Extract atomic check information from the view model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static AtomicCheckDTO ExtractAtomicCheckDTOFromViewModel(
            AtomicCheckFormViewModel model)
        {
            var atomicCheckDTO = new AtomicCheckDTO
            {
                AtomicCheckId = model.Id,
                AtomicCheckReport = model.Report.IsNullOrWhiteSpace() ? "":
                    HttpUtility.HtmlDecode(HTMLHelper.CleanHtmlReport(model.Report)),
                AtomicCheckRemarks = model.Remarks.IsNullOrWhiteSpace() ? "" : model.Remarks,
                AtomicCheckState = (Byte)FormHelper.ExtractDropDownListViewModel(model.Status),
                AtomicCheckValidationState = (Byte)FormHelper.ExtractDropDownListViewModel(model.ValidationStatus),
                AtomicCheckSummary = model.Summary,
                Screening = new ScreeningDTO
                {
                    ScreeningId = model.ScreeningId,
                    ScreeningPhysicalPath = model.ScreeningPhysicalPath,
                }
            };
            atomicCheckDTO.Attachment = BuildAtomicCheckReportAttachments(model.AttachmentFiles, atomicCheckDTO);
            return atomicCheckDTO;
        }

        /// <summary>
        /// Build atomic check attachment from post method
        /// </summary>
        /// <param name="cV"></param>
        /// <param name="attachments"></param>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        private static ICollection<AttachmentDTO> BuildAtomicCheckReportAttachments(
            IEnumerable<HttpPostedFileBase> attachments, AtomicCheckDTO atomicCheckDTO)
        {
            var attachmentDTOs = new List<AttachmentDTO>();
            if (attachments != null)
            {
                //build attachment files
                var attachmentFiles = attachments.Select(
                    e => new AttachmentDTO
                    {
                        AttachmentCreatedDate = DateTime.Now,
                        AttachmentFileType = e.ContentType,
                        AttachmentName = e.FileName,
                        AttachmentFilePath =
                            Path.Combine(FileHelper.GetAtomicCheckReportAttachmentPhysicalPath(atomicCheckDTO.Screening, atomicCheckDTO),
                                (e.FileName))
                    }).ToList();
                attachmentDTOs.AddRange(attachmentFiles);
            }
            return attachmentDTOs;
        }
    
    }
}