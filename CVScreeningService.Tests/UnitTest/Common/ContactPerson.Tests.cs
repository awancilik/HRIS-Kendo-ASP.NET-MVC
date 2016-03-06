using System.Linq;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Common;
using CVScreeningService.Services.Common;
using NUnit.Framework;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.Services.ErrorHandling;

namespace CVScreeningService.Tests.UnitTest.Common
{
    [TestFixture]
    public class ContactPerson
    {
        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IErrorMessageFactoryService _errorMessageFactoryService;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());
            Utilities.InitLocations(_commonService);
        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            var persons = _unitOfWork.ContactPersonRepository.GetAll();
            foreach (var person in persons.Reverse())
            {
                _unitOfWork.ContactPersonRepository.Delete(person);
            }
        }

        /// <summary>
        // Test to create a contact person
        /// </summary>
        [Test]
        public void CreateContactPerson()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988877",
                WorkPhoneNumber = "622199988878",
                MobilePhoneNumber = "622199988879",
                WebSite = "www.mysite.com",
                OfficialEmail = "my@email.com"
            };

            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 1
                }
            };

            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact"
            };
            
            var error = _commonService.CreateContactPerson(ref contactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.ContactPersonRepository.CountAll());
        }

        /// <summary>
        /// Test to get a contact person
        /// </summary>
        [Test]
        public void GetContactPerson()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999",
                MobilePhoneNumber = "622199988777",
                WebSite = "www.mysitetoretrieve.com",
                OfficialEmail = "my@emailtoretrieve.com"
            };

            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 7
                }
            };

            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact to retrieve"
            };

            var error = _commonService.CreateContactPerson(ref contactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.ContactPersonRepository.CountAll());

            var contactPersonGetDTO = _commonService.GetContactPerson(contactPersonDTO.ContactPersonId);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(contactPersonDTO.ContactPersonName, contactPersonGetDTO.ContactPersonName);
            Assert.AreEqual(contactPersonDTO.Address.Street, contactPersonGetDTO.Address.Street);
            Assert.AreEqual(contactPersonDTO.Address.PostalCode, contactPersonGetDTO.Address.PostalCode);
            Assert.AreEqual(contactPersonDTO.Address.Location.LocationId, contactPersonGetDTO.Address.Location.LocationId);
            Assert.AreEqual(contactPersonDTO.ContactInfo.HomePhoneNumber, contactPersonGetDTO.ContactInfo.HomePhoneNumber);
            Assert.AreEqual(contactPersonDTO.ContactInfo.MobilePhoneNumber, contactPersonGetDTO.ContactInfo.MobilePhoneNumber);
            Assert.AreEqual(contactPersonDTO.ContactInfo.WorkPhoneNumber, contactPersonGetDTO.ContactInfo.WorkPhoneNumber);
            Assert.AreEqual(contactPersonDTO.ContactInfo.OfficialEmail, contactPersonGetDTO.ContactInfo.OfficialEmail);
            Assert.AreEqual(contactPersonDTO.ContactInfo.WebSite, contactPersonGetDTO.ContactInfo.WebSite);
        }

        /// <summary>
        // Test to get a contact person not existing
        /// </summary>
        [Test]
        public void GetContactPersonNotFound()
        {
            var person = _commonService.GetContactPerson(10);
            Assert.AreEqual(person, null);
        }


        /// <summary>
        // Test to get all contact persons existing
        /// </summary>
        [Test]
        public void GetAllContactPersons()
        {
            var contactInfoDTO1 = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988877",
                WorkPhoneNumber = "622199988878",
                MobilePhoneNumber = "622199988879",
                WebSite = "www.mysite.com",
                OfficialEmail = "my@email.com"
            };

            var addressDTO1 = new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 7
                }
            };

            var contactPersonDTO1 = new ContactPersonDTO
            {
                Address = addressDTO1,
                ContactInfo = contactInfoDTO1,
                ContactPersonName = "My contact"
            };

            var error = _commonService.CreateContactPerson(ref contactPersonDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.ContactPersonRepository.CountAll());

            var contactInfoDTO2 = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999",
                MobilePhoneNumber = "622199988777",
                WebSite = "www.mysitetoretrieve.com",
                OfficialEmail = "my@emailtoretrieve.com"
            };

            var addressDTO2 = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 7
                }
            };

            var contactPersonDTO2 = new ContactPersonDTO
            {
                Address = addressDTO2,
                ContactInfo = contactInfoDTO2,
                ContactPersonName = "My contact to retrieve"
            };

            error = _commonService.CreateContactPerson(ref contactPersonDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(2, _unitOfWork.ContactPersonRepository.CountAll());

            var persons = _commonService.GetAllContactPersons();
            Assert.AreEqual(2, persons.Count);

            Assert.AreEqual(persons[0].ContactPersonName, "My contact");
            Assert.AreEqual(persons[0].ContactInfo.HomePhoneNumber, "622199988877");
            Assert.AreEqual(persons[0].ContactInfo.WorkPhoneNumber, "622199988878");
            Assert.AreEqual(persons[0].ContactInfo.MobilePhoneNumber, "622199988879");
            Assert.AreEqual(persons[0].ContactInfo.WebSite, "www.mysite.com");
            Assert.AreEqual(persons[0].ContactInfo.OfficialEmail, "my@email.com");
            Assert.AreEqual(persons[0].Address.Street, "Jalan Cipete, 4");
            Assert.AreEqual(persons[0].Address.PostalCode, "12780");
            Assert.AreEqual(persons[0].Address.Location.LocationId, 7);


            Assert.AreEqual(persons[1].ContactPersonName, "My contact to retrieve");
            Assert.AreEqual(persons[1].ContactInfo.HomePhoneNumber, "622199988888");
            Assert.AreEqual(persons[1].ContactInfo.WorkPhoneNumber, "622199988999");
            Assert.AreEqual(persons[1].ContactInfo.MobilePhoneNumber, "622199988777");
            Assert.AreEqual(persons[1].ContactInfo.WebSite, "www.mysitetoretrieve.com");
            Assert.AreEqual(persons[1].ContactInfo.OfficialEmail, "my@emailtoretrieve.com");
            Assert.AreEqual(persons[1].Address.Street, "Jalan Cipete, 1");
            Assert.AreEqual(persons[1].Address.PostalCode, "12790");
            Assert.AreEqual(persons[1].Address.Location.LocationId, 7);
            

        }

        /// <summary>
        /// Test to delete a contact person
        /// </summary>
        [Test]
        public void DeleteContactPerson()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988877",
                WorkPhoneNumber = "622199988878",
                MobilePhoneNumber = "622199988879",
                WebSite = "www.mysite.com",
                OfficialEmail = "my@email.com"
            };

            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 7
                }
            };

            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact 2"
            };

            var error = _commonService.CreateContactPerson(ref contactPersonDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.ContactPersonRepository.CountAll());

            error = _commonService.DeleteContactPerson(contactPersonDTO.ContactPersonId);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(0, _unitOfWork.ContactPersonRepository.CountAll());
        }

        /// <summary>
        // Test to delete a contact person not existing
        /// </summary>
        [Test]
        public void DeleteContactPersonNotFound()
        {
            var error = _commonService.DeleteContactPerson(10);
            Assert.AreEqual(ErrorCode.COMMON_CONTACT_PERSON_NOT_FOUND, error);
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

