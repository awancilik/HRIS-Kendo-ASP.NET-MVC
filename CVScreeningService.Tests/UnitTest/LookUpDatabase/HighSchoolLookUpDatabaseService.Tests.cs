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
    public class HighSchoolLookUpDatabaseService
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private ILookUpDatabaseService<HighSchoolDTO> _highSchoolService;
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
            _highSchoolService = new Services.LookUpDatabase.HighSchoolLookUpDatabaseService(_unitOfWork, _factory);
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
            var highSchool1 = new HighSchool
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "HighSchool 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://highSchool1.com"
            };

            //Deactivated Data should not be included
            var highSchool2 = new HighSchool
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Perancis",
                    PostalCode = "3141",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "HighSchool 2",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 2",
                QualificationPlaceIsDeactivated = true,
                QualificationPlaceWebSite = "http://highSchool2.com"
            };

            //Normal data
            var highSchool3 = new HighSchool
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "HighSchool 3",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 3",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://highSchool3.com"
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

            _unitOfWork.QualificationPlaceRepository.Add(highSchool1);
            _unitOfWork.QualificationPlaceRepository.Add(highSchool2);
            _unitOfWork.QualificationPlaceRepository.Add(highSchool3);
            _unitOfWork.QualificationPlaceRepository.Add(court1);
        }

        [Test]
        public void GetAllQualificationPlaces()
        {
            var qualificationPlaces = _highSchoolService.GetAllQualificationPlaces();
            Assert.AreEqual(2, qualificationPlaces.Count);
        }

        [Test]
        public void GetQualificationPlace()
        {
            var highSchoolActual = _highSchoolService.GetQualificationPlace(14);
            var highSchoolExpected = new HighSchool
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "HighSchool 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceWebSite = "http://highSchool1.com"
            };
            Assert.AreNotEqual(null, highSchoolActual);
            Assert.AreEqual(highSchoolExpected.QualificationPlaceName, highSchoolActual.QualificationPlaceName);
            Assert.AreEqual(highSchoolExpected.QualificationPlaceCategory, highSchoolActual.QualificationPlaceCategory);
            Assert.AreEqual(highSchoolExpected.QualificationPlaceDescription, highSchoolActual.QualificationPlaceDescription);
            Assert.AreEqual(highSchoolExpected.QualificationPlaceWebSite, highSchoolActual.QualificationPlaceWebSite);
            
        }

        [Test]
        public void CreateOrUpdateQualificationPlace()
        {
            var highSchoolDTO = new HighSchoolDTO
            {
                QualificationPlaceName = "HighSchool New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description HighSchool New",
                QualificationPlaceWebSite = "http://highSchoolnew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 1
                    }
                }
            
            };

            var errorCode = _highSchoolService.CreateOrEditQualificationPlace(ref highSchoolDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);

            var highSchoolActual = _unitOfWork.QualificationPlaceRepository.GetAll().ToArray()[4];
            Assert.AreNotEqual(null, highSchoolActual.QualificationPlaceId);
            Assert.AreEqual(highSchoolDTO.QualificationPlaceName, highSchoolActual.QualificationPlaceName);
            Assert.AreEqual(highSchoolDTO.QualificationPlaceCategory, highSchoolActual.QualificationPlaceCategory);
            Assert.AreEqual(highSchoolDTO.QualificationPlaceDescription, highSchoolActual.QualificationPlaceDescription);
            Assert.AreEqual(highSchoolDTO.QualificationPlaceWebSite, highSchoolActual.QualificationPlaceWebSite);
        
        }

        [Test]
        public void DeleteQualificationPlace()
        {
            var errorCode = _highSchoolService.DeleteQualificationPlace(new HighSchoolDTO { QualificationPlaceId = 6 });
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            errorCode = _highSchoolService.DeleteQualificationPlace(new HighSchoolDTO { QualificationPlaceId = 8 });
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            errorCode = _highSchoolService.DeleteQualificationPlace(new HighSchoolDTO { QualificationPlaceId = 6 });
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_ALREADY_DEACTIVATED, errorCode);
            errorCode = _highSchoolService.DeleteQualificationPlace(new HighSchoolDTO { QualificationPlaceId = 1 });
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND, errorCode);
        }
    }
}