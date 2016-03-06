using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.ClientAccount;
using CVScreeningWeb.ViewModels.Contact;

namespace CVScreeningWeb.Controllers
{
    [Authorize]
    [Filters.HandleError]
    [Authorize(Roles = "Administrator")]
    public class ClientAccountController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IClientService _clientService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        /// <summary>
        /// Default contructor for Client Account controlller
        /// </summary>
        /// <param name="userManagementService">Handled by Ninject</param>
        /// <param name="clientService">Handled by Ninject</param>
        /// <param name="errorMessageFactoryService">Handled by Ninject</param>
        public ClientAccountController(IUserManagementService userManagementService, IClientService clientService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _clientService = clientService;
            _userManagementService = userManagementService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }

        /// <summary>
        /// Page to display a grid view for client account
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Index()
        {
            List<UserProfileDTO> clientAccounts = _userManagementService.GetAllClientProfiles();
            IEnumerable<ClientAccountManageViewModel> clientAccountVms = clientAccounts.Select(
                c => new ClientAccountManageViewModel
                {
                    Company =
                        c.ClientCompanyForClientUserProfile != null
                            ? c.ClientCompanyForClientUserProfile.ClientCompanyName
                            : "",
                    Email = c.UserName,
                    Position = c.ContactInfo.Position,
                    FullName = c.FullName,
                    IsDeactivated = c.UserIsDeactivated,
                    Id = c.UserId
                });
            return View(clientAccountVms);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            UserProfileDTO clientAccountDTO = _userManagementService.GetClientProfileById(id);
            if (clientAccountDTO == null)
                return RedirectToAction("Index", "Error", new {errorCodeParameter = ErrorCode.ACCOUNT_USERID_NOT_FOUND});

            var allCompanies = _clientService.GetAllClientCompanies();
            var clientAccountVm = new ClientAccountFormViewModel
            {
                Id = clientAccountDTO.UserId,
                Email = clientAccountDTO.UserName,
                FullName = clientAccountDTO.FullName,
                Comment = clientAccountDTO.Remarks,
                Position = clientAccountDTO.ContactInfo.Position,
                ClientCompany = FormHelper.BuildDropDownListViewModel(
                    allCompanies.ToDictionary(e => e.ClientCompanyId, e => e.ClientCompanyName),
                    clientAccountDTO.ClientCompanyForClientUserProfile.ClientCompanyId),
                HomePhoneNumber = new PhoneViewModel{
                    FullNumber = clientAccountDTO.ContactInfo.HomePhoneNumber
                },
                MobilePhoneNumber = new PhoneViewModel{
                    FullNumber = clientAccountDTO.ContactInfo.MobilePhoneNumber
                },
                AddressViewModel = AddressHelper.BuildAddressViewModel(clientAccountDTO.Address)
            };
            return View(clientAccountVm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            var clientAccountDTO = _userManagementService.GetUserProfileById(id);
            if (clientAccountDTO == null)
                return RedirectToAction("Index", "Error",
                    new {errorCodeParameter = ErrorCode.ACCOUNT_USERID_NOT_FOUND});
            var allCompanies = _clientService.GetAllClientCompanies();
            var clientAccountVm = new ClientAccountFormEditViewModel
            {
                Email = clientAccountDTO.UserName,
                FullName = clientAccountDTO.FullName,
                Comment = clientAccountDTO.Remarks,
                ClientCompany = FormHelper.BuildDropDownListViewModel(
                    allCompanies.ToDictionary(e => e.ClientCompanyId, e => e.ClientCompanyName),
                    clientAccountDTO.ClientCompanyForClientUserProfile.ClientCompanyId
                    ),
                Position = clientAccountDTO.ContactInfo.Position,
                HomePhoneNumber = ContactHelper.BuildPhoneNumberViewModel(false, clientAccountDTO.ContactInfo.HomePhoneNumber),
                MobilePhoneNumber =
                    ContactHelper.BuildPhoneNumberViewModel(false, clientAccountDTO.ContactInfo.MobilePhoneNumber),
                AddressViewModel = AddressHelper.BuildAddressViewModel(clientAccountDTO.Address),
                Id = clientAccountDTO.UserId,
            };
            return View(clientAccountVm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var clientAccountVm = (ClientAccountFormCreateViewModel) InstatiateFormViewModel(new ClientAccountFormCreateViewModel());
            return View("Create", clientAccountVm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ClientAccountFormCreateViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (ClientAccountFormCreateViewModel) InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var clientAccountDTO = new UserProfileDTO
            {
                UserId = iModel.Id ?? 0,
                UserName = iModel.Email,
                Remarks = iModel.Comment,
                FullName = iModel.FullName,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel),
                ContactInfo = new ContactInfoDTO
                {
                    HomePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.HomePhoneNumber),
                    MobilePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.MobilePhoneNumber),
                    Position = iModel.Position
                },
                TenantId = TenantHelper.GetTenantId(Request.Url.Host)
            };

            //if edit mode, then clientCompany is equal to Null
            var clientCompanyDTO = _clientService.GetClientCompany(FormHelper.ExtractDropDownListViewModel(iModel.ClientCompany));
            var errorCode = _userManagementService.CreateClientProfile(ref clientAccountDTO,
                clientCompanyDTO, iModel.Password);
            
            if( errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index");
            iModel = (ClientAccountFormCreateViewModel) InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ClientAccountFormEditViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = (ClientAccountFormEditViewModel)InstatiateFormViewModel(iModel);
                return View(iModel);
            }

            var clientAccountDTO = new UserProfileDTO
            {
                UserId = iModel.Id ?? 0,
                UserName = iModel.Email,
                Remarks = iModel.Comment,
                FullName = iModel.FullName,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel),
                ContactInfo = new ContactInfoDTO
                {
                    HomePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.HomePhoneNumber),
                    MobilePhoneNumber = ContactHelper.ExtractPhoneNumberViewModel(iModel.MobilePhoneNumber),
                    Position = iModel.Position
                },
                TenantId = TenantHelper.GetTenantId(Request.Url.Host)
            };

            var errorCode = _userManagementService.EditClientProfile(ref clientAccountDTO);
            
            if( errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index");
            
            iModel = (ClientAccountFormEditViewModel) InstatiateFormViewModel(iModel);
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Deactivate(int id)
        {
            _userManagementService.DeactivateClientProfileById(id);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Activate(int id)
        {
            _userManagementService.ReactivateClientProfileById(id);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        private ClientAccountFormViewModel InstatiateFormViewModel(ClientAccountFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.HomePhoneNumber = ContactHelper.BuildPhoneNumberViewModel(false);
            viewModel.MobilePhoneNumber = ContactHelper.BuildPhoneNumberViewModel(false);
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            viewModel.ClientCompany = FormHelper.BuildDropDownListViewModel(
                _clientService.GetAllClientCompanies().Where(e => !e.ClientCompanyIsDeactivated)
                    .ToDictionary(c => c.ClientCompanyId, c => c.ClientCompanyName));
            return viewModel;
        }
    }
}