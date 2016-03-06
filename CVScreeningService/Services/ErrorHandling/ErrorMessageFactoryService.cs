using CVScreeningCore.Error;

namespace CVScreeningService.Services.ErrorHandling
{
    public class ErrorMessageFactoryService : IErrorMessageFactoryService
    {
        private readonly IErrorFactory _errorFactory;

        public ErrorMessageFactoryService(IErrorFactory errorFactory)
        {
            _errorFactory = errorFactory;
        }

        public string Create(ErrorCode errorCode)
        {
            return _errorFactory.Create(errorCode);
        }
    }
}