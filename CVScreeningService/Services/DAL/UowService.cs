using CVScreeningDAL.UnitOfWork;

namespace CVScreeningService.Services.DAL
{
    public class UowService : IUowService
    {
        private readonly IUnitOfWork _uow;

        public UowService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public void Save()
        {
            _uow.Commit();
        }

        public void Dispose()
        {
            _uow.Commit();
        }
    }
}