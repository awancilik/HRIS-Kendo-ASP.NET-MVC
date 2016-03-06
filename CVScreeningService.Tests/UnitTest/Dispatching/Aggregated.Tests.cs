using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningCore.Models.AtomicCheckValidationState;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Client;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.DispatchingManagement;
using CVScreeningService.Services.LookUpDatabase;
using CVScreeningService.Services.Notification;
using CVScreeningService.Services.Permission;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using FakeItEasy;
using NUnit.Framework;

namespace CVScreeningService.Tests.UnitTest.Dispatching
{
    [TestFixture]
    public class Aggregated
    {
        // 1. Declare global object
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
        }

        private IUnitOfWork _unitOfWork;
        private IQualificationPlaceFactory _factory;
        private ICommonService _commonService;
        private IScreeningService _screeningService;
        private IClientService _clientService;
        private IPermissionService _permissionService;
        private ISystemTimeService _systemTimeService;
        private IQualificationService _qualificationService;
        private IUserManagementService _userManagementService;
        private ILookUpDatabaseService<PoliceDTO> _policeService;
        private ILookUpDatabaseService<ImmigrationOfficeDTO> _immigrationOfficeService;
        private ILookUpDatabaseService<CompanyDTO> _companyService;
        private ILookUpDatabaseService<HighSchoolDTO> _highSchoolService;
        private ILookUpDatabaseService<FacultyDTO> _facultyService;
        private ILookUpDatabaseService<CertificationPlaceDTO> _certificationService;
        private ILookUpDatabaseService<CourtDTO> _courtLookUpDatabaseService;
        private IProfessionalQualificationService _professionalQualificationService;
        private IDispatchingManagementService _dispatchingManagementService;
        private IWebSecurity _webSecurity;

        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager1;
        private UserProfileDTO _qualityControl1;
        private UserProfileDTO _accountManager2;
        private UserProfileDTO _qualityControl2;
        private UserProfileDTO _productionManager;
        private UserProfileDTO _qualifier;
        private UserProfileDTO _screenerOffice1;
        private UserProfileDTO _screenerOffice2;
        private UserProfileDTO _screenerOffice3;
        private UserProfileDTO _screenerOffice4;
        private UserProfileDTO _screenerOffice5;
        private UserProfileDTO _screenerOffice6;
        private UserProfileDTO _screenerOnField1;
        private UserProfileDTO _screenerOnField2;
        private UserProfileDTO _screenerOnField3;
        private UserProfileDTO _client1;
        private UserProfileDTO _client2;
        private UserProfileDTO _hr;

        private ClientCompanyDTO _clientCompany1;
        private ClientCompanyDTO _clientCompany2;
        private ClientContractDTO _clientContract1;
        private ScreeningLevelDTO _screeningLevel1;
        private ScreeningLevelVersionDTO _screeningLevelVersion1;
        private ImmigrationOfficeDTO _immigrationOfficeDTO;
        private PoliceDTO _police1DTO;
        private PoliceDTO _police2DTO;
        private CompanyDTO _companyDTO;
        private CompanyDTO _currentCompanyDTO;
        private ScreeningQualificationDTO _qualificationBaseDTO;
        private HighSchoolDTO _highSchoolDTO;
        private FacultyDTO _facultyDTO;
        private CertificationPlaceDTO _certificationPlaceDTO;
        private CourtDTO _commercialCourtDTO;
        private IEnumerable<BaseQualificationPlaceDTO> _qualificationPlacesDTO;

        private ScreeningDTO _screeningDTO;
        private string _AtomicCheckSubDistrictName;

        // 2. Runs Once Before All of The Following Methods
        // Declare Global Objects Which Are Global For Test Class, e.g. Mock Objects
        [TestFixtureSetUp]
        public void RunsOnceBeforeAll()
        {
            var notificationService = A.Fake<INotificationService>();
            _unitOfWork = new InMemoryUnitOfWork();
            _systemTimeService = A.Fake<ISystemTimeService>();
            A.CallTo(() => _systemTimeService.GetCurrentDateTime()).Returns(new DateTime(2014, 12, 1));
            _webSecurity = A.Fake<IWebSecurity>();
            A.CallTo(() => _webSecurity.GetCurrentUserName()).Returns("Administrator");

            _factory = new QualificationPlaceFactory(_unitOfWork);
            _commonService = new CommonService(_unitOfWork);
            _userManagementService = new UserManagementService(_unitOfWork, _commonService, _systemTimeService);
            _clientService = new ClientService(_unitOfWork, _commonService, _userManagementService, _permissionService);
            _policeService = new PoliceLookUpDatabaseService(_unitOfWork, _factory);
            _immigrationOfficeService = new ImmigrationOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _companyService = new CompanyLookUpDatabaseService(_unitOfWork, _factory);
            _highSchoolService = new HighSchoolLookUpDatabaseService(_unitOfWork, _factory);
            _facultyService = new FacultyLookUpDatabaseService(_unitOfWork, _factory);
            _certificationService = new CertificationPlaceLookUpDatabaseService(_unitOfWork, _factory);
            _courtLookUpDatabaseService = new CourtLookUpDatabaseService(_unitOfWork, _factory);
            _qualificationService = new QualificationService(_unitOfWork, _commonService);
            _professionalQualificationService = new ProfessionalQualificationService(_unitOfWork);
            _screeningService = new ScreeningService(_unitOfWork, _permissionService, _userManagementService, _systemTimeService, _webSecurity, notificationService);
            _dispatchingManagementService = new DispatchingManagementService(_unitOfWork, _userManagementService,
                _systemTimeService, _screeningService);

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);

            _admin = Utilities.BuildAdminAccountSample();

            ErrorCode error = _userManagementService.CreateUserProfile(ref _admin,
                new List<string>(new List<string> {"Administrator"}),
                "123456", false);
            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);
            _screeningService = new ScreeningService(_unitOfWork, _permissionService, _userManagementService,
                _systemTimeService, _webSecurity, notificationService);

            InitializeTest();
            _screeningDTO = CreateAndQualifyScreening();
            Utilities.InitScreeners(_unitOfWork);
            Utilities.InitSkillMatrix(_unitOfWork);
        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks

        private void InitializeTest()
        {
            Utilities.InitRoles(_userManagementService);
            Utilities.InitTypeOfCheck(_screeningService, _unitOfWork);
            Utilities.InitPermissionForProductionManager(_unitOfWork);
            Utilities.InitPermissionForQualifier(_unitOfWork);

            // Create sample user account for testing
            _accountManager1 = Utilities.BuildAccountManagerAccountSample();
            ErrorCode error = _userManagementService.CreateUserProfile(ref _accountManager1,
                new List<string>(new List<string> {"Account manager"}), "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _accountManager2 = Utilities.BuildAccountManagerAccountSample();
            _accountManager2.UserName = "am2@am.com";
            error = _userManagementService.CreateUserProfile(ref _accountManager2,
                new List<string>(new List<string> {"Account manager"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl1 = Utilities.BuildQualityControlAccountSample();
            error = _userManagementService.CreateUserProfile(ref _qualityControl1,
                new List<string>(new List<string> {"Quality control"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl2 = Utilities.BuildQualityControlAccountSample();
            _qualityControl2.UserName = "qc2@qc.com";
            error = _userManagementService.CreateUserProfile(ref _qualityControl2,
                new List<string>(new List<string> {"Quality control"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _productionManager = Utilities.BuildProductionManagerAccountSample();
            error = _userManagementService.CreateUserProfile(ref _productionManager,
                new List<string>(new List<string> {"Production manager"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualifier = Utilities.BuildQualifierAccountSample();
            error = _userManagementService.CreateUserProfile(ref _qualifier,
                new List<string>(new List<string> {"Qualifier"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice1 = Utilities.BuildScreenerAccountOfficeDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOffice1,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOffice2 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice2.UserName = "screener2@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice2,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOnField1 = Utilities.BuildScreenerAccountOnFieldDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOnField1,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOnField2 = Utilities.BuildScreenerAccountOnFieldDTO();
            _screenerOnField2.UserName = "screeneronfield2@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOnField2,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOffice3 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice3.UserName = "screener3@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice3,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice4 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice4.UserName = "screener4@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice4,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice5 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice5.UserName = "screener5@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice5,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice6 = Utilities.BuildScreenerAccountOfficeDTO();
            _screenerOffice6.UserName = "screener6@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOffice6,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _screenerOnField3 = Utilities.BuildScreenerAccountOnFieldDTO();
            _screenerOnField3.UserName = "screeneronfield3@office.com";
            error = _userManagementService.CreateUserProfile(ref _screenerOnField3,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            _hr = Utilities.BuildHrAccountSample();
            error = _userManagementService.CreateUserProfile(ref _hr,
                new List<string>(new List<string> {"HR"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany1 = Utilities.BuildClientCompanySample();
            _clientCompany1.AccountManagers = new List<UserProfileDTO> {_accountManager1};
            error = _clientService.CreateClientCompany(ref _clientCompany1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _client1 = Utilities.BuildClientAccountSample();
            error = _userManagementService.CreateClientProfile(ref _client1, _clientCompany1, "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany2 = Utilities.BuildClientCompanySample();
            _clientCompany2.ClientCompanyName = "My client company 2";
            _clientCompany2.AccountManagers = new List<UserProfileDTO> {_accountManager2};
            error = _clientService.CreateClientCompany(ref _clientCompany2);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _client2 = Utilities.BuildClientAccountSample();
            _client2.UserName = "myclient2@mytest.com";
            error = _userManagementService.CreateClientProfile(ref _client2, _clientCompany2, "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientContract1 = Utilities.BuildClientContractDTO();
            error = _clientService.CreateClientContract(_clientCompany1, ref _clientContract1);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningLevel1 = Utilities.BuildScreeningLevelDTO();

            _screeningLevelVersion1 = Utilities.BuildScreeningLevelVersionDTO();
            _screeningLevelVersion1.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();
            _screeningLevelVersion1.ScreeningLevelVersionTurnaroundTime = 10;

            error = _clientService.CreateScreeningLevel(_clientContract1, ref _screeningLevel1,
                ref _screeningLevelVersion1);
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

            _currentCompanyDTO = Utilities.BuildCurrentCompanyDTO();
            error = _companyService.CreateOrEditQualificationPlace(ref _currentCompanyDTO);
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

            ProfessionalQualificationDTO professionalQualificationDTO = Utilities.BuildProfessionalQualificationDTO();
            _certificationPlaceDTO = Utilities.BuildCertificationPlaceDTO();

            error = _certificationService.CreateOrEditQualificationPlace(ref _certificationPlaceDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            professionalQualificationDTO.QualificationPlace = new List<CertificationPlaceDTO>
            {
                _certificationPlaceDTO
            };

            error = _professionalQualificationService.CreateOrUpdateProfessionalQualification(
                ref professionalQualificationDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Retrieve screening directly from repository
            ProfessionalQualification professionalQualification =
                _unitOfWork.ProfessionalQualificationRepository.Single(
                    u => u.ProfessionalQualificationId == professionalQualificationDTO.ProfessionalQualificationId);

            _commercialCourtDTO = Utilities.BuildCommercialCourtDTO();
            error = _courtLookUpDatabaseService.CreateOrEditQualificationPlace(ref _commercialCourtDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }


        /// <summary>
        ///     Create a screening the application, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateScreening()
        {
            // Create screening
            ScreeningDTO screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersion1, _qualityControl1);
            ErrorCode error = _screeningService.CreateScreening(screeningDTO.ScreeningLevelVersion, ref screeningDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Retrieve screening directly from repository
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);
            foreach (AtomicCheck atomicCheck in screening.AtomicCheck)
            {
                _unitOfWork.AtomicCheckRepository.Add(atomicCheck);
            }
            return screeningDTO;
        }


        /// <summary>
        ///     Create a screening the application and qualify it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateAndQualifyScreening()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            // Qualification started
            ScreeningQualificationDTO qualificationBaseDTO = Utilities.BuildScreeningQualificationDTO();
            ErrorCode error = _qualificationService
                .SetQualificationBase(screeningDTO, ref qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // All the atomic check are now qualified
            var qualifications = new List<BaseQualificationPlaceDTO>
            {
                _currentCompanyDTO,
                _companyDTO,
                _police1DTO,
                _immigrationOfficeDTO,
                _highSchoolDTO,
                _facultyDTO,
                _commercialCourtDTO,
                _certificationPlaceDTO
            };

            _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            return screeningDTO;
        }

        [Test]
        public void Best_screener_on_already_assigned_test_should_be_chosen()
        {
            // arrange 
            Utilities.InitScreeners(_unitOfWork);
            Utilities.InitSkillMatrix(_unitOfWork);
            Utilities.InitAtomicChecks(_unitOfWork);

            IEnumerable<AtomicCheck> atomicChecks = _unitOfWork.AtomicCheckRepository.GetAll();

            webpages_UserProfile screenerE =
                _unitOfWork.UserProfileRepository.GetAll().FirstOrDefault(e => e.FullName == "screenerE");
            AtomicCheck atomicCheck =
                atomicChecks.FirstOrDefault(e => e.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckMeta.Any(
                    f => f.TypeOfCheckMetaKey == TypeOfCheckMeta.kAlreadyAssignedCriterion
                         && f.TypeOfCheckMetaValue == TypeOfCheckMeta.kYesValue)
                                                 && e.AtomicCheckCategory == TypeOfCheckMeta.kOnFieldCategory);
            AtomicCheckDTO atomicCheckDTO =
                _screeningService.GetAtomicCheck(atomicCheck.AtomicCheckId);

            //assign to screener E
            _screeningService.AssignAtomicCheck(ref atomicCheckDTO,
                _userManagementService.GetUserProfileById(screenerE.UserId));

            //add another atomic check
            var anotherAtomicCheckWithSameQualificationPlace = new AtomicCheck
            {
                AtomicCheckCategory = atomicCheck.AtomicCheckCategory,
                Attachment = atomicCheck.Attachment,
                AtomicCheckState = atomicCheck.AtomicCheckState,
                QualificationPlace = atomicCheck.QualificationPlace,
                TypeOfCheckScreeningLevelVersion = atomicCheck.TypeOfCheckScreeningLevelVersion,
                AtomicCheckValidationState = atomicCheck.AtomicCheckValidationState,
                AtomicCheckType = atomicCheck.AtomicCheckType,
                Permission = atomicCheck.Permission,
                Screening = atomicCheck.Screening
            };
            _unitOfWork.AtomicCheckRepository.Add(anotherAtomicCheckWithSameQualificationPlace);
            _unitOfWork.Commit();

            //set atomic check DTO from another atomic check with same qualification place
            atomicCheckDTO = _screeningService.GetAtomicCheck(anotherAtomicCheckWithSameQualificationPlace.AtomicCheckId);

            // act
            IDictionary<int, float> dictionary =
                _dispatchingManagementService.GetAggregatedValues(atomicCheckDTO);

            float highestScreenerValue = dictionary.Max(e => e.Value);
            int highestScreenerId = dictionary.First(e => e.Value.Equals(highestScreenerValue)).Key;

            // assert
            int expectedScreenerId =
                _unitOfWork.UserProfileRepository.First(e => e.FullName == "screenerE").UserId;
            Assert.AreEqual(expectedScreenerId, highestScreenerId);

            foreach (AtomicCheck atomCheck in _unitOfWork.AtomicCheckRepository.GetAll().Reverse())
            {
                if (atomCheck.Equals(anotherAtomicCheckWithSameQualificationPlace))
                    _unitOfWork.AtomicCheckRepository.Delete(atomCheck);
            }
        }


        [Test]
        public void Best_screener_on_availability_test_should_be_chosen()
        {
            // arrange 
            Utilities.InitScreeners(_unitOfWork);
            Utilities.InitSkillMatrix(_unitOfWork);
            Utilities.InitAtomicChecks(_unitOfWork);

            IEnumerable<AtomicCheck> atomicChecks = _unitOfWork.AtomicCheckRepository.GetAll();

            // Get an atomic check which has geographical and belongs to on field category
            AtomicCheck atomicCheck =
                atomicChecks.FirstOrDefault(
                    e =>
                        e.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckMeta.Any(
                            f => f.TypeOfCheckMetaKey == TypeOfCheckMeta.kAvailabilityCriterion
                                 && f.TypeOfCheckMetaValue == TypeOfCheckMeta.kYesValue)
                        && e.AtomicCheckCategory == TypeOfCheckMeta.kOnFieldCategory);

            // Set the holiday of screener D with no holiday (related to availibility)
            var userLeave = new UserLeaveDTO
            {
                UserLeaveName = A.Dummy<string>(),
                UserLeaveStartDate = new DateTime(2014, 11, 20),
                UserLeaveEndDate = new DateTime(2014, 12, 10),
                UserLeaveRemarks = A.Dummy<string>()
            };

            foreach (
                UserProfileDTO screener in
                    _userManagementService.GetAllUserScreeners().Where(screener => screener.FullName != "screenerD"))
            {
                _userManagementService.CreateLeave(screener, ref userLeave);
            }
            _unitOfWork.Commit();

            AtomicCheckDTO atomicCheckDTO = _screeningService.GetAtomicCheck(atomicCheck.AtomicCheckId);

            // act
            IDictionary<int, float> dictionary =
                _dispatchingManagementService.GetAggregatedValues(atomicCheckDTO);

            float highestScreenerValue = dictionary.Max(e => e.Value);
            int highestScreenerId = dictionary.First(e => e.Value.Equals(highestScreenerValue)).Key;

            // assert
            int expectedScreenerId =
                _unitOfWork.UserProfileRepository.First(e => e.FullName == "screenerD").UserId;
            Assert.AreEqual(expectedScreenerId, highestScreenerId);
        }

        [Test]
        public void Best_screener_on_geographical_test_should_be_chosen()
        {
            // arrange 
            Utilities.InitScreeners(_unitOfWork);
            Utilities.InitSkillMatrix(_unitOfWork);
            Utilities.InitAtomicChecks(_unitOfWork);

            IEnumerable<AtomicCheck> atomicChecks = _unitOfWork.AtomicCheckRepository.GetAll();

            // Get an atomic check which has geographical and belongs to on field category
            AtomicCheck atomicCheck =
                atomicChecks.FirstOrDefault(
                    e => e.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckMeta.Any(
                            f => f.TypeOfCheckMetaKey == TypeOfCheckMeta.kGeographicalCriterion)
                        && e.AtomicCheckCategory == TypeOfCheckMeta.kOnFieldCategory);

            // Set the address of screener B with the closest address
            webpages_UserProfile screenerB = _unitOfWork.UserProfileRepository.First(
                e => e.FullName == "screenerB");
            screenerB.Address = atomicCheck.QualificationPlace.Address;
            _unitOfWork.Commit();

            AtomicCheckDTO atomicCheckDTO = _screeningService.GetAtomicCheck(atomicCheck.AtomicCheckId);

            // act
            IDictionary<int, float> dictionary =
                _dispatchingManagementService.GetAggregatedValues(atomicCheckDTO);
            float highestScreenerValue = dictionary.Max(e => e.Value);
            int highestScreenerId = dictionary.First(e => e.Value.Equals(highestScreenerValue)).Key;

            // assert
            int expectedScreenerId = _unitOfWork.UserProfileRepository.First(e => e.FullName == "screenerB").UserId;
            Assert.AreEqual(expectedScreenerId, highestScreenerId);
        }

        [Test]
        public void Best_screener_on_skill_test_should_be_chosen()
        {
            // arrange 
            Utilities.InitScreeners(_unitOfWork);
            Utilities.InitSkillMatrix(_unitOfWork);
            Utilities.InitAtomicChecks(_unitOfWork);

            IEnumerable<AtomicCheck> atomicChecks = _unitOfWork.AtomicCheckRepository.GetAll();
            AtomicCheck atomicCheck =
                atomicChecks.FirstOrDefault(
                    e =>
                        e.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckMeta.Any(
                            f => f.TypeOfCheckMetaKey == TypeOfCheckMeta.kSkillCriterion)
                        && e.AtomicCheckCategory == TypeOfCheckMeta.kOfficeCategory);

            QualificationPlace qualificationPlace = atomicCheck.QualificationPlace;
            SkillMatrix skillMatrix =
                _unitOfWork.SkillMatrixRepository.First(
                    e => e.webpages_UserProfile.FullName == "screenerA"
                         && e.SkillMeta == 1
                         && e.QualificationPlace.QualificationPlaceId == qualificationPlace.QualificationPlaceId);

            skillMatrix.SkillValue = 90; //set skill of screener A with the highest value
            _unitOfWork.Commit();

            AtomicCheckDTO atomicCheckDTO = _screeningService.GetAtomicCheck(atomicCheck.AtomicCheckId);
            
            // act
            IDictionary<int, float> dictionary =
                _dispatchingManagementService.GetAggregatedValues(atomicCheckDTO);

            float highestScreenerValue = dictionary.Max(e => e.Value);
            int highestScreenerId = dictionary.First(e => e.Value.Equals(highestScreenerValue)).Key;

            // assert
            int expectedScreenerId =
                _unitOfWork.UserProfileRepository.First(e => e.FullName == "screenerA").UserId;
            Assert.AreEqual(expectedScreenerId, highestScreenerId);
        }


        [Test]
        public void Best_screener_on_workload_test_should_be_chosen()
        {
            // arrange 
            Utilities.InitScreeners(_unitOfWork);
            Utilities.InitSkillMatrix(_unitOfWork);
            Utilities.InitAtomicChecks(_unitOfWork);


            IEnumerable<AtomicCheck> atomicChecks = _unitOfWork.AtomicCheckRepository.GetAll();


            foreach (
                UserProfileDTO screenerDTO in
                    _userManagementService.GetAllUserScreeners().Where(e => e.FullName != "screenerC"))
            {
                AtomicCheck atomicCheckNotProcessed = atomicChecks.FirstOrDefault(e => !e.IsAssigned()
                                                                                       &&
                                                                                       e.AtomicCheckCategory ==
                                                                                       screenerDTO.ScreenerCategory);
                AtomicCheckDTO atomicCheckNotProcessedDTO =
                    _screeningService.GetAtomicCheck(atomicCheckNotProcessed.AtomicCheckId);
                _screeningService.AssignAtomicCheck(ref atomicCheckNotProcessedDTO,
                    _userManagementService.GetUserProfileById(screenerDTO.UserId));
            }
            _unitOfWork.Commit();

            webpages_UserProfile screenerC =
                _unitOfWork.UserProfileRepository.GetAll().FirstOrDefault(e => e.FullName == "screenerC");
            AtomicCheck atomicCheck =
                atomicChecks.FirstOrDefault(e => !e.IsAssigned() && e.AtomicCheckCategory == screenerC.ScreenerCategory);
            AtomicCheckDTO atomicCheckDTO = _screeningService.GetAtomicCheck(atomicCheck.AtomicCheckId);

            // act
            IDictionary<int, float> dictionary =
                _dispatchingManagementService.GetAggregatedValues(atomicCheckDTO);

            float highestScreenerValue = dictionary.Max(e => e.Value);
            int highestScreenerId = dictionary.First(e => e.Value.Equals(highestScreenerValue)).Key;

            // assert
            int expectedScreenerId =
                _unitOfWork.UserProfileRepository.First(e => e.FullName == "screenerC").UserId;
            Assert.AreEqual(expectedScreenerId, highestScreenerId);
        }

        [Test]
        public void Specific_case_should_be_handled()
        {
            // arrange 
            Utilities.InitScreeners(_unitOfWork);
            Utilities.InitSkillMatrix(_unitOfWork);
            Utilities.InitAtomicChecks(_unitOfWork);

            var screener = _unitOfWork.UserProfileRepository.First(e => e.FullName == "screenerF");
            var screening = _unitOfWork.ScreeningRepository.First(e => e.AtomicCheck.Count > 0);

            // Get existing employment check
            var employmentCheck =
                screening.AtomicCheck.First(
                    e =>
                        e.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckCode ==
                        (byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD);
            
            // Set directly screener
            employmentCheck.Screener = screener;
            // Create new atomic check media
            var atomicCheckMedia = new AtomicCheck()
            {
                TypeOfCheckScreeningLevelVersion = new TypeOfCheckScreeningLevelVersion
                {
                    TypeOfCheck = _unitOfWork.TypeOfCheckRepository
                        .First(e => e.TypeOfCheckCode == (byte)TypeOfCheckEnum.MEDIA_CHECK_SIMPLIFIED)
                },
                Screening = screening,
                AtomicCheckState = (int)AtomicCheckStateType.NEW,
                AtomicCheckValidationState = (int)AtomicCheckValidationStateType.NOT_PROCESSED
            };
            _unitOfWork.AtomicCheckRepository.Add(atomicCheckMedia);
            screening.AtomicCheck.Add(atomicCheckMedia);
            _unitOfWork.Commit();

            AtomicCheckDTO atomicCheckMediaDTO =
                _screeningService.GetAtomicCheck(atomicCheckMedia.AtomicCheckId);
            var screenerResult = _dispatchingManagementService.GetScreenerForSpecificCase(atomicCheckMediaDTO);
            Assert.AreEqual(screener.UserName, screenerResult.UserName);
        }
    }
}