using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using NUnit.Framework;
using System.Linq;



namespace CVScreeningService.Tests.UnitTest.Client
{
    [TestFixture]
    public class ClientContactPerson
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private IClientService _clientService;
        private ICommonService _commonService;
        private IPermissionService _permissionService;
        private IUserManagementService _userManagementService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private ISystemTimeService _systemTimeService;
        private UserProfileDTO _accountManager;
        private ClientCompanyDTO  _clientCompany;


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
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);

            _permissionService = new PermissionService(_unitOfWork, _accountManager.UserName);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);

            _clientCompany = Utilities.BuildClientCompanySample();
            _clientCompany.AccountManagers = new List<UserProfileDTO> {_accountManager};
            _clientService.CreateClientCompany(ref _clientCompany);
        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            IList<ContactPersonDTO> contacts = _clientService.GetAllClientContactPersonsByCompany(_clientCompany);
            foreach (var contact in contacts.Reverse())
            {
                _clientService.DeleteClientContactPerson(_clientCompany, contact);
            }
        }

        private ContactPersonDTO BuildContactPersonDTO()
        {
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 14
                }
            };

            var contactInfoDTO = new ContactInfoDTO
            {
                MobilePhoneNumber = "622199988877",
                WorkPhoneNumber = "622199988878",

            };

            var clientContactPersonDTO = new ContactPersonDTO
            {
                ContactPersonName = "My contact",
                Address = addressDTO,
                ContactComments = "My comments",
                ContactInfo = contactInfoDTO
            };
            return clientContactPersonDTO;
        }

        /// <summary>
        // Test to create a client company contact 
        /// </summary>
        [Test]
        public void CreateClientContactPerson()
        {
            var count = _unitOfWork.ContactPersonRepository.CountAll();

            var clientContactPersonDTO = BuildContactPersonDTO();

            var error = _clientService.CreateClientContactPerson(_clientCompany, ref clientContactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());

        }


        /// <summary>
        // Test to create a client company contact already existing
        /// </summary>
        [Test]
        public void CreateClientContactPersonAlreadyExisting()
        {
            var count = _unitOfWork.ContactPersonRepository.CountAll();

            var clientContactPersonDTO = BuildContactPersonDTO();

            var error = _clientService.CreateClientContactPerson(_clientCompany, ref clientContactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());

            error = _clientService.CreateClientContactPerson(_clientCompany, ref clientContactPersonDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTACT_PERSON_ALREADY_EXISTS, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());
        }

        /// <summary>
        // Test to create a client company contact not existing
        /// </summary>
        [Test]
        public void CreateClientContactPersonCompanyNotFound()
        {
            var count = _unitOfWork.ContactPersonRepository.CountAll();
            var clientContactPersonDTO = BuildContactPersonDTO();
            var error = _clientService.CreateClientContactPerson(new ClientCompanyDTO {ClientCompanyId = 10}, ref clientContactPersonDTO);

            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_NOT_FOUND, error);
            Assert.AreEqual(count , _unitOfWork.ContactPersonRepository.CountAll());

        }



        /// <summary>
        // Test to edit a client company contact
        /// </summary>
        [Test]
        public void EditClientContactPerson()
        {
            var count = _unitOfWork.ContactPersonRepository.CountAll();

            var clientContactPersonDTO = BuildContactPersonDTO();
            var error = _clientService.CreateClientContactPerson(_clientCompany, ref clientContactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());


            var clientContactPersonEditDTO = BuildContactPersonDTO();
            clientContactPersonEditDTO.ContactPersonId = clientContactPersonDTO.ContactPersonId;
            clientContactPersonEditDTO.Address.Street = "Jalan Cipete, 4 Edit";
            clientContactPersonEditDTO.Address.PostalCode = "12880";
            clientContactPersonEditDTO.Address.Location.LocationId = 13;
            clientContactPersonEditDTO.ContactInfo.MobilePhoneNumber = "6221998838383";
            clientContactPersonEditDTO.ContactInfo.HomePhoneNumber = "6221999030303";
            clientContactPersonEditDTO.ContactPersonName = "My contact edit";
            clientContactPersonEditDTO.ContactComments = "My comments edit";

            error = _clientService.EditClientContactPerson(ref clientContactPersonEditDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            var clientContactPersonGetDTO = _clientService.GetClientContactPerson(clientContactPersonEditDTO.ContactPersonId);

            Assert.AreEqual("My contact edit", clientContactPersonGetDTO.ContactPersonName);
            Assert.AreEqual("My comments edit", clientContactPersonGetDTO.ContactComments);
            Assert.AreEqual("6221998838383", clientContactPersonGetDTO.ContactInfo.MobilePhoneNumber);
            Assert.AreEqual("6221999030303", clientContactPersonGetDTO.ContactInfo.HomePhoneNumber);
            Assert.AreEqual(13, clientContactPersonGetDTO.Address.Location.LocationId);
            Assert.AreEqual("12880", clientContactPersonGetDTO.Address.PostalCode);
            Assert.AreEqual("Jalan Cipete, 4 Edit", clientContactPersonGetDTO.Address.Street);


        }

        /// <summary>
        // Test to edit a client company contact not existing
        /// </summary>
        [Test]
        public void EditClientContactPersonNotFound()
        {
            var contactPersonDTO = new ContactPersonDTO {ContactPersonId = 20};
            var error = _clientService.EditClientContactPerson(ref contactPersonDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTACT_PERSON_NOT_FOUND, error);
        }

        /// <summary>
        // Test to create a client company contact
        /// </summary>
        [Test]
        public void DetailsClientContactPerson()
        {
            var count = _unitOfWork.ContactPersonRepository.CountAll();
            var clientContactPersonExpectedDTO = BuildContactPersonDTO();

            var clientContactPersonDTO = BuildContactPersonDTO();
            var error = _clientService.CreateClientContactPerson(_clientCompany, ref clientContactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());

            var clientContactPersonGetDTO = _clientService.GetClientContactPerson(clientContactPersonDTO.ContactPersonId);
            Assert.AreEqual(clientContactPersonExpectedDTO.ContactPersonName, clientContactPersonGetDTO.ContactPersonName);
            Assert.AreEqual(clientContactPersonExpectedDTO.ContactComments, clientContactPersonGetDTO.ContactComments);
            Assert.AreEqual(clientContactPersonExpectedDTO.ContactInfo.MobilePhoneNumber, clientContactPersonGetDTO.ContactInfo.MobilePhoneNumber);
            Assert.AreEqual(clientContactPersonExpectedDTO.ContactInfo.HomePhoneNumber, clientContactPersonGetDTO.ContactInfo.HomePhoneNumber);
            Assert.AreEqual(clientContactPersonExpectedDTO.Address.Location.LocationId, clientContactPersonGetDTO.Address.Location.LocationId);
        }



        /// <summary>
        // Test to get a client company contact not existing
        /// </summary>
        [Test]
        public void DetailsClientContactPersonNotFound()
        {
            var clientContactPersonGetDTO = _clientService.GetClientContactPerson(20);
            Assert.AreEqual(clientContactPersonGetDTO, null);
        }


 

        /// <summary> 
        // Test to delete a client company contact
        /// </summary>
        [Test]
        public void DeleteClientContactPerson()
        {
            var count = _unitOfWork.ContactPersonRepository.CountAll();

            var clientContactPersonDTO = BuildContactPersonDTO();

            var error = _clientService.CreateClientContactPerson(_clientCompany, ref clientContactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());

            error = _clientService.DeleteClientContactPerson(_clientCompany, clientContactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count, _unitOfWork.ContactPersonRepository.CountAll());

        }

        /// <summary>
        // Test to delete a client company contact not existing
        /// </summary>
        [Test]
        public void DeleteClientContactPersonNotFound()
        {
            var count = _unitOfWork.ContactPersonRepository.CountAll();
            var clientContactPersonDTO = BuildContactPersonDTO();

            var error = _clientService.DeleteClientContactPerson(new ClientCompanyDTO{ClientCompanyId = 10}, clientContactPersonDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_NOT_FOUND, error);

            error = _clientService.DeleteClientContactPerson(_clientCompany, new ContactPersonDTO{ContactPersonId = 10});
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_CONTACT_PERSON_NOT_FOUND, error);

            Assert.AreEqual(count, _unitOfWork.ContactPersonRepository.CountAll());
        }


        /// <summary>
        // Test to get all client company contact
        /// </summary>
        [Test]
        public void GetAllClientContactPerson()
        {
            var count = _unitOfWork.ContactPersonRepository.CountAll();
            var clientContactPersonDTO1 = BuildContactPersonDTO();
            var error = _clientService.CreateClientContactPerson(_clientCompany, ref clientContactPersonDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ContactPersonRepository.CountAll());

            var clientContactPersonDTO2 = BuildContactPersonDTO();
            clientContactPersonDTO2.ContactPersonName = "My name 2";
            error = _clientService.CreateClientContactPerson(_clientCompany, ref clientContactPersonDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 2, _unitOfWork.ContactPersonRepository.CountAll());

            var clientContacts = _clientService.GetAllClientContactPersonsByCompany(_clientCompany);
            Assert.AreEqual(clientContacts.Count, 2);
            Assert.AreEqual(clientContactPersonDTO1.ContactPersonName, clientContacts.ElementAt(0).ContactPersonName);
            Assert.AreEqual(clientContactPersonDTO2.ContactPersonName, clientContacts.ElementAt(1).ContactPersonName);

        }


        /// <summary>
        // Test to get all client company contact for a company not existing
        /// </summary>
        [Test]
        public void GetAllClientContactPersonNotExisting()
        {
            var clientContacts = _clientService.GetAllClientContactPersonsByCompany(new ClientCompanyDTO{ClientCompanyId = 20});
            Assert.AreEqual(clientContacts, null);
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

