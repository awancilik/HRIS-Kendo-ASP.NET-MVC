using System.Linq;
using CVScreeningCore.Error;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using NUnit.Framework;
using System.Collections.Generic;

namespace CVScreeningService.Tests.UnitTest.Client
{
    [TestFixture]
    public class ClientContract
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private IClientService _clientService;
        private IPermissionService _permissionService;
        private ICommonService _commonService;
        private IUserManagementService _userManagementService;
        private ISystemTimeService _systemTimeService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private UserProfileDTO _accountManager;
        private ClientCompanyDTO _clientCompany;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _systemTimeService = new SystemTimeService();
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, _systemTimeService);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);

            // Create sample user account for testing
            _accountManager = Utilities.BuildAccountSample();

            var error = _userManagementService.CreateUserProfile(ref _accountManager,
                    new List<string>(new List<string> { "Administrator" }),
                    "123456", false);

            _permissionService = new PermissionService(_unitOfWork, _accountManager.UserName);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);

            _clientCompany = Utilities.BuildClientCompanySample();
            _clientCompany.AccountManagers = new List<UserProfileDTO> { _accountManager };
            _clientService.CreateClientCompany(ref _clientCompany);
        }

        private ClientContractDTO BuildContractDTO()
        {
            var contractDTO = new ClientContractDTO
            {
                ContractReference = "INT-BC",
                ContractDescription = "Contract description",
                ContractYear = "2014",
                IsContractEnabled = true
            };
            return contractDTO;
        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            IList<ClientContractDTO> contracts = _clientService.GetAllClientContractsByCompany(_clientCompany);
            foreach (var contract in contracts.Reverse())
            {
                _clientService.DeleteClientContract(_clientCompany, contract);
            }
        }

        /// <summary>
        // Test to create a client company contract 
        /// </summary>
        [Test]
        public void CreateClientContractPerson()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();

            var clientContractDTO = BuildContractDTO();

            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());
        }


        /// <summary>
        // Test to create a client company contract already existing
        /// </summary>
        [Test]
        public void CreateClientContractAlreadyExisting()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();

            var clientContractDTO = BuildContractDTO();

            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientContractRepository.CountAll());

            error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTRACT_ALREADY_EXISTS, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientContractRepository.CountAll());
        }

        /// <summary>
        // Test to create a client company contract not existing
        /// </summary>
        [Test]
        public void CreateClientContractCompanyNotFound()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();
            var clientContractDTO = BuildContractDTO();
            var error = _clientService.CreateClientContract(new ClientCompanyDTO { ClientCompanyId = 10 }, ref clientContractDTO);

            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_NOT_FOUND, error);
            Assert.AreEqual(count, _unitOfWork.ClientContractRepository.CountAll());
        }


        /// <summary>
        // Test to edit a client company contract
        /// </summary>
        [Test]
        public void EditClientContract()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();

            var clientContractDTO = BuildContractDTO();
            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientContractRepository.CountAll());


            var clientContractEditDTO = BuildContractDTO();
            clientContractEditDTO.ContractId = clientContractDTO.ContractId;
            clientContractEditDTO.ContractDescription = "My description edit";
            clientContractEditDTO.ContractReference = "INT-ZZZ";
            clientContractEditDTO.ContractYear = "2015";


            error = _clientService.EditClientContract(ref clientContractEditDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var clientContractGetDTO = _clientService.GetClientContract(clientContractEditDTO.ContractId);

            Assert.AreEqual("My description edit", clientContractGetDTO.ContractDescription);
            Assert.AreEqual("INT-ZZZ", clientContractGetDTO.ContractReference);
            Assert.AreEqual("2015", clientContractGetDTO.ContractYear);

        }

        /// <summary>
        // Test to edit a client company contract not existing
        /// </summary>
        [Test]
        public void EditClientContractNotFound()
        {
            var clientContractDTO = new ClientContractDTO { ContractId = 20 };
            var error = _clientService.EditClientContract(ref clientContractDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND, error);
        }



        /// <summary>
        // Test to create a client company contract
        /// </summary>
        [Test]
        public void DetailsClientContract()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();
            var clientContractExpecedDTO = BuildContractDTO();

            var clientContractDTO = BuildContractDTO();
            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientContractRepository.CountAll());

            var clientContractGetDTO = _clientService.GetClientContract(clientContractDTO.ContractId);
            Assert.AreEqual(clientContractExpecedDTO.ContractReference, clientContractGetDTO.ContractReference);
            Assert.AreEqual(clientContractExpecedDTO.ContractYear, clientContractGetDTO.ContractYear);
            Assert.AreEqual(clientContractExpecedDTO.ContractDescription, clientContractGetDTO.ContractDescription);
            Assert.AreEqual(clientContractExpecedDTO.IsContractEnabled, clientContractGetDTO.IsContractEnabled);
        }



        /// <summary>
        // Test to get a client company contract not existing
        /// </summary>
        [Test]
        public void DetailsClientContractNotFound()
        {
            var clientContractDTO = _clientService.GetClientContract(20);
            Assert.AreEqual(clientContractDTO, null);
        }


        /// <summary> 
        // Test to delete a client company contract
        /// </summary>
        [Test]
        public void DeleteClientContract()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();

            var clientContractDTO = BuildContractDTO();

            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());

            error = _clientService.DeleteClientContract(_clientCompany, clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count, _unitOfWork.ClientContractRepository.CountAll());

        }

        /// <summary>
        // Test to delete a client company contract not existing
        /// </summary>
        [Test]
        public void DeleteClientContractNotFound()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();
            var clientContractDTO = BuildContractDTO();

            var error = _clientService.DeleteClientContract(new ClientCompanyDTO { ClientCompanyId = 10 }, clientContractDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_NOT_FOUND, error);

            error = _clientService.DeleteClientContract(_clientCompany, new ClientContractDTO { ContractId = 10 });
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND, error);

            Assert.AreEqual(count, _unitOfWork.ClientContractRepository.CountAll());
        }



        /// <summary>
        // Test to disable a client contract
        /// </summary>
        [Test]
        public void DisableClientContract()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();

            var clientContractDTO = BuildContractDTO();
            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO);

            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientContractRepository.CountAll());
            Assert.AreEqual(true, clientContractDTO.IsContractEnabled);

            error = _clientService.DisableClientContract(ref clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            clientContractDTO = _clientService.GetClientContract(clientContractDTO.ContractId);
            Assert.AreEqual(false, clientContractDTO.IsContractEnabled);
        }

        /// <summary>
        // Test to disable a client contract not existing
        /// </summary>
        [Test]
        public void DisableClientContractNotFound()
        {
            var clientContractDTO = new ClientContractDTO
            {
                ContractId = 10
            };
            var error = _clientService.DisableClientContract(ref clientContractDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND, error);
        }



        /// <summary>
        // Test to enable a client contract
        /// </summary>
        [Test]
        public void EnableClientContract()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();

            var clientContractDTO = BuildContractDTO();
            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO);

            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientContractRepository.CountAll());
            Assert.AreEqual(true, clientContractDTO.IsContractEnabled);

            error = _clientService.DisableClientContract(ref clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            clientContractDTO = _clientService.GetClientContract(clientContractDTO.ContractId);
            Assert.AreEqual(false, clientContractDTO.IsContractEnabled);

            error = _clientService.EnableClientContract(ref clientContractDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            clientContractDTO = _clientService.GetClientContract(clientContractDTO.ContractId);
            Assert.AreEqual(true, clientContractDTO.IsContractEnabled);
        }

        /// <summary>
        // Test to enable a contract not existing
        /// </summary>
        [Test]
        public void EnableClientContractNotFound()
        {
            var clientContractDTO = new ClientContractDTO
            {
                ContractId = 10
            };
            var error = _clientService.EnableClientContract(ref clientContractDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTRACT_NOT_FOUND, error);
        }


        /// <summary>
        // Test to get all client company contract
        /// </summary>
        [Test]
        public void GetAllClientContract()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();
            var clientContractDTO1 = BuildContractDTO();
            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientContractRepository.CountAll());

            var clientContractDTO2 = BuildContractDTO();
            clientContractDTO2.ContractReference = "INT-XXX";
            error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 2, _unitOfWork.ClientContractRepository.CountAll());

            var clientContracts = _clientService.GetAllClientContractsByCompany(_clientCompany);
            Assert.AreEqual(clientContracts.Count, 2);
            Assert.AreEqual(clientContractDTO1.ContractReference, clientContracts.ElementAt(0).ContractReference);
            Assert.AreEqual(clientContractDTO2.ContractReference, clientContracts.ElementAt(1).ContractReference);

        }


        /// <summary>
        // Test to get all client company contact that are only enable
        /// </summary>
        [Test]
        public void GetAllClientContractOnlyEnable()
        {
            var count = _unitOfWork.ClientContractRepository.CountAll();
            var clientContractDTO1 = BuildContractDTO();
            var error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientContractRepository.CountAll());

            var clientContractDTO2 = BuildContractDTO();
            clientContractDTO2.ContractReference = "INT-XXX";
            error = _clientService.CreateClientContract(_clientCompany, ref clientContractDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 2, _unitOfWork.ClientContractRepository.CountAll());

            error = _clientService.DisableClientContract(ref clientContractDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var clientContracts = _clientService.GetAllClientContractsByCompany(_clientCompany);
            Assert.AreEqual(clientContracts.Count, 2);
            Assert.AreEqual(clientContractDTO1.ContractReference, clientContracts.ElementAt(0).ContractReference);
            Assert.AreEqual(clientContractDTO2.ContractReference, clientContracts.ElementAt(1).ContractReference);

            clientContracts = _clientService.GetAllClientContractsByCompany(_clientCompany, false);
            Assert.AreEqual(clientContracts.Count, 1);
            Assert.AreEqual(clientContractDTO2.ContractReference, clientContracts.ElementAt(0).ContractReference);

            error = _clientService.DisableClientContract(ref clientContractDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            clientContracts = _clientService.GetAllClientContractsByCompany(_clientCompany,false);
            Assert.AreEqual(clientContracts.Count, 0);

        }



        /// <summary>
        // Test to get all client company contact for a company not existing
        /// </summary>
        [Test]
        public void GetAllClientContactPersonNotExisting()
        {
            var clientContracts = _clientService.GetAllClientContractsByCompany(new ClientCompanyDTO { ClientCompanyId = 20 });
            Assert.AreEqual(clientContracts, null);
        }


        // 4. Runs Twice; Once after Test Case 1 and Once After Test Case 2
        // Dispose Objects Used in Each Test which are no longer required
        [TearDown]
        public void RunOnceAfterEachTests()
        {

        }

        // 5. Runs Once After All of The Aformentioned Methods
        // Dispose all Mocks and Global Objects
        [TestFixtureTearDown]
        public void RunOnceAfterAll()
        {

        }



    }
}

