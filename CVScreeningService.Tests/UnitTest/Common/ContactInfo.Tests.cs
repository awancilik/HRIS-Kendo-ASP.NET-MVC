using CVScreeningCore.Error;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Common;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.Common
{
    [TestFixture]
    public class ContactInfo
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
        }


        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {

        }

        /// <summary>
        // Test to create a contact info
        /// </summary>
        [Test]
        public void CreateContactInfo()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988877",
                WorkPhoneNumber = "622199988878",
                MobilePhoneNumber = "622199988879",
                WebSite = "www.mysite.com",
                OfficialEmail = "my@email.com"
            };

            var error = _commonService.CreateContactInfo(ref contactInfoDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.ContactInfoRepository.CountAll());
        }

        /// <summary>
        /// Test to delete a contact info
        /// </summary>
        [Test]
        public void DeleteContactInfo()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622119988877",
                WorkPhoneNumber = "622119988878",
                MobilePhoneNumber = "622119988879",
                WebSite = "www.mysite.com",
                OfficialEmail = "my@email.com"
            };

            var error = _commonService.CreateContactInfo(ref contactInfoDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(2, contactInfoDTO.ContactInfoId);
            Assert.AreEqual(2, _unitOfWork.ContactInfoRepository.CountAll());

            error = _commonService.DeleteContactInfo(contactInfoDTO.ContactInfoId);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.ContactInfoRepository.CountAll());

        }

        /// <summary>
        // Test to delete a contact info not existing
        /// </summary>
        [Test]
        public void DeleteContactInfoNotFound()
        {
            var error = _commonService.DeleteContactInfo(10);
            Assert.AreEqual(ErrorCode.COMMON_CONTACT_INFO_NOT_FOUND, error);
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

