namespace CVScreeningService.Services.Reporting
{
    public interface IPDFConverter
    {
        byte[] Convert(string source, string commandLocation, int screeningId, string hostName);
    }
}