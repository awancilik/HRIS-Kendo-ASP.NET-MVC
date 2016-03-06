using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.UserManagement;
using Nalysa.Common.Log;
using UserProfile = CVScreeningCore.Models.webpages_UserProfile;

namespace CVScreeningService.Services.Client
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class ClientService : IClientService
    {
        private readonly ICommonService _commonService;
        private readonly IUnitOfWork _uow;
        private readonly IUserManagementService _userManagementService;
        private readonly IPermissionService _permissionService;


        public ClientService(
            IUnitOfWork uow,
            ICommonService commonService,
            IUserManagementService userManagementService,
            IPermissionService permissionService)
        {
            _uow = uow;
            _commonService = commonService;
            _userManagementService = userManagementService;
            _permissionService = permissionService;
            Mapper.CreateMap<UserProfile, UserProfileDTO>();
            Mapper.CreateMap<ContactPerson, ContactPersonDTO>();
            Mapper.CreateMap<Address, AddressDTO>();
            Mapper.CreateMap<ContactInfo, ContactInfoDTO>();
            Mapper.CreateMap<ClientCompany, ClientCompanyDTO>();
            Mapper.CreateMap<Contract, ClientContractDTO>();
            Mapper.CreateMap<ScreeningLevel, ScreeningLevelDTO>();
            Mapper.CreateMap<ScreeningLevelVersion, ScreeningLevelVersionDTO>();
            Mapper.CreateMap<TypeOfCheckScreeningLevelVersion, TypeOfCheckScreeningLevelVersionDTO>();
            Mapper.CreateMap<TypeOfCheck, TypeOfCheckDTO>();
        }

        #region Client company

        /// <summary>
        /// Create client company
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateClientCompany(ref ClientCompanyDTO clientCompanyDTO)
        {
            try
            {
                var clientCompanyName = clientCompanyDTO.ClientCompanyName;

                // Account manager not fill
                if (clientCompanyDTO.AccountManagers == null || clientCompanyDTO.AccountManagers.Count == 0)
                {
                    return ErrorCode.CLIENT_COMPANY_ACCOUNT_MANAGER_MISSING;
                }

                // Company client already existing
                if (_uow.ClientCompanyRepository.Exist(
                    u => string.Compare(u.ClientCompanyName, clientCompanyName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return ErrorCode.CLIENT_COMPANY_ALREADY_EXISTS;
                }

                var clientCompany = new ClientCompany
                {
                    ClientCompanyName = clientCompanyDTO.ClientCompanyName,
                    Category = clientCompanyDTO.Category,
                    Description = clientCompanyDTO.Description,
                    ClientCompanyIsDeactivated = false,
                };

                // Address creation
                var locationId = clientCompanyDTO.Address.Location.LocationId;
                var address = new Address
                {
                    Street = clientCompanyDTO.Address.Street,
                    PostalCode = clientCompanyDTO.Address.PostalCode,
                    Location = _uow.LocationRepository.Single(l => l.LocationId == locationId)
                };
                clientCompany.Address = _uow.AddressRepository.Add(address);


                // Contact info creation
                var contactInfo = new ContactInfo
                {
                    WebSite = clientCompanyDTO.ContactInfo.WebSite
                };
                clientCompany.ContactInfo = _uow.ContactInfoRepository.Add(contactInfo);


                // Account manager
                foreach (var accountManagerDTO in clientCompanyDTO.AccountManagers)
                {
                    var accountManagerDTOGet = _userManagementService.GetUserProfilebyName(accountManagerDTO.UserName);
                    if (accountManagerDTOGet == null)
                    {
                        LogManager.Instance.Error(
                            string.Format("Function: {0}. Account manager not existing. Object: {1}",
                                MethodBase.GetCurrentMethod().Name,
                                accountManagerDTO.UserName));
                        return ErrorCode.UNKNOWN_ERROR;
                    }
                    var accountManager = _uow.UserProfileRepository.Single(
                        u => u.UserId == accountManagerDTOGet.UserId);
                    clientCompany.AddAccountManager(accountManager);
                }

                _uow.ClientCompanyRepository.Add(clientCompany);
                _uow.Commit();

                clientCompanyDTO.ClientCompanyId = clientCompany.ClientCompanyId;
                clientCompanyDTO = Mapper.Map<ClientCompany, ClientCompanyDTO>(clientCompany);
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Object: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        clientCompanyDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Edit client company
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode EditClientCompany(ref ClientCompanyDTO clientCompanyDTO)
        {
            try
            {
                var companyId = clientCompanyDTO.ClientCompanyId;

                // Company client already existing
                if (!_uow.ClientCompanyRepository.Exist(u => u.ClientCompanyId == companyId))
                {
                    return ErrorCode.CLIENT_COMPANY_NOT_FOUND;
                }

                // Account manager not fill
                if (clientCompanyDTO.AccountManagers == null || clientCompanyDTO.AccountManagers.Count == 0)
                {
                    return ErrorCode.CLIENT_COMPANY_ACCOUNT_MANAGER_MISSING;
                }

                var locationId = clientCompanyDTO.Address.Location.LocationId;

                var clientCompany =
                    _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == companyId);

                clientCompany.ClientCompanyName = clientCompanyDTO.ClientCompanyName;
                clientCompany.Category = clientCompanyDTO.Category;
                clientCompany.ClientCompanyIsDeactivated = clientCompanyDTO.ClientCompanyIsDeactivated;
                clientCompany.Description = clientCompanyDTO.Description;

                clientCompany.Address.Street = clientCompanyDTO.Address.Street;
                clientCompany.Address.PostalCode = clientCompanyDTO.Address.PostalCode;
                clientCompany.Address.Location =
                    _uow.LocationRepository.Single(l => l.LocationId == locationId);
                clientCompany.ContactInfo.WebSite = clientCompanyDTO.ContactInfo.WebSite;
                clientCompany.ClearAccountManagers();

                // Account manager
                foreach (var accountManagerDTO in clientCompanyDTO.AccountManagers)
                {
                    var accountManagerDTOGet = _userManagementService.GetUserProfilebyName(accountManagerDTO.UserName);
                    var accountManager = _uow.UserProfileRepository.Single(u => u.UserId == accountManagerDTOGet.UserId);
                    clientCompany.AddAccountManager(accountManager);
                }

                _uow.Commit();

                clientCompanyDTO = Mapper.Map<ClientCompany, ClientCompanyDTO>(clientCompany);
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Object: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        clientCompanyDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Delete client company
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteClientCompany(int id)
        {
            try
            {
                if (!_uow.ClientCompanyRepository.Exist(u => u.ClientCompanyId == id))
                {
                    return ErrorCode.CLIENT_COMPANY_NOT_FOUND;
                }

                var clientCompany = _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == id);
                var accountManagers = clientCompany.AccountManagers;
                foreach (var accountManager in accountManagers.Reverse())
                {
                    clientCompany.RemoveAccountManager(accountManager);
                }

                // Delete address, contact info and contact person
                _commonService.DeleteAddress(clientCompany.Address.AddressId, false);
                _commonService.DeleteContactInfo(clientCompany.ContactInfo.ContactInfoId, false);
                _uow.ClientCompanyRepository.Delete(clientCompany);

                _uow.Commit();
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Client company Id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        id, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Deactivate client company
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode DeactivateClientCompany(ref ClientCompanyDTO clientCompanyDTO)
        {
            try
            {
                var id = clientCompanyDTO.ClientCompanyId;
                if (GetClientCompany(clientCompanyDTO.ClientCompanyId) == null)
                    return ErrorCode.CLIENT_COMPANY_NOT_FOUND;

                var clientCompany = _uow.ClientCompanyRepository.Single(
                    u => u.ClientCompanyId == id);

                clientCompany.ClientCompanyIsDeactivated = true;
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Client company Id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        clientCompanyDTO.ClientCompanyId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }


        /// <summary>
        /// Reactivate client company
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode ReactivateClientCompany(ref ClientCompanyDTO clientCompanyDTO)
        {
            try
            {
                var id = clientCompanyDTO.ClientCompanyId;
                if (GetClientCompany(clientCompanyDTO.ClientCompanyId) == null)
                    return ErrorCode.CLIENT_COMPANY_NOT_FOUND;

                var clientCompany = _uow.ClientCompanyRepository.Single(
                    u => u.ClientCompanyId == id);

                clientCompany.ClientCompanyIsDeactivated = false;
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Client company Id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        clientCompanyDTO.ClientCompanyId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Get client company by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ClientCompanyDTO GetClientCompany(int id)
        {
            if (!_uow.ClientCompanyRepository.Exist(u => u.ClientCompanyId == id))
            {
                return null;
            }

            var clientCompany = _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == id);
            return Mapper.Map<ClientCompany, ClientCompanyDTO>(clientCompany);
        }

        /// <summary>
        /// Get all client companies
        /// </summary>
        /// <returns></returns>
        public virtual List<ClientCompanyDTO> GetAllClientCompanies()
        {
            var clientCompanies =
                _uow.ClientCompanyRepository.GetAll();
            return clientCompanies.Select(Mapper.Map<ClientCompany, ClientCompanyDTO>).ToList();
        }

        /// <summary>
        /// Get all client companies for AM
        /// </summary>
        /// <returns></returns>
        public virtual List<ClientCompanyDTO> GetAllClientCompaniesForAccountManager()
        {
            var am = _uow.UserProfileRepository.Single(
                u =>
                    string.Compare(u.UserName, WebMatrix.WebData.WebSecurity.CurrentUserName,
                        StringComparison.OrdinalIgnoreCase) == 0);

            if (!am.IsAccountManager())
                return null;

            var clientCompanies = am.ClientCompaniesForAM;
            return clientCompanies.Select(Mapper.Map<ClientCompany, ClientCompanyDTO>).ToList();
        }

        /// <summary>
        /// Retrieve all the client account linked to a company
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<UserProfileDTO> GetClientAccountsFromClientCompany(ClientCompanyDTO clientCompanyDTO)
        {
            if (!_uow.ClientCompanyRepository.Exist(u => u.ClientCompanyId == clientCompanyDTO.ClientCompanyId))
            {
                return null;
            }

            var clientCompany =
                _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == clientCompanyDTO.ClientCompanyId);
            return clientCompany.ClientUserProfiles.Select(Mapper.Map<UserProfile, UserProfileDTO>).ToList();
        }

        #endregion

        #region Client contact person

        /// <summary>
        /// Create client company contact person
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <param name="contactPersonDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateClientContactPerson(
            ClientCompanyDTO clientCompanyDTO, ref ContactPersonDTO contactPersonDTO)
        {
            try
            {
                var clientCompanyId = clientCompanyDTO.ClientCompanyId;
                var contactPersonName = contactPersonDTO.ContactPersonName;

                // Company client already existing
                if (!_uow.ClientCompanyRepository.Exist(
                    u => u.ClientCompanyId == clientCompanyId))
                {
                    return ErrorCode.CLIENT_COMPANY_NOT_FOUND;
                }

                var clientCompany = _uow.ClientCompanyRepository.Single(
                    u => u.ClientCompanyId == clientCompanyId);


                // Company contact person already existing
                if (clientCompany.ContactPerson.Any(u => u.ContactPersonName == contactPersonName))
                {
                    return ErrorCode.CLIENT_COMPANY_CONTACT_PERSON_ALREADY_EXISTS;
                }

                var clientContactPerson = new ContactPerson
                {
                    ContactPersonName = contactPersonDTO.ContactPersonName,
                    ContactComments = contactPersonDTO.ContactComments
                };

                // Address creation
                var locationId = contactPersonDTO.Address.Location.LocationId;
                var address = new Address
                {
                    Street = contactPersonDTO.Address.Street,
                    PostalCode = contactPersonDTO.Address.PostalCode,
                    Location = _uow.LocationRepository.Single(l => l.LocationId == locationId)
                };
                clientContactPerson.Address = _uow.AddressRepository.Add(address);

                // Contact info creation
                var contactInfo = new ContactInfo
                {
                    MobilePhoneNumber = contactPersonDTO.ContactInfo.MobilePhoneNumber,
                    HomePhoneNumber = contactPersonDTO.ContactInfo.HomePhoneNumber,
                    Position = contactPersonDTO.ContactInfo.Position
                };
                clientContactPerson.ContactInfo = _uow.ContactInfoRepository.Add(contactInfo);

                // Add contact person to the company
                clientCompany.ContactPerson.Add(_uow.ContactPersonRepository.Add(clientContactPerson));
                _uow.Commit();

                contactPersonDTO = Mapper.Map<ContactPerson, ContactPersonDTO>(clientContactPerson);
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Client company Id: {1}. Contact: {2} Error: {3}",
                        MethodBase.GetCurrentMethod().Name,
                        clientCompanyDTO.ClientCompanyId, contactPersonDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Edit client contact person
        /// </summary>
        /// <param name="contactPersonDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode EditClientContactPerson(ref ContactPersonDTO contactPersonDTO)
        {
            try
            {
                var contactPersonId = contactPersonDTO.ContactPersonId;

                // Company client already existing
                if (!_uow.ContactPersonRepository.Exist(
                    u => u.ContactPersonId == contactPersonId))
                {
                    return ErrorCode.CLIENT_COMPANY_CONTACT_PERSON_NOT_FOUND;
                }

                var clientContactPerson = _uow.ContactPersonRepository.Single(
                    u => u.ContactPersonId == contactPersonId);

                var locationId = contactPersonDTO.Address.Location.LocationId;

                clientContactPerson.ContactPersonName = contactPersonDTO.ContactPersonName;
                clientContactPerson.ContactComments = contactPersonDTO.ContactComments;
                clientContactPerson.Address.Street = contactPersonDTO.Address.Street;
                clientContactPerson.Address.PostalCode = contactPersonDTO.Address.PostalCode;
                clientContactPerson.Address.Location = _uow.LocationRepository.Single(l => l.LocationId == locationId);
                clientContactPerson.ContactInfo.MobilePhoneNumber = contactPersonDTO.ContactInfo.MobilePhoneNumber;
                clientContactPerson.ContactInfo.HomePhoneNumber = contactPersonDTO.ContactInfo.HomePhoneNumber;
                clientContactPerson.ContactInfo.Position = contactPersonDTO.ContactInfo.Position;

                _uow.Commit();
                contactPersonDTO = Mapper.Map<ContactPerson, ContactPersonDTO>(clientContactPerson);
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Contact id: {1} Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        contactPersonDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }


        /// <summary>
        /// Delete client contract person
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <param name="contactPersonDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteClientContactPerson(ClientCompanyDTO clientCompanyDTO,
            ContactPersonDTO contactPersonDTO)
        {
            try
            {
                var clientCompanyId = clientCompanyDTO.ClientCompanyId;
                var contactPersonId = contactPersonDTO.ContactPersonId;
                if (!_uow.ClientCompanyRepository.Exist(u => u.ClientCompanyId == clientCompanyId))
                {
                    return ErrorCode.CLIENT_COMPANY_NOT_FOUND;
                }

                var clientCompany = _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == clientCompanyId);


                if (clientCompany.ContactPerson.Count(u => u.ContactPersonId == contactPersonId) == 0)
                {
                    return ErrorCode.CLIENT_COMPANY_CONTACT_PERSON_NOT_FOUND;
                }

                var contactPerson = _uow.ContactPersonRepository.Single(u => u.ContactPersonId == contactPersonId);
                clientCompany.ContactPerson.Remove(contactPerson);
                _uow.ContactPersonRepository.Delete(contactPerson);
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Contact person Id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        contactPersonDTO.ContactPersonId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Get client contact person
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ContactPersonDTO GetClientContactPerson(int id)
        {
            if (!_uow.ContactPersonRepository.Exist(u => u.ContactPersonId == id))
            {
                return null;
            }

            var contactPerson = _uow.ContactPersonRepository.Single(u => u.ContactPersonId == id);
            return Mapper.Map<ContactPerson, ContactPersonDTO>(contactPerson);
        }


        /// <summary>
        /// Get all client contacts for a company
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <returns></returns>
        public virtual List<ContactPersonDTO> GetAllClientContactPersonsByCompany(ClientCompanyDTO clientCompanyDTO)
        {
            var clientCompanyId = clientCompanyDTO.ClientCompanyId;

            // Company client already existing
            if (!_uow.ClientCompanyRepository.Exist(
                u => u.ClientCompanyId == clientCompanyId))
            {
                return null;
            }

            var clientCompany = _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == clientCompanyId);
            return clientCompany.ContactPerson.Select(Mapper.Map<ContactPerson, ContactPersonDTO>).ToList();
        }

        #endregion

        #region Client contract

        /// <summary>
        /// Create client contract
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <param name="clientContractDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateClientContract(ClientCompanyDTO clientCompanyDTO,
            ref ClientContractDTO clientContractDTO)
        {
            try
            {
                var clientCompanyId = clientCompanyDTO.ClientCompanyId;
                var contractReference = clientContractDTO.ContractReference;
                var contractYear = clientContractDTO.ContractYear;


                // Company client already existing
                if (!_uow.ClientCompanyRepository.Exist(
                    u => u.ClientCompanyId == clientCompanyId))
                {
                    return ErrorCode.CLIENT_COMPANY_NOT_FOUND;
                }

                var clientCompany = _uow.ClientCompanyRepository.Single(
                    u => u.ClientCompanyId == clientCompanyId);


                // Company contact person already existing
                if (clientCompany.Contract.Any(u => u.ContractReference == contractReference
                                                    && u.ContractYear == contractYear))
                {
                    return ErrorCode.CLIENT_COMPANY_CONTRACT_ALREADY_EXISTS;
                }

                var clientContract = new Contract
                {
                    ContractReference = clientContractDTO.ContractReference,
                    ContractYear = clientContractDTO.ContractYear,
                    ContractDescription = clientContractDTO.ContractDescription,
                    IsContractEnabled = true,
                    ClientCompany = clientCompany
                };

                clientContract.GrantPermissionForAllAccountManagers();
                clientContract.GrantPermissionForAllClients();

                // Add contract to the company
                clientCompany.Contract.Add(_uow.ClientContractRepository.Add(clientContract));
                _uow.Commit();

                clientContractDTO = Mapper.Map<Contract, ClientContractDTO>(clientContract);
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Client company Id: {1}. Contract: {2} Error: {3}",
                        MethodBase.GetCurrentMethod().Name,
                        clientCompanyDTO.ClientCompanyId, clientContractDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Edit client contract
        /// </summary>
        /// <param name="clientContractDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode EditClientContract(ref ClientContractDTO clientContractDTO)
        {
            var contractReference = clientContractDTO.ContractReference;
            var contractYear = clientContractDTO.ContractYear;
            try
            {
                var contractId = clientContractDTO.ContractId;

                // Company client contract not existing
                if (!_uow.ClientContractRepository.Exist(
                    u => u.ContractId == contractId))
                {
                    return ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND;
                }

                var contract = _uow.ClientContractRepository.Single(
                    u => u.ContractId == contractId);

                var clientCompany = contract.ClientCompany;
                // Company contact person already existing
                if (clientCompany.Contract.Any(u => u.ContractReference == contractReference
                                                    && u.ContractYear == contractYear
                                                    && u.ContractId != contractId))
                {
                    return ErrorCode.CLIENT_COMPANY_CONTRACT_ALREADY_EXISTS;
                }


                contract.ContractReference = clientContractDTO.ContractReference;
                contract.ContractYear = clientContractDTO.ContractYear;
                contract.ContractDescription = clientContractDTO.ContractDescription;
                contract.IsContractEnabled = clientContractDTO.IsContractEnabled;

                _uow.Commit();

                clientContractDTO = Mapper.Map<Contract, ClientContractDTO>(contract);
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Client contract: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        clientContractDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Delete a client contract
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <param name="clientContractDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteClientContract(ClientCompanyDTO clientCompanyDTO,
            ClientContractDTO clientContractDTO)
        {
            try
            {
                var clientCompanyId = clientCompanyDTO.ClientCompanyId;
                var contractId = clientContractDTO.ContractId;
                if (!_uow.ClientCompanyRepository.Exist(u => u.ClientCompanyId == clientCompanyId))
                {
                    return ErrorCode.CLIENT_COMPANY_NOT_FOUND;
                }

                var clientCompany = _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == clientCompanyId);


                if (clientCompany.Contract.Count(u => u.ContractId == contractId) == 0)
                {
                    return ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND;
                }

                var contract = _uow.ClientContractRepository.Single(u => u.ContractId == contractId);
                clientCompany.Contract.Remove(contract);
                _uow.ClientContractRepository.Delete(contract);
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Contract Id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        clientContractDTO.ContractId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }


        /// <summary>
        /// Get client contract
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kContractViewPermission)]
        public virtual ClientContractDTO GetClientContract(int id)
        {
            if (!_uow.ClientContractRepository.Exist(u => u.ContractId == id))
            {
                return null;
            }

            var contract = _uow.ClientContractRepository.Single(u => u.ContractId == id);
            return Mapper.Map<Contract, ClientContractDTO>(contract);
        }

        /// <summary>
        /// Get all client contracts by company
        /// </summary>
        /// <param name="clientCompanyDTO"></param>
        /// <param name="includeDisabled">If equal to true, include all contracts, else include only enable contracts</param>
        /// <returns></returns>
        public virtual List<ClientContractDTO> GetAllClientContractsByCompany(ClientCompanyDTO clientCompanyDTO,
            bool includeDisabled = true)
        {
            var clientCompanyId = clientCompanyDTO.ClientCompanyId;

            // Company client already existing
            if (!_uow.ClientCompanyRepository.Exist(
                u => u.ClientCompanyId == clientCompanyId))
            {
                return null;
            }


            var clientCompany = _uow.ClientCompanyRepository.Single(u => u.ClientCompanyId == clientCompanyId);
            return includeDisabled
                ? clientCompany.Contract.Where(contract => _permissionService.IsGranted(
                    CVScreeningCore.Models.Permission.kContractViewPermission,
                    contract.ContractId)).Select(Mapper.Map<Contract, ClientContractDTO>).ToList()
                : clientCompany.Contract.Where(
                    contract => contract.IsContractEnabled == true && _permissionService.IsGranted(
                        CVScreeningCore.Models.Permission.kContractViewPermission,
                        contract.ContractId))
                    .Select(Mapper.Map<Contract, ClientContractDTO>)
                    .ToList();
        }

        /// <summary>
        /// Enable a client contract
        /// </summary>
        /// <param name="clientContractDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode EnableClientContract(ref ClientContractDTO clientContractDTO)
        {
            try
            {
                var id = clientContractDTO.ContractId;
                if (GetClientContract(id) == null)
                    return ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND;

                var contract = _uow.ClientContractRepository.Single(
                    u => u.ContractId == id);

                contract.IsContractEnabled = true;
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Client contract Id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        clientContractDTO.ContractId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Disable a client contract
        /// </summary>
        /// <param name="clientContractDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode DisableClientContract(ref ClientContractDTO clientContractDTO)
        {
            try
            {
                var id = clientContractDTO.ContractId;
                if (GetClientContract(id) == null)
                    return ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND;

                var contract = _uow.ClientContractRepository.Single(
                    u => u.ContractId == id);

                contract.IsContractEnabled = false;
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Client contract Id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        clientContractDTO.ContractId, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        #endregion

        #region Client screening level

        /// <summary>
        /// Create a screening level and a screening level version equal to version 1
        /// </summary>
        /// <param name="clientContractDTO"></param>
        /// <param name="screeningLevelDTO"></param>
        /// <param name="screeningLevelVersionDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode CreateScreeningLevel(
            ClientContractDTO clientContractDTO,
            ref ScreeningLevelDTO screeningLevelDTO,
            ref ScreeningLevelVersionDTO screeningLevelVersionDTO)
        {
            int? clientContractId = clientContractDTO != null ? clientContractDTO.ContractId : -1;
            var screeningLevelName = screeningLevelDTO.ScreeningLevelName;
            try
            {
                // Company client already existing
                if (!_uow.ClientContractRepository.Exist(
                    u => u.ContractId == clientContractId))
                {
                    return ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND;
                }

                var contract = _uow.ClientContractRepository.Single(
                    u => u.ContractId == clientContractId);

                var screeningOfContract = contract.ScreeningLevel;

                // Screening level name already existing for this company
                if (screeningOfContract.Any(u => u.ScreeningLevelName == screeningLevelName))
                {
                    return ErrorCode.SCREENING_LEVEL_ALREADY_EXISTS;
                }

                var screeningLevel = new ScreeningLevel
                {
                    ScreeningLevelName = screeningLevelDTO.ScreeningLevelName,
                    Contract = contract
                };

                // First version to add in database
                screeningLevelVersionDTO.ScreeningLevelVersionNumber = 1;
                ScreeningLevelVersion screeningLevelVersion;
                if (AddScreeningLevelVersion(
                    ref screeningLevel, screeningLevelVersionDTO, out screeningLevelVersion) != ErrorCode.NO_ERROR)
                {
                    return ErrorCode.UNKNOWN_ERROR;
                }

                screeningLevel.GrantPermissionForAllAccountManagers();
                screeningLevel.GrantPermissionForAllClients();

                screeningOfContract.Add(_uow.ScreeningLevelRepository.Add(screeningLevel));
                _uow.Commit();

                screeningLevelDTO = Mapper.Map<ScreeningLevel, ScreeningLevelDTO>(screeningLevel);
                screeningLevelVersionDTO =
                    Mapper.Map<ScreeningLevelVersion, ScreeningLevelVersionDTO>(screeningLevelVersion);

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format(
                        "Function: {0}. Contract Id: {1}. Screening level: {2}. Screening level version: {3}. Error: {4}",
                        MethodBase.GetCurrentMethod().Name,
                        clientContractDTO.ContractId,
                        screeningLevelDTO,
                        screeningLevelVersionDTO,
                        ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }


        /// <summary>
        /// Edit screening level. Only the screening level fields that does not have any impacts with screening process can be edited
        /// </summary>
        /// <param name="screeningLevelDTO"></param>
        /// <param name="screeningLevelVersionDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode EditScreeningLevel(
            ref ScreeningLevelDTO screeningLevelDTO,
            ref ScreeningLevelVersionDTO screeningLevelVersionDTO)
        {
            int? screeningLevelId = screeningLevelDTO != null ? screeningLevelDTO.ScreeningLevelId : -1;
            int? screeningLevelVersionId = screeningLevelVersionDTO != null
                ? screeningLevelVersionDTO.ScreeningLevelVersionId
                : -1;
            var endDate = screeningLevelVersionDTO.ScreeningLevelVersionEndDate.Date;
            var startDate = screeningLevelVersionDTO.ScreeningLevelVersionStartDate.Date;

            try
            {
                // Screening level not existing
                if (!_uow.ScreeningLevelRepository.Exist(
                    u => u.ScreeningLevelId == screeningLevelId))
                {
                    return ErrorCode.SCREENING_LEVEL_NOT_FOUND;
                }

                // Screening level not existing
                if (!_uow.ScreeningLevelVersionRepository.Exist(
                    u => u.ScreeningLevelVersionId == screeningLevelVersionId))
                {
                    return ErrorCode.SCREENING_LEVEL_VERSION_NOT_FOUND;
                }

                var screeningLevel = _uow.ScreeningLevelRepository.Single(
                    u => u.ScreeningLevelId == screeningLevelId);

                // Check if screening level version does not overlap
                var screeningLevelOverlap = screeningLevel.ScreeningLevelVersion.Where(
                    u => u.ScreeningLevelVersionStartDate < endDate && u.ScreeningLevelVersionEndDate > startDate
                         && u.ScreeningLevelVersionId != screeningLevelVersionId);

                if (screeningLevelOverlap.Any())
                {
                    return ErrorCode.SCREENING_LEVEL_VERSION_OVERLAPPING;
                }

                var screeningLevelVersion = _uow.ScreeningLevelVersionRepository.Single(
                    u => u.ScreeningLevelVersionId == screeningLevelVersionId);

                screeningLevel.ScreeningLevelName =
                    screeningLevelDTO.ScreeningLevelName;
                screeningLevelVersion.ScreeningLevelVersionDescription =
                    screeningLevelVersionDTO.ScreeningLevelVersionDescription;
                screeningLevelVersion.ScreeningLevelVersionStartDate =
                    screeningLevelVersionDTO.ScreeningLevelVersionStartDate.Date;
                screeningLevelVersion.ScreeningLevelVersionEndDate =
                    screeningLevelVersionDTO.ScreeningLevelVersionEndDate.Date;
                screeningLevelVersion.ScreeningLevelVersionTurnaroundTime =
                    screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime;
                screeningLevelVersion.ScreeningLevelVersionLanguage =
                    screeningLevelVersionDTO.ScreeningLevelVersionLanguage;
                screeningLevelVersion.ScreeningLevelVersionAllowedToContactCandidate =
                    screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCandidate;
                screeningLevelVersion.ScreeningLevelVersionAllowedToContactCurrentCompany =
                    screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCurrentCompany;


                _uow.Commit();

                screeningLevelDTO = Mapper.Map<ScreeningLevel, ScreeningLevelDTO>(screeningLevel);
                screeningLevelVersionDTO =
                    Mapper.Map<ScreeningLevelVersion, ScreeningLevelVersionDTO>(screeningLevelVersion);

                return ErrorCode.NO_ERROR;
            }
            catch (Exception)
            {
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Delete a screening level and all the screening level version existing
        /// </summary>
        /// <param name="clientContractDTO"></param>
        /// <param name="screeningLevelDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode DeleteScreeningLevel(ClientContractDTO clientContractDTO,
            ScreeningLevelDTO screeningLevelDTO)
        {
            int? clientContractId = clientContractDTO != null ? clientContractDTO.ContractId : -1;
            int? screeningLevelId = screeningLevelDTO != null ? screeningLevelDTO.ScreeningLevelId : -1;

            try
            {
                // Company client already existing
                if (!_uow.ClientContractRepository.Exist(
                    u => u.ContractId == clientContractId))
                {
                    return ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND;
                }

                // Screening level not existing
                if (!_uow.ScreeningLevelRepository.Exist(
                    u => u.ScreeningLevelId == screeningLevelId))
                {
                    return ErrorCode.SCREENING_LEVEL_NOT_FOUND;
                }

                var contract = _uow.ClientContractRepository.Single(
                    u => u.ContractId == clientContractId);
                var screeningLevel = _uow.ScreeningLevelRepository.Single(
                    u => u.ScreeningLevelId == screeningLevelId);

                foreach (var screeningLevelVersion in screeningLevel.ScreeningLevelVersion.Reverse())
                {
                    foreach (var typeOfCheck in screeningLevelVersion.TypeOfCheckScreeningLevelVersion.Reverse())
                    {
                        screeningLevelVersion.TypeOfCheckScreeningLevelVersion.Remove(typeOfCheck);
                    }
                    screeningLevel.ScreeningLevelVersion.Remove(screeningLevelVersion);
                    _uow.ScreeningLevelVersionRepository.Delete(screeningLevelVersion);
                }
                contract.ScreeningLevel.Remove(screeningLevel);
                _uow.ScreeningLevelRepository.Delete(screeningLevel);
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Contract: {1}. Screening level: {2}. Error: {3}",
                        MethodBase.GetCurrentMethod().Name,
                        clientContractDTO, screeningLevelDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }


        /// <summary>
        /// Update screening level. Used to create a new screening level with a version incremented
        /// </summary>
        /// <param name="screeningLevelDTO"></param>
        /// <param name="newScreeningLevelVersionDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode UpdateScreeningLevel(ref ScreeningLevelDTO screeningLevelDTO,
            ref ScreeningLevelVersionDTO newScreeningLevelVersionDTO)
        {
            int? screeningLevelId = screeningLevelDTO != null ? screeningLevelDTO.ScreeningLevelId : -1;
            var endDate = newScreeningLevelVersionDTO.ScreeningLevelVersionEndDate.Date;
            var startDate = newScreeningLevelVersionDTO.ScreeningLevelVersionStartDate.Date;
            try
            {
                // Screening level not existing
                if (!_uow.ScreeningLevelRepository.Exist(
                    u => u.ScreeningLevelId == screeningLevelId))
                {
                    return ErrorCode.SCREENING_LEVEL_NOT_FOUND;
                }

                var screeningLevel = _uow.ScreeningLevelRepository.Single(
                    u => u.ScreeningLevelId == screeningLevelId);

                // Check if screening level version does not overlap
                var screeningLevelOverlap = screeningLevel.ScreeningLevelVersion.Where(
                    u => u.ScreeningLevelVersionStartDate < endDate && u.ScreeningLevelVersionEndDate > startDate);

                if (screeningLevelOverlap.Any())
                {
                    return ErrorCode.SCREENING_LEVEL_VERSION_OVERLAPPING;
                }

                // Get maximum from version number already existing and increment it
                newScreeningLevelVersionDTO.ScreeningLevelVersionNumber
                    = screeningLevel.ScreeningLevelVersion.Max(u => u.ScreeningLevelVersionNumber) + 1;

                // Create new screening level version
                ScreeningLevelVersion screeningLevelVersion;
                if (AddScreeningLevelVersion(
                    ref screeningLevel, newScreeningLevelVersionDTO, out screeningLevelVersion) != ErrorCode.NO_ERROR)
                {
                    return ErrorCode.UNKNOWN_ERROR;
                }

                _uow.Commit();
                screeningLevelDTO = Mapper.Map<ScreeningLevel, ScreeningLevelDTO>(screeningLevel);
                newScreeningLevelVersionDTO =
                    Mapper.Map<ScreeningLevelVersion, ScreeningLevelVersionDTO>(screeningLevelVersion);

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Screening level: {1}. Screening level version: {2}. Error: {3}",
                        MethodBase.GetCurrentMethod().Name,
                        screeningLevelDTO, newScreeningLevelVersionDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Get screening level by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>        
        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission)]
        public virtual ScreeningLevelDTO GetScreeningLevel(int id)
        {
            try
            {
                // Screening level not existing
                if (!_uow.ScreeningLevelRepository.Exist(u => u.ScreeningLevelId == id))
                {
                    return null;
                }
                var screeningLevel = _uow.ScreeningLevelRepository.Single(
                    u => u.ScreeningLevelId == id);

                return Mapper.Map<ScreeningLevel, ScreeningLevelDTO>(screeningLevel);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Screening level id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name, id, ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Get all the screening levels by contract
        /// </summary>
        /// <param name="clientContractDTO"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kContractViewPermission)]
        public virtual List<ScreeningLevelDTO> GetScreeningLevelsByContract(ClientContractDTO clientContractDTO)
        {
            int? clientContractId = clientContractDTO != null ? clientContractDTO.ContractId : -1;

            try
            {
                // Company client already existing
                if (!_uow.ClientContractRepository.Exist(
                    u => u.ContractId == clientContractId))
                {
                    return null;
                }
                var contract = _uow.ClientContractRepository.Single(
                    u => u.ContractId == clientContractId);

                return contract.ScreeningLevel.Select(Mapper.Map<ScreeningLevel, ScreeningLevelDTO>).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Contract: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name, clientContractDTO, ex.Message));
                return null;
            }
        }


        /// <summary>
        /// Get all the screening level version by screening level
        /// </summary>
        /// <param name="screeningLevelDTO"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningLevelViewPermission)]
        public virtual List<ScreeningLevelVersionDTO> GetScreeningLevelVersionsByScreeningLevel(
            ScreeningLevelDTO screeningLevelDTO)
        {
            int? screeningLevelId = screeningLevelDTO != null ? screeningLevelDTO.ScreeningLevelId : -1;

            try
            {
                // Screening level not existing
                if (!_uow.ScreeningLevelRepository.Exist(u => u.ScreeningLevelId == screeningLevelId))
                {
                    return null;
                }
                var screeningLevel = _uow.ScreeningLevelRepository.Single(
                    u => u.ScreeningLevelId == screeningLevelId);

                return screeningLevel.ScreeningLevelVersion.Select(
                    Mapper.Map<ScreeningLevelVersion, ScreeningLevelVersionDTO>).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Screening level: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name, screeningLevelDTO, ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Add a screening level version to a screening level
        /// </summary>
        /// <param name="screeningLevelDTO">Screening level POCO updated with screening level version</param>
        /// <param name="screeningLevelVersionDTO"></param>
        /// <param name="screeningLevelVersion"></param>
        /// <returns></returns>
        private ErrorCode AddScreeningLevelVersion(
            ref ScreeningLevel screeningLevelDTO,
            ScreeningLevelVersionDTO screeningLevelVersionDTO,
            out ScreeningLevelVersion screeningLevelVersion)
        {
            try
            {
                screeningLevelVersion = new ScreeningLevelVersion
                {
                    ScreeningLevelVersionNumber = screeningLevelVersionDTO.ScreeningLevelVersionNumber,
                    ScreeningLevelVersionDescription = screeningLevelVersionDTO.ScreeningLevelVersionDescription,
                    ScreeningLevelVersionStartDate = screeningLevelVersionDTO.ScreeningLevelVersionStartDate.Date,
                    ScreeningLevelVersionEndDate = screeningLevelVersionDTO.ScreeningLevelVersionEndDate.Date,
                    ScreeningLevelVersionTurnaroundTime = screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime,
                    ScreeningLevelVersionLanguage = screeningLevelVersionDTO.ScreeningLevelVersionLanguage,
                    ScreeningLevelVersionAllowedToContactCandidate = 
                        screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCandidate,
                    ScreeningLevelVersionAllowedToContactCurrentCompany =
                        screeningLevelVersionDTO.ScreeningLevelVersionAllowedToContactCurrentCompany,
                    ScreeningLevel = screeningLevelDTO
                };

                // Add type of check to the screening level version
                foreach (var typeOfCheckDTO in screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion)
                {
                    var typeOfCheckName = typeOfCheckDTO.TypeOfCheck.CheckName;
                    var typeOfCheck = _uow.TypeOfCheckRepository.Single(u => u.CheckName == typeOfCheckName);
                    var typeOfCheckScreeningLevelVersion = new TypeOfCheckScreeningLevelVersion
                    {
                        ScreeningLevelVersion = screeningLevelVersion,
                        TypeOfCheck = typeOfCheck,
                        TypeOfCheckScreeningComments = typeOfCheckDTO.TypeOfCheckScreeningComments ?? ""
                    };
                    screeningLevelVersion.TypeOfCheckScreeningLevelVersion.Add(typeOfCheckScreeningLevelVersion);
                }

                screeningLevelVersion.GrantPermissionForAllAccountManagers();
                screeningLevelVersion.GrantPermissionForAllClients();

                screeningLevelDTO.ScreeningLevelVersion.Add(
                    _uow.ScreeningLevelVersionRepository.Add(screeningLevelVersion));
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                screeningLevelVersion = null;
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Screening level: {1}. Screening level version: {2}. Error: {3}",
                        MethodBase.GetCurrentMethod().Name,
                        screeningLevelDTO,
                        screeningLevelVersionDTO,
                        ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Get screening level version by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RequirePermission(CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission)]
        public virtual ScreeningLevelVersionDTO GetScreeningLevelVersion(int id)
        {
            try
            {
                // Screening level not existing
                if (!_uow.ScreeningLevelVersionRepository.Exist(u => u.ScreeningLevelVersionId == id))
                {
                    return null;
                }

                var screeningLevelVersion = _uow.ScreeningLevelVersionRepository.Single(
                    u => u.ScreeningLevelVersionId == id);

                return Mapper.Map<ScreeningLevelVersion, ScreeningLevelVersionDTO>(screeningLevelVersion);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Screening level version id: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name, id, ex.Message));
                return null;
            }
        }

        #endregion
    }
}