using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Screening;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Screening;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.ScreeningLevel;

namespace CVScreeningWeb.Controllers
{
    
    [Filters.HandleError]
    public class ScreeningLevelController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IScreeningService _screeningService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        public ScreeningLevelController(IClientService clientService, IScreeningService screeningService,
            IErrorMessageFactoryService errorMessageFactoryService)
        {
            _clientService = clientService;
            _screeningService = screeningService;
            _errorMessageFactoryService = errorMessageFactoryService;
            ViewBag.IsKendoEnabled = true;
        }

        //
        // GET: /Leave/Index
        [Authorize(Roles = "Administrator, Client")]
        public ActionResult Index(int id)
        {
            var clientContractDTO = _clientService.GetClientContract(id);
            if (clientContractDTO == null)
            {
                const ErrorCode error = ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            var screeningLevelsDTO = _clientService.GetScreeningLevelsByContract(clientContractDTO);
            var screeningLevelsVm = new ScreeningLevelsManageViewModel
            {
                ContractId = clientContractDTO.ContractId,
                ContractReference = clientContractDTO.ContractReference,
                ContractYear = clientContractDTO.ContractYear,
                ScreeningLevels = new List<ScreeningLevelManageViewModel>()
            };

            foreach (var screeningLevelDTO in screeningLevelsDTO)
            {
                foreach (var screeningLevelVersionDTO in screeningLevelDTO.ScreeningLevelVersion)
                {
                    screeningLevelsVm.ScreeningLevels.Add(new ScreeningLevelManageViewModel
                    {
                        ScreeningLevelName = screeningLevelDTO.ScreeningLevelName,
                        ScreeningLevelId = screeningLevelDTO.ScreeningLevelId,
                        
                        ScreeningLevelVersion = new ScreeningLevelVersionFormViewModel
                        {
                            ScreeningLevelVersionId = screeningLevelVersionDTO.ScreeningLevelVersionId,
                            Description = screeningLevelVersionDTO.ScreeningLevelVersionDescription,
                            StartDate = screeningLevelVersionDTO.ScreeningLevelVersionStartDate.ToShortDateString(),
                            EndDate = screeningLevelVersionDTO.ScreeningLevelVersionEndDate.ToShortDateString(),
                            AllowedToContact = screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCandidate,
                            Language = screeningLevelVersionDTO.ScreeningLevelVersionLanguage,
                            TurnaroundTime = screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime,
                            VersionNumber = screeningLevelVersionDTO.ScreeningLevelVersionNumber
                        }
                    });
                }
            }

            return View(screeningLevelsVm);
        }

        //
        // GET: /ScreeningLevel/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create(int id)
        {
            var clientContractDTO = _clientService.GetClientContract(id);

            if (clientContractDTO == null)
            {
                const ErrorCode error = ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            var aViewModel = new ScreeningLevelFormViewModel
            {
                ContractId = clientContractDTO.ContractId,
                ContractReference = clientContractDTO.ContractReference,
                ContractYear = clientContractDTO.ContractYear,
                TurnaroundTime = 5,
                StartDate = null,
                EndDate = null,
                TypeOfChecks = BuildTypeOfCheckBoxesViewModels(),
                AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary())
            };

            return View(aViewModel);
        }

        
        // POST: /Leave/Create
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create(ScreeningLevelFormViewModel iModel)
        {

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Screening level creation failed.");
                iModel.TypeOfChecks = BuildTypeOfCheckBoxesViewModels();
                iModel.AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary());
                return View(iModel);
            }

            if (!iModel.TypeOfChecks.Any(typeOfCheckVm => typeOfCheckVm.TypeOfCheckBox.Checked))
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(ErrorCode.SCREENING_LEVEL_SELECT_ANY_CHECKS));
                iModel.TypeOfChecks = BuildTypeOfCheckBoxesViewModels();
                iModel.AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary());
                return View(iModel);
            }

            var contractDTO = new ClientContractDTO()
            {
                ContractId = iModel.ContractId,
            };

            var screeningLevelDTO = new ScreeningLevelDTO
            {
                ScreeningLevelName = iModel.ScreeningLevelName,
            };

            var screeningLevelVersionDTO = new ScreeningLevelVersionDTO
            {
                ScreeningLevelVersionDescription = iModel.Description,
                ScreeningLevelVersionStartDate = (DateTime) iModel.StartDate,
                ScreeningLevelVersionEndDate = (DateTime) iModel.EndDate,
                ScreeningLevelVersionLanguage = iModel.Language,
                ScreeningLevelVersionTurnaroundTime = iModel.TurnaroundTime,
                ScreeningLevelVersionAllowedToContactCandidate = iModel.AllowedToContact,
                ScreeningLevelVersionAllowedToContactCurrentCompany = 
                    GetAllowedContactCurrentCompanyDictionary()[FormHelper.ExtractDropDownListViewModel(iModel.AllowedToContactCurrentCompany)],
                TypeOfCheckScreeningLevelVersion = new List<TypeOfCheckScreeningLevelVersionDTO>()
            };

            // Fill type of check DTO

            foreach (var typeOfCheckVm in iModel.TypeOfChecks.Where(typeOfCheckVm => typeOfCheckVm.TypeOfCheckBox.Checked))
            {
                screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion.Add(new TypeOfCheckScreeningLevelVersionDTO
                    {
                        TypeOfCheckId = typeOfCheckVm.TypeOfCheckId,
                        TypeOfCheckScreeningComments = typeOfCheckVm.TypeOfCheckScreeningComments,
                        TypeOfCheck = _screeningService.GetTypeOfCheck(typeOfCheckVm.TypeOfCheckId)
                    }
                );
            }

            var error = _clientService.CreateScreeningLevel(contractDTO, ref screeningLevelDTO, ref screeningLevelVersionDTO);
            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Index", "ScreeningLevel", new { id = iModel.ContractId });
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            iModel.AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary());
            return View(iModel);
        }

        /// <summary>
        /// Action method - Get details about screening level
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, Client")]
        public ActionResult Details(int id, int secondaryId)
        {
            var screeningLevelDTO = _clientService.GetScreeningLevel(id);

            if (screeningLevelDTO == null)
                return RedirectToAction("Index", "Error", new { errorCodeParameter = ErrorCode.SCREENING_LEVEL_NOT_FOUND });

            if (!screeningLevelDTO.ScreeningLevelVersion.Exists(u => u.ScreeningLevelVersionId == secondaryId))
                return RedirectToAction("Index", "Error", new { errorCodeParameter = ErrorCode.SCREENING_LEVEL_VERSION_NOT_FOUND });

            var screeningLevelVersionDTO =
                screeningLevelDTO.ScreeningLevelVersion.Single(u => u.ScreeningLevelVersionId == secondaryId);

            var aViewModel = new ScreeningLevelFormViewModel
            {
                ContractId = screeningLevelDTO.Contract.ContractId,
                ContractReference = screeningLevelDTO.Contract.ContractReference,
                ContractYear = screeningLevelDTO.Contract.ContractYear,
                ScreeningLevelName = screeningLevelDTO.ScreeningLevelName,
                Description = screeningLevelVersionDTO.ScreeningLevelVersionDescription,
                StartDate = screeningLevelVersionDTO.ScreeningLevelVersionStartDate,
                EndDate = screeningLevelVersionDTO.ScreeningLevelVersionEndDate,
                Language = screeningLevelVersionDTO.ScreeningLevelVersionLanguage,
                TurnaroundTime = screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime,
                AllowedToContact = screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCandidate,
                AllowedToContactCurrentCompany = 
                    FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary(), 
                    selectedValue: screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCurrentCompany),
                TypeOfChecks = new List<TypeOfCheckForScreeningLevelViewModel>()
            };

            aViewModel.TypeOfChecks = screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion.Select(
                    typeOfCheckDTO => new TypeOfCheckForScreeningLevelViewModel
                    {
                        TypeOfCheckBox = new CheckBox
                        {
                            Text = typeOfCheckDTO.TypeOfCheck.CheckName,
                            Checked = true
                        },
                        TypeOfCheckId = typeOfCheckDTO.TypeOfCheckId,
                        TypeOfCheckName = typeOfCheckDTO.TypeOfCheck.CheckName,
                        TypeOfCheckScreeningComments = typeOfCheckDTO.TypeOfCheckScreeningComments
                    }).ToList();

            return View(aViewModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IDictionary<int, string> GetAllowedContactCurrentCompanyDictionary()
        {
            return new Dictionary<int, string>
            {
                {0, ScreeningLevelVersion.kNotAllowedContactCurrentCompany},
                {1, ScreeningLevelVersion.kHR},
                {2, ScreeningLevelVersion.kReceptionist}
            };
        }

        /// <summary>
        /// Action method - Edit screening level
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id, int secondaryId)
        {
            var screeningLevelDTO = _clientService.GetScreeningLevel(id);

            if (screeningLevelDTO == null)
                return RedirectToAction("Index", "Error", new { errorCodeParameter = ErrorCode.SCREENING_LEVEL_NOT_FOUND });

            if (!screeningLevelDTO.ScreeningLevelVersion.Exists(u => u.ScreeningLevelVersionId == secondaryId))
                return RedirectToAction("Index", "Error", new { errorCodeParameter = ErrorCode.SCREENING_LEVEL_VERSION_NOT_FOUND });

            var screeningLevelVersionDTO =
                screeningLevelDTO.ScreeningLevelVersion.Single(u => u.ScreeningLevelVersionId == secondaryId);

            var aViewModel = new ScreeningLevelFormViewModel
            {
                ContractId = screeningLevelDTO.Contract.ContractId,
                ScreeningLevelId = id,
                ScreeningLevelVersionId =  secondaryId,
                ContractReference = screeningLevelDTO.Contract.ContractReference,
                ContractYear = screeningLevelDTO.Contract.ContractYear,
                ScreeningLevelName = screeningLevelDTO.ScreeningLevelName,
                Description = screeningLevelVersionDTO.ScreeningLevelVersionDescription,
                StartDate = screeningLevelVersionDTO.ScreeningLevelVersionStartDate,
                EndDate = screeningLevelVersionDTO.ScreeningLevelVersionEndDate,
                Language = screeningLevelVersionDTO.ScreeningLevelVersionLanguage,
                TurnaroundTime = screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime,
                AllowedToContact = screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCandidate,
                AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary(), selectedValue: screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCurrentCompany)
            };

            return View(aViewModel);
        }


        /// <summary>
        /// POST - Edit screening level
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(ScreeningLevelFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Screening level edition failed.");
                iModel.AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary());
                return View(iModel);
            }

            var screeningLevelDTO = new ScreeningLevelDTO
            {
                ScreeningLevelId = iModel.ScreeningLevelId,
                ScreeningLevelName = iModel.ScreeningLevelName,
            };

            var screeningLevelVersionDTO = new ScreeningLevelVersionDTO
            {
                ScreeningLevelVersionId = iModel.ScreeningLevelVersionId,
                ScreeningLevelVersionDescription = iModel.Description,
                ScreeningLevelVersionStartDate = (DateTime)iModel.StartDate,
                ScreeningLevelVersionEndDate = (DateTime)iModel.EndDate,
                ScreeningLevelVersionLanguage = iModel.Language,
                ScreeningLevelVersionTurnaroundTime = iModel.TurnaroundTime,
                ScreeningLevelVersionAllowedToContactCandidate = iModel.AllowedToContact,
                ScreeningLevelVersionAllowedToContactCurrentCompany = 
                    GetAllowedContactCurrentCompanyDictionary()[FormHelper.ExtractDropDownListViewModel(iModel.AllowedToContactCurrentCompany)],
                TypeOfCheckScreeningLevelVersion = new List<TypeOfCheckScreeningLevelVersionDTO>()
            };


            var error = _clientService.EditScreeningLevel(ref screeningLevelDTO, ref screeningLevelVersionDTO);
            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Index", "ScreeningLevel", new { id = iModel.ContractId });
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            iModel.AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary());
            return View(iModel);
        }



        /// <summary>
        /// Action method - Edit screening level
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public ActionResult Update(int id, int secondaryId)
        {
            var screeningLevelDTO = _clientService.GetScreeningLevel(id);

            if (screeningLevelDTO == null)
                return RedirectToAction("Index", "Error", new { errorCodeParameter = ErrorCode.SCREENING_LEVEL_NOT_FOUND });

            if (!screeningLevelDTO.ScreeningLevelVersion.Exists(u => u.ScreeningLevelVersionId == secondaryId))
                return RedirectToAction("Index", "Error", new { errorCodeParameter = ErrorCode.SCREENING_LEVEL_VERSION_NOT_FOUND });

            var screeningLevelVersionDTO =
                screeningLevelDTO.ScreeningLevelVersion.Single(u => u.ScreeningLevelVersionId == secondaryId);

            var aViewModel = new ScreeningLevelFormViewModel
            {
                ContractId = screeningLevelDTO.Contract.ContractId,
                ScreeningLevelId = id,
                ScreeningLevelVersionId = secondaryId,
                ContractReference = screeningLevelDTO.Contract.ContractReference,
                ContractYear = screeningLevelDTO.Contract.ContractYear,
                ScreeningLevelName = screeningLevelDTO.ScreeningLevelName,
                Description = screeningLevelVersionDTO.ScreeningLevelVersionDescription,
                StartDate = screeningLevelVersionDTO.ScreeningLevelVersionStartDate,
                EndDate = screeningLevelVersionDTO.ScreeningLevelVersionEndDate,
                Language = screeningLevelVersionDTO.ScreeningLevelVersionLanguage,
                TurnaroundTime = screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime,
                AllowedToContact = screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCandidate,
                AllowedToContactCurrentCompany=  FormHelper.BuildDropDownListViewModel(
                    GetAllowedContactCurrentCompanyDictionary(), selectedValue: screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCurrentCompany),
                TypeOfChecks = BuildTypeOfCheckBoxesViewModels()
            };
            return View(aViewModel);
        }


        //
        // POST: /Update/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Update(ScreeningLevelFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Screening level update failed.");
                iModel.TypeOfChecks = BuildTypeOfCheckBoxesViewModels();
                iModel.AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary());
                return View(iModel);
            }

            var screeningLevelDTO = new ScreeningLevelDTO
            {
                ScreeningLevelId = iModel.ScreeningLevelId,
                ScreeningLevelName = iModel.ScreeningLevelName,
            };

            var screeningLevelVersionDTO = new ScreeningLevelVersionDTO
            {
                ScreeningLevelVersionId = iModel.ScreeningLevelVersionId,
                ScreeningLevelVersionDescription = iModel.Description,
                ScreeningLevelVersionStartDate = (DateTime)iModel.StartDate,
                ScreeningLevelVersionEndDate = (DateTime)iModel.EndDate,
                ScreeningLevelVersionLanguage = iModel.Language,
                ScreeningLevelVersionTurnaroundTime = iModel.TurnaroundTime,
                ScreeningLevelVersionAllowedToContactCandidate = iModel.AllowedToContact,
                ScreeningLevelVersionAllowedToContactCurrentCompany = 
                    GetAllowedContactCurrentCompanyDictionary()[FormHelper.ExtractDropDownListViewModel(iModel.AllowedToContactCurrentCompany)],
                TypeOfCheckScreeningLevelVersion = new List<TypeOfCheckScreeningLevelVersionDTO>()
            };

            // Fill type of check DTO
            foreach (var typeOfCheckVm in iModel.TypeOfChecks.Where(typeOfCheckVm => typeOfCheckVm.TypeOfCheckBox.Checked))
            {
                screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion.Add(new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheckId = typeOfCheckVm.TypeOfCheckId,
                    TypeOfCheckScreeningComments = typeOfCheckVm.TypeOfCheckScreeningComments,
                    TypeOfCheck = _screeningService.GetTypeOfCheck(typeOfCheckVm.TypeOfCheckId)
                }
                );
            }


            var error = _clientService.UpdateScreeningLevel(ref screeningLevelDTO, ref screeningLevelVersionDTO);
            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Index", "ScreeningLevel", new { id = iModel.ContractId });
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            iModel.TypeOfChecks = BuildTypeOfCheckBoxesViewModels();
            iModel.AllowedToContactCurrentCompany = FormHelper.BuildDropDownListViewModel(GetAllowedContactCurrentCompanyDictionary());
            return View(iModel);
        }


        /// <summary>
        /// Build type of checks view model
        /// </summary>
        /// <returns></returns>
        private List<TypeOfCheckForScreeningLevelViewModel> BuildTypeOfCheckBoxesViewModels()
        {
            return _screeningService.GetAllTypeOfChecks().Select(
                typeOfCheckDTO => new TypeOfCheckForScreeningLevelViewModel
            {
                TypeOfCheckBox = new CheckBox
                {
                    Text = typeOfCheckDTO.CheckName
                }, 
                TypeOfCheckId = typeOfCheckDTO.TypeOfCheckId, 
                TypeOfCheckName = typeOfCheckDTO.CheckName
            }).ToList();
        }

    }
}