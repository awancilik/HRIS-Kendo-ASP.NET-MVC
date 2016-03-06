using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.Settings;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.AtomicCheck;

namespace CVScreeningWeb.Controllers
{
    [Authorize]
    [Filters.HandleError]
    public class AtomicCheckController : Controller
    {
        private readonly IScreeningService _screeningService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly ISettingsService _settingsService;
        private readonly INotificationService _notificationService;

        public AtomicCheckController(
            IScreeningService screeningService, IErrorMessageFactoryService errorMessageFactoryService,
            ISettingsService settingsService, INotificationService notificationService)
        {
            _screeningService = screeningService;
            _errorMessageFactoryService = errorMessageFactoryService;
            _settingsService = settingsService;
            _notificationService = notificationService;
        }

        public ActionResult ManageForScreening(int id)
        {
            var atomicChecks = _screeningService.GetAllBaseAtomicChecksForScreening(new ScreeningDTO {ScreeningId = id});
            var viewModels = AtomicCheckHelper.BuildAtomicCheckManageViewModels(atomicChecks, _settingsService.GetAllPublicHolidays());
            return View(viewModels);
        }

        public ActionResult Detail(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GET Action - Atomic check edition 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var atomicCheckDTO = _screeningService.GetAtomicCheck(id);
            if (atomicCheckDTO == null)
            {
                const ErrorCode errorCode = ErrorCode.ATOMIC_CHECK_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });
            }
            var model = AtomicCheckHelper.BuildAtomicCheckFormViewModel(atomicCheckDTO);
            model.PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "";

            // Create folder in the server to store report images, files and attachements
            Directory.CreateDirectory(FileHelper.GetAtomicCheckReportPhysicalPath(atomicCheckDTO.Screening, atomicCheckDTO));
            Directory.CreateDirectory(FileHelper.GetAtomicCheckPictureReportPhysicalPath(atomicCheckDTO.Screening, atomicCheckDTO));

            return View(model);
        }

        /// <summary>
        /// POST Action - Atomic check edition 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Filters.NotificationAttribute]
        public ActionResult Edit(AtomicCheckFormViewModel model)
        {
            RouteData.Values.Add("notificationService", _notificationService);
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var atomicCheckDTO = AtomicCheckHelper.ExtractAtomicCheckDTOFromViewModel(model);

            //Validate attachment files
            if ((model.AttachmentFiles != null &&
                 (!model.AttachmentFiles.Any(FileHelper.ValidateFileSize) ||
                  !model.AttachmentFiles.Any(FileHelper.ValidateImageContentType))))
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(ErrorCode.FILE_NOT_VALIDATED));
                return View(AtomicCheckHelper.BuildAtomicCheckFormViewModel(_screeningService.GetAtomicCheck(model.Id)));
            }

            SaveFiles(model, atomicCheckDTO.Attachment, atomicCheckDTO);

            var errorCode = _screeningService.EditAtomicCheck(ref atomicCheckDTO);
            if (errorCode != ErrorCode.NO_ERROR)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return View(AtomicCheckHelper.BuildAtomicCheckFormViewModel(_screeningService.GetAtomicCheck(model.Id)));
            }
            return !String.IsNullOrEmpty(model.PreviousPage)
                ? (ActionResult)Redirect(model.PreviousPage)
                : RedirectToAction("ManageForScreening", "AtomicCheck", new { id = model.ScreeningId });
        }

        /// <summary>
        /// Save attachments of a report in the filesystem
        /// </summary>
        /// <param name="model"></param>
        /// <param name="attachment"></param>
        /// <param name="atomicCheckDTO"></param>
        private void SaveFiles(AtomicCheckFormViewModel model, ICollection<AttachmentDTO> attachment,
            AtomicCheckDTO atomicCheckDTO)
        {
            for (var i = 0; i < attachment.Count; i++)
            {
                var attachmentDTO = attachment.ToArray()[i];

                if (model.AttachmentFiles == null)
                    return;
                Directory.CreateDirectory(FileHelper.GetAtomicCheckReportAttachmentPhysicalPath(atomicCheckDTO.Screening, atomicCheckDTO));
                model.AttachmentFiles.ToArray()[i].SaveAs(attachmentDTO.AttachmentFilePath);
            }
        }

        /// <summary>
        /// Action GET - Assign atomic check to a screener
        /// </summary>
        /// <param name="id">Atomic check id</param>
        /// <param name="secondaryId">Screening id</param>
        /// <returns></returns>
        public ActionResult AssignedTo(int id, int secondaryId)
        {
            var model = new AtomicCheckAssignToViewModel
            {
                AtomicCheckId = id,
                ScreeningId = secondaryId,
                PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : ""
            };
            return PartialView("_AssignTo", model);
        }


        /// <summary>
        /// Action POST - Assign atomic check to a screener
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Filters.NotificationAttribute]
        public ActionResult AssignedTo(AtomicCheckAssignToViewModel model)
        {
            RouteData.Values.Add("notificationService", _notificationService);
            if (!ModelState.IsValid)
            {
                return PartialView("_AssignTo", model);
            }  

            var atomicCheckDTO = new AtomicCheckDTO {AtomicCheckId = model.AtomicCheckId};
            var screenerDTO = new UserProfileDTO { UserId = model.UserProfileId };

            var errorCode = _screeningService.AssignAtomicCheck(ref atomicCheckDTO, screenerDTO);
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
        /// Child action used to build atomic check action menu
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult ActionMenu(AtomicCheckManageViewModel model)
        {
            return PartialView("_Action",model);
        }
    }
}
