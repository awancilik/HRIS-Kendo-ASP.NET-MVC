using CVScreeningService.Filters;

namespace CVScreeningService.Services.UserManagement
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class WebSecurity : IWebSecurity
    {
        public WebSecurity()
        {

        }

        public string GetCurrentUserName()
        {
            return WebMatrix.WebData.WebSecurity.CurrentUserName;
        }
    }
}