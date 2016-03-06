using System.Collections.Generic;
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
    public class ProfessionalQualificationService
    {
        #region NUnit Test Template

        // 1. Declare global object
        private ICommonService _commonService;
        private IProfessionalQualificationService _professionalQualificationService;
        private IUnitOfWork _unitOfWork;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _professionalQualificationService = new Services.LookUpDatabase.ProfessionalQualificationService(_unitOfWork);
            new Services.LookUpDatabase.CertificationPlaceLookUpDatabaseService(
                _unitOfWork, new QualificationPlaceFactory(_unitOfWork));
            new ErrorMessageFactoryService(new ResourceErrorFactory());
            Utilities.InitLocations(_commonService);
        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            var objects = _unitOfWork.ProfessionalQualificationRepository.GetAll();
            if (objects == null) return;
            foreach (var professionalQualification in objects.Reverse())
            {
                _unitOfWork.ProfessionalQualificationRepository.Delete(professionalQualification);
            }
            Initialize();
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

        private void Initialize()
        {
            var professionalQualification1 = new ProfessionalQualification
            {
                ProfessionalQualificationName = ".Net",
                ProfessionalQualificationCode = "N001"
            };

            var professionalQualification2 = new ProfessionalQualification
            {
                ProfessionalQualificationName = "Java",
                ProfessionalQualificationCode = "J001"
            };

            var certificationPlace1 = new CertificationPlace
            {
                Address = new Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "CertificationPlace 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://certificationPlace1.com"
            };

            var certificationPlace2 = new CertificationPlace
            {
                Address = new Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "CertificationPlace 2",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 2",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://certificationPlace3.com"
            };

            _unitOfWork.QualificationPlaceRepository.Add(certificationPlace1);
            _unitOfWork.QualificationPlaceRepository.Add(certificationPlace2);

            professionalQualification1.QualificationPlace = new List<QualificationPlace>
            {
                certificationPlace1, certificationPlace2
            };
            professionalQualification2.QualificationPlace = new List<QualificationPlace>
            {
                certificationPlace2
            };
            _unitOfWork.ProfessionalQualificationRepository.Add(professionalQualification1);
            _unitOfWork.ProfessionalQualificationRepository.Add(professionalQualification2);
        }

        [Test]
        public void GetAllProfessionalQualifications()
        {
            var professionalQualifications =
                _professionalQualificationService.GetAllProfessionalQualifications();
            Assert.AreNotEqual(null, professionalQualifications);
            Assert.AreEqual(2, professionalQualifications.Count);
        }

        [Test]
        public void GetProfessionalQualification()
        {
            var professionalQualificationExpected = new ProfessionalQualification
            {
                ProfessionalQualificationName = ".Net",
                ProfessionalQualificationCode = "N001"
            };
            var certificationPlace1 = new CertificationPlace
            {
                Address = new Address
                {
                    Street = "Jl Kuningan",
                    PostalCode = "1234",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "CertificationPlace 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://certificationPlace1.com"
            };
            _unitOfWork.QualificationPlaceRepository.Add(certificationPlace1);
            professionalQualificationExpected.QualificationPlace = new List<QualificationPlace>
            {
                certificationPlace1
            };

            var professionalQualificationActual =
                _professionalQualificationService.GetProfessionalQualification(8);

            Assert.AreNotEqual(null, professionalQualificationActual);
            Assert.AreEqual(professionalQualificationExpected.ProfessionalQualificationName,
                professionalQualificationActual.ProfessionalQualificationName);
            Assert.AreEqual(professionalQualificationExpected.ProfessionalQualificationCode,
                professionalQualificationActual.ProfessionalQualificationCode);
        }

        [Test]
        public void GetQualificationPlaces()
        {
            var certificationPlaceExpected = new CertificationPlace
            {
                Address = new Address
                {
                    Street = "Jl Jerman",
                    PostalCode = "31211",
                    Location = _unitOfWork.LocationRepository.First(l => l.LocationId == 7)
                },
                QualificationPlaceName = "CertificationPlace 2",
                QualificationPlaceCategory = "Private",
                QualificationPlaceDescription = "Description 2",
                QualificationPlaceIsDeactivated = false,
                QualificationPlaceWebSite = "http://certificationPlace3.com"
            };

            var certificationPlaceActual =
                _professionalQualificationService.GetCertificationPlaces(11).ToArray()[0];

            Assert.AreNotEqual(null, certificationPlaceActual);
            Assert.AreEqual(certificationPlaceExpected.QualificationPlaceName,
                certificationPlaceActual.QualificationPlaceName);
            Assert.AreEqual(certificationPlaceExpected.QualificationPlaceCategory,
                certificationPlaceActual.QualificationPlaceCategory);
            Assert.AreEqual(certificationPlaceExpected.QualificationPlaceDescription,
                certificationPlaceActual.QualificationPlaceDescription);
            Assert.AreEqual(certificationPlaceExpected.QualificationPlaceWebSite,
                certificationPlaceActual.QualificationPlaceWebSite);
     
        }

        [Test]
        public void CreateOrUpdateProfessionalQualification()
        {
            var professionalQualificationDTO = new ProfessionalQualificationDTO
            {
                ProfessionalQualificationName = "Ruby",
                ProfessionalQualificationCode = "N004"
            };
            var certificationPlace1 = new CertificationPlaceDTO()
            {
                QualificationPlaceName = "CertificationPlace 1",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description 1",
                QualificationPlaceWebSite = "http://certificationPlace1.com"
            };
            professionalQualificationDTO.QualificationPlace = new List<CertificationPlaceDTO>
            {
                certificationPlace1
            };


            var errorCode =
                _professionalQualificationService.CreateOrUpdateProfessionalQualification(
                    ref professionalQualificationDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);

            var professionalQualificationActual =
                _unitOfWork.ProfessionalQualificationRepository.GetAll().ToArray()[2];
            Assert.AreEqual(professionalQualificationDTO.ProfessionalQualificationName,
                professionalQualificationActual.ProfessionalQualificationName);
            Assert.AreEqual(professionalQualificationDTO.ProfessionalQualificationCode,
                professionalQualificationActual.ProfessionalQualificationCode);
        }

        [Test]
        public void DeleteProfessionalQualification()
        {
            var errorCode = _professionalQualificationService.DeleteProfessionalQualification(5);
            Assert.AreEqual(ErrorCode.NO_ERROR, errorCode);
            Assert.AreEqual(1,_unitOfWork.ProfessionalQualificationRepository.CountAll());

            errorCode = _professionalQualificationService.DeleteProfessionalQualification(5);
            Assert.AreEqual(ErrorCode.DBLOOKUP_PROFESSIONAL_QUALIFICATION_NOT_FOUND, errorCode);

        }
    }
}