using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningCore.Models.ScreeningState;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Screening;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Reporting;
using CVScreeningService.Services.Screening;
using CVScreeningWeb.Helpers;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Report;
using Nalysa.Common.Log;

namespace CVScreeningWeb.Controllers
{
    [Filters.HandleError]
    public class ReportController : Controller
    {
        //
        // GET: /Report/
        private readonly IPDFConverter _pdfConverter;
        private readonly IScreeningService _screeningService;
        private readonly IClientService _clientService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;
        private readonly INotificationService _notificationService;

        public ReportController(IPDFConverter pdfConverter, 
            IScreeningService screeningService,
            IClientService clientService,
            IErrorMessageFactoryService errorMessageFactoryService, INotificationService notificationService)
        {
            _pdfConverter = pdfConverter;
            _screeningService = screeningService;
            _clientService = clientService;
            _errorMessageFactoryService = errorMessageFactoryService;
            _notificationService = notificationService;
        }

        [Authorize]
        public ActionResult ManageReport(int id)
        {
            var viewModel = GenerateManageReportViewModel(id);
            
            //if any modelstate of errors are present, then set them to viewModel
            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
                viewModel.ErrorMessages = ViewData.ModelState.Keys
                    .SelectMany(key => ViewData.ModelState[key].Errors).Select(e => e.ErrorMessage);
            }
            return View(viewModel);
        }

        [Authorize]
        public ActionResult ManageReportWithErrorMessage(int id)
        {
            var viewModel = GenerateManageReportViewModel(id);
            
            return View("ManageReport", viewModel);
        }

        [Authorize]
        public ActionResult SubmittedReport(int id)
        {
            var screeningDTO = _screeningService.GetBaseScreening(id);
            var viewModel = new ReportByScreeningViewModel
            {
                ScreeningId = screeningDTO.ScreeningId,
            };

            // Retreive already submitted reports
            var reportManageViewModels = new List<ReportManageViewModel>();
            reportManageViewModels.AddRange(
                (
                    from e in screeningDTO.ScreeningReport
                    where e.Screening.ScreeningId == id && e.ScreeningReportVersion > 0
                    select new ReportManageViewModel
                    {
                        Id = e.ScreeningReportId,
                        Status = e.ScreeningReportVersion > 0 ? Report.Submitted : Report.NotSubmitted,
                        SubmittedDate = e.ScreeningReportSubmittedDate.ToShortDateString(),
                        Type = e.ScreeningReportGenerationType,
                        Version = Convert.ToString(e.ScreeningReportVersion),
                        ScreeningStatus = Convert.ToString((ScreeningStateType)screeningDTO.ScreeningState)
                    }));
    
            viewModel.ReportManageViewModels = reportManageViewModels;
            return View("ManageReport", viewModel);
        }


        [Authorize]
        public ReportByScreeningViewModel GenerateManageReportViewModel(int id)
        {
            var screeningDTO = _screeningService.GetScreening(id);

            var viewModel = new ReportByScreeningViewModel();
            viewModel.ScreeningId = screeningDTO.ScreeningId;

            // Retrieve already submitted reports
            var reportManageViewModels = new List<ReportManageViewModel>();
            reportManageViewModels.AddRange(
                (
                    from e in screeningDTO.ScreeningReport
                    where e.Screening.ScreeningId == id && e.ScreeningReportVersion > 0
                    select new ReportManageViewModel
                    {
                        Id = e.ScreeningReportId,
                        Status = e.ScreeningReportVersion > 0 ? Report.Submitted : Report.NotSubmitted,
                        SubmittedDate = e.ScreeningReportSubmittedDate.ToShortDateString(),
                        Type = e.ScreeningReportGenerationType,
                        Version = Convert.ToString(e.ScreeningReportVersion),
                        ScreeningStatus = Convert.ToString((ScreeningStateType)screeningDTO.ScreeningState)
                    }));

            // Add manual report that has been uploaded
            if (screeningDTO.ScreeningReport.Any(e => e.ScreeningReportVersion == 0))
            {
                var manualReport = screeningDTO.ScreeningReport.First(e => e.ScreeningReportVersion == 0);
                reportManageViewModels.Add(new ReportManageViewModel
                {
                    Id = manualReport.ScreeningReportId,
                    Status = Report.NotSubmitted,
                    SubmittedDate = Resources.Common.NA,
                    Type = Report.Manual,
                    Version = Resources.Common.NA,
                    ScreeningStatus = Convert.ToString((ScreeningStateType)screeningDTO.ScreeningState)
                });
            }

            var ret = _screeningService.AreAllAtomicChecksValidated(screeningDTO);

            //Add automatic generated one which has not been submitted
            if (_screeningService.AreAllAtomicChecksValidated(screeningDTO) && !ReportHelper.IsSameContent(screeningDTO))
            {
                reportManageViewModels.Add(new ReportManageViewModel
                {
                    Id = 0,
                    Status = Report.NotSubmitted,
                    Type = Report.Automatic,
                    SubmittedDate = Resources.Common.NA,
                    Version = Resources.Common.NA,
                    ScreeningStatus = Convert.ToString((ScreeningStateType)screeningDTO.ScreeningState)
                });    
            }
            viewModel.ReportManageViewModels = reportManageViewModels;
            return viewModel;
        }

        [Authorize]
        public ActionResult Index(ReportViewModel reportViewModel)
        {
            return View(reportViewModel);
        }

        [AllowAnonymous]
        public ActionResult CoverPage(int id)
        {
            var screeningDTO = _screeningService.GetScreeningForCover(id);
            return View(ReportHelper.GenerateCoverReportViewModel(screeningDTO));
        }

        /// <summary>
        /// Download manual report
        /// </summary>
        /// <param name="id">Screening ID</param>
        /// <param name="secondaryId">Screening report Id</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult DownloadManualReport(int id, int secondaryId)
        {
            var hostName = Request.Url.GetLeftPart(UriPartial.Authority);
            var screeningReportDTO = _screeningService.GetReport(secondaryId);
            var relativePath = screeningReportDTO.ScreeningReportUrl.Replace(hostName, "");
            return File(relativePath, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        /// <summary>
        /// Download automatic report
        /// </summary>
        /// <param name="id">Screening ID</param>
        /// <param name="secondaryId">Screening report Id</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult DownloadAutomaticReport(int id, int secondaryId)
        {
            // Report generated on the fly
            if (secondaryId == 0)
            {
                var screeningDTO = _screeningService.GetScreening(id);
                var reportViewModel = ReportHelper.GenerateReportViewModel(screeningDTO);
                return GeneratePDF(reportViewModel);                
            }
            // Report already submitted    
            else
            {
                var screeningReportDTO = _screeningService.GetReport(secondaryId);
                return GeneratePDF((ReportViewModel) FileHelper.ByteArrayToObject(screeningReportDTO.ScreeningReportContent));  
            }
        }

        /// <summary>
        /// Return a PDF file from given view model.
        /// View: Views/Report/Index.cshtml
        /// </summary>
        /// <param name="reportViewModel"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult GeneratePDF(ReportViewModel reportViewModel)
        {
            // refers to master page of report
            const string relativePath = "~/Views/Report/Index.cshtml";

            // for displaying view models
            ViewData.Model = reportViewModel;

            //get hostName
            var hostName = Request.Url.GetLeftPart(UriPartial.Authority)+"/";

            var view = ViewEngines.Engines.FindView(ControllerContext, relativePath, null);

            using (var writer = new StringWriter())
            {
                var context = new ViewContext(ControllerContext, view.View, ViewData, TempData, writer);
                view.View.Render(context, writer);
                writer.Flush();
                var content = writer.ToString();
                var pdfBuf = _pdfConverter.Convert(content, Server.MapPath(FileHelper.GetFileFolder()), reportViewModel.ScreeningId, hostName);
                if (pdfBuf == null) return HttpNotFound();
                return File(pdfBuf, "application/pdf");
            }
        }

        [Authorize]
        public ActionResult UploadManualReport(int id)
        {
            return PartialView("_uploadManualReport", new UploadManualReportViewModel
            {
                ScreeningId = id
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UploadManualReport(UploadManualReportViewModel iModel)
        {
            if (!IsPDFExtention(iModel.ManualReport))
            {
                ModelState.AddModelError("error", _errorMessageFactoryService
                    .Create(ErrorCode.REPORT_NOT_VALID_EXTENSION));
                TempData["ViewData"] = ViewData;
                return RedirectToAction("ManageReport", new { id = iModel.ScreeningId });
            }

            var screeningDTO = _screeningService.GetBaseScreening(iModel.ScreeningId);
            var folder = FileHelper.GenerateManualReportFilePath(screeningDTO);
            var reportFileName = FileHelper.GenerateCurrentReportFileName(screeningDTO, iModel.ManualReport);

            if (iModel.ManualReport != null)
            {
                Directory.CreateDirectory(folder);
                var physicalPath = Path.Combine(folder, reportFileName);
                iModel.ManualReport.SaveAs(physicalPath);
            }

            var errorCode = SaveManualReport(screeningDTO, folder, reportFileName);
            if (errorCode != ErrorCode.NO_ERROR)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                TempData["ViewData"] = ViewData;
            }

            //if success then return a json object
            return RedirectToAction("ManageReport", new {id = iModel.ScreeningId});
        }

        private ErrorCode SaveManualReport(ScreeningBaseDTO screeningDTO, string folder, string reportFileName)
        {
            //get hostName
            var hostName = Request.Url.GetLeftPart(UriPartial.Authority) + "/";

            var screeningReport = new ScreeningReportDTO
            {
                Screening = screeningDTO,
                ScreeningReportFilePath = Path.Combine(folder, reportFileName),
                ScreeningReportUrl = FileHelper.PhysicalToVirtualPath(
                    Path.Combine(folder, reportFileName), hostName),
                ScreeningReportGenerationType = Report.Manual,
                ScreeningReportSubmittedDate = DateTime.Now
            };

            LogManager.Instance.Info(String.Format("Folder: {0}", folder));
            LogManager.Instance.Info(String.Format("ReportFileName: {0}", reportFileName));
            LogManager.Instance.Info(String.Format("ScreeningReportFilePath: {0}", screeningReport.ScreeningReportFilePath));
            LogManager.Instance.Info(String.Format("ScreeningReportUrl: {0}", screeningReport.ScreeningReportFilePath));
            LogManager.Instance.Info(String.Format("ScreeningReportFilePath: {0}", screeningReport.ScreeningReportFilePath));


            return _screeningService.CreateManualReport(screeningDTO, ref screeningReport);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manualReport"></param>
        /// <returns></returns>
        private bool IsPDFExtention(HttpPostedFileBase manualReport)
        {
            return manualReport.FileName.Contains(".pdf");
        }

        /// <summary>
        /// Submit report the client and send email to him
        /// </summary>
        /// <param name="id">Screening id</param>
        /// <param name="secondaryId">Screening report id</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult SubmitReport(int id, int secondaryId)
        {
            var model = new SubmitReportViewModel
            {
                ReportId = secondaryId,
                ScreeningId = id
            };
            return PartialView("_submitReport", model);
        }

        /// <summary>
        /// Submit report the client and send email to him
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Filters.NotificationAttribute]
        public ActionResult SubmitReport(SubmitReportViewModel viewModel)
        {
            ErrorCode errorCode;
            ScreeningReportDTO screeningReportDTO = null;
            ClientCompanyDTO clientCompanyDTO = null;
            RouteData.Values.Add("notificationService", _notificationService);

            if (!ModelState.IsValid)
            {
                return PartialView("_submitReport", viewModel);
            }  

            var screeningDTO = _screeningService.GetScreening(viewModel.ScreeningId);
            clientCompanyDTO = screeningDTO.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany;
            var accounts = _clientService.GetClientAccountsFromClientCompany(clientCompanyDTO);


            // AUTOMATIC REPORT: secondaryId is equal to 0 when an automatic report needs to be submitted
            if (viewModel.ReportId == 0)
            {
                screeningReportDTO = new ScreeningReportDTO
                {
                    ScreeningReportContent = FileHelper.ObjectToByteArray(ReportHelper.GenerateReportViewModel(screeningDTO)),
                    ScreeningReportGenerationType = ScreeningReport.kAutomaticGenerationType,
                };
            }
            // MANUAL REPORT: secondaryId is not equal to 0 when a manual report needs to be submitted
            else
            {
                screeningReportDTO = _screeningService.GetReport(viewModel.ReportId);
            }

            errorCode = _screeningService.SubmitReport( ref screeningReportDTO, ref screeningDTO);
            if (errorCode != ErrorCode.NO_ERROR)
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(errorCode));
                return PartialView("_submitReport", viewModel);
            }

            if (!MailHelper.SendReportEmail(clientCompanyDTO, accounts, screeningDTO, screeningReportDTO, Request.Url))
            {
                ModelState.AddModelError("", _errorMessageFactoryService.Create(ErrorCode.REPORT_SUBMIT_MAIL_FAILURE));
                return PartialView("_submitReport", viewModel);
            }

            return Json(new
            {
                redirectTo = Url.Action("ManageReport", "Report", new
                {
                    id = viewModel.ScreeningId
                })
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Report id</param>
        /// <returns></returns>
        [Authorize]
        public ActionResult DownloadResult(int id)
        {
            return null;
        }

    }
}