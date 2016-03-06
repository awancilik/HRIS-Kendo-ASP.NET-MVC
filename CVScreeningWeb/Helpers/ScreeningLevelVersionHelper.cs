using System;
using CVScreeningService.DTO.Client;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.Helpers
{
    public class ScreeningLevelVersionHelper
    {
        /// <summary>
        /// To get chosen id of screening level version
        /// </summary>
        /// <param name="iModel"></param>
        /// <returns></returns>
        public static int ExtractScreeningLevelVersionViewModel(ScreeningLevelVersionViewModel iModel)
        {
            return Convert.ToInt32(iModel.ScreeningLevelVersionId);
        }

        /// <summary>
        /// To initialize cascading dropdown list for screening level version
        /// </summary>
        /// <param name="screeningLevelVersion">Not null for edit mode</param>
        /// <param name="clientCompany">Not null for creation from client</param>
        /// <returns></returns>
        public static ScreeningLevelVersionViewModel BuildScreeningLevelVersionViewModel(
            ScreeningLevelVersionDTO screeningLevelVersion = null, bool clientMode = false)
        {
            return new ScreeningLevelVersionViewModel
            {
                ScreeningLevelVersionId =
                    screeningLevelVersion == null ? "0" : screeningLevelVersion.ScreeningLevelVersionId + "",
                ScreeningLevelVersionName =
                    screeningLevelVersion == null ? "" : screeningLevelVersion.ScreeningLevelVersionNumber + "",
                ScreeningLevelId = screeningLevelVersion == null || screeningLevelVersion.ScreeningLevel == null
                    ? "0"
                    : screeningLevelVersion.ScreeningLevel.ScreeningLevelId + "",
                ScreeningLevelName = screeningLevelVersion == null || screeningLevelVersion.ScreeningLevel == null
                    ? ""
                    : screeningLevelVersion.ScreeningLevel.ScreeningLevelName,
                ContractId = screeningLevelVersion == null || screeningLevelVersion.ScreeningLevel == null ||
                             screeningLevelVersion.ScreeningLevel.Contract == null
                    ? "0"
                    : screeningLevelVersion.ScreeningLevel.Contract.ContractId + "",
                ContractName = screeningLevelVersion == null || screeningLevelVersion.ScreeningLevel == null ||
                               screeningLevelVersion.ScreeningLevel.Contract == null
                    ? ""
                    : screeningLevelVersion.ScreeningLevel.Contract.ContractReference,
                ClientCompanyId = screeningLevelVersion == null || screeningLevelVersion.ScreeningLevel == null ||
                                  screeningLevelVersion.ScreeningLevel.Contract == null ||
                                  screeningLevelVersion.ScreeningLevel.Contract.ClientCompany == null
                    ? "0"
                    : screeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyId + "",
                ClientCompanyName = screeningLevelVersion == null || screeningLevelVersion.ScreeningLevel == null ||
                                    screeningLevelVersion.ScreeningLevel.Contract == null ||
                                    screeningLevelVersion.ScreeningLevel.Contract.ClientCompany == null
                    ? ""
                    : screeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyName,
                IsClientMode = clientMode
            };
        }
    }
}