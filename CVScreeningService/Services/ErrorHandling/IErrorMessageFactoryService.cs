using CVScreeningCore.Error;

namespace CVScreeningService.Services.ErrorHandling
{
    public interface IErrorMessageFactoryService
    {
        string Create(ErrorCode errorCode);
    }
}