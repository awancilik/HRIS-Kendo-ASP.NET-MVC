using System.ComponentModel;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.LookUpDatabase
{
    public class DrivingLicenseOfficeLookUpDatabaseService
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private ILookUpDatabaseService<DrivingLicenseOfficeDTO> _drivingLicenseOfficeService;
        private IQualificationPlaceFactory _factory;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _factory = new QualificationPlaceFactory(_unitOfWork);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());
            _drivingLicenseOfficeService = new Services.LookUpDatabase.DrivingLicenseOfficeLookUpDatabaseService(_unitOfWork, _factory);
            Utilities.InitLocations(_commonService);
        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            var objects = _unitOfWork.QualificationPlaceRepository.GetAll();
            if (objects == null) return;
            foreach (var qualificationPlace in objects.Reverse())
            {
                _unitOfWork.QualificationPlaceRepository.Delete(qualificationPlace);
            }
            InitializeQualificationPlaces();
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

        #endregion

        private void InitializeQualificationPlaces()
        {
            //Normal Data
            var drivingLicenseOffice1 = new DrivingLicenseCheckingOffice()
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "DrivingLicenseOffice 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://drivingLicenseOffice1.com"
            };

            //Deactivated Data should not be included
            var drivingLicenseOffice2 = new DrivingLicenseCheckingOffice
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Perancis",
                    PostalCode = "3141",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "DrivingLicenseOffice 2",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 2",
                QualificationPlaceIsDeactivated = true,
                QualificationPlaceWebSite = "http://drivingLicenseOffice2.com"
            };

            //Normal data
            var drivingLicenseOffice3 = new DrivingLicenseCheckingOffice
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "DrivingLicenseOffice 3",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 3",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://drivingLicenseOffice3.com"
            };

            //Other derived object should not be included
            var court1 = new Court
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "Court 1",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 3",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://court1.com"
            };

            _unitOfWork.QualificationPlaceRepository.Add(drivingLicenseOffice1);
            _unitOfWork.QualificationPlaceRepository.Add(drivingLicenseOffice2);
            _unitOfWork.QualificationPlaceRepository.Add(drivingLicenseOffice3);
            _unitOfWork.QualificationPlaceRepository.Add(court1);
        }

        [Test]
        public void GetAllQualificationPlaces()
        {
            var qualificationPlaces = _drivingLicenseOfficeService.GetAllQualificationPlaces();
            Assert.AreEqual(2, qualificationPlaces.Count);
        }

        [Test]
        public void GetQualificationPlace()
        {
            var drivingLicenseOfficeActual = _drivingLicenseOfficeService.GetQualificationPlace(14);
            var drivingLicenseOfficeExpected = new DrivingLicenseCheckingOffice
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "DrivingLicenseOffice 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceWebSite = "http://drivingLicenseOffice1.com"
            };
            Assert.AreNotEqual(null, drivingLicenseOfficeActual);
            Assert.AreEqual(drivingLicenseOfficeExpected.QualificationPlaceName, drivingLicenseOfficeActual.QualificationPlaceName);
            Assert.AreEqual(drivingLicenseOfficeExpected.QualificationPlaceCategory, drivingLicenseOfficeActual.QualificationPlaceCategory);
            Assert.AreEqual(drivingLicenseOfficeExpected.QualificationPlaceDescription, drivingLicenseOfficeActual.QualificationPlaceDescription);
            Assert.AreEqual(drivingLicenseOfficeExpected.QualificationPlaceWebSite, drivingLicenseOfficeActual.QualificationPlaceWebSite);
        }

        [Test]
        public void CreateOrUpdateQualificationPlace()
        {
            var drivingLicenseOfficeDTO = new DrivingLicenseOfficeDTO
            {
                QualificationPlaceName = "DrivingLicenseOffice New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description DrivingLicenseOffice New",
                QualificationPlaceWebSite = "http://drivingLicenseOfficenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 1
                    }
                }
            };

            var errorCode = _drivingLicenseOfficeService.CreateOrEditQualificationPlace(ref drivingLicenseOfficeDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);

            var drivingLicenseOfficeActual = _unitOfWork.QualificationPlaceRepository.GetAll().ToArray()[4];
            Assert.AreNotEqual(null, drivingLicenseOfficeActual.QualificationPlaceId);
            Assert.AreEqual(drivingLicenseOfficeDTO.QualificationPlaceName, drivingLicenseOfficeActual.QualificationPlaceName);
            Assert.AreEqual(drivingLicenseOfficeDTO.QualificationPlaceCategory, drivingLicenseOfficeActual.QualificationPlaceCategory);
            Assert.AreEqual(drivingLicenseOfficeDTO.QualificationPlaceDescription, drivingLicenseOfficeActual.QualificationPlaceDescription);
            Assert.AreEqual(drivingLicenseOfficeDTO.QualificationPlaceWebSite, drivingLicenseOfficeActual.QualificationPlaceWebSite);
        }

        [Test]
        public void DeleteQualificationPlace()
        {
            var errorCode = _drivingLicenseOfficeService.DeleteQualificationPlace(new DrivingLicenseOfficeDTO { QualificationPlaceId = 6 });
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            errorCode = _drivingLicenseOfficeService.DeleteQualificationPlace(new DrivingLicenseOfficeDTO { QualificationPlaceId = 8 });
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            errorCode = _drivingLicenseOfficeService.DeleteQualificationPlace(new DrivingLicenseOfficeDTO { QualificationPlaceId = 6 });
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_ALREADY_DEACTIVATED, errorCode);
            errorCode = _drivingLicenseOfficeService.DeleteQualificationPlace(new DrivingLicenseOfficeDTO { QualificationPlaceId = 1 });
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND, errorCode);
        }
    }
}