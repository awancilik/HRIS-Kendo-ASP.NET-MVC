using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
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
    public class Qualification
    {
        #region NUnit Test Template

        // 1. Declare global object
        private IUnitOfWork _unitOfWork;
        private IQualificationPlaceFactory _factory;
        private ICommonService _commonService;
        private IPermissionService _permissionService;
        private IScreeningService _screeningService;
        private IClientService _clientService;
        private IWebSecurity _webSecurity;
        private IQualificationService _qualificationService;
        private IUserManagementService _userManagementService;
        private ILookUpDatabaseService<PoliceDTO> _policeService;
        private ILookUpDatabaseService<ImmigrationOfficeDTO> _immigrationOfficeService;
        private ILookUpDatabaseService<CompanyDTO> _companyService;

        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager;
        private UserProfileDTO _qualityControl;
        private UserProfileDTO _screenerOffice;
        private UserProfileDTO _screenerOnField;
        private ClientCompanyDTO _clientCompany;
        private ClientContractDTO _clientContract;
        private ScreeningLevelDTO _screeningLevelDTO;
        private ScreeningLevelVersionDTO _screeningLevelVersionDTO;
        private ScreeningDTO _screeningDTO;
        private ScreeningDTO _screeningDTO2;
        private ScreeningDTO _screeningDTO3;
        private ScreeningDTO _screeningDTO4;
        private ImmigrationOfficeDTO _immigrationOfficeDTO;
        private PoliceDTO _policeDTO;
        private CompanyDTO _companyDTO;


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
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);
            _policeService = new Services.LookUpDatabase.PoliceLookUpDatabaseService(_unitOfWork, _factory);
            _immigrationOfficeService = new Services.LookUpDatabase.ImmigrationOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _companyService = new Services.LookUpDatabase.CompanyLookUpDatabaseService(_unitOfWork, _factory);
            _qualificationService = new QualificationService(_unitOfWork, _commonService);

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);

            _admin = Utilities.BuildAdminAccountSample();
            var error = _userManagementService.CreateUserProfile(ref _admin,
                    new System.Collections.Generic.List<string>(new List<string> { "Administrator" }),
                    "123456", false);
            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);
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

            _screenerOffice = Utilities.BuildScreenerAccountOfficeDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOffice,
                    new System.Collections.Generic.List<string>(new List<string> { "Screener" }),
                    "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOnField = Utilities.BuildScreenerAccountOnFieldDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOnField,
                    new System.Collections.Generic.List<string>(new List<string> { "Screener" }),
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

            _screeningDTO2 = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            _screeningDTO2.ScreeningFullName = "Screening 2";
            error = _screeningService.CreateScreening(_screeningDTO2.ScreeningLevelVersion, ref _screeningDTO2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningDTO3 = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            _screeningDTO3.ScreeningFullName = "Screening 3";
            error = _screeningService.CreateScreening(_screeningDTO3.ScreeningLevelVersion, ref _screeningDTO3);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningDTO4 = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            _screeningDTO4.ScreeningFullName = "Screening 4";
            error = _screeningService.CreateScreening(_screeningDTO4.ScreeningLevelVersion, ref _screeningDTO4);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _immigrationOfficeDTO = Utilities.BuildImmigrationOfficeDTO();
            error = _immigrationOfficeService.CreateOrEditQualificationPlace(ref _immigrationOfficeDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _policeDTO = Utilities.BuildPoliceOfficeDTO();
            error = _policeService.CreateOrEditQualificationPlace(ref _policeDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _companyDTO = Utilities.BuildCompanyDTO();
            error = _companyService.CreateOrEditQualificationPlace(ref _companyDTO);
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
        public void SetAndGetQualificationBase()
        {
            var qualificationBaseDTO = Utilities.BuildScreeningQualificationDTO();
            var error = _qualificationService.SetQualificationBase(_screeningDTO, ref qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var qualificationBaseGetDTO = _qualificationService.GetQualificationBase(_screeningDTO);
            Assert.AreEqual("Single", qualificationBaseGetDTO.ScreeningQualificationMaritalStatus);
            Assert.AreEqual(new DateTime(1985, 12, 28), qualificationBaseGetDTO.ScreeningQualificationBirthDate);
            Assert.AreEqual("Jakarta", qualificationBaseGetDTO.ScreeningQualificationBirthPlace);
            Assert.AreEqual("M", qualificationBaseGetDTO.ScreeningQualificationGender);
            Assert.AreEqual("11CK3030", qualificationBaseGetDTO.ScreeningQualificationIDCardNumber);
            Assert.AreEqual("Mother", qualificationBaseGetDTO.ScreeningQualificationRelationshipWithCandidate);
            Assert.AreEqual(14, qualificationBaseGetDTO.CurrentAddress.Location.LocationId);
            Assert.AreEqual(14, qualificationBaseGetDTO.IDCardAddress.Location.LocationId);
            Assert.AreEqual(14, qualificationBaseGetDTO.CVAddress.Location.LocationId);
            Assert.AreEqual("62-2199988888", qualificationBaseGetDTO.PersonalContactInfo.MobilePhoneNumber);
            Assert.AreEqual("62-2199988999", qualificationBaseGetDTO.PersonalContactInfo.HomePhoneNumber);
            Assert.AreEqual("My mother", qualificationBaseGetDTO.EmergencyContactPerson.ContactPersonName);
            Assert.AreEqual("62-2199988888", qualificationBaseGetDTO.EmergencyContactPerson.ContactInfo.HomePhoneNumber);
        }

        [Test]
        public void SetQualificationBaseScreeningNotExisting()
        {
            var qualificationBaseDTO = Utilities.BuildScreeningQualificationDTO();
            var error = _qualificationService.SetQualificationBase(new ScreeningDTO {ScreeningId = 20},
                ref qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.SCREENING_NOT_FOUND, error);
        }

        [Test]
        public void GetQualificationBaseScreeningNotExisting()
        {
            var qualificationBase = _qualificationService.GetQualificationBase(new ScreeningDTO { ScreeningId = 20 });
            Assert.AreEqual(null, qualificationBase);
        }


        [Test]
        public void SetAndGetQualificationPlaces()
        {
            var qualifications = new List<BaseQualificationPlaceDTO> {_policeDTO, _immigrationOfficeDTO};

            var qualificationsGet = _qualificationService.GetQualificationPlaces(_screeningDTO);
            Assert.AreEqual(0, qualificationsGet.Count());

            var error = _qualificationService.SetQualificationPlaces(_screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            qualificationsGet = _qualificationService.GetQualificationPlaces(_screeningDTO);
            Assert.AreEqual(2, qualificationsGet.Count());

            var policeDTO = qualificationsGet.Single(u => u.GetType() == typeof (PoliceDTO));
            var immigrationOfficeDTO = qualificationsGet.Single(u => u.GetType() == typeof(ImmigrationOfficeDTO));

            Assert.AreEqual(_policeDTO.QualificationPlaceId, policeDTO.QualificationPlaceId);
            Assert.AreEqual(_immigrationOfficeDTO.QualificationPlaceId, immigrationOfficeDTO.QualificationPlaceId);

            qualifications.Add(_companyDTO);
            error = _qualificationService.SetQualificationPlaces(_screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            qualificationsGet = _qualificationService.GetQualificationPlaces(_screeningDTO);

            policeDTO = qualificationsGet.Single(u => u.GetType() == typeof(PoliceDTO));
            immigrationOfficeDTO = qualificationsGet.Single(u => u.GetType() == typeof(ImmigrationOfficeDTO));
            var companyDTO = qualificationsGet.Single(u => u.GetType() == typeof(CompanyDTO));

            Assert.AreEqual(3, qualificationsGet.Count());
            Assert.AreEqual(_policeDTO.QualificationPlaceId, policeDTO.QualificationPlaceId);
            Assert.AreEqual(_immigrationOfficeDTO.QualificationPlaceId, immigrationOfficeDTO.QualificationPlaceId);
            Assert.AreEqual(_companyDTO.QualificationPlaceId, companyDTO.QualificationPlaceId);


        }

        [Test]
        public void SetQualificationPlacesScreeningNotExisting()
        {
            var qualifications = new List<BaseQualificationPlaceDTO>();
            var error = _qualificationService.SetQualificationPlaces(new ScreeningDTO{ScreeningId = 20}, qualifications);
            Assert.AreEqual(ErrorCode.SCREENING_NOT_FOUND, error);

            qualifications.Add(new CourtDTO{QualificationPlaceId = 30});
            error = _qualificationService.SetQualificationPlaces(_screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND, error);
        }

        [Test]
        public void GetWronglyQualificationPlaces()
        {
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == _screeningDTO2.ScreeningId);
            foreach (var atomicCheck in screening.AtomicCheck)
            {
                _unitOfWork.AtomicCheckRepository.Add(atomicCheck);
            }

            var qualifications = new List<BaseQualificationPlaceDTO> { _policeDTO, _immigrationOfficeDTO };

            var qualificationsGet = _qualificationService.GetQualificationPlaces(_screeningDTO2);
            Assert.AreEqual(0, qualificationsGet.Count());

            var error = _qualificationService.SetQualificationPlaces(_screeningDTO2, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            var wrongQualifiedPlaces = _qualificationService.GetWronglyQualifiedQualificationPlaces(_screeningDTO2);
            Assert.AreEqual(0, wrongQualifiedPlaces.Count());

            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO2);
            var policeAtomicCheck =
                atomicChecks.Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.POLICE_CHECK);

            error = _screeningService.AssignAtomicCheck(ref policeAtomicCheck, _screenerOnField);

            policeAtomicCheck.AtomicCheckState = (Byte) AtomicCheckStateType.WRONGLY_QUALIFIED;
            error =_screeningService.EditAtomicCheck(ref policeAtomicCheck);

            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            wrongQualifiedPlaces = _qualificationService.GetWronglyQualifiedQualificationPlaces(_screeningDTO2);
            Assert.AreEqual(1, wrongQualifiedPlaces.Count());


        }


        [Test]
        public void SetAtomicCheckAsNotApplicable()
        {
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == _screeningDTO3.ScreeningId);
            foreach (var atomicCheck in screening.AtomicCheck)
            {
                _unitOfWork.AtomicCheckRepository.Add(atomicCheck);
            }

            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO3);
            Assert.AreEqual(0, atomicChecks.Count(u => u.AtomicCheckState == (Byte)AtomicCheckStateType.NOT_APPLICABLE));


            var policeAtomicCheck =
                atomicChecks.Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.POLICE_CHECK);

            IDictionary<AtomicCheckDTO, bool> atomicCheckDTO = new Dictionary<AtomicCheckDTO, bool>();
            atomicCheckDTO[policeAtomicCheck] = true;
            _qualificationService.SetAtomicChecksAsNotApplicable(_screeningDTO3, atomicCheckDTO);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO3);
            Assert.AreEqual(1, atomicChecks.Count(u => u.AtomicCheckState == (Byte)AtomicCheckStateType.NOT_APPLICABLE));
            
            atomicCheckDTO[policeAtomicCheck] = false;
            _qualificationService.SetAtomicChecksAsNotApplicable(_screeningDTO3, atomicCheckDTO);

            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO3);
            Assert.AreEqual(0, atomicChecks.Count(u => u.AtomicCheckState == (Byte)AtomicCheckStateType.NOT_APPLICABLE));

        }


        [Test]
        public void SetAtomicCheckAlreadyQualifiedAsNotApplicable()
        {
            var screening = _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == _screeningDTO4.ScreeningId);
            foreach (var atomicCheck in screening.AtomicCheck)
            {
                _unitOfWork.AtomicCheckRepository.Add(atomicCheck);
            }

            var atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO4);
            Assert.AreEqual(0, atomicChecks.Count(u => u.AtomicCheckState == (Byte)AtomicCheckStateType.NOT_APPLICABLE));
            
            var qualifications = new List<BaseQualificationPlaceDTO> { _policeDTO, _immigrationOfficeDTO };

            var error = _qualificationService.SetQualificationPlaces(_screeningDTO4, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO4);
            var policeAtomicCheck =
                atomicChecks.Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte)TypeOfCheckEnum.POLICE_CHECK);

            IDictionary<AtomicCheckDTO, bool> atomicCheckDTO = new Dictionary<AtomicCheckDTO, bool>();
            atomicCheckDTO[policeAtomicCheck] = true;
            error =_qualificationService.SetAtomicChecksAsNotApplicable(_screeningDTO4, atomicCheckDTO);
            Assert.AreEqual(ErrorCode.ATOMIC_CHECK_NOT_APPLICABLE_UPDATE_ERROR, error);


            atomicChecks = _screeningService.GetAllAtomicChecksForScreening(_screeningDTO3);
            Assert.AreEqual(0, atomicChecks.Count(u => u.AtomicCheckState == (Byte)AtomicCheckStateType.NOT_APPLICABLE));


        }

    }
}