using System;
using System.Linq;
using System.Web.Mvc;
using CVScreeningService.DTO.Settings;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Settings;
using CVScreeningWeb.ViewModels.PublicHoliday;
using CVScreeningCore.Error;


namespace CVScreeningWeb.Controllers
{
    [Filters.HandleError]
    [Authorize(Roles = "Administrator, HR")]
    public class PublicHolidayController : Controller
    {

        private readonly ISettingsService _settingsService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        /// <summary>
        ///     Constructor to pass service
        /// </summary>
        /// <param name="settingsService"></param>
        /// <param name="errorMessageFactoryService"></param>
        public PublicHolidayController(
            ISettingsService settingsService, IErrorMessageFactoryService errorMessageFactoryService)
        {
            _settingsService = settingsService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }

        //
        // GET: /PublicHoliday/

        public ActionResult Index()
        {
            var publicHolidays = _settingsService.GetAllPublicHolidays().Where(
                u => u.PublicHolidayStartDate.Year >= DateTime.Now.Year); ;
            var publicHolidaysVm = publicHolidays.Select(
                e => new PublicHolidayUniqueViewModel
                {
                    Id = e.PublicHolidayId,
                    Name = e.PublicHolidayName,
                    StartDate = e.PublicHolidayStartDate.ToShortDateString(),
                    EndDate = e.PublicHolidayEndDate.ToShortDateString(),
                    Remarks = e.PublicHolidayRemarks
                }).ToList();

            var aViewModel = new PublicHolidayManageViewModel
            {
                PublicHolidays = publicHolidaysVm
            };

            ViewBag.IsKendoEnabled = true;
            return View(aViewModel);
        }


        //
        // GET: /PublicHoliday/Create
        public ActionResult Create(int? id)
        {
            var aViewModel = new PublicHolidayFormViewModel
            {
                StartDate = null,
                EndDate = null
            };
            ViewBag.IsKendoEnabled = true;
            return View(aViewModel);
        }

        //
        // POST: /PublicHoliday/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PublicHolidayFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Public holiday creation failed.");
                ViewBag.IsKendoEnabled = true;
                return View(iModel);
            }

            var publicHolidayDTO = new PublicHolidayDTO
            {
                PublicHolidayName = iModel.Name,
                PublicHolidayRemarks = iModel.Remarks,
                PublicHolidayStartDate = (DateTime)iModel.StartDate,
                PublicHolidayEndDate = (DateTime)iModel.EndDate
            };

            var error = _settingsService.CreatePublicHoliday(ref publicHolidayDTO);
            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Index", "PublicHoliday");
            }

            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            ViewBag.IsKendoEnabled = true;
            return View(iModel);
        }



        //
        // GET: /PublicHoliday/Edit
        public ActionResult Edit(int id)
        {
            var publicHolidayDTO = _settingsService.GetPublicHoliday(id);
            if (publicHolidayDTO == null)
            {
                const ErrorCode error = ErrorCode.PUBLIC_HOLIDAY_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            var aViewModel = new PublicHolidayFormViewModel
            {
                StartDate = publicHolidayDTO.PublicHolidayStartDate,
                EndDate = publicHolidayDTO.PublicHolidayEndDate,
                Remarks = publicHolidayDTO.PublicHolidayRemarks,
                Name = publicHolidayDTO.PublicHolidayName,
                Id = publicHolidayDTO.PublicHolidayId
            };

            ViewBag.IsKendoEnabled = true;
            return View(aViewModel);
        }

        //
        // POST: /PublicHoliday/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PublicHolidayFormViewModel iModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Public holiday edition failed.");
                ViewBag.IsKendoEnabled = true;
                return View(iModel);
            }

            var publicHolidayDTO = new PublicHolidayDTO
            {
                PublicHolidayId = iModel.Id,
                PublicHolidayName = iModel.Name,
                PublicHolidayRemarks = iModel.Remarks,
                PublicHolidayStartDate = (DateTime)iModel.StartDate,
                PublicHolidayEndDate = (DateTime)iModel.EndDate
            };

            var error = _settingsService.EditPublicHoliday(publicHolidayDTO);
            if (error == ErrorCode.NO_ERROR)
            {
                return RedirectToAction("Index", "PublicHoliday", null);
            }
            ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
            ViewBag.IsKendoEnabled = true;
            return View(iModel);
        }


        //
        // GET: /PublicHoliday/Delete/id
        public ActionResult Delete(int id)
        {
            ErrorCode error;
            var publicHolidayDTO = _settingsService.GetPublicHoliday(id);

            if (publicHolidayDTO == null)
            {
                error = ErrorCode.PUBLIC_HOLIDAY_NOT_FOUND;
                ModelState.AddModelError("", _errorMessageFactoryService.Create(error));
                return RedirectToAction("Index", "Error", error);
            }

            error = _settingsService.DeletePublicHoliday(id);
            return error == ErrorCode.NO_ERROR
                    ? RedirectToAction("Index", "PublicHoliday")
                    : RedirectToAction("Index", "Error", new { errorCodeParameter = error });
        }

    }
}
