using System.Linq;
using System.Web.Mvc;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Discussion;
using CVScreeningService.Services.Discussion;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Settings;
using CVScreeningWeb.ViewModels.Discussion;


namespace CVScreeningWeb.Controllers
{
    [Authorize]
    [Filters.HandleError]
    public class DiscussionController : Controller
    {
        // Internal attributes
        private readonly IDiscussionService _discussionService;
        private readonly IErrorMessageFactoryService _errorMessageFactoryService;

        /// <summary>
        ///     Constructor to pass service
        /// </summary>
        /// <param name="discussionService"></param>
        /// <param name="errorMessageFactoryService"></param>
        public DiscussionController(
            IDiscussionService discussionService, 
            IErrorMessageFactoryService errorMessageFactoryService)
        {
            _discussionService = discussionService;
            _errorMessageFactoryService = errorMessageFactoryService;
        }


        /// <summary>
        /// Controller action - Display disussion content
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Index(int id)
        {
            var discussion = _discussionService.GetDiscussion(id);
            var messages = _discussionService.GetMessages(new DiscussionDTO{DiscussionId = id});
            if (discussion == null || messages == null)
            {
                return View();
            }

            var viewModel = new DiscussionDetailsViewModel
            {
                DiscussionId = id,
                DiscussionTitle = discussion.DiscussionTitle,
                Messages = messages.Select(
                    item => new MessageDetailsViewModel
                    {
                        MessageId = item.MessageId,
                        Message = item.MessageContent,
                        CreatedByFullName = item.MessageCreatedBy.FullName,
                        CreatedByUserName = item.MessageCreatedBy.UserName,
                        CreatedDate = item.MessageCreatedDate
                    })
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(MessageFormViewModel viewModel)
        {
            var discussionDTO = new DiscussionDTO
            {
                DiscussionId = viewModel.DiscussionId
            };
            var messageDTO = new MessageDTO
            {
                MessageContent = viewModel.Message
            };

            var errorCode = _discussionService.CreateMessage(ref discussionDTO, ref messageDTO);
            return Json(new
            {
                Success = errorCode == ErrorCode.NO_ERROR,
                Message = errorCode == ErrorCode.NO_ERROR ? "" : _errorMessageFactoryService.Create(errorCode)
            });
        }
    }
}
