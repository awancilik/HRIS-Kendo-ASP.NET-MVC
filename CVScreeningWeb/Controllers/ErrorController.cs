using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningWeb.ViewModels.Error;


namespace CVScreeningWeb.Controllers
{
    [Filters.HandleError]
    [AllowAnonymous]
    public class ErrorController : Controller
    {

        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        /// <summary>
        ///     Constructor to pass service
        /// </summary>
        public ErrorController(IErrorMessageFactoryService errorMessageFactoryService)
        {
            _errorMessageFactoryService = errorMessageFactoryService;
        }


        /// <summary>
        /// Action used to return error to end users
        /// </summary>
        /// <param name="errorCodeParameter"></param>
        /// <returns></returns>
        public ActionResult Index(ErrorCode errorCodeParameter = ErrorCode.UNKNOWN_ERROR)
        {
            var errorVm = new ErrorIndexViewModel
            {
                ErrorMessage = _errorMessageFactoryService.Create(errorCodeParameter)
            };

            return View(errorVm);
        }

        /// <summary>
        /// Action used to return error 404 to end users (Page not found)
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public ActionResult Error404(ErrorCode? errorCode)
        {
            return View();
        }

        /// <summary>
        /// Action used to return error 404 to end users (Page not found)
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public ActionResult Error403(ErrorCode? errorCode)
        {
            return View();
        }

        /// <summary>
        /// Action used to return error 500 to end users (Page not found)
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public ActionResult Error500(ErrorCode? errorCode)
        {
            return View();
        }

    }
}
