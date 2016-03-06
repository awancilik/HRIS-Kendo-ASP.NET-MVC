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
    public class ClientCompany
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private IClientService _clientService;
        private ICommonService _commonService;
        private IPermissionService _permissionService;
        private IUserManagementService _userManagementService;
        private ISystemTimeService _systemTimeService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private UserProfileDTO _accountManager1;
        private UserProfileDTO _accountManager2;


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
            _accountManager1 = Utilities.BuildAccountSample();

            if (_userManagementService.GetUserProfilebyName(_accountManager1.UserName) == null)
            {
                var error = _userManagementService.CreateUserProfile(ref _accountManager1,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);
            }
            else
            {
                _accountManager1 = _userManagementService.GetUserProfilebyName(_accountManager1.UserName);
            }

            _permissionService = new PermissionService(_unitOfWork, _accountManager1.UserName);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);

            // Create sample user account for testing
            _accountManager2 = Utilities.BuildAccountSample();
            _accountManager2.UserName = "myaccountmanager@test.com";

            if (_userManagementService.GetUserProfilebyName(_accountManager2.UserName) == null)
            {
                var error = _userManagementService.CreateUserProfile(ref _accountManager2,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456",
                    false);
            }
            else
            {
                _accountManager2 = _userManagementService.GetUserProfilebyName(_accountManager2.UserName);
            }
        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            var clientCompanies = _unitOfWork.ClientCompanyRepository.GetAll();
            foreach (var clientCompany in clientCompanies.Reverse())
            {
                _clientService.DeleteClientCompany(clientCompany.ClientCompanyId);
            }
        }


        /// <summary>
        // Test to create a client company
        /// </summary>
        [Test]
        public void CreateClientCompany()
        {
            var count = _unitOfWork.ClientCompanyRepository.CountAll();

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
                WebSite = "www.mycompany.com",
            };

            // Public holiday from today to today + 1
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company",
                Category = "IT",
                Description = "My description",
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                AccountManagers = new List<UserProfileDTO>
                    {
                        _accountManager1, _accountManager2
                    }
                    
            };

            var error = _clientService.CreateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());
        }


        /// <summary>
        // Test to create a client company already existing
        /// </summary>
        [Test]
        public void CreateClientCompanyAlreadyExisting()
        {
            var count = _unitOfWork.ClientCompanyRepository.CountAll();

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
                WebSite = "www.mycompany.com",
            };


            // Public holiday from today to today + 1
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company already existing",
                Category = "IT",
                Description = "My description",
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                AccountManagers = new List<UserProfileDTO> { _accountManager1 }
            };

            var error = _clientService.CreateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());

            error = _clientService.CreateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_ALREADY_EXISTS, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());
        }



        /// <summary>
        // Test to edit a client company
        /// </summary>
        [Test]
        public void EditClientCompany()
        {
            var count = _unitOfWork.ClientCompanyRepository.CountAll();

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
                WebSite = "www.mycompany.com",
            };

            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company",
                Category = "IT",
                Description = "My description",
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                AccountManagers = new List<UserProfileDTO>
                    {
                        _accountManager1, _accountManager2
                    }

            };

            var error = _clientService.CreateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());

            var clientCompanyEditDTO = _clientService.GetClientCompany(clientCompanyDTO.ClientCompanyId);
            clientCompanyEditDTO.Address.Street = "Jalan Cipete, 4, Edit";
            clientCompanyEditDTO.Address.PostalCode = "12800";
            clientCompanyEditDTO.Address.Location.LocationId = 13;
            clientCompanyEditDTO.ContactInfo.WebSite = "www.mycompanyedit.com";
            clientCompanyEditDTO.ClientCompanyName = "My client company edit";
            clientCompanyEditDTO.Category = "Telecom edit";
            clientCompanyEditDTO.Description = "My description edit";

            clientCompanyEditDTO.AccountManagers = new List<UserProfileDTO>
            {
                _accountManager2
            };

            error = _clientService.EditClientCompany(ref clientCompanyEditDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());


            var clientCompanyGetDTO = _clientService.GetClientCompany(clientCompanyEditDTO.ClientCompanyId);
            Assert.AreEqual(clientCompanyGetDTO.ClientCompanyId, clientCompanyEditDTO.ClientCompanyId);
            Assert.AreEqual(clientCompanyGetDTO.Description, "My description edit");
            Assert.AreEqual(clientCompanyGetDTO.ClientCompanyName, "My client company edit");
            Assert.AreEqual(clientCompanyGetDTO.ClientCompanyIsDeactivated, false);
            Assert.AreEqual(clientCompanyGetDTO.ContactInfo.WebSite, "www.mycompanyedit.com");
            Assert.AreEqual(clientCompanyGetDTO.Address.Street, "Jalan Cipete, 4, Edit");
            Assert.AreEqual(clientCompanyGetDTO.Address.PostalCode, "12800");
            Assert.AreEqual(clientCompanyGetDTO.Address.Location.LocationId, 13);
            Assert.AreEqual(
                clientCompanyGetDTO.AccountManagers.ElementAt(0).UserName, _accountManager2.UserName);

        }

        /// <summary>
        // Test to edit a client company not existing
        /// </summary>
        [Test]
        public void EditClientCompanyNotFound()
        {
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyId = 10
            };
            var error = _clientService.EditClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_NOT_FOUND, error);
        }

        /// <summary>
        // Test to create a client company
        /// </summary>
        [Test]
        public void DetailsClientCompany()
        {
            
            var count = _unitOfWork.ClientCompanyRepository.CountAll();

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
                WebSite = "www.mycompany.com",
            };

            // Public holiday from today to today + 1
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company",
                Category = "IT",
                Description = "My description",
                Address = addressDTO,
                ClientCompanyIsDeactivated = false,
                ContactInfo = contactInfoDTO,
                AccountManagers = new List<UserProfileDTO>
                    {
                        _accountManager1,
                        _accountManager2
                    }

            };

            var error = _clientService.CreateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());

            var clientCompanyGetDTO = _clientService.GetClientCompany(clientCompanyDTO.ClientCompanyId);
            Assert.AreEqual(clientCompanyGetDTO.ClientCompanyId, clientCompanyDTO.ClientCompanyId);
            Assert.AreEqual(clientCompanyGetDTO.ClientCompanyName, "My client company");
            Assert.AreEqual(clientCompanyGetDTO.Description, "My description");
            Assert.AreEqual(clientCompanyGetDTO.ClientCompanyIsDeactivated, false);
            Assert.AreEqual(clientCompanyGetDTO.ContactInfo.WebSite, "www.mycompany.com");
            Assert.AreEqual(clientCompanyGetDTO.Address.Street, "Jalan Cipete, 4");
            Assert.AreEqual(clientCompanyGetDTO.Address.PostalCode, "12780");
            Assert.AreEqual(clientCompanyGetDTO.Address.Location.LocationId, 14);
            Assert.AreEqual(
                clientCompanyGetDTO.AccountManagers.ElementAt(0).UserName, _accountManager1.UserName);
            Assert.AreEqual(
                clientCompanyGetDTO.AccountManagers.ElementAt(1).UserName, _accountManager2.UserName);
        }



        /// <summary>
        // Test to get a client company not existing
        /// </summary>
        [Test]
        public void DetailsClientCompanyNotFound()
        {
            var clientCompany = _clientService.GetClientCompany(1);
            Assert.AreEqual(clientCompany, null);
        }


        /// <summary>
        // Test to deactivate a client company
        /// </summary>
        [Test]
        public void DeactivateClientCompany()
        {
            var count = _unitOfWork.ClientCompanyRepository.CountAll();

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
                WebSite = "www.mycompany.com",
            };

            // Public holiday from today to today + 1
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company",
                Category = "IT",
                Description = "My description",
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                AccountManagers = new List<UserProfileDTO>
                    {
                        _accountManager1, _accountManager2
                    }

            };

            var error = _clientService.CreateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());
            Assert.AreEqual(false, clientCompanyDTO.ClientCompanyIsDeactivated);


            error = _clientService.DeactivateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            
            clientCompanyDTO = _clientService.GetClientCompany(clientCompanyDTO.ClientCompanyId);
            Assert.AreEqual(true, clientCompanyDTO.ClientCompanyIsDeactivated);
        }

        /// <summary>
        // Test to deactivate a client company not existing
        /// </summary>
        [Test]
        public void DeactivateClientCompanyNotFound()
        {
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyId = 10
            };
            var error = _clientService.DeactivateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_NOT_FOUND, error);
        }



        /// <summary>
        // Test to reactivate a client company
        /// </summary>
        [Test]
        public void ReactivateClientCompany()
        {
            var count = _unitOfWork.ClientCompanyRepository.CountAll();

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
                WebSite = "www.mycompany.com",
            };

            // Public holiday from today to today + 1
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company",
                Category = "IT",
                Description = "My description",
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                AccountManagers = new List<UserProfileDTO>
                    {
                        _accountManager1, _accountManager2
                    }

            };

            var error = _clientService.CreateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());
            Assert.AreEqual(false, clientCompanyDTO.ClientCompanyIsDeactivated);


            error = _clientService.DeactivateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            clientCompanyDTO = _clientService.GetClientCompany(clientCompanyDTO.ClientCompanyId);
            Assert.AreEqual(true, clientCompanyDTO.ClientCompanyIsDeactivated);

            error = _clientService.ReactivateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            clientCompanyDTO = _clientService.GetClientCompany(clientCompanyDTO.ClientCompanyId);
            Assert.AreEqual(false, clientCompanyDTO.ClientCompanyIsDeactivated);
        }

        /// <summary>
        // Test to reactivate a client company not existing
        /// </summary>
        [Test]
        public void ReactivateClientCompanyNotFound()
        {
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyId = 10
            };
            var error = _clientService.DeactivateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_NOT_FOUND, error);
        }


        /// <summary>
        // Test to delete a client company
        /// </summary>
        [Test]
        public void DeleteClientCompany()
        {
            var count = _unitOfWork.ClientCompanyRepository.CountAll();

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
                WebSite = "www.mycompany.com",
            };

            // Public holiday from today to today + 1
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company",
                Category = "IT",
                Description = "My description",
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                AccountManagers = new List<UserProfileDTO>
                    {
                        _accountManager1, _accountManager2
                    }

            };

            var error = _clientService.CreateClientCompany(ref clientCompanyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());

            error = _clientService.DeleteClientCompany(clientCompanyDTO.ClientCompanyId);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count, _unitOfWork.ClientCompanyRepository.CountAll());
        }

        /// <summary>
        // Test to delete a client company not existing
        /// </summary>
        [Test]
        public void DeleteClientCompanyNotFound()
        {
            var error = _clientService.DeleteClientCompany(10);
            Assert.AreEqual(ErrorCode.CLIENT_COMPANY_NOT_FOUND, error);
        }


        /// <summary>
        // Test to get all client company
        /// </summary>
        [Test]
        public void GetClientCompanies()
        {
            var count = _unitOfWork.ClientCompanyRepository.CountAll();

            var addressDTO1 = new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 14
                }
            };

            var contactInfoDTO1 = new ContactInfoDTO
            {
                WebSite = "www.mycompany.com",
            };

            var clientCompanyDTO1 = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company",
                Category = "IT",
                Description = "My description",
                Address = addressDTO1,
                ContactInfo = contactInfoDTO1,
                AccountManagers = new List<UserProfileDTO>
                    {
                        _accountManager1, _accountManager2
                    }

            };

            var error = _clientService.CreateClientCompany(ref clientCompanyDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 1, _unitOfWork.ClientCompanyRepository.CountAll());

            var addressDTO2 = new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 14
                }
            };

            var contactInfoDTO2 = new ContactInfoDTO
            {
                WebSite = "www.mycompany.com",
            };

            var clientCompanyDTO2 = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company 2",
                Category = "IT",
                Description = "My description",
                Address = addressDTO2,
                ContactInfo = contactInfoDTO2,
                AccountManagers = new List<UserProfileDTO>
                    {
                        _accountManager1, _accountManager2
                    }

            };

            error = _clientService.CreateClientCompany(ref clientCompanyDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(count + 2, _unitOfWork.ClientCompanyRepository.CountAll());

            var companies = _clientService.GetAllClientCompanies();
            Assert.AreEqual(2, companies.Count);
        }


        // 4. Runs Twice; Once after Test Case 1 and Once After Test Case 2
        // Dispose Objects Used in Each Test which are no longer required
        [TearDown]
        public void RunOnceAfterEachTests()
        {
            var clientCompanies = _unitOfWork.ClientCompanyRepository.GetAll();
            foreach (var clientCompany in clientCompanies.Reverse())
            {
                _clientService.DeleteClientCompany(clientCompany.ClientCompanyId);
            }
        }

        // 5. Runs Once After All of The Aformentioned Methods
        // Dispose all Mocks and Global Objects
        [TestFixtureTearDown]
        public void RunOnceAfterAll()
        {

        }



    }
}

