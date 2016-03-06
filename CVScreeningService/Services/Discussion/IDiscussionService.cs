using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Discussion;

namespace CVScreeningService.Services.Discussion
{
    public interface IDiscussionService
    {
        #region Discussion


        /// <summary>
        /// Create discussion in the application
        /// </summary>
        /// <param name="discussionDTO"></param>
        /// <param name="messageDTO"></param>
        /// <returns></returns>
        ErrorCode CreateMessage(ref DiscussionDTO discussionDTO, ref MessageDTO messageDTO);

        /// <summary>
        /// Get all the messages of a discussion
        /// </summary>
        /// <param name="discussionDTO"></param>
        /// <returns></returns>
        IEnumerable<MessageDTO> GetMessages(DiscussionDTO discussionDTO);

        /// <summary>
        /// Get information of a discussion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        DiscussionDTO GetDiscussion(int id);

        #endregion
    }
}