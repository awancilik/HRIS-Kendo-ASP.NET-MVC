using System.Resources;

namespace CVScreeningCore.Error
{
    public class ResourceErrorFactory : IErrorFactory
    {
        private readonly ResourceManager _resourceManager;

        public ResourceErrorFactory()
        {
            var assembly = this.GetType().Assembly;
            _resourceManager = new ResourceManager("CVScreeningCore.Error.ErrorResource", assembly);
        }

        public string Create(ErrorCode errorCode)
        {
           return _resourceManager.GetString(errorCode.ToString());
        }
    }
}