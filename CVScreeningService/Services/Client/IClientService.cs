using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.Services.Client
{
    public interface IClientService
    {
        #region Client company

        ErrorCode CreateClientCompany(ref ClientCompanyDTO clientCompanyDTO);
        ErrorCode EditClientCompany(ref ClientCompanyDTO clientCompanyDTO);
        ErrorCode DeleteClientCompany(int id);
        ErrorCode DeactivateClientCompany(ref ClientCompanyDTO clientCompanyDTO);
        ErrorCode ReactivateClientCompany(ref ClientCompanyDTO clientCompanyDTO);

        ClientCompanyDTO GetClientCompany(int id);
        List<ClientCompanyDTO> GetAllClientCompanies();
        List<ClientCompanyDTO> GetAllClientCompaniesForAccountManager();

        IEnumerable<UserProfileDTO> GetClientAccountsFromClientCompany(ClientCompanyDTO clientCompanyDTO);

            #endregion

        #region Client contact person

        ErrorCode CreateClientContactPerson(ClientCompanyDTO clientCompanyDTO, ref ContactPersonDTO contactPersonDTO);
        ErrorCode EditClientContactPerson(ref ContactPersonDTO contactPersonDTO);
        ErrorCode DeleteClientContactPerson(ClientCompanyDTO clientCompanyDTO, ContactPersonDTO contactPersonDTO);
        ContactPersonDTO GetClientContactPerson(int id);
        List<ContactPersonDTO> GetAllClientContactPersonsByCompany(ClientCompanyDTO clientCompanyDTO);

        #endregion

        #region Client contract

        ErrorCode CreateClientContract(ClientCompanyDTO clientCompanyDTO, ref ClientContractDTO clientContractDTO);
        ErrorCode EditClientContract(ref ClientContractDTO clientContractDTO);
        ErrorCode DeleteClientContract(ClientCompanyDTO clientCompanyDTO, ClientContractDTO clientContractDTO);
        ClientContractDTO GetClientContract(int id);

        List<ClientContractDTO> GetAllClientContractsByCompany(ClientCompanyDTO clientCompanyDTO,
            bool includeDisabled = true);

        ErrorCode EnableClientContract(ref ClientContractDTO clientContractDTO);
        ErrorCode DisableClientContract(ref ClientContractDTO clientContractDTO);

        #endregion

        #region Client screening level

        ErrorCode CreateScreeningLevel(
            ClientContractDTO clientContractDTO, ref ScreeningLevelDTO screeningLevelDTO,
            ref ScreeningLevelVersionDTO screeningLevelVersionDTO);

        ErrorCode EditScreeningLevel(ref ScreeningLevelDTO screeningLevelDTO,
            ref ScreeningLevelVersionDTO screeningLevelVersionDTO);

        ErrorCode UpdateScreeningLevel(ref ScreeningLevelDTO screeningLevelDTO,
            ref ScreeningLevelVersionDTO newScreeningLevelVersionDTO);

        ErrorCode DeleteScreeningLevel(ClientContractDTO clientContractDTO, ScreeningLevelDTO screeningLevelDTO);

        ScreeningLevelDTO GetScreeningLevel(int id);
        List<ScreeningLevelDTO> GetScreeningLevelsByContract(ClientContractDTO clientContractDTO);
        List<ScreeningLevelVersionDTO> GetScreeningLevelVersionsByScreeningLevel(ScreeningLevelDTO screeningLevelDTO);

        #endregion
    }
}