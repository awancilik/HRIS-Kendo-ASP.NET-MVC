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
    public class CompanyLookUpDatabaseService
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private ILookUpDatabaseService<CompanyDTO> _companyService;
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
            _companyService = new Services.LookUpDatabase.CompanyLookUpDatabaseService(_unitOfWork, _factory);
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
            var company1 = new Company
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "Company 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://company1.com"
            };

            //Deactivated Data should not be included
            var company2 = new Company
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Perancis",
                    PostalCode = "3141",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "Company 2",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 2",
                QualificationPlaceIsDeactivated = true,
                QualificationPlaceWebSite = "http://company2.com"
            };

            //Normal data
            var company3 = new Company
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "Company 3",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 3",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://company3.com"
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

            _unitOfWork.QualificationPlaceRepository.Add(company1);
            _unitOfWork.QualificationPlaceRepository.Add(company2);
            _unitOfWork.QualificationPlaceRepository.Add(company3);
            _unitOfWork.QualificationPlaceRepository.Add(court1);
        }

        [Test]
        public void GetAllQualificationPlaces()
        {
            var qualificationPlaces = _companyService.GetAllQualificationPlaces();
            Assert.AreEqual(2,qualificationPlaces.Count);
        }

        [Test]
        public void GetQualificationPlace()
        {
            var companyActual = _companyService.GetQualificationPlace(14);
            var companyExpected = new Company
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "Company 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceWebSite = "http://company1.com"
            };
            Assert.AreNotEqual(null, companyActual);
            Assert.AreEqual(companyExpected.QualificationPlaceName, companyActual.QualificationPlaceName);
            Assert.AreEqual(companyExpected.QualificationPlaceCategory, companyActual.QualificationPlaceCategory);
            Assert.AreEqual(companyExpected.QualificationPlaceDescription, companyActual.QualificationPlaceDescription);
            Assert.AreEqual(companyExpected.QualificationPlaceWebSite, companyActual.QualificationPlaceWebSite);
        }

        [Test]
        public void CreateOrUpdateQualificationPlace()
        {
            var companyDTO = new CompanyDTO
            {
                QualificationPlaceName = "Company New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description Company New",
                QualificationPlaceWebSite = "http://companynew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 1
                    }
                }
            };

            var errorCode = _companyService.CreateOrEditQualificationPlace(ref companyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            
            var companyActual = _unitOfWork.QualificationPlaceRepository.GetAll().ToArray()[4];
            Assert.AreNotEqual(null, companyActual.QualificationPlaceId);
            Assert.AreEqual(companyDTO.QualificationPlaceName, companyActual.QualificationPlaceName);
            Assert.AreEqual(companyDTO.QualificationPlaceCategory, companyActual.QualificationPlaceCategory);
            Assert.AreEqual(companyDTO.QualificationPlaceDescription, companyActual.QualificationPlaceDescription);
            Assert.AreEqual(companyDTO.QualificationPlaceWebSite, companyActual.QualificationPlaceWebSite);
           
        }

        [Test]
        public void DeleteQualificationPlace()
        {
            var errorCode = _companyService.DeleteQualificationPlace(new CompanyDTO{QualificationPlaceId = 6});
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            errorCode = _companyService.DeleteQualificationPlace(new CompanyDTO { QualificationPlaceId = 8 });
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            errorCode = _companyService.DeleteQualificationPlace(new CompanyDTO { QualificationPlaceId = 6 });
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_ALREADY_DEACTIVATED, errorCode);
            errorCode = _companyService.DeleteQualificationPlace(new CompanyDTO { QualificationPlaceId = 1 });
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND, errorCode);
        }
    }
}