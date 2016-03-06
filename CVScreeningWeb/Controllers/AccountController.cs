using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.Account;
using Postal;
using UserProfile = CVScreeningCore.Models.webpages_UserProfile;

namespace CVScreeningWeb.Controllers
{
    [Filters.HandleError]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly IUserManagementService _userManagementService;

        /// <summary>
        ///     Constructor to pass service
        /// </summary>
        /// <param name="userManagementService"></param>
        public AccountController(
            IUserManagementService userManagementService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _userManagementService = userManagementService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }


        /// <summary>
        ///     GET - Login controller - Log into the application
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        /// <summary>
        ///     POST - Login controller - Log into the application
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountLoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Login failed.");
                return View();
            }

            var errorCode = _userManagementService.Login(model.UserName, model.Password);
            if (errorCode == ErrorCode.NO_ERROR)
            {
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View();
        }


        /// <summary>
        ///     POST - Logoff controller to log off from the application
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult LogOff()
        {
            _userManagementService.Logout();
            return RedirectToAction("Index", "Home");
        }


        /// <summary>
        ///     GET - Manage account controller - Retrieve all user profiles in the application
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, HR")]
        public ActionResult Manage()
        {
            var userProfilesDTO = _userManagementService.GetAllUserProfiles();
            var userProfileVm = userProfilesDTO.Select(
                e => new AccountManageViewModel
                {
                    Id = e.UserId,
                    FullName = e.FullName,
                    UserName = e.UserName,
                    IsDeactivated = e.UserIsDeactivated,
                    Roles = _userManagementService.GetRolesByUserProfileAsString(e.UserId)
                }).ToList();
            ViewBag.IsKendoEnabled = true;
            return View(userProfileVm);
        }


        /// <summary>
        ///     GET - Create account controller - Create user profile into the application
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, HR")]
        public ActionResult Create()
        {
            var aViewModel = new AccountCreateViewModel();
            InstanciateModel(ref aViewModel);

            ViewBag.IsKendoEnabled = true;
            return View(aViewModel);
        }

        /// <summary>
        ///     POST - Create account controller - Create user profile into the application
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, HR")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AccountCreateViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                InstanciateModel(ref iModel);
                return View(iModel);
            }

            var address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel);

            var contactInfo = new ContactInfoDTO
            {
                HomePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.HomePhoneNumber),
                WorkPhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.WorkPhoneNumber)
            };

            var contactPerson = new ContactPersonDTO
            {
                ContactPersonName = iModel.EmergencyContactName,
                ContactInfo = new ContactInfoDTO
                {
                    MobilePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.EmergencyMobilePhoneNumber),
                    WorkPhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.EmergencyWorkPhoneNumber)
                }
            };

            var userProfileDTO = new UserProfileDTO
            {
                FullName = iModel.FullName,
                UserName = iModel.UserName,
                Address = address,
                Remarks = iModel.Remarks,
                ContactInfo = contactInfo,
                ContactPerson = contactPerson,
                TenantId = TenantHelper.GetTenantId(Request.Url.Host),
                ScreenerCategory = AccountHelper.ExtractScreenerCategoryViewModel(iModel.ScreenerCategory)
            };

            var aRoles = new List<string>();
            if (iModel.SelectedRoles != null)
            {
                aRoles.AddRange(
                    iModel.SelectedRoles.Select(
                        itRoleId => _userManagementService.GetAllRoleById(Convert.ToInt32(itRoleId)))
                        .Select(currentRole => currentRole.RoleName));
            }

            var error = _userManagementService.CreateUserProfile(ref userProfileDTO, aRoles, iModel.Password);

            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Manage", "Account");
            }

            InstanciateModel(ref iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            ViewBag.IsKendoEnabled = true;
            return View(iModel);
        }

        /// <summary>
        ///     GET - Edit account controller - Edit user profile account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, HR")]
        public ActionResult Edit(int id)
        {
            var aUserProfileDTO = _userManagementService.GetUserProfileById(id);

            if (aUserProfileDTO == null)
            {
                const ErrorCode errorCode = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
            }

            var rolesOfUserDTO = _userManagementService.GetRolesByUserProfile(id);
            var rolesAllDTO = _userManagementService.GetAllRoles(false);


            var aViewModel = new AccountEditViewModel
            {
                UserId = aUserProfileDTO.UserId,
                UserName = aUserProfileDTO.UserName,
                FullName = aUserProfileDTO.FullName ?? string.Empty,
                Remarks = aUserProfileDTO.Remarks ?? string.Empty,
                AddressViewModel = AddressHelper.BuildAddressViewModel(aUserProfileDTO.Address),
                HomePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, aUserProfileDTO.ContactInfo.HomePhoneNumber),
                WorkPhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, aUserProfileDTO.ContactInfo.WorkPhoneNumber),
                EmergencyContactName = aUserProfileDTO.ContactPerson.ContactPersonName,
                EmergencyMobilePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false,
                        aUserProfileDTO.ContactPerson.ContactInfo.MobilePhoneNumber),
                EmergencyWorkPhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false,
                        aUserProfileDTO.ContactPerson.ContactInfo.WorkPhoneNumber),
                Roles = rolesAllDTO.Select(role => new SelectListItem()
                {
                    Text = role.RoleName,
                    Value = role.RoleId + ""
                }).ToList(),
                ScreenerCategory = AccountHelper.BuildScreenerCategoryViewModel(aUserProfileDTO.ScreenerCategory)
            };

            foreach (
                var roleOfUserDTO in
                    rolesOfUserDTO.Where(roleOfUserDTO => aViewModel.Roles.Exists(u => u.Text == roleOfUserDTO.RoleName))
                )
            {
                aViewModel.Roles.Single(u => u.Text == roleOfUserDTO.RoleName).Selected = true;
            }


            ViewBag.IsKendoEnabled = true;
            return View(aViewModel);
        }

        /// <summary>
        ///     POST - Edit account controller - Edit user profile account
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, HR")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AccountEditViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                InstanciateModel(ref iModel);
                return View(iModel);
            }

            var address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel);

            var contactInfo = new ContactInfoDTO
            {
                HomePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.HomePhoneNumber),
                WorkPhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.WorkPhoneNumber)
            };

            var contactPerson = new ContactPersonDTO
            {
                ContactPersonName = iModel.EmergencyContactName,
                ContactInfo = new ContactInfoDTO
                {
                    MobilePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.EmergencyMobilePhoneNumber),
                    WorkPhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.EmergencyWorkPhoneNumber)
                }
            };

            var userProfileDTO = new UserProfileDTO
            {
                UserId = iModel.UserId,
                FullName = iModel.FullName,
                UserName = iModel.UserName,
                Remarks = iModel.Remarks,
                Address = address,
                ContactInfo = contactInfo,
                ContactPerson = contactPerson,
                ScreenerCategory = AccountHelper.ExtractScreenerCategoryViewModel(iModel.ScreenerCategory)
            };

            var aRoles = new List<string>();
            if (iModel.SelectedRoles != null)
            {
                aRoles.AddRange(
                    iModel.SelectedRoles.Select(
                        itRoleId => _userManagementService.GetAllRoleById(Convert.ToInt32(itRoleId)))
                        .Select(currentRole => currentRole.RoleName));
            }

            var error = _userManagementService.EditUserProfile(ref userProfileDTO, aRoles);

            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Manage", "Account");
            }

            InstanciateModel(ref iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            ViewBag.IsKendoEnabled = true;
            return View(iModel);
        }

        /// <summary>
        ///     GET - Details account controller - Retrieve details for a specific user
        /// </summary>
        /// <param name="id">UserId of the controller</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, HR")]
        public ActionResult Details(int id)
        {
            var aUserProfile = _userManagementService.GetUserProfileById(id);

            if (aUserProfile == null)
            {
                var errorCode = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
            }

            var rolesOfUserDTO = _userManagementService.GetRolesByUserProfile(id);
            var rolesAllDTO = _userManagementService.GetAllRoles(false);

            var aViewModel = new AccountDetailsViewModel
            {
                UserName = aUserProfile.UserName,
                FullName = aUserProfile.FullName ?? "",
                Remarks = aUserProfile.Remarks ?? "",
                AddressViewModel = AddressHelper.BuildAddressViewModel(aUserProfile.Address),
                HomePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, aUserProfile.ContactInfo.HomePhoneNumber),
                WorkPhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, aUserProfile.ContactInfo.WorkPhoneNumber),
                EmergencyContactName = aUserProfile.ContactPerson.ContactPersonName,
                EmergencyMobilePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false,
                        aUserProfile.ContactPerson.ContactInfo.MobilePhoneNumber),
                EmergencyWorkPhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false,
                        aUserProfile.ContactPerson.ContactInfo.WorkPhoneNumber),
                Roles = rolesAllDTO.Select(role => new SelectListItem()
                {
                    Text = role.RoleName,
                    Value = role.RoleId.ToString(),
                    Selected = false
                }).ToList(),
                ScreenerCategory = aUserProfile.ScreenerCategory
            };
            foreach (var roleOfUserDTO in
                rolesOfUserDTO.Where(roleOfUserDTO => aViewModel.Roles.Exists(u => u.Text == roleOfUserDTO.RoleName)))
            {
                aViewModel.Roles.Single(u => u.Text == roleOfUserDTO.RoleName).Selected = true;
            }
            return View(aViewModel);
        }

        /// <summary>
        ///     GET - Activate account controller - Activate an account
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, HR")]
        public ActionResult Activate(int id)
        {
            var aUserProfileDTO = _userManagementService.GetUserProfileById(id);

            if (aUserProfileDTO == null)
            {
                const ErrorCode errorCode = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
            }

            var error = _userManagementService.ReactivateUserProfileByName(aUserProfileDTO.UserName);
            return error == ErrorCode.NO_ERROR
                ? RedirectToAction("Manage", "Account")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = error});
        }

        /// <summary>
        ///     GET - Deactivate account controller - Deactivate an account
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, HR")]
        public ActionResult Deactivate(int id)
        {
            var aUserProfileDTO = _userManagementService.GetUserProfileById(id);

            if (aUserProfileDTO == null)
            {
                const ErrorCode errorCode = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
            }

            var error = _userManagementService.DeactivateUserProfileByName(aUserProfileDTO.UserName);
            return error == ErrorCode.NO_ERROR
                ? RedirectToAction("Manage", "Account")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = error});
        }


        /// <summary>
        ///     GET - Update password controller - Update personal password, action done by end user
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult UpdatePassword()
        {
            ViewBag.ReturnUrl = Url.Action("UpdatePassword");
            return View();
        }

        /// <summary>
        ///     POST - Update password controller - Update personal password, action done by end user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult UpdatePassword(AccountUpdatePasswordViewModel model)
        {
            ViewBag.ReturnUrl = Url.Action("UpdatePassword");

            if (!ModelState.IsValid)
                return View(model);

            // UpdatePassword will throw an exception rather than return false in certain failure scenarios.
            var error = _userManagementService.UpdatePassword(User.Identity.Name, model.OldPassword,
                model.NewPassword);

            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            return View();
        }


        /// <summary>
        ///     GET - Recover password controller - Recover password, used when an end user has forgotten his password
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult RecoverPassword(AccountRecoverPasswordViewModel.AccountRecoverPasswordMessageId? message)
        {
            ViewBag.ReturnUrl = Url.Action("RecoverPassword");
            return View();
        }


        /// <summary>
        ///     POST - Recover password controller - Recover password, used when an end user has forgotten his password
        /// </summary>
        /// <param name="iViewModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecoverPassword(AccountRecoverPasswordViewModel iViewModel)
        {
            if (!ModelState.IsValid)
                return View(iViewModel);

            var aUserProfile = _userManagementService.GetUserProfilebyName(iViewModel.UserName);
            if (aUserProfile == null)
            {
                ViewBag.Success = false;
                return View();
            }

            // RecoverPassword will throw an exception rather than return false in certain failure scenarios.
            var tokenGenerated = _userManagementService.GeneratePasswordResetToken(iViewModel.UserName);

            dynamic email = new Email("RecoverPassword");
            email.To = iViewModel.UserName;
            email.UserName = aUserProfile.FullName;
            email.ConfirmationToken = tokenGenerated;
            email.Link = Request.Url.OriginalString + "Confirmation";
            email.Send();
            ViewBag.MailSent = true;
            return View();
        }

        /// <summary>
        ///     GET - Recover password confirmation controller - Link sent to end user by email to reset his personal password
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult RecoverPasswordConfirmation(string id)
        {
            ViewBag.TokenGenerated = id;
            return View();
        }

        /// <summary>
        ///     GET - Recover password confirmation controller - Link sent to end user by email to reset his personal password
        /// </summary>
        /// <param name="iViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RecoverPasswordConfirmation(AccountRecoverPasswordConfirmationViewModel iViewModel)
        {
            if (!ModelState.IsValid)
                return View(iViewModel);

            var error = _userManagementService.RecoverPassword(iViewModel.TokenGenerated, iViewModel.ConfirmPassword);

            if (error == ErrorCode.NO_ERROR)
            {
                ViewBag.PasswordRecovered = true;
                return View();
            }
            ViewBag.PasswordRecovered = false;
            return View();
        }


        /// <summary>
        ///     GET - Reset password controller - Reset password by administrator
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, HR")]
        public ActionResult ResetPassword(int id)
        {
            var userProfileDTO = _userManagementService.GetUserProfileById(id);

            if (userProfileDTO == null)
            {
                var error = ErrorCode.ACCOUNT_USERID_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            var aViewModel = new AccountResetPasswordViewModel
            {
                UserId = userProfileDTO.UserId,
                UserName = userProfileDTO.UserName
            };

            aViewModel.PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "";
            ViewBag.ReturnUrl = Url.Action("ResetPassword");
            return View(aViewModel);
        }

        /// <summary>
        ///     POST - Reset password controller - Reset password by administrator
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator, HR")]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(AccountResetPasswordViewModel model)
        {
            ViewBag.ReturnUrl = Url.Action("ResetPassword");

            if (!ModelState.IsValid)
                return View(model);

            // UpdatePassword will throw an exception rather than return false in certain failure scenarios.
            var error = _userManagementService.ResetPassword(model.UserName, model.NewPassword);

            if (error == ErrorCode.NO_ERROR)
            {
                return !String.IsNullOrEmpty(model.PreviousPage)
                    ? (ActionResult)Redirect(model.PreviousPage)
                    : RedirectToAction("Manage", "Account");
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            return View();
        }


        /// <summary>
        ///     GET - Edit personal profile
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult EditProfile()
        {
            var userProfile = _userManagementService.GetUserProfilebyName(User.Identity.Name);
            var viewModel = new AccountEditProfileViewModel
            {
                UserName = userProfile.UserName,
                FullName = userProfile.FullName,
                UserId = userProfile.UserId,
                CurrentUserPhoto = userProfile.UserPhoto
            };

            return View(viewModel);
        }

        /// <summary>
        ///     POST - Edit personal profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditProfile(AccountEditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            //Profile picture is uploaded
            if (model.PictureFile != null && (!FileHelper.ValidateProfilePictureFileSize(model.PictureFile)
                                              || !FileHelper.ValidateContentType(model.PictureFile)))
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(
                    ErrorCode.FILE_NOT_VALIDATED));
                return View(model);
            }

            var userProfile = _userManagementService.GetUserProfileById(model.UserId);

            // Convert picture file upload to byte[]
            if (model.PictureFile != null)
            {
                var buf = new byte[model.PictureFile.ContentLength];
                model.PictureFile.InputStream.Read(buf, 0, buf.Length);
                userProfile.UserPhoto = buf;
                userProfile.UserPhotoContentType = model.PictureFile.ContentType;
            }

            var error = _userManagementService.EditPersonalProfile(ref userProfile);
            if (error == ErrorCode.NO_ERROR)
            {
                Session["Picture"] = userProfile.UserPhoto;
                Session["ContentTypePicture"] = userProfile.UserPhotoContentType;
                Session["IsDefault"] = false;
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            return View();
        }


        /// <summary>
        ///     GET - Retrieve personal profile picture
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns></returns>
        [Authorize]
        public FileResult GetProfilePicture(string userName)
        {
            if (Session["Picture"] == null && Session["ContenTypePicture"] == null)
            {
                var userProfile = _userManagementService.GetUserProfilebyName(userName);
                Session["IsDefault"] = true;

                if (userProfile.UserPhoto != null)
                {
                    Session["Picture"] = userProfile.UserPhoto;
                    Session["ContentTypePicture"] = userProfile.UserPhotoContentType;
                    Session["IsDefault"] = false;
                }
            }

            var isDefault = (bool) Session["IsDefault"];
            if (!isDefault)
            {
                var picture = Session["Picture"] as byte[];
                var contentTypePicture = Session["ContentTypePicture"] as string;
                return File(picture, contentTypePicture);
            }
            return File("/Images/avatar1_small.jpg", System.Net.Mime.MediaTypeNames.Image.Jpeg);
        }

        #region Helpers

        public JsonResult GetUserScreenerJSON()
        {
            var screeners = _userManagementService.GetAllUserScreeners();
            return Json(screeners.Select(e =>
                new
                {
                    ScreenerId = e.UserId,
                    ScreenerName = e.FullName
                }),
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOfficeScreenerJSON()
        {
            var screeners =
                _userManagementService.GetAllUserScreeners()
                    .Where(e => e.ScreenerCategory == SkillMatrix.kCategories[1]);
            return Json(screeners.Select(e =>
                new
                {
                    ScreenerId = e.UserId,
                    ScreenerName = e.FullName
                }),
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOnFieldScreenerJSON()
        {
            var screeners =
                _userManagementService.GetAllUserScreeners()
                    .Where(e => e.ScreenerCategory == SkillMatrix.kCategories[2]);
            return Json(screeners.Select(e =>
                new
                {
                    ScreenerId = e.UserId,
                    ScreenerName = e.FullName
                }),
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Instanciate view model before displaying the view
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewModel"></param>
        private void InstanciateModel<T>(ref T viewModel) where T : AccountBaseViewModel
        {
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            viewModel.Roles = RoleHelper.BuildRoleViewModel(_userManagementService.GetAllRoles(false));
            viewModel.HomePhoneNumber = ContactHelper.BuildPhoneNumberViewModel(false);
            viewModel.WorkPhoneNumber = ContactHelper.BuildPhoneNumberViewModel(false);
            viewModel.EmergencyMobilePhoneNumber = ContactHelper.BuildPhoneNumberViewModel(false);
            viewModel.EmergencyWorkPhoneNumber = ContactHelper.BuildPhoneNumberViewModel(false);
            viewModel.ScreenerCategory = AccountHelper.BuildScreenerCategoryViewModel();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}