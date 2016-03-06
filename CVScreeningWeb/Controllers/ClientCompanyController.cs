using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.ClientCompany;
using CVScreeningWeb.ViewModels.Contract;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.Controllers
{
    [Filters.HandleError]
    public class ClientCompanyController : Controller
    {
        private readonly IClientService _clientService;
        private readonly ICommonService _commonService;
        private readonly IUserManagementService _userManagementService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;


        public ClientCompanyController(IUserManagementService userManagementService,
            IClientService clientService, ICommonService commonService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _clientService = clientService;
            _commonService = commonService;
            _userManagementService = userManagementService;
			_errorMessageFactoryService = errorMessageFactoryService;
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            List<ClientCompanyDTO> clientCompanyDTOs = _clientService.GetAllClientCompanies();
            IEnumerable<ClientCompanyManageViewModel> clientCompanyVms =
                from c in clientCompanyDTOs
                select new ClientCompanyManageViewModel
                {
                    Id = c.ClientCompanyId,
                    Company = c.ClientCompanyName,
                    Category = c.Category,
                    IsDeactivated = c.ClientCompanyIsDeactivated,
                    PhoneNumber = c.ContactInfo.WorkPhoneNumber,
                    AccountManager = new InlineListViewModel
                    {
                        List = c.AccountManagers.Select(e => e.FullName),
                        Saparation = ","
                    }
                };
            return View(clientCompanyVms);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Detail(int id)
        {
            ClientCompanyDTO clientCompanyDTO = _clientService.GetClientCompany(id);
            if (clientCompanyDTO == null)
                return RedirectToAction("Index", "Error", new {errorCodeParameter = ErrorCode.CLIENT_COMPANY_NOT_FOUND});
            var clientCompanyVm = new ClientCompanyFormViewModel
            {
                Company = clientCompanyDTO.ClientCompanyName,
                Description = clientCompanyDTO.Description,
                Id = clientCompanyDTO.ClientCompanyId,
                Website = clientCompanyDTO.ContactInfo.WebSite,
                AccountManagers = FormHelper.BuildSelectListViewModel(clientCompanyDTO.AccountManagers.ToDictionary(
                    e => e.UserId, e => e.FullName)),
                Category = FormHelper.BuildDropDownListViewModel(GetCategories(),
                    selectedValue: clientCompanyDTO.Category),
                AddressViewModel = AddressHelper.BuildAddressViewModel(clientCompanyDTO.Address)
            };
            return View(clientCompanyVm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            ClientCompanyDTO clientCompanyDTO = _clientService.GetClientCompany(id);
            if (clientCompanyDTO == null)
                return RedirectToAction("Index", "Error", new {errorCodeParameter = ErrorCode.CLIENT_COMPANY_NOT_FOUND});
            var clientCompanyVm = new ClientCompanyFormViewModel
            {
                Company = clientCompanyDTO.ClientCompanyName,
                Category =
                    FormHelper.BuildDropDownListViewModel(GetCategories(), selectedValue: clientCompanyDTO.Category),
                Description = clientCompanyDTO.Description,
                Id = clientCompanyDTO.ClientCompanyId,
                Website = clientCompanyDTO.ContactInfo.WebSite,
                AddressViewModel = AddressHelper.BuildAddressViewModel(clientCompanyDTO.Address),
                AccountManagers = FormHelper.BuildSelectListViewModel(
                    GetAllAccountManagers().ToDictionary(e => e.UserId, e => e.FullName),
                    GetSelectedAccountManagerIds(id).ToList()),
                IsDeactivated = clientCompanyDTO.ClientCompanyIsDeactivated
            };
            return View("Create", clientCompanyVm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            var clientCompanyVm = InstatiateVieModel(new ClientCompanyFormViewModel());
            return View(clientCompanyVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create(ClientCompanyFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                iModel = InstatiateVieModel(iModel);
                return View(iModel);
            }

            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyId = iModel.Id ?? 0,
                Category = GetCategories()[FormHelper.ExtractDropDownListViewModel(iModel.Category)],
                Description = iModel.Description,
                ClientCompanyName = iModel.Company,
                ContactInfo = new ContactInfoDTO
                {
                    WebSite = iModel.Website
                },
                ClientCompanyIsDeactivated = iModel.IsDeactivated,
                Address = AddressHelper.ExtractAddressViewModel(iModel.AddressViewModel),
            };


            clientCompanyDTO.AccountManagers = GetAllAccountManagers() != null && iModel.AccountManagers != null
                ? GetAllAccountManagers().Where(
                    e => FormHelper.ExtractSelectListViewModel(iModel.AccountManagers).Contains(e.UserId + ""))
                    .ToList() : null;

            ErrorCode errorCode = iModel.Id == null
                ? _clientService.CreateClientCompany(ref clientCompanyDTO)
                : _clientService.EditClientCompany(ref clientCompanyDTO);
            
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("Index", "ClientCompany");

            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            iModel = InstatiateVieModel(iModel);
            return View(iModel);
        }

        [Authorize(Roles = "Administrator, Client")]
        public ActionResult ManageContract(int? id)
        {
            var clientCompanyId = 0;
            if (id != null) clientCompanyId = (int)id;

            if (id == null && Roles.IsUserInRole("Client"))
            {
                var userProfile = _userManagementService.GetCurrentUser();
                clientCompanyId = userProfile.ClientCompanyForClientUserProfile.ClientCompanyId;
            }


            var contracts = _clientService.GetAllClientContractsByCompany(new ClientCompanyDTO
            {
                ClientCompanyId = clientCompanyId
            });

            var contractVms = contracts.Select(contract => new ContractManageViewModel
            {
                Id = contract.ContractId,
                Description = contract.ContractDescription,
                ReferenceNumber = contract.ContractReference,
                Year = contract.ContractYear,
                IsEnabled = contract.IsContractEnabled
            }).ToList();

            var contractClientCompanyVm = new ContractClientCompanyManageViewModel
            {
                ClientCompanyId = clientCompanyId,
                ContractManageViewModels = contractVms
            };
            return View(contractClientCompanyVm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult CreateContract(int clientCompanyId)
        {
            return View(new ContractFormViewModel
            {
                ClientCompanyId = clientCompanyId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateContract(ContractFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.
                    Create(ErrorCode.COMMON_FORM_VALIDATION_ERROR));
                return View(iModel);
            }

            var contractDTO = new ClientContractDTO
            {
                ContractId = iModel.Id ?? 0,
                ContractDescription = iModel.Description,
                ContractReference = iModel.ReferenceNumber,
                IsContractEnabled = iModel.IsEnabled,
                ContractYear = iModel.Year
            };
            var clientCompany = _clientService.GetClientCompany(iModel.ClientCompanyId);
            var errorCode = iModel.Id == null
                ? _clientService.CreateClientContract(clientCompany, ref contractDTO)
                : _clientService.EditClientContract(ref contractDTO);
            if (errorCode == ErrorCode.NO_ERROR)
                return RedirectToAction("ManageContract", "ClientCompany", new {id = iModel.ClientCompanyId});
            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult EditContract(int id)
        {
            var contractDTO = _clientService.GetClientContract(id);
            if (contractDTO == null)
                return RedirectToAction("Index", "Error",
                    new {errorCodeParameter = ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND});

            var contractVm = new ContractFormViewModel
            {
                Id = contractDTO.ContractId,
                ClientCompanyId = contractDTO.ClientCompany.ClientCompanyId,
                ReferenceNumber = contractDTO.ContractReference,
                Description = contractDTO.ContractDescription,
                IsEnabled = contractDTO.IsContractEnabled, 
                Year = contractDTO.ContractYear
            };
            return View("CreateContract", contractVm);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DeactivateContract(int id, int clientCompanyId)
        {
            var clientContractDTO = new ClientContractDTO
            {
                ContractId = id
            };
            ErrorCode errorCode = _clientService.DisableClientContract(ref clientContractDTO);
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("ManageContract", "ClientCompany", new {id = clientCompanyId})
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ActivateContract(int id, int clientCompanyId)
        {
            var clientContractDTO = new ClientContractDTO
            {
                ContractId = id
            };
            ErrorCode errorCode = _clientService.EnableClientContract(ref clientContractDTO);
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("ManageContract", "ClientCompany", new {id = clientCompanyId})
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Deactivate(int id)
        {
            var clientCompanyDTO = new ClientCompanyDTO {ClientCompanyId = id};
            ErrorCode errorCode = _clientService.DeactivateClientCompany(ref clientCompanyDTO);
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "ClientCompany")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Activate(int id)
        {
            var clientCompanyDTO = new ClientCompanyDTO {ClientCompanyId = id};
            ErrorCode errorCode = _clientService.ReactivateClientCompany(ref clientCompanyDTO);
            return errorCode == ErrorCode.NO_ERROR
                ? RedirectToAction("Index", "ClientCompany")
                : RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
        }

        [Authorize(Roles = "Administrator")]
        public JsonResult GetClientCompanies()
        {
            List<ClientCompanyDTO> allClientCompanies = _clientService.GetAllClientCompanies();

            return Json(allClientCompanies.Select
                (c => new {c.ClientCompanyId, c.ClientCompanyName}),
                JsonRequestBehavior.AllowGet);
        }


        private IEnumerable<UserProfileDTO> GetAllAccountManagers()
        {
            return _userManagementService.GetUserProfilesByRoles("Account manager");
        }

        private IEnumerable<int> GetSelectedAccountManagerIds(int id)
        {
            var clientCompanyDTO = _clientService.GetClientCompany(id);
            return clientCompanyDTO.AccountManagers.Select(am => am.UserId);
        }

        private IDictionary<int, string> GetCategories()
        {
            return new Dictionary<int, string>
            {
                {1, "Agriculture"}, 
                {2, "Arts"},
                {3, "Construction"},
                {4, "Consumer Goods"},
                {5, "Corporate Services"},
                {6, "Education"},
                {7, "Finance"},
                {8, "Government"},
                {9, "Information Technology"},
                {10, "Legal"},
                {11, "Manufacturing"},
                {12, "Media"},
                {13, "Medical and Health Care"},
                {14, "Organizations and Nonprofit"},
                {15, "Recreation, Travel, and Entertainment"},
                {16, "Security Investigation"},
                {17, "Service Industry"},
                {18, "Transportation"}
            };
        }

        private ClientCompanyFormViewModel InstatiateVieModel(ClientCompanyFormViewModel iModel)
        {
            var viewModel = iModel;
            viewModel.Category = FormHelper.BuildDropDownListViewModel(GetCategories());
            viewModel.AccountManagers = FormHelper.BuildSelectListViewModel(GetAllAccountManagers()
                .ToDictionary(e => e.UserId, e => e.FullName));
            viewModel.AddressViewModel = AddressHelper.BuildAddressViewModel();
            return viewModel;
        }

    }
}