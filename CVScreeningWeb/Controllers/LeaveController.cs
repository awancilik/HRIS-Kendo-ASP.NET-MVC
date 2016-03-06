using System;
using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb.ViewModels.Leave;
using UserProfile = CVScreeningCore.Models.webpages_UserProfile;

namespace CVScreeningWeb.Controllers
{
    
    [Filters.HandleError]
    [Authorize(Roles = "Administrator, HR")]
    public class LeaveController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        /// <summary>
        ///     Constructor to pass service
        /// </summary>
        /// <param name="userManagementService"></param>
        public LeaveController(
            IUserManagementService userManagementService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _userManagementService = userManagementService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }
        
        //
        // GET: /Leave/Index
        public ActionResult Index(int id)
        {
            var userLeaves = _userManagementService.GeAllUserLeavesByUserId(id);
            var user = _userManagementService.GetUserProfileById(id);

            if (user == null || userLeaves == null)
            {
                var error = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            userLeaves = userLeaves.Where(u => u.UserLeaveStartDate.Year == DateTime.Now.Year || 
                 u.UserLeaveEndDate.Year == DateTime.Now.Year );
            var userLeaveVm = new LeaveManageViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName,
                UserId = user.UserId,
                UserLeaves = userLeaves.Select(userLeave => new LeaveUniqueViewModel
                {
                    No = userLeave.UserLeaveId,
                    Name = userLeave.UserLeaveName,
                    Remarks = userLeave.UserLeaveRemarks,
                    StartDate = userLeave.UserLeaveStartDate.ToShortDateString(),
                    EndDate = userLeave.UserLeaveEndDate.ToShortDateString()
                }).ToList()
            };

            ViewBag.IsKendoEnabled = true;
            return View(userLeaveVm);

        }

        //
        // GET: /Leave/Create
        public ActionResult Create(int id)
        {
            var user = _userManagementService.GetUserProfileById(id);

            if (user == null)
            {
                var error = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            var aViewModel = new LeaveFormViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName,
                UserId = user.UserId,
                StartDate = null,
                EndDate = null
            };
            ViewBag.IsKendoEnabled = true;
            return View(aViewModel);
        }

        //
        // POST: /Leave/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LeaveFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Leave creation failed.");
                ViewBag.IsKendoEnabled = true;
                return View(iModel);
            }

            var userProfileDTO = new UserProfileDTO
            {
                UserId = iModel.UserId
            };

            var userLeaveDTO = new UserLeaveDTO
            {
                UserLeaveName = iModel.Name,
                UserLeaveRemarks = iModel.Remarks,
                UserLeaveStartDate = (DateTime)iModel.StartDate,
                UserLeaveEndDate = (DateTime)iModel.EndDate
            };

            var error = _userManagementService.CreateLeave(userProfileDTO, ref userLeaveDTO);
            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Index", "Leave", new { id = iModel.UserId });
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            ViewBag.IsKendoEnabled = true;
            return View(iModel);
        }



        //
        // GET: /Leave/Edit
        public ActionResult Edit(int id, int secondaryId)
        {
            var userDTO = _userManagementService.GetUserProfileById(id);
            var userLeaveDTO = _userManagementService.GetUserLeave(secondaryId);

            if (userDTO == null)
            {
                var error = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            if (userLeaveDTO == null)
            {
                var error = ErrorCode.USER_LEAVE_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            var aViewModel = new LeaveFormViewModel
            {
                UserName = userDTO.UserName,
                FullName = userDTO.FullName,
                UserId = userDTO.UserId,
                LeaveId = secondaryId,
                StartDate = userLeaveDTO.UserLeaveStartDate,
                EndDate = userLeaveDTO.UserLeaveEndDate,
                Remarks = userLeaveDTO.UserLeaveRemarks,
                Name = userLeaveDTO.UserLeaveName
            };
            ViewBag.IsKendoEnabled = true;
            return View(aViewModel);
        }

        //
        // POST: /Leave/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LeaveFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Leave edition failed.");
                ViewBag.IsKendoEnabled = true;
                return View(iModel);
            }

            var userLeaveDTO = new UserLeaveDTO
            {
                UserLeaveId = iModel.LeaveId,
                UserLeaveName = iModel.Name,
                UserLeaveRemarks = iModel.Remarks,
                UserLeaveStartDate = (DateTime)iModel.StartDate,
                UserLeaveEndDate = (DateTime)iModel.EndDate
            };
            var error = _userManagementService.EditLeave(userLeaveDTO);
            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Index", "Leave", new { id = iModel.UserId });
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            ViewBag.IsKendoEnabled = true;
            return View(iModel);
        }


        //
        // GET: /Leave/Delete/id
        public ActionResult Delete(int id, int secondaryId)
        {
            ErrorCode error;
            var userDTO = _userManagementService.GetUserProfileById(id);
            var userLeaveDTO = _userManagementService.GetUserLeave(secondaryId);

            if (userDTO == null)
            {
                error = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            if (userLeaveDTO == null)
            {
                error = ErrorCode.USER_LEAVE_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            var userProfileDTO = new UserProfileDTO
            {
                UserId = id
            };

            error = _userManagementService.DeleteUserLeave(userProfileDTO, secondaryId);
            return error == ErrorCode.NO_ERROR 
                    ? RedirectToAction("Index", "Leave", new { id = id }) 
                    : RedirectToAction("Index", "Error", new { errorCodeParameter = error });


        }

    }
}