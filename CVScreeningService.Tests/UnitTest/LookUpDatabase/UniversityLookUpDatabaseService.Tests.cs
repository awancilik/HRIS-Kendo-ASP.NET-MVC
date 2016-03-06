using System.Data;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.ErrorHandling;
using CVScreeningService.Services.LookUpDatabase;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.LookUpDatabase
{
    public class UniversityLookUpDatabaseService 
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private ICommonService _commonService;
        private IErrorMessageFactoryService _errorMessageFactoryService;
        private IUniversityLookUpDatabaseService _universityService;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _universityService = new Services.LookUpDatabase.UniversityLookUpDatabaseService(_unitOfWork);
            _errorMessageFactoryService = new ErrorMessageFactoryService(new ResourceErrorFactory());
            Utilities.InitLocations(_commonService);
        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            var objects = _unitOfWork.UniversityRepository.GetAll();
            if (objects == null) return;
            foreach (var university in objects.Reverse())
            {
                _unitOfWork.UniversityRepository.Delete(university);
            }
            Initialize();
        }

        private void Initialize()
        {
            var university1 = new University
            {
                UniversityName = "ITS",
                UniversityWebSite = "http://its.ac.id"
            };
            var university2 = new University
            {
                UniversityName = "Universitas Indonesia",
                UniversityWebSite = "http://ui.ac.id"
            };
            var faculty1 = new Faculty
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "Faculty 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://faculty1.com"
            };
            var faculty3 = new Faculty
            {
                Address = new CVScreeningCore.Models.Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "Faculty 3",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 3",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://faculty3.com"
            };
            university1.QualificationPlace.Add(faculty1);
            university1.QualificationPlace.Add(faculty3);
            university2.QualificationPlace.Add(faculty1);

            _unitOfWork.UniversityRepository.Add(university1);
            _unitOfWork.UniversityRepository.Add(university2);
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

        [Test]
        public void GetAllUniversities()
        {
            var universities = _universityService.GetAllUniversities();
            Assert.AreNotEqual(null, universities);
            Assert.AreEqual(2, universities.Count);
        }

        [Test]
        public void GetUniversity()
        {
            var universityActual = _universityService.GetUniversity(8);
            Assert.AreNotEqual(null, universityActual);

            var universityExpected = new UniversityDTO
            {
                UniversityName = "ITS",
                UniversityWebSite = "http://its.ac.id"
            };

            Assert.AreEqual(universityExpected.UniversityName, universityActual.UniversityName);
            Assert.AreEqual(universityExpected.UniversityWebSite, universityActual.UniversityWebSite);

        }

        [Test]
        public void CreateOrUpdateUniversity()
        {
            var universityDTO = new UniversityDTO
            {
                UniversityName = "Unair",
                UniversityWebSite = "http://ua.ac.id"
            };

            var errorCode = _universityService.CreateOrUpdateUniversity(ref universityDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            Assert.AreEqual(3, _unitOfWork.UniversityRepository.CountAll());

            var universityActual = _unitOfWork.UniversityRepository.GetAll().ToArray()[2];
            Assert.AreEqual(universityDTO.UniversityName, universityActual.UniversityName);
            Assert.AreEqual(universityDTO.UniversityWebSite, universityActual.UniversityWebSite);
        }

        [Test]
        public void DeleteUniversity()
        {
            var errorCode = _universityService.DeleteUniversity(4);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            Assert.AreEqual(1, _unitOfWork.UniversityRepository.CountAll());

            errorCode = _universityService.DeleteUniversity(-1);
            Assert.AreEqual(ErrorCode.DBLOOKUP_UNIVERSITY_NOT_FOUND, errorCode);
        }
    }
}