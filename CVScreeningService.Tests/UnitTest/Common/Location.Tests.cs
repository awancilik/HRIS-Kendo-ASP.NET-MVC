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
    public class Location
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
        // Test to create a country location
        /// </summary>
        [Test]
        public void CreateLocation()
        {
            // Create country location INDONESIA
            var countryDTO = new LocationDTO
            {
                LocationLevel = (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY,
                LocationName = "Indonesia",
                LocationParent = null,
                LocationParentLocationId = null
            };
            var error = _commonService.CreateLocation(ref countryDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(1, _unitOfWork.LocationRepository.CountAll());


            // Create province location DKI Jakarta
            var provinceDTO = new LocationDTO
            {
                LocationLevel = (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_PROVINCE,
                LocationName = "DKI Jakarta",
                LocationParent = countryDTO,
                LocationParentLocationId = countryDTO.LocationId
            };
            error = _commonService.CreateLocation(ref provinceDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(2, _unitOfWork.LocationRepository.CountAll());

            // Create city location Jakarta Selatan
            var cityJakartaSelatanDTO = new LocationDTO
            {
                LocationLevel = (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY,
                LocationName = "Jakarta Selatan",
                LocationParent = provinceDTO,
                LocationParentLocationId = provinceDTO.LocationId
            };
            error = _commonService.CreateLocation(ref cityJakartaSelatanDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(3, _unitOfWork.LocationRepository.CountAll());

            // Create city location Jakarta Barat
            var cityJakartaBaratDTO = new LocationDTO
            {
                LocationLevel = (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY,
                LocationName = "Jakarta Barat",
                LocationParent = provinceDTO,
                LocationParentLocationId = provinceDTO.LocationId
            };
            error = _commonService.CreateLocation(ref cityJakartaBaratDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(4, _unitOfWork.LocationRepository.CountAll());


            // Create district location Cilandak
            var districtCilandakDTO = new LocationDTO
            {
                LocationLevel = (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_DISTRICT,
                LocationName = "Cilandak",
                LocationParent = cityJakartaSelatanDTO,
                LocationParentLocationId = cityJakartaSelatanDTO.LocationId
            };
            error = _commonService.CreateLocation(ref districtCilandakDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(5, _unitOfWork.LocationRepository.CountAll());

            // Create subdistrict location Cipete
            var districtCipeteDTO = new LocationDTO
            {
                LocationLevel = (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT,
                LocationName = "Cipete",
                LocationParent = districtCilandakDTO,
                LocationParentLocationId = districtCilandakDTO.LocationId
            };
            error = _commonService.CreateLocation(ref districtCipeteDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(6, _unitOfWork.LocationRepository.CountAll());


            // Create subdistrict location Cilandak Utara
            var districtCilandakUtaraDTO = new LocationDTO
            {
                LocationLevel = (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT,
                LocationName = "Cilandak Utara",
                LocationParent = districtCilandakDTO,
                LocationParentLocationId = districtCilandakDTO.LocationId
            };
            error = _commonService.CreateLocation(ref districtCilandakUtaraDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(7, _unitOfWork.LocationRepository.CountAll());

        }


        /// <summary>
        /// Test to retrieve all the locations
        /// </summary>
        [Test]
        public void GetAllLocations()
        {
            var locations = _commonService.GetAllLocations();
            Assert.AreEqual(7, locations.Count);
        }

        /// <summary>
        // Test to retrieve location by level
        /// </summary>
        [Test]
        public void GetAllLocationsByLevel()
        {
            var locations = _commonService.GetAllLocationsByLevel(
                (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY);
            Assert.AreEqual(1, locations.Count);
            Assert.AreEqual(locations[0].LocationName, "Indonesia");

            locations = _commonService.GetAllLocationsByLevel(
                (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_PROVINCE);
            Assert.AreEqual(1, locations.Count);
            Assert.AreEqual(locations[0].LocationName, "DKI Jakarta");

            locations = _commonService.GetAllLocationsByLevel(
                (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY);
            Assert.AreEqual(2, locations.Count);
            Assert.AreEqual(locations[0].LocationName, "Jakarta Selatan");
            Assert.AreEqual(locations[1].LocationName, "Jakarta Barat");

            locations = _commonService.GetAllLocationsByLevel(
                (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_DISTRICT);
            Assert.AreEqual(1, locations.Count);
            Assert.AreEqual(locations[0].LocationName, "Cilandak");

            locations = _commonService.GetAllLocationsByLevel(
                (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT);
            Assert.AreEqual(2, locations.Count);
            Assert.AreEqual(locations[0].LocationName, "Cipete");
            Assert.AreEqual(locations[1].LocationName, "Cilandak Utara");

        }

        /// <summary>
        /// Test to retrieve location by id
        /// </summary>
        [Test]
        public void GetLocation()
        {
            var location = _commonService.GetLocation(1);
            Assert.AreEqual(location.LocationName, "Indonesia");

            location = _commonService.GetLocation(3);
            Assert.AreEqual(location.LocationName, "Jakarta Selatan");
        }

        [Test]
        public void HaveSameDirectParentLocation()
        {
            var location1 = _commonService.GetLocationByName("Cipete");
            var location2 = _commonService.GetLocationByName("Cilandak Utara");
            var location3 = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT,
                LocationName = "Tegal Parang",
                LocationParent = _commonService.GetLocationByName("Cilandak")
            };

            var value = _commonService.HaveSameDirectParent(new[] {location1, location2, location3});
            Assert.AreEqual(true, value);
        }

        /// <summary>
        /// Test to retrieve location by id that does not exist
        /// </summary>
        [Test]
        public void GetLocationNotFound()
        {
            var location = _commonService.GetLocation(12);
            Assert.AreEqual(location, null);
        }

        /// <summary>
        /// Test to retrieve location by name
        /// </summary>
        [Test]
        public void GetLocationByName()
        {
            var location = _commonService.GetLocation(1);
            Assert.AreEqual(location.LocationName, "Indonesia");

            location = _commonService.GetLocation(3);
            Assert.AreEqual(location.LocationName, "Jakarta Selatan");
        }


        /// <summary>
        /// Test to retrieve location by name that does not exist
        /// </summary>
        [Test]
        public void GetLocationByNameNotFound()
        {
            var location = _commonService.GetLocationByName("SSSS");
            Assert.AreEqual(location, null);

            location = _commonService.GetLocationByName("Tssss");
            Assert.AreEqual(location, null);
        }

        /// <summary>
        // Test to get parent location of sub district Cilandak Utara
        /// </summary>
        [Test]
        public void GetLocationParent()
        {
            var location = _commonService.GetLocationParent(6, LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY);
            Assert.AreEqual("Indonesia", location.LocationName);

            location = _commonService.GetLocationParent(6, LocationDTO.LocationLevelEnum.LOCATION_LEVEL_PROVINCE);
            Assert.AreEqual("DKI Jakarta", location.LocationName);

            location = _commonService.GetLocationParent(6, LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY);
            Assert.AreEqual("Jakarta Selatan", location.LocationName);

            location = _commonService.GetLocationParent(6, LocationDTO.LocationLevelEnum.LOCATION_LEVEL_DISTRICT);
            Assert.AreEqual("Cilandak", location.LocationName);
        }


        /// <summary>
        // Test to get parent location of sub district not found
        /// </summary>
        [Test]
        public void GetLocationParentNotFound()
        {
            var location = _commonService.GetLocationParent(8, LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY);
            Assert.AreEqual(null, location);
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

