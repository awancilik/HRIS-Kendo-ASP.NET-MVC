using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.Contact;

namespace CVScreeningWeb.Controllers
{

    [Filters.HandleError]
    public class ContactController : Controller
    {
        private readonly ILookUpDatabaseService<QualificationPlaceDTO> _lookUpDatabaseService;
        private readonly ICommonService _commonService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly IClientService _clientService;

        public ContactController(ILookUpDatabaseService<QualificationPlaceDTO> lookUpDatabaseService,
            ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService,
            IClientService clientService)
        {
            _lookUpDatabaseService = lookUpDatabaseService;
            _commonService = commonService;
            _errorMessageFactoryService = errorMessageFactoryService;
            _clientService = clientService;
        }

        #region Qualification place contact

        [Authorize(Roles = "Administrator, Screener, Production manager")]
        public ActionResult ManageForQualificationPlace(int id)
        {
            var contactPersonDtos = _lookUpDatabaseService.GetContactPersons(id);
            var contactPersonVMs =
                (   from contactPerson in contactPersonDtos
                    select new ContactManageViewModel
                    {
                        Id = contactPerson.ContactPersonId,
                        Name = contactPerson.ContactPersonName,
                        Position = contactPerson.ContactInfo.Position,
                        EmailAddress = contactPerson.ContactInfo.OfficialEmail,
                        PhoneNumber = contactPerson.ContactInfo.MobilePhoneNumber, 
                        IsDeactivated = contactPerson.IsContactDeactivated
                    }).ToList();
            var contactPersonForQualificationPlaceVm = new ContactManageQualificationPlaceViewModel
            {
                QualificationPlaceId = id,
                QualificationPlaceName = _lookUpDatabaseService.GetQualificationPlace(id).QualificationPlaceName,
                Contacts = contactPersonVMs
            };
            return View(contactPersonForQualificationPlaceVm);
        }

        //GET: 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qualificationId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, Screener, Production manager")]
        public ActionResult CreateForQualificationPlace(int qualificationId)
        {
            var contactPersonVm = InstatiateFormViewModel(new ContactFormViewModel
            {
                QualificationPlaceId = qualificationId
            });
            contactPersonVm.QualificationPlaceId = qualificationId;
            return View(contactPersonVm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Screener, Production manager")]
        public ActionResult CreateForQualificationPlace(ContactFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var contactPersonDto = new ContactPersonDTO
            {
                ContactPersonId = iModel.Id,
                ContactPersonName = iModel.Name,
                QualificationPlaceId = iModel.QualificationPlaceId,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel),
                ContactComments = iModel.Comment,
                IsContactDeactivated = false,
                ContactInfo = new ContactInfoDTO
                {
                    MobilePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.MobilePhoneNumber),
                    WorkPhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.WorkPhoneNumber),
                    Position = iModel.Position,
                    OfficialEmail = iModel.EmailAddress
                }
            };

            var error = _commonService.CreateContactPerson(ref contactPersonDto);
            if (error == ErrorCode.NO_ERROR)
                return RedirectToAction("ManageForQualificationPlace", "Contact",
                    new { id = iModel.QualificationPlaceId });
            iModel = InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            return View(iModel);
        }

        [Authorize(Roles = "Administrator, Screener, Production manager")]
        public ActionResult EditQualificationPlace(int id, int secondaryId)
        {
            var contactPerson = _commonService.GetContactPerson(id);
            var contactPersonVm = new ContactFormViewModel
            {
                Id = contactPerson.ContactPersonId,
                Name = contactPerson.ContactPersonName,
                Position = contactPerson.ContactInfo.Position,
                EmailAddress = contactPerson.ContactInfo.OfficialEmail,
                Comment = contactPerson.ContactComments,
                AddressViewModel = AddressHelper.BuildAddressViewModel(contactPerson.Address),
                MobilePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(true, contactPerson.ContactInfo.MobilePhoneNumber),
                WorkPhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, contactPerson.ContactInfo.WorkPhoneNumber),
                QualificationPlaceId = secondaryId
            };

            return View("CreateForQualificationPlace", contactPersonVm);
        }

        [Authorize(Roles = "Administrator, Screener, Production manager")]
        public ActionResult DetailQualificationPlace(int id, int secondaryId)
        {
            var contactPerson = _commonService.GetContactPerson(id);
            var contactPersonVm = new ContactFormViewModel
            {
                Id = contactPerson.ContactPersonId,
                Name = contactPerson.ContactPersonName,
                Position = contactPerson.ContactInfo.Position,
                EmailAddress = contactPerson.ContactInfo.OfficialEmail,
                Comment = contactPerson.ContactComments,
                AddressViewModel = AddressHelper.BuildAddressViewModel(contactPerson.Address),
                QualificationPlaceId = (int)secondaryId,
                MobilePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(true, contactPerson.ContactInfo.MobilePhoneNumber),
                WorkPhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, contactPerson.ContactInfo.WorkPhoneNumber),
            };
            return View("DetailForQualificationPlace", contactPersonVm);
        }

        [Authorize(Roles = "Administrator, Screener, Production manager")]
        public ActionResult DeleteQualificationPlace(int id, int qualificationPlaceId)
        {
            var errorCode = _commonService.DeleteContactPerson(id);
            if (errorCode != ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });

            return RedirectToAction("ManageForQualificationPlace", "Contact", new { id = qualificationPlaceId });
        }

        [Authorize(Roles = "Administrator, Screener, Production manager")]
        public ActionResult DeactivateQualificationPlace(int id, int qualificationPlaceId)
        {
            var errorCode = _commonService.DeactivateContactPerson(id);
            if (errorCode != ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });

            return RedirectToAction("ManageForQualificationPlace", "Contact", new { id = qualificationPlaceId });
        }

        [Authorize(Roles = "Administrator, Screener, Production manager")]
        public ActionResult Activate(int id, int qualificationPlaceId)
        {
            var errorCode = _commonService.ActivateContactPerson(id);
            if (errorCode != ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });

            return RedirectToAction("ManageForQualificationPlace", "Contact", new { id = qualificationPlaceId });
        }

        #endregion

        #region Client company contact

        [Authorize(Roles = "Administrator")]
        public ActionResult ManageForClientCompany(int id)
        {
            var contactPersonDtos = _clientService.GetAllClientContactPersonsByCompany(new ClientCompanyDTO
            {
                ClientCompanyId = id
            });

            var contactPersonVMs =
                (   from contactPerson in contactPersonDtos
                    select new ContactManageViewModel
                    {
                        Id = contactPerson.ContactPersonId,
                        Name = contactPerson.ContactPersonName,
                        EmailAddress = contactPerson.ContactInfo.OfficialEmail,
                        Position = contactPerson.ContactInfo.Position,
                        PhoneNumber = contactPerson.ContactInfo.MobilePhoneNumber,
                        IsDeactivated = contactPerson.IsContactDeactivated
                    }).ToList();

            var contactPersonForQualificationPlaceVm = new ContactManageClientCompanyViewModel
            {
                ClientCompanyId = id,
                Contacts = contactPersonVMs
            };
            return View(contactPersonForQualificationPlaceVm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientCompanyId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateForClientCompany(int clientCompanyId)
        {
            var contactPersonVm = InstatiateFormViewModel(new ContactFormViewModel
            {
                ClientCompanyId = clientCompanyId
            });
            return View(contactPersonVm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateForClientCompany(ContactFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var contactPersonDto = new ContactPersonDTO
            {
                ContactPersonId = iModel.Id,
                ContactPersonName = iModel.Name,
                ClientCompanyId = iModel.ClientCompanyId,
                ContactComments = iModel.Comment,
                IsContactDeactivated = false,
                ContactInfo = new ContactInfoDTO
                {
                    MobilePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.MobilePhoneNumber),
                    WorkPhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.WorkPhoneNumber),
                    Position = iModel.Position,
                    OfficialEmail = iModel.EmailAddress
                }
            };

            var error = _commonService.CreateContactPerson(ref contactPersonDto);
            if (error == ErrorCode.NO_ERROR)
                return RedirectToAction("ManageForClientCompany", "Contact",
                    new { id = iModel.ClientCompanyId });

            iModel = InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            return View(iModel);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult EditClientCompany(int id)
        {
            var contactPerson = _commonService.GetContactPerson(id);
            var contactPersonVm = new ContactFormViewModel
            {
                Id = contactPerson.ContactPersonId,
                Name = contactPerson.ContactPersonName,
                Position = contactPerson.ContactInfo.Position,
                EmailAddress = contactPerson.ContactInfo.OfficialEmail,
                Comment = contactPerson.ContactComments,
                MobilePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, contactPerson.ContactInfo.MobilePhoneNumber),
                WorkPhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, contactPerson.ContactInfo.WorkPhoneNumber),
                ClientCompanyId = contactPerson.ClientCompanyId
            };

            return View("CreateForClientCompany", contactPersonVm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DetailClientCompany(int id, int? secondaryId)
        {
            var contactPerson = _commonService.GetContactPerson(id);
            var contactPersonVm = new ContactFormViewModel
            {
                Id = contactPerson.ContactPersonId,
                Name = contactPerson.ContactPersonName,
                Position = contactPerson.ContactInfo.Position,
                EmailAddress = contactPerson.ContactInfo.OfficialEmail,
                Comment = contactPerson.ContactComments,
                ClientCompanyId = contactPerson.ClientCompanyId,
                AddressViewModel = AddressHelper.BuildAddressViewModel(contactPerson.Address),
                MobilePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(true, contactPerson.ContactInfo.MobilePhoneNumber),
                WorkPhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, contactPerson.ContactInfo.WorkPhoneNumber),
            };

            return View("DetailForClientCompany", contactPersonVm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteClientCompany(int id, int clientCompanyId)
        {
            var errorCode = _commonService.DeleteContactPerson(id);
            if (errorCode != ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });
            return RedirectToAction("ManageForClientCompany", "Contact",
                new { id = clientCompanyId });
        }

        #endregion


        #region Helpers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        private ContactFormViewModel InstatiateFormViewModel(ContactFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            viewModel.MobilePhoneNumber = ContactHelper.BuildPhoneNumberViewModel(true);
            viewModel.WorkPhoneNumber = ContactHelper.BuildPhoneNumberViewModel(false);
            return viewModel;
        }
        #endregion

        public ActionResult ActivateQualificationPlace(int id, int qualificationplaceid)
        {
            var errorCode = _commonService.ActivateContactPerson(id);
            if (errorCode != ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "Error", new { errorCodeParameter = errorCode });

            return RedirectToAction("ManageForQualificationPlace", "Contact", new { id = qualificationplaceid });
        }
    }
}