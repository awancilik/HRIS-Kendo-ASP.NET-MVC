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
    public class PopulationOfficeLookUpDatabaseService
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private ILookUpDatabaseService<PopulationOfficeDTO> _populationOfficeService;
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
            _populationOfficeService = new Services.LookUpDatabase.PopulationOfficeLookUpDatabaseService(_unitOfWork, _factory);
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
            var populationOffice1 = new PopulationOffice
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "PopulationOffice 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://populationOffice1.com"
            };

            //Deactivated Data should not be included
            var populationOffice2 = new PopulationOffice
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Perancis",
                    PostalCode = "3141",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "PopulationOffice 2",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 2",
                QualificationPlaceIsDeactivated = true,
                QualificationPlaceWebSite = "http://populationOffice2.com"
            };

            //Normal data
            var populationOffice3 = new PopulationOffice
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "populationOffice 3",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 3",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://populationOffice3.com"
            };

            //Other derived object should not be included
            var ImmigrationOffice1 = new ImmigrationOffice
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "Immigration Office 1",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 3",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://immigrationOffice.com"
            };

            _unitOfWork.QualificationPlaceRepository.Add(populationOffice1);
            _unitOfWork.QualificationPlaceRepository.Add(populationOffice2);
            _unitOfWork.QualificationPlaceRepository.Add(populationOffice3);
            _unitOfWork.QualificationPlaceRepository.Add(ImmigrationOffice1);
        }

        [Test]
        public void GetAllQualificationPlaces()
        {
            var qualificationPlaces = _populationOfficeService.GetAllQualificationPlaces();
            Assert.AreEqual(2, qualificationPlaces.Count);
        }

        [Test]
        public void GetQualificationPlace()
        {
            var populationOfficeActual = _populationOfficeService.GetQualificationPlace(14);
            var populationOfficeExpected = new PopulationOffice
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "PopulationOffice 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceWebSite = "http://populationOffice1.com"
            };
            Assert.AreNotEqual(null, populationOfficeActual);
            Assert.AreEqual(populationOfficeExpected.QualificationPlaceName, populationOfficeActual.QualificationPlaceName);
            Assert.AreEqual(populationOfficeExpected.QualificationPlaceCategory, populationOfficeActual.QualificationPlaceCategory);
            Assert.AreEqual(populationOfficeExpected.QualificationPlaceDescription, populationOfficeActual.QualificationPlaceDescription);
            Assert.AreEqual(populationOfficeExpected.QualificationPlaceWebSite, populationOfficeActual.QualificationPlaceWebSite);
        }

        [Test]
        public void CreateOrUpdateQualificationPlace()
        {
            var populationOfficeDTO = new PopulationOfficeDTO
            {
                QualificationPlaceName = "PopulationOffice New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description PopulationOffice New",
                QualificationPlaceWebSite = "http://populationOfficenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 1
                    }
                }
            };

            var errorCode = _populationOfficeService.CreateOrEditQualificationPlace(ref populationOfficeDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);

            var populationOfficeActual = _unitOfWork.QualificationPlaceRepository.GetAll().ToArray()[4];
            Assert.AreNotEqual(null, populationOfficeActual.QualificationPlaceId);
            Assert.AreEqual(populationOfficeDTO.QualificationPlaceName, populationOfficeActual.QualificationPlaceName);
            Assert.AreEqual(populationOfficeDTO.QualificationPlaceCategory, populationOfficeActual.QualificationPlaceCategory);
            Assert.AreEqual(populationOfficeDTO.QualificationPlaceDescription, populationOfficeActual.QualificationPlaceDescription);
            Assert.AreEqual(populationOfficeDTO.QualificationPlaceWebSite, populationOfficeActual.QualificationPlaceWebSite);
        }

        [Test]
        public void DeleteQualificationPlace()
        {
            var errorCode = _populationOfficeService.DeleteQualificationPlace(new PopulationOfficeDTO { QualificationPlaceId = 6 });
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            errorCode = _populationOfficeService.DeleteQualificationPlace(new PopulationOfficeDTO { QualificationPlaceId = 8 });
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            errorCode = _populationOfficeService.DeleteQualificationPlace(new PopulationOfficeDTO { QualificationPlaceId = 6 });
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_ALREADY_DEACTIVATED, errorCode);
            errorCode = _populationOfficeService.DeleteQualificationPlace(new PopulationOfficeDTO { QualificationPlaceId = 1 });
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND, errorCode);
        }
    }
}