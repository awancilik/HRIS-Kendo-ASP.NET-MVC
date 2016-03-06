using System;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Screening;
using CVScreeningService.Services.File;
using CVScreeningService.Services.UserManagement;

namespace CVScreeningWeb.Controllers
{
    [Authorize]
    [Filters.HandleError]
    public class SharedController : Controller
    {
        private readonly IFileService _fileService;
        private readonly IUserManagementService _userManagementService;

        public SharedController(IFileService fileService, IUserManagementService userManagementService)
        {
            _fileService = fileService;
            _userManagementService = userManagementService;
        }

        /// <summary>
        /// Download file to client browser
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Fileresult representing file content</returns>
        public FileResult DownloadFile(string id)
        {
            var attachment = _fileService.GetAttachment(Convert.ToInt32(id));

            if (!ValidateLegitimatedUser(attachment))
                return null;

            var fileBytes = System.IO.File.ReadAllBytes(attachment.AttachmentFilePath);
            var fileName = attachment.AttachmentName;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        //!TODO Handling error when deleting attachment, required to use Ajax => not in the scope so far
        /// <summary>
        /// Delete atomic check attachment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteAtomicCheckAttachment(int id, int secondaryId)
        {
            ErrorCode error = _fileService.DeleteAttachment(Convert.ToInt32(id));
            return RedirectToAction("Edit", "AtomicCheck", new { id = secondaryId });
        }


        private bool ValidateLegitimatedUser(AttachmentDTO attachment)
        {
            if (!User.IsInRole("Client")) return true;
              
            var user = _userManagementService.GetUserProfilebyName(User.Identity.Name);
            return (attachment.ClientCompanyId == user.ClientCompanyForClientUserProfile.ClientCompanyId);
        }
    }
}