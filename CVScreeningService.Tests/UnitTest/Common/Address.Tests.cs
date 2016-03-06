using CVScreeningCore.Error;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Common;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.Common
{
    [TestFixture]
    public class Address
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

        }

        /// <summary>
        // Test to create an address
        /// </summary>
        [Test]
        public void CreateAddress()
        {
            var addressDTO1 = new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 1
                }
            };

            var error = _commonService.CreateAddress(ref addressDTO1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.AddressRepository.CountAll());

            var addressDTO2 = new AddressDTO
            {
                Street = "Jalan Cipete, 14",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 1
                }
            };

            error = _commonService.CreateAddress(ref addressDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(2, _unitOfWork.AddressRepository.CountAll());

        }

        /// <summary>
        /// Test to delete an address
        /// </summary>
        [Test]
        public void DeleteAddress()
        {
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 10",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 7
                }
            };

            var error = _commonService.CreateAddress(ref addressDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(3, addressDTO.AddressId);
            Assert.AreEqual(3, _unitOfWork.AddressRepository.CountAll());

            error = _commonService.DeleteAddress(addressDTO.AddressId);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(2, _unitOfWork.AddressRepository.CountAll());

        }

        /// <summary>
        // Test to delete an address not existing
        /// </summary>
        [Test]
        public void DeleteAddressNotFound()
        {
            var error = _commonService.DeleteAddress(10);
            Assert.AreEqual(ErrorCode.COMMON_ADDRESS_NOT_FOUND, error);
        }

        /// <summary>
        // Test to get all address
        /// </summary>
        [Test]
        public void GetAllAddresses()
        {
            var addresses = _commonService.GetAllAddresses();
            Assert.AreNotEqual(null, addresses);
            Assert.AreEqual(2, _unitOfWork.AddressRepository.CountAll());

            Assert.AreEqual(addresses[0].Street, "Jalan Cipete, 4");
            Assert.AreEqual(addresses[0].PostalCode, "12780");

            Assert.AreEqual(addresses[1].Street, "Jalan Cipete, 14");
            Assert.AreEqual(addresses[0].PostalCode, "12780");

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

