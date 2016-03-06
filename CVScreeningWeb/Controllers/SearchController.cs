using Castle.Core.Internal;
using CVScreeningCore.Models;
using CVScreeningCore.Models.ScreeningState;
using CVScreeningService.DTO.Client;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.UserManagement;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.Screening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CVScreeningWeb.ViewModels.Shared;
using CVScreeningService.Services.Settings;

namespace CVScreeningWeb.Controllers
{
    public class SearchController : Controller
    {
        
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly IClientService _clientService;
        private readonly IUserManagementService _userManagementService;
        private readonly IScreeningService _screeningService;
        private readonly ISettingsService _settingsService;

        public SearchController(IErrorMessageFactoryService errorMessageFactoryService, 
            IClientService clientService,
            IUserManagementService userManagementService,
            IScreeningService screeningService,
            ISettingsService settingsService)
        {
            _errorMessageFactoryService = errorMessageFactoryService;
            _clientService = clientService;
            _userManagementService = userManagementService;
            _screeningService = screeningService;
            _settingsService = settingsService;
        }
        
        public ActionResult Index()
        {
            var companiesDictionary = GenerateClientDictionary();
            var screening = _screeningService.GetAllScreenings();
            var screeningSearchVM = new ScreeningSearchViewModel()
            {
                Status = FormHelper.BuildDropDownListViewModel(GenerateStatusDictionnary()),
                Client = FormHelper.BuildDropDownListViewModel(companiesDictionary),
                ScreeningManageList = ScreeningHelper.BuildScreeningManageViewModels(screening, _settingsService.GetAllPublicHolidays())
            };
            return View(screeningSearchVM);
        }

        [HttpPost]
        public ActionResult Index(ScreeningSearchViewModel iModel)
        {
            if (User.IsInRole(webpages_Roles.kClientRole))
            {
                iModel.Client = new DropDownListViewModel()
                {
                    PostData = "0"
                };
            }
            var companiesDictionary = GenerateClientDictionary();
            var screening = _screeningService.SearchScreening(iModel.Name.IsNullOrEmpty() ? "" : iModel.Name,
                IsClientAvailableOnDictionary(int.Parse(iModel.Client.PostData)) ? iModel.Client.PostData : "",
                iModel.StartingDate,
                iModel.EndingDate,
                ScreeningStateFactory.IsStatusAvailableInEnumList(int.Parse(iModel.Status.PostData))? iModel.Status.PostData : "");
            var screeningSearchVm = new ScreeningSearchViewModel()
            {
                Name = iModel.Name,
                Status = FormHelper.BuildDropDownListViewModel(GenerateStatusDictionnary(), int.Parse(iModel.Status.PostData)),
                Client = FormHelper.BuildDropDownListViewModel(companiesDictionary,int.Parse(iModel.Client.PostData)),
                ScreeningManageList = ScreeningHelper.BuildScreeningManageViewModels(screening, _settingsService.GetAllPublicHolidays()),
                StartingDate = iModel.StartingDate,
                EndingDate = iModel.EndingDate
            };
            return View(screeningSearchVm);
        }

        private IDictionary<int, string> GenerateClientDictionary()
        {
            IEnumerable<ClientCompanyDTO> allCompanies = new List<ClientCompanyDTO>();
            if (User.IsInRole(webpages_Roles.kAdministratorRole))
            {
                allCompanies = _clientService.GetAllClientCompanies();
            }
            else if (User.IsInRole(webpages_Roles.kAccountManagerRole))
            {
                allCompanies = _clientService.GetAllClientCompaniesForAccountManager();
            }
            var companiesDictionary = allCompanies.ToDictionary(e => e.ClientCompanyId, e => e.ClientCompanyName);
            companiesDictionary.Add(0, "Select Client...");
            companiesDictionary = companiesDictionary.OrderBy(c => c.Key).ToDictionary(e => e.Key, e => e.Value);
            return companiesDictionary;
        }

        private IDictionary<int, string> GenerateStatusDictionnary()
        {
            if (User.IsInRole(webpages_Roles.kClientRole))
            {
                return ScreeningStateFactory.GetAllStatusForClient();
            }
            return ScreeningStateFactory.GetAllStatus();
        }


        private bool IsClientAvailableOnDictionary(int client)
        {
            var allCompanies = _clientService.GetAllClientCompanies();
            var companiesDictionary = allCompanies.ToDictionary(e => e.ClientCompanyId, e => e.ClientCompanyName);
            return companiesDictionary.ContainsKey(client);
        }

    }
}
