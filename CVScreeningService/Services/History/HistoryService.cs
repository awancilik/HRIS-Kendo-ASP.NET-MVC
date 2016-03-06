using AutoMapper;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.History;
using CVScreeningService.Filters;
using UserProfile = CVScreeningCore.Models.webpages_UserProfile;

namespace CVScreeningService.Services.History
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class HistoryService : IHistoryService
    {
        private readonly IUnitOfWork _uow;

        public HistoryService(IUnitOfWork uow)
        {
            _uow = uow;
            Mapper.CreateMap<CVScreeningCore.Models.History, HistoryDTO>();

        }

        #region History services

        #endregion
    }
}