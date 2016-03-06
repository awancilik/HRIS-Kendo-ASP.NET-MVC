using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.DispatchingManagement;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.Settings;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.Screening;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.Controllers
{
    [Authorize]
    [Filters.HandleError]
    
    public class ScreeningController : Controller
    {
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly IScreeningService _screeningService;
        private readonly IClientService _clientService;
        private readonly IUserManagementService _userManagementService;
        private readonly ISettingsService _settingsService;
        private readonly INotificationService _notificationService;
        private readonly IDispatchingManagementService _dispatchingService;

        public ScreeningController(IErrorMessageFactoryService errorMessageFactoryService,
            IScreeningService screeningService, IClientService clientService,
            IUserManagementService userManagementService,
            ISettingsService settingsService, INotificationService notificationService,
            IDispatchingManagementService dispatchingService)
        {
            _errorMessageFactoryService = errorMessageFactoryService;
            _screeningService = screeningService;
            _clientService = clientService;
            _userManagementService = userManagementService;
            _settingsService = settingsService;
            _notificationService = notificationService;
            _dispatchingService = dispatchingService;
        }

        /// <summary>
        /// Display grid view of all screening.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var screening = _screeningService.GetAllScreenings();

            if (User.IsInRole("Client"))
            {
                var user = _userManagementService.GetUserProfilebyName(User.Identity.Name);
                screening =
                    screening.Where(
                        e => user.ClientCompanyForClientUserProfile.ClientCompanyId == e.ClientCompanyId).ToList();
            }

            return View(ScreeningHelper.BuildScreeningManageViewModels(screening, _settingsService.GetAllPublicHolidays()));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var viewModel = InitializeViewModel(new ScreeningFormViewModel());
            return View(viewModel);
        }

        private ScreeningFormViewModel InitializeViewModel(ScreeningFormViewModel iModel)
        {
            iModel.ScreeningLevelVersion = ScreeningLevelVersionHelper.BuildScreeningLevelVersionViewModel();

            // if the user is a client, then filter based on the company belonging to him
            if (User.IsInRole("Client"))
            {
                var user = _userManagementService.GetUserProfilebyName(User.Identity.Name);
                iModel.ScreeningLevelVersion.IsClientMode = true;
                iModel.ScreeningLevelVersion.ClientCompanyId = user.ClientCompanyForClientUserProfile.ClientCompanyId + "";
            }

            iModel.PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "";
            return iModel;
        }

        [HttpPost]
        [Filters.NotificationAttribute]
        public ActionResult Create(ScreeningFormViewModel iModel)
        {
            RouteData.Values.Add("notificationService", _notificationService);
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = InitializeViewModel(iModel);
                return View(iModel);
            }

            //CV file is required
            if (iModel.CVFile == null)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.SCREENING_CV_FILE_REQUIRED));
                iModel = InitializeViewModel(iModel);
                return View(iModel);
            }

            //Validate CV file and attachment files
            if (!FileHelper.ValidateFileSize(iModel.CVFile) || !FileHelper.ValidateContentType(iModel.CVFile) ||
                (iModel.Attachments != null &&
                 (!iModel.AttachmentFiles.Any(FileHelper.ValidateFileSize) ||
                  !iModel.AttachmentFiles.Any(FileHelper.ValidateContentType))))
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(
                    ErrorCode.FILE_NOT_VALIDATED));
                iModel = InitializeViewModel(iModel);
                return View(iModel);
            }

            var screeningLevelVersioDTO =
                _screeningService.GetActiveScreeningLevelVersion(Convert.ToInt32(
                        iModel.ScreeningLevelVersion.ScreeningLevelId));

            // No screening level version active for this date
            if (screeningLevelVersioDTO == null)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(
                    ErrorCode.SCREENING_LEVEL_VERSION_NOT_EXISTING_FOR_PERIOD));
                iModel = InitializeViewModel(iModel);
                return View(iModel);
            }

            var screeningDTO = new ScreeningDTO
            {
                ScreeningFullName = iModel.Name,
                ScreeningAdditionalRemarks = iModel.AdditionnalRemarks,
                ScreeningComments = iModel.Comments,
                ScreeningLevelVersion = screeningLevelVersioDTO,
            };

            screeningDTO.ScreeningPhysicalPath = Server.MapPath(FileHelper.GenerateScreeningFilePath(
                screeningDTO.ScreeningLevelVersion, screeningDTO.ScreeningFullName));
            screeningDTO.ScreeningVirtualPath = FileHelper.GenerateScreeningFilePath(
                screeningDTO.ScreeningLevelVersion, screeningDTO.ScreeningFullName).TrimStart('~');

            screeningDTO.Attachment = BuildScreeningAttachments(iModel.CVFile, iModel.AttachmentFiles,
                screeningDTO);
            SaveFiles(iModel, screeningDTO.Attachment, screeningDTO);

            ErrorCode errorCode = _screeningService.CreateScreening(screeningLevelVersioDTO, ref screeningDTO);
            if (errorCode == ErrorCode.NO_ERROR)
            {
                return !String.IsNullOrEmpty(iModel.PreviousPage)
                    ? (ActionResult) Redirect(iModel.PreviousPage)
                    : RedirectToAction("Index");
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            iModel = InitializeViewModel(iModel);
            return View(iModel);
        }

        private void SaveFiles(ScreeningFormViewModel iModel, ICollection<AttachmentDTO> attachment,
            ScreeningDTO screeningDTO)
        {
            for (var i = 0; i < attachment.Count; i++)
            {
                var attachmentDTO = attachment.ToArray()[i];
                //make sure that the first item in the list is CV file
                if (i == 0 && attachmentDTO.AttachmentFileType.Equals("CV"))
                {
                    Directory.CreateDirectory(screeningDTO.ScreeningPhysicalPath);
                    Directory.CreateDirectory(FileHelper.GetScreeningReportPhysicalPath(screeningDTO));
                    iModel.CVFile.SaveAs(attachmentDTO.AttachmentFilePath);
                }
                else
                {
                    if (iModel.AttachmentFiles == null)
                        return;
                    Directory.CreateDirectory(FileHelper.GetScreeningAttachmentPhysicalPath(screeningDTO));
                    iModel.AttachmentFiles.ToArray()[i - 1].SaveAs(attachmentDTO.AttachmentFilePath);
                }
            }
        }

        private void SaveAttachmentsFiles(ScreeningFormViewModel iModel, ICollection<AttachmentDTO> attachment,
            ScreeningDTO screeningDTO)
        {
            for (var i = 0; i < attachment.Count; i++)
            {
                var attachmentDTO = attachment.ToArray()[i];
                Directory.CreateDirectory(FileHelper.GetScreeningAttachmentPhysicalPath(screeningDTO));
                iModel.AttachmentFiles.ToArray()[i].SaveAs(attachmentDTO.AttachmentFilePath);
            }
        }

        private ICollection<AttachmentDTO> BuildScreeningAttachments(HttpPostedFileBase cV,
            IEnumerable<HttpPostedFileBase> attachments, ScreeningDTO screeningDTO)
        {
            var attachmentDTOs = new List<AttachmentDTO>();
            //build CV file
            var CV = new AttachmentDTO
            {
                AttachmentCreatedDate = DateTime.Now,
                AttachmentFileType = "CV",
                AttachmentName = FileHelper.BuildCVFileName(cV),
                AttachmentFilePath = Path.Combine(screeningDTO.ScreeningPhysicalPath,
                    FileHelper.BuildCVFileName(cV))
            };
            attachmentDTOs.Add(CV);

            if (attachments != null)
            {
                //build attachment files
                var attachmentFiles = attachments.Select(
                    e => new AttachmentDTO
                    {
                        AttachmentCreatedDate = DateTime.Now,
                        AttachmentFileType = e.ContentType,
                        AttachmentName = FileHelper.BuildAttachmentName(e.FileName),
                        AttachmentFilePath =
                            Path.Combine(FileHelper.GetScreeningAttachmentPhysicalPath(screeningDTO),
                                (e.FileName))
                    }).ToList();
                attachmentDTOs.AddRange(attachmentFiles);
            }
            return attachmentDTOs;
        }

        private ICollection<AttachmentDTO> BuildScreeningAttachments(
            IEnumerable<HttpPostedFileBase> attachments, ScreeningDTO screeningDTO)
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
                        AttachmentName = FileHelper.BuildAttachmentName(e.FileName),
                        AttachmentFilePath =
                            Path.Combine(FileHelper.GetScreeningAttachmentPhysicalPath(screeningDTO),
                                (e.FileName))
                    }).ToList();
                attachmentDTOs.AddRange(attachmentFiles);
            }
            return attachmentDTOs;
        }


        public ActionResult Edit(int id)
        {
            var screeningDTO = _screeningService.GetScreening(id);
            var attacmentDTOs = screeningDTO.Attachment.Where(e => !e.AttachmentFileType.Equals("CV"));
            var cVDTO = screeningDTO.Attachment.First(e => e.AttachmentFileType.Equals("CV"));
            var screeningViewModel = new ScreeningFormViewModel
            {
                Id = screeningDTO.ScreeningId,
                Name = screeningDTO.ScreeningFullName,
                Comments = screeningDTO.ScreeningComments,
                AdditionnalRemarks = screeningDTO.ScreeningAdditionalRemarks,
                ScreeningLevelVersion = ScreeningLevelVersionHelper.BuildScreeningLevelVersionViewModel(
                    screeningDTO.ScreeningLevelVersion, User.IsInRole("Client")),
                ScreeningVirtualPath = screeningDTO.ScreeningVirtualPath,
                ScreeningPhysicalPath = screeningDTO.ScreeningPhysicalPath,
                Attachments = attacmentDTOs.Select(e => new AttachmentViewModel
                {
                    Id = e.AttachmentId,
                    FileName = e.AttachmentName,
                    FilePath = e.AttachmentFilePath
                }),
                CV = new AttachmentViewModel
                {
                    Id = cVDTO.AttachmentId,
                    FileName = cVDTO.AttachmentName,
                    FilePath = cVDTO.AttachmentFilePath
                },
                PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : ""
            };
            return View(screeningViewModel);
        }


        [HttpPost]
        public ActionResult Edit(ScreeningFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = InitializeViewModel(iModel);
                return View(iModel);
            }


            //Validate attachment files
            if (iModel.Attachments != null && (!iModel.AttachmentFiles.Any(FileHelper.ValidateFileSize) ||
                                               !iModel.AttachmentFiles.Any(FileHelper.ValidateContentType)))
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(
                    ErrorCode.FILE_NOT_VALIDATED));
                iModel = InitializeViewModel(iModel);
                return View(iModel);
            }

            var screeningDTO = new ScreeningDTO
            {
                ScreeningFullName = iModel.Name,
                ScreeningAdditionalRemarks = iModel.AdditionnalRemarks,
                ScreeningId = iModel.Id,
                ScreeningPhysicalPath = iModel.ScreeningPhysicalPath,
                ScreeningVirtualPath = iModel.ScreeningVirtualPath
            };

            screeningDTO.Attachment = BuildScreeningAttachments(iModel.AttachmentFiles, screeningDTO);
            SaveAttachmentsFiles(iModel, screeningDTO.Attachment, screeningDTO);

            ErrorCode errorCode = _screeningService.EditScreening(ref screeningDTO);
            if (errorCode == ErrorCode.NO_ERROR)
            {
                return !String.IsNullOrEmpty(iModel.PreviousPage)
                    ? (ActionResult) Redirect(iModel.PreviousPage)
                    : RedirectToAction("Index");
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            iModel = InitializeViewModel(iModel);
            return View(iModel);
        }

        public ActionResult Detail(int id)
        {
            var screeningDTO = _screeningService.GetScreening(id);
            var attacmentDTOs = screeningDTO.Attachment.Where(e => !e.AttachmentFileType.Equals("CV"));
            var cVDTO = screeningDTO.Attachment.First(e => e.AttachmentFileType.Equals("CV"));
            var screeningViewModel = new ScreeningFormViewModel
            {
                Id = screeningDTO.ScreeningId,
                Name = screeningDTO.ScreeningFullName,
                Comments = screeningDTO.ScreeningComments,
                AdditionnalRemarks = screeningDTO.ScreeningAdditionalRemarks,
                ScreeningLevelVersion = ScreeningLevelVersionHelper.BuildScreeningLevelVersionViewModel(
                    screeningDTO.ScreeningLevelVersion, User.IsInRole("Client")),
                Attachments = attacmentDTOs.Select(e => new AttachmentViewModel
                {
                    Id = e.AttachmentId,
                    FileName = e.AttachmentName,
                    FilePath = e.AttachmentFilePath
                }),
                CV = new AttachmentViewModel
                {
                    Id = cVDTO.AttachmentId,
                    FileName = cVDTO.AttachmentName,
                    FilePath = cVDTO.AttachmentFilePath
                },
                PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : ""
            };
            return View(screeningViewModel);
        }

        public JsonResult GetClientCompanyJSON()
        {
            var clientCompanies = User.IsInRole(webpages_Roles.kAccountManagerRole)
                ? _clientService.GetAllClientCompaniesForAccountManager()
                : _clientService.GetAllClientCompanies();
            return
                Json(
                    clientCompanies.Select(
                        e => new {ClientCompanyId = e.ClientCompanyId, ClientCompanyName = e.ClientCompanyName}),
                    JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetScreenerJSON(int id)
        {
            var atomicCheck = _screeningService.GetAtomicCheck(id);

            var screeners = _userManagementService.GetUserProfilesByRoles(webpages_Roles.kScreenerRole).Where(
                u => u.ScreenerCategory == atomicCheck.AtomicCheckCategory);

            return
                Json(
                    screeners.Select(
                        e => new {screenerId = e.UserId, screenerName = e.FullName}),
                    JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetQualityControlJSON()
        {
            var qualityControls = _userManagementService.GetUserProfilesByRoles(webpages_Roles.kQualityControlRole);
            return
                Json(
                    qualityControls.Select(
                        e => new { qualityControlId = e.UserId, qualityControlName = e.FullName }), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetContractJSON(int clientCompanyId)
        {
            var contract = _clientService.GetAllClientContractsByCompany(new ClientCompanyDTO
            {
                ClientCompanyId = clientCompanyId
            }).FirstOrDefault();

            return Json(
                contract != null
                    ? new {ContractId = contract.ContractId, ContractName = contract.ContractReference}
                    : new {ContractId = 0, ContractName = ""},
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetScreeningLevelByCompanyJSON(int clientCompanyId)
        {
            var contract = _clientService.GetAllClientContractsByCompany(new ClientCompanyDTO
            {
                ClientCompanyId = clientCompanyId
            });

            return contract.Any() 
                ? GetScreeningLevelJSON(contract.First(e => e.IsContractEnabled).ContractId) : null;
        }

        public JsonResult GetScreeningLevelJSON(int contractId)
        {
            var screeningLevels = _clientService.GetScreeningLevelsByContract(new ClientContractDTO
            {
                ContractId = contractId
            });

            return Json(
                screeningLevels.Select(
                    e => new
                    {
                        ScreeningLevelId = e.ScreeningLevelId,
                        ScreeningLevelName = e.ScreeningLevelName
                    }),
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetScreeningLevelVersionJSON(int screeningLevelId)
        {
            var screeningLevelVersions = _screeningService.GetAllScreeningLevelVersionsByScreeningLevel(screeningLevelId);

            return Json(screeningLevelVersions.Select(
                e =>
                    new
                    {
                        ScreeningLevelVersionId = e.ScreeningLevelVersionId,
                        ScreeningLevelVersionName = e.ScreeningLevelVersionNumber
                    }),
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTypeOfCheckJSON()
        {
            var typeOfChecks = _screeningService.GetAllTypeOfChecks();
            return Json(typeOfChecks.Select(e =>
                new
                {
                    TypeOfCheckId = e.TypeOfCheckId,
                    TypeOfCheckName = e.CheckName
                }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Deactivate(int id)
        {
            ErrorCode errorCode = _screeningService.DeactivateScreening(id);
            if (!ModelState.IsValid || errorCode != ErrorCode.NO_ERROR)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            }
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Action GET - Assign a screener to a quality control
        /// </summary>
        /// <param name="id">Screening id</param>
        /// <returns></returns>
        public ActionResult AssignedTo(int id)
        {
            var model = new ScreeningAssignedToViewModel
            {
                ScreeningId = id,
                PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : ""
            };
            return PartialView("_AssignTo", model);
        }


        /// <summary>
        /// Action POST - Assign a screener to a quality control
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AssignedTo(ScreeningAssignedToViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AssignTo", model);
            }

            var screeningDTO = new ScreeningDTO { ScreeningId = model.ScreeningId };
            var screenerDTO = new UserProfileDTO { UserId = model.UserProfileId };

            var errorCode = _screeningService.AssignScreening(ref screeningDTO, screenerDTO);
            if (errorCode != ErrorCode.NO_ERROR)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return PartialView("_AssignTo", model);
            }
            return Json(new
            {
                redirectTo = model.PreviousPage
            });
        }



        /// <summary>
        /// Child action used to build screening action menu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult ActionMenu(ScreeningManageViewModel model)
        {
            return PartialView("_Action", model);
        }

        public ActionResult DispatchAtomicChecks()
        {
            _dispatchingService.DispatchAtomicChecks();
            return RedirectToAction("Index");
        }
    }
}