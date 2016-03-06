using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using FakeItEasy;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.Screening
{
    [TestFixture]
    public class AtomicCheck
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private IQualificationPlaceFactory _factory;
        private ICommonService _commonService;
        private IScreeningService _screeningService;
        private IClientService _clientService;
        private IWebSecurity _webSecurity;
        private IPermissionService _permissionService;
        private IQualificationService _qualificationService;
        private IUserManagementService _userManagementService;
        private ILookUpDatabaseService<PoliceDTO> _policeService;
        private ILookUpDatabaseService<ImmigrationOfficeDTO> _immigrationOfficeService;
        private ILookUpDatabaseService<CompanyDTO> _companyService;
        private ILookUpDatabaseService<HighSchoolDTO> _highSchoolService;
        private ILookUpDatabaseService<FacultyDTO> _facultyService;
        private ILookUpDatabaseService<DrivingLicenseOfficeDTO> _drivingLicenseOfficeService;

        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager;
        private UserProfileDTO _qualityControl;
        private ClientCompanyDTO _clientCompany;
        private ClientContractDTO _clientContract;
        private ScreeningLevelDTO _screeningLevelDTO;
        private ScreeningLevelVersionDTO _screeningLevelVersionDTO;
        private ScreeningDTO _screeningDTO;
        private ImmigrationOfficeDTO _immigrationOfficeDTO;
        private PoliceDTO _police1DTO;
        private PoliceDTO _police2DTO;
        private CompanyDTO _companyDTO;
        private ScreeningQualificationDTO _qualificationBaseDTO;
        private HighSchoolDTO _highSchoolDTO;
        private FacultyDTO _facultyDTO;
        private IEnumerable<BaseQualificationPlaceDTO> _qualificationPlacesDTO;


        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            var notificationService = A.Fake<INotificationService>();
            var systemTime = A.Fake<ISystemTimeService>();
            A.CallTo(() => systemTime.GetCurrentDateTime()).Returns(new DateTime(2014, 12, 1));
            _webSecurity = A.Fake<IWebSecurity>();
            A.CallTo(() => _webSecurity.GetCurrentUserName()).Returns("Administrator");

            _factory = new QualificationPlaceFactory(_unitOfWork);
            _unitOfWork = new InMemoryUnitOfWork();
            _commonService = new CommonService(_unitOfWork);
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, systemTime);
            _policeService = new Services.LookUpDatabase.PoliceLookUpDatabaseService(_unitOfWork, _factory);
            _immigrationOfficeService = new Services.LookUpDatabase.ImmigrationOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _companyService = new Services.LookUpDatabase.CompanyLookUpDatabaseService(_unitOfWork, _factory);
            _highSchoolService = new Services.LookUpDatabase.HighSchoolLookUpDatabaseService(_unitOfWork, _factory);
            _facultyService = new Services.LookUpDatabase.FacultyLookUpDatabaseService(_unitOfWork, _factory);
            _drivingLicenseOfficeService = new Services.LookUpDatabase.DrivingLicenseOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _qualificationService = new QualificationService(_unitOfWork, _commonService);

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);


            _admin = Utilities.BuildAdminAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _admin,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);
            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);
            _screeningService = new ScreeningService(
                _unitOfWork, _permissionService, _userManagementService, systemTime, _webSecurity, notificationService);

            InitializeTest();


        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            //InitializeScreening();
        }


        private void InitializeTest()
        {
            Utilities.InitTypeOfCheck(_screeningService, _unitOfWork);

            // Create sample user account for testing
            _accountManager = Utilities.BuildAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _accountManager,
                    new System.Collections.Generic.List<string>(new List<string> { "Account manager" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl = Utilities.BuildAccountSample();
            _qualityControl.UserName = "quality@control.com";
            error = _userManagementService.CreateUserProfile(ref _qualityControl,
                    new System.Collections.Generic.List<string>(new List<string> { "Quality control" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany = Utilities.BuildClientCompanySample();
            _clientCompany.AccountManagers = new List<UserProfileDTO> { _accountManager };
            error = _clientService.CreateClientCompany(ref _clientCompany);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientContract = Utilities.BuildClientContractDTO();
            error = _clientService.CreateClientContract(_clientCompany, ref _clientContract);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningLevelDTO = Utilities.BuildScreeningLevelDTO();

            _screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            _screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion = Utilities.BuildTypeOfCheckListForScreeningListDTO();
            _screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime = 10;

            error = _clientService.CreateScreeningLevel(_clientContract, ref _screeningLevelDTO, ref _screeningLevelVersionDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            error = _screeningService.CreateScreening(_screeningDTO.ScreeningLevelVersion, ref _screeningDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _immigrationOfficeDTO = Utilities.BuildImmigrationOfficeDTO();
            error = _immigrationOfficeService.CreateOrEditQualificationPlace(ref _immigrationOfficeDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _police1DTO = Utilities.BuildPoliceOfficeDTO();
            error = _policeService.CreateOrEditQualificationPlace(ref _police1DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _police2DTO = Utilities.BuildPoliceOfficeDTO();
            _police2DTO.QualificationPlaceName = "Police 2";
            error = _policeService.CreateOrEditQualificationPlace(ref _police2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _companyDTO = Utilities.BuildCompanyDTO();
            error = _companyService.CreateOrEditQualificationPlace(ref _companyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _highSchoolDTO = Utilities.BuildHighSchoolDTO();
            error = _highSchoolService.CreateOrEditQualificationPlace(ref _highSchoolDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _unitOfWork.UniversityRepository.Add(new University
            {
                UniversityId = 1,
                UniversityName = "ITS"
            });

            _facultyDTO = Utilities.BuildFaculyDTO();
            error = _facultyService.CreateOrEditQualificationPlace(ref _facultyDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualificationBaseDTO = Utilities.BuildScreeningQualificationDTO();
            error = _qualificationService.SetQualificationBase(_screeningDTO, ref _qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualificationPlacesDTO = new List<BaseQualificationPlaceDTO> { _police1DTO, _immigrationOfficeDTO, _companyDTO };
            error = _qualificationService.SetQualificationPlaces(_screeningDTO, _qualificationPlacesDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

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
        public void GetAllAtomicChecks()
        {
            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO);
            Assert.AreEqual(13, atomicChecks.Count());
            Assert.AreEqual(atomicChecks.ElementAt(0).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.PROFESSIONNAL_QUALIFICATION_CHECK);
            Assert.AreEqual(atomicChecks.ElementAt(1).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD);
            Assert.AreEqual(atomicChecks.ElementAt(2).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD);
            Assert.AreEqual(atomicChecks.ElementAt(3).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.PASSPORT_CHECK);
            Assert.AreEqual(atomicChecks.ElementAt(4).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.POLICE_CHECK);
            Assert.AreEqual(atomicChecks.ElementAt(5).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);
            Assert.AreEqual(atomicChecks.ElementAt(6).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);
            Assert.AreEqual(atomicChecks.ElementAt(7).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.ID_CHECK);
            Assert.AreEqual(atomicChecks.ElementAt(8).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.BANKRUPTY_CHECK);
            Assert.AreEqual(atomicChecks.ElementAt(9).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.NEIGHBOURHOOD_CHECK);
            Assert.AreEqual(atomicChecks.ElementAt(10).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.NEIGHBOURHOOD_CHECK);
            Assert.AreEqual(atomicChecks.ElementAt(11).TypeOfCheck.TypeOfCheckCode, (Byte)TypeOfCheckEnum.NEIGHBOURHOOD_CHECK);

            var policeAtomicCheck =
                atomicChecks.Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.POLICE_CHECK);
            var companyAtomicCheck =
                atomicChecks.Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD
                && u.AtomicCheckType == CVScreeningCore.Models.AtomicCheck.kOtherCompanyType);
            var immigrationAtomicCheck =
                atomicChecks.Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.PASSPORT_CHECK);


            Assert.AreEqual(_police1DTO.QualificationPlaceName, policeAtomicCheck.QualificationPlace.QualificationPlaceName);
            Assert.AreEqual(_companyDTO.QualificationPlaceName, companyAtomicCheck.QualificationPlace.QualificationPlaceName);
            Assert.AreEqual(_immigrationOfficeDTO.QualificationPlaceName, immigrationAtomicCheck.QualificationPlace.QualificationPlaceName);

            _qualificationPlacesDTO = new List<BaseQualificationPlaceDTO>
            {
                _facultyDTO, _police1DTO, _immigrationOfficeDTO, _companyDTO, _highSchoolDTO
            };
            var error = _qualificationService.SetQualificationPlaces(_screeningDTO, _qualificationPlacesDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO);
            var educationAtomicCheck = 
                atomicChecks.Where(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);

            Assert.AreEqual(2, educationAtomicCheck.Count());
            Assert.AreEqual(_facultyDTO.QualificationPlaceName, 
                educationAtomicCheck.ElementAt(1).QualificationPlace.QualificationPlaceName);
            Assert.AreEqual(_highSchoolDTO.QualificationPlaceName, 
                educationAtomicCheck.ElementAt(0).QualificationPlace.QualificationPlaceName);

            _qualificationPlacesDTO = new List<BaseQualificationPlaceDTO>
            {
                _facultyDTO, _police1DTO, _immigrationOfficeDTO, _companyDTO, _highSchoolDTO, _police2DTO
            };
            error = _qualificationService.SetQualificationPlaces(_screeningDTO, _qualificationPlacesDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO);
            Assert.AreEqual(14, atomicChecks.Count());

            var policeAtomicChecks =
                atomicChecks.Where(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.POLICE_CHECK);

            Assert.AreEqual(2, policeAtomicChecks.Count());
            Assert.AreEqual(_police1DTO.QualificationPlaceName,
                policeAtomicChecks.ElementAt(0).QualificationPlace.QualificationPlaceName);
            Assert.AreEqual(_police2DTO.QualificationPlaceName,
                policeAtomicChecks.ElementAt(1).QualificationPlace.QualificationPlaceName);

        }


        [Test]
        public void CheckAtomicChecksCreation()
        {
            var screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            screeningDTO.ScreeningFullName = "Screening full name 2";
            var error = _screeningService.CreateScreening(screeningDTO.ScreeningLevelVersion, ref screeningDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(13, atomicChecks.Count());

            var faculty2DTO = Utilities.BuildFaculyDTO();
            faculty2DTO.QualificationPlaceName = "Faculty New 2";
            error = _facultyService.CreateOrEditQualificationPlace(ref faculty2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualificationPlacesDTO = new List<BaseQualificationPlaceDTO>
            {
                _facultyDTO, _police1DTO, _immigrationOfficeDTO, _companyDTO, faculty2DTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, _qualificationPlacesDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(14, atomicChecks.Count());

            _qualificationPlacesDTO = new List<BaseQualificationPlaceDTO>
            {
                _facultyDTO, _police1DTO, _immigrationOfficeDTO, _companyDTO, faculty2DTO, _highSchoolDTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, _qualificationPlacesDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(14, atomicChecks.Count());

            var educationAtomicChecks =
                atomicChecks.Where(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD)
                .OrderBy(u => u.QualificationPlace.QualificationPlaceName);

            Assert.AreEqual(3, educationAtomicChecks.Count());
            Assert.AreEqual(_facultyDTO.QualificationPlaceName,
                educationAtomicChecks.ElementAt(0).QualificationPlace.QualificationPlaceName);
            Assert.AreEqual(faculty2DTO.QualificationPlaceName,
                educationAtomicChecks.ElementAt(1).QualificationPlace.QualificationPlaceName);
            Assert.AreEqual(_highSchoolDTO.QualificationPlaceName,
                educationAtomicChecks.ElementAt(2).QualificationPlace.QualificationPlaceName);

            var highSchool2DTO = Utilities.BuildHighSchoolDTO();
            highSchool2DTO.QualificationPlaceName = "High school New 2";
            error = _highSchoolService.CreateOrEditQualificationPlace(ref highSchool2DTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualificationPlacesDTO = new List<BaseQualificationPlaceDTO>
            {
                _facultyDTO, _police1DTO, _immigrationOfficeDTO, _companyDTO, faculty2DTO, highSchool2DTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, _qualificationPlacesDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(15, atomicChecks.Count());
        }

        [Test]
        public void CheckAtomicChecksCreationQualificationNotCompatible()
        {
            var screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            screeningDTO.ScreeningFullName = "Screening full name 3";
            var error = _screeningService.CreateScreening(screeningDTO.ScreeningLevelVersion, ref screeningDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(13, atomicChecks.Count());

            var drivingLicenseDTO = Utilities.BuildDrivingLicenseOfficeDTO();
            error = _drivingLicenseOfficeService.CreateOrEditQualificationPlace(ref drivingLicenseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualificationPlacesDTO = new List<BaseQualificationPlaceDTO>
            {
                _facultyDTO,
                _police1DTO,
                _immigrationOfficeDTO,
                _companyDTO,
                drivingLicenseDTO,
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, _qualificationPlacesDTO);
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_NOT_COMPATIBLE_WITH_SCREENING, error);
        }


        [Test]
        public void GetAllAtomicChecksIsQualified()
        {
            var screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            screeningDTO.ScreeningFullName = "Screening full name 4";
            var error = _screeningService.CreateScreening(screeningDTO.ScreeningLevelVersion, ref screeningDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(13, atomicChecks.Count());
            Assert.AreEqual(atomicChecks.ElementAt(0).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(1).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(2).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(3).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(4).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(5).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(6).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(7).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(8).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(9).IsQualified, false);
            Assert.AreEqual(atomicChecks.ElementAt(10).IsQualified, false);

            var qualificationPlacesDTO = new List<BaseQualificationPlaceDTO>
            {
                _facultyDTO, _police1DTO, _immigrationOfficeDTO, _companyDTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualificationPlacesDTO);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(4, atomicChecks.Count(u => u.IsQualified == true));

            var educationAtomicChecks =
                atomicChecks.Where(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);

            Assert.AreEqual(1, atomicChecks.Count(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD
                && u.IsQualified));

            qualificationPlacesDTO = new List<BaseQualificationPlaceDTO>
            {
                _facultyDTO, _police1DTO, _immigrationOfficeDTO, _companyDTO, _highSchoolDTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualificationPlacesDTO);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            educationAtomicChecks =
                atomicChecks.Where(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD);

            Assert.AreEqual(5, atomicChecks.Count(u => u.IsQualified == true));
            Assert.AreEqual(2, atomicChecks.Count(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.EDUCATION_CHECK_STANDARD
                && u.IsQualified));

            var qualificationBaseDTO = Utilities.BuildScreeningQualificationDTO();
            error = _qualificationService.SetQualificationBase(screeningDTO, ref qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(9, atomicChecks.Count(u => u.IsQualified));
            Assert.AreEqual(4, atomicChecks.Count(u => !u.IsQualified));

        }
    }
}