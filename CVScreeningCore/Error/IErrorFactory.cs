namespace CVScreeningCore.Error
{
    public interface IErrorFactory
    {
        string Create(ErrorCode errorCode);
    }
}