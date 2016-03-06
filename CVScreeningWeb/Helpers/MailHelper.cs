using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using Postal;

namespace CVScreeningWeb.Helpers
{
    public class MailHelper
    {

        /// <summary>
        /// Helper to send email to recover password
        /// </summary>
        public static void SendRecoverPasswordEmail(
            string userName,
            string fullName,
            string tokenGenerated,
            string link
            )
        {
            dynamic email = new Email("RecoverPassword");
            email.To = userName;
            email.UserName = fullName;
            email.ConfirmationToken = tokenGenerated;
            email.Link = link;
            email.Send();
        }

        /// <summary>
        /// Helper to send email when report has been submitted
        /// </summary>
        public static bool SendReportEmail(
            ClientCompanyDTO clientCompanyDTO,
            IEnumerable<UserProfileDTO> accountsDTO,
            ScreeningBaseDTO screeningDTO,
            ScreeningReportDTO screeningReportDTO,
            Uri uri)
        {
            try
            {
                var link = string.Format("{0}://{1}:{2}/Report/Download{3}Report/{4}/{5}",
                            uri.Scheme,
                            uri.Host,
                            uri.Port,
                            screeningReportDTO.ScreeningReportGenerationType,
                            screeningDTO.ScreeningId,
                            screeningReportDTO.ScreeningReportId);

                foreach (var clientAccount in accountsDTO)
                {
                    dynamic email = new Email("SubmitReport");
                    email.To = clientAccount.UserName;
                    email.ClientFullName = clientAccount.FullName;
                    email.ScreeningFullName = screeningDTO.ScreeningFullName;
                    email.ReportLink = link;
                    email.Send();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}