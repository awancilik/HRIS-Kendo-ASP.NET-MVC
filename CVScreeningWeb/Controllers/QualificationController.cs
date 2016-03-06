using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Screening;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.ViewModels.Qualification;

namespace CVScreeningWeb.Controllers
{
    [Filters.HandleError]
    [Authorize(Roles = "Administrator, Qualifier")]
    public class QualificationController : Controller
    {
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly IQualificationService _qualificationService;
        private readonly IScreeningService _screeningService;

        public QualificationController(IScreeningService screeningService,
            IQualificationService qualificationService,
            IErrorMessageFactoryService errorMessageFactoryService)
        {
            _qualificationService = qualificationService;
            _screeningService = screeningService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }


        /// <summary>
        ///     Qualification action for a screening
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int id)
        {
            var screeningDTO = _screeningService.GetBaseScreening(id);

            if (screeningDTO == null)
            {
                const ErrorCode errorCode = ErrorCode.SCREENING_NOT_FOUND;
                ModelState.AddModelError(errorCode.ToString(), _errorMessageFactoryService.Create(errorCode));
                return RedirectToAction("Index", "Error", new {errorCodeParameter = errorCode});
            }

            ScreeningQualificationDTO qualificationBaseDTO = _qualificationService.GetQualificationBase(screeningDTO);
            IEnumerable<BaseQualificationPlaceDTO> qualificationPlacesDTO =
                _qualificationService.GetQualificationPlaces(screeningDTO);
            IEnumerable<BaseQualificationPlaceDTO> wrongQualificationPlacesDTO =
                _qualificationService.GetWronglyQualifiedQualificationPlaces(screeningDTO);
            IEnumerable<AtomicCheckBaseDTO> atomicChecks = _screeningService.GetAllBaseAtomicChecksForScreening(screeningDTO);
            QualificationFormViewModel viewModel = QualificationHelper.BuildQualificationFormViewModel(
                screeningDTO, atomicChecks, qualificationBaseDTO, qualificationPlacesDTO, wrongQualificationPlacesDTO);

            viewModel.PreviousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "";
            return View(viewModel);
        }

        /// <summary>
        ///     Qualification action for a screening
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(QualificationFormViewModel iModel)
        {
            ErrorCode errorCode;
            List<int> requalifiedQualifications = null;
            if (!ModelState.IsValid)
            {
                var errorScreeningDTO = new ScreeningDTO {ScreeningId = iModel.ScreeningId};
                ScreeningQualificationDTO errorQualificationBaseDTO = _qualificationService.GetQualificationBase(errorScreeningDTO);
                IEnumerable<BaseQualificationPlaceDTO> errorQualificationPlacesDTO =
                    _qualificationService.GetQualificationPlaces(errorScreeningDTO);
                IEnumerable<BaseQualificationPlaceDTO> errorWrongQualificationPlacesDTO =
                    _qualificationService.GetWronglyQualifiedQualificationPlaces(errorScreeningDTO);
                IEnumerable<AtomicCheckBaseDTO> atomicChecks = _screeningService.GetAllBaseAtomicChecksForScreening(errorScreeningDTO);
                QualificationFormViewModel viewModel = QualificationHelper.BuildQualificationFormViewModel(
                    errorScreeningDTO, atomicChecks, errorQualificationBaseDTO, errorQualificationPlacesDTO, errorWrongQualificationPlacesDTO);
                viewModel.PreviousPage = iModel.PreviousPage;
                return View(viewModel);
            }

            var screeningDTO = _screeningService.GetBaseScreening(iModel.ScreeningId);
            if (screeningDTO == null)
            {
                errorCode = ErrorCode.SCREENING_NOT_FOUND;
                ModelState.AddModelError(errorCode.ToString(), _errorMessageFactoryService.Create(errorCode));
                return RedirectToAction("Index", "Error", 
                    new {errorCodeParameter = errorCode});
            }

            var qualificationBaseDTO = QualificationHelper.ExtractScreeningQualificationDTOFromViewModel(iModel);
            IEnumerable<BaseQualificationPlaceDTO> qualificationPlacesDTO =
                QualificationHelper.ExtractScreeningQualificationPlacesDTOFromViewModel(iModel,
                    out requalifiedQualifications);
            IDictionary<AtomicCheckDTO, bool> atomicChecksNotApplicableDTO =
                QualificationHelper.ExtractNotApplicableQualificationFromViewModel(iModel);
            IEnumerable<BaseQualificationPlaceDTO> wrongQualificationPlacesDTO =
                _qualificationService.GetWronglyQualifiedQualificationPlaces(screeningDTO);

            errorCode = _qualificationService.SetAtomicChecksAsNotApplicable(screeningDTO, atomicChecksNotApplicableDTO);
            if (errorCode != ErrorCode.NO_ERROR)
            {
                qualificationBaseDTO = _qualificationService.GetQualificationBase(screeningDTO);
                qualificationPlacesDTO = _qualificationService.GetQualificationPlaces(screeningDTO);
                IEnumerable<AtomicCheckBaseDTO> atomicChecks = _screeningService.GetAllBaseAtomicChecksForScreening(screeningDTO);
                QualificationFormViewModel viewModel = QualificationHelper.BuildQualificationFormViewModel(
                    screeningDTO, atomicChecks, qualificationBaseDTO, qualificationPlacesDTO,
                    wrongQualificationPlacesDTO);
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return View(viewModel);
            }

            errorCode = _qualificationService.SetQualificationBase(screeningDTO, ref qualificationBaseDTO);
            if (errorCode != ErrorCode.NO_ERROR)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return View(iModel);
            }

            errorCode = _qualificationService.SetQualificationPlaces(
                screeningDTO, qualificationPlacesDTO, requalifiedQualifications);
            
            if (errorCode == ErrorCode.NO_ERROR)
            {
                return !String.IsNullOrEmpty(iModel.PreviousPage)
                    ? (ActionResult) Redirect(iModel.PreviousPage)
                    : RedirectToAction("Index", "Screening");
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
            return View(iModel);
        }
    }
}