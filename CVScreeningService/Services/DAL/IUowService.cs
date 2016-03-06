namespace CVScreeningService.Services.DAL
{
    public interface IUowService
    {
        void Save();
        void Dispose();
    }
}