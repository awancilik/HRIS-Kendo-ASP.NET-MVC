using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
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
    public class Skill
    {
        #region NUnit Test Template

        private UserProfileDTO _accountManager1;
        private UserProfileDTO _accountManager2;
        private UserProfileDTO _admin;
        private CertificationPlaceDTO _certificationPlaceDTO;
        private ILookUpDatabaseService<CertificationPlaceDTO> _certificationService;
        private UserProfileDTO _client1;
        private UserProfileDTO _client2;
        private ClientCompanyDTO _clientCompany1;
        private ClientCompanyDTO _clientCompany2;
        private ClientContractDTO _clientContract1;
        private IClientService _clientService;
        private CourtDTO _commercialCourtDTO;
        private ICommonService _commonService;
        private CompanyDTO _companyDTO;
        private ILookUpDatabaseService<CompanyDTO> _companyService;
        private ILookUpDatabaseService<CourtDTO> _courtLookUpDatabaseService;
        private IDispatchingManagementService _dispatchingManagementService;
        private IQualificationPlaceFactory _factory;
        private FacultyDTO _facultyDTO;
        private ILookUpDatabaseService<FacultyDTO> _facultyService;
        private HighSchoolDTO _highSchoolDTO;
        private ILookUpDatabaseService<HighSchoolDTO> _highSchoolService;
        private UserProfileDTO _hr;
        private ImmigrationOfficeDTO _immigrationOfficeDTO;
        private ILookUpDatabaseService<ImmigrationOfficeDTO> _immigrationOfficeService;
        private IPermissionService _permissionService;
        private PoliceDTO _police1DTO;
        private PoliceDTO _police2DTO;
        private ILookUpDatabaseService<PoliceDTO> _policeService;
        private UserProfileDTO _productionManager;
        private IProfessionalQualificationService _professionalQualificationService;
        private ScreeningQualificationDTO _qualificationBaseDTO;
        private IEnumerable<BaseQualificationPlaceDTO> _qualificationPlacesDTO;
        private IQualificationService _qualificationService;
        private UserProfileDTO _qualifier;
        private UserProfileDTO _qualityControl1;
        private UserProfileDTO _qualityControl2;
        private UserProfileDTO _screenerOffice1;
        private UserProfileDTO _screenerOffice2;
        private UserProfileDTO _screenerOffice3;
        private UserProfileDTO _screenerOffice4;
        private UserProfileDTO _screenerOffice5;
        private UserProfileDTO _screenerOffice6;
        private UserProfileDTO _screenerOnField1;
        private UserProfileDTO _screenerOnField2;
        private UserProfileDTO _screenerOnField3;
        private ScreeningDTO _screeningDTO;
        private ScreeningLevelDTO _screeningLevel1;
        private ScreeningLevelVersionDTO _screeningLevelVersion1;
        private IScreeningService _screeningService;
        private ISystemTimeService _systemTimeService;
        private IUnitOfWork _unitOfWork;
        private IUserManagementService _userManagementService;
        private IWebSecurity _webSecurity;

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
            ErrorCode error = _qualificationService.SetQualificationBase(screeningDTO, ref qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // All the atomic check are now qualified
            var qualifications = new List<BaseQualificationPlaceDTO>
            {
                _companyDTO,
                _police1DTO,
                _immigrationOfficeDTO,
                _highSchoolDTO,
                _facultyDTO,
                _commercialCourtDTO,
                _certificationPlaceDTO
            };

            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            return screeningDTO;
        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
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
        public void DEF_Value_Should_Return_Default_Value()
        {
            //Arrange
            TypeOfCheck typeOfCheckWhichHasQualificationPlace =
                _unitOfWork.TypeOfCheckRepository.First(e => e.HasQualificationPlace());
            webpages_UserProfile screenerOffice =
                _unitOfWork.UserProfileRepository.First(
                    e => e.IsScreener() && e.ScreenerCategory == TypeOfCheckMeta.kOfficeCategory);
            QualificationPlace qualificayPlaceBelongsToTypeOfCheck = _unitOfWork.QualificationPlaceRepository.First(e =>
                e.GetTypeOfChecksCompatible()
                    .Any(f => Convert.ToByte(f) == typeOfCheckWhichHasQualificationPlace.TypeOfCheckCode));

            IEnumerable<AtomicCheckDTO> atomicChecksFromTypeOfCheck =
                _screeningService.GetAllAtomicChecksForScreening(_screeningDTO);
            
            AtomicCheckDTO atomicCheckRelated =
                atomicChecksFromTypeOfCheck.First(
                    e => e.QualificationPlace.QualificationPlaceId == 
                        qualificayPlaceBelongsToTypeOfCheck.QualificationPlaceId);

            var defaultValue = new DefaultMatrix
            {
                TypeOfCheck = typeOfCheckWhichHasQualificationPlace,
                webpages_UserProfile = screenerOffice,
                DefaultValue = 50
            };
            _unitOfWork.DefaultMatrixRepository.Add(defaultValue);

            int key = (SkillMatrix.kCategories.FirstOrDefault(f => f.Value == TypeOfCheckMeta.kOfficeCategory).Key);
            SkillMatrix skillMatrix =
                _unitOfWork.SkillMatrixRepository.First(e => e.webpages_UserProfile.UserId == screenerOffice.UserId &&
                                                             e.QualificationPlace.QualificationPlaceId ==
                                                             qualificayPlaceBelongsToTypeOfCheck.QualificationPlaceId);
            if (skillMatrix == null)
            {
                _unitOfWork.SkillMatrixRepository.Add(new SkillMatrix
                {
                    QualificationPlace = qualificayPlaceBelongsToTypeOfCheck,
                    webpages_UserProfile = screenerOffice,
                    SkillValue = SkillMatrix.kDefaultValue,
                    SkillMeta = key
                });
            }
            else
            {
                skillMatrix.SkillValue = SkillMatrix.kDefaultValue;
            }
            _unitOfWork.Commit();

            //Act
            IDictionary<int, float> dictionary =
                _dispatchingManagementService.GetScreenerSkillWeight(atomicCheckRelated);
            float actualValue = dictionary[screenerOffice.UserId];
            float expectedValue =
                Convert.ToSingle(_dispatchingManagementService.GetDefaultMatrixValue(screenerOffice.UserId,
                    typeOfCheckWhichHasQualificationPlace.TypeOfCheckId));

            //Assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Assigned_Value_Should_Return_Properly()
        {
            //Arrange
            int properValue = 55;
            TypeOfCheck typeOfCheckWhichHasQualificationPlace =
                _unitOfWork.TypeOfCheckRepository.First(e => e.HasQualificationPlace());
            webpages_UserProfile screenerOffice =
                _unitOfWork.UserProfileRepository.First(
                    e => e.IsScreener() && e.ScreenerCategory == TypeOfCheckMeta.kOfficeCategory);
            QualificationPlace qualificayPlaceBelongsToTypeOfCheck = _unitOfWork.QualificationPlaceRepository.First(e =>
                e.GetTypeOfChecksCompatible()
                    .Any(f => Convert.ToByte(f) == typeOfCheckWhichHasQualificationPlace.TypeOfCheckCode));

            IEnumerable<AtomicCheckDTO> atomicChecksFromTypeOfCheck =
                _screeningService.GetAllAtomicChecksForScreening(_screeningDTO);
            AtomicCheckDTO atomicCheckRelated =
                atomicChecksFromTypeOfCheck.First(
                    e =>
                        e.QualificationPlace.QualificationPlaceId ==
                        qualificayPlaceBelongsToTypeOfCheck.QualificationPlaceId);

            int key = (SkillMatrix.kCategories.FirstOrDefault(f => f.Value == TypeOfCheckMeta.kOfficeCategory).Key);

            SkillMatrix skillMatrix =
                _unitOfWork.SkillMatrixRepository.First(e => e.webpages_UserProfile.UserId == screenerOffice.UserId &&
                                                             e.QualificationPlace.QualificationPlaceId ==
                                                             qualificayPlaceBelongsToTypeOfCheck.QualificationPlaceId);
            if (skillMatrix == null)
            {
                skillMatrix = new SkillMatrix
                {
                    QualificationPlace = qualificayPlaceBelongsToTypeOfCheck,
                    webpages_UserProfile = screenerOffice,
                    SkillValue = properValue,
                    SkillMeta = key
                };
                _unitOfWork.SkillMatrixRepository.Add(skillMatrix);

            }
            else
            {
                skillMatrix.SkillValue = properValue;
            }
            _unitOfWork.Commit();

            //Act
            IDictionary<int, float> dictionary =
                _dispatchingManagementService.GetScreenerSkillWeight(atomicCheckRelated);
            float actualValue = dictionary[screenerOffice.UserId];
            float expectedValue = Convert.ToSingle(properValue);
            //Assert

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void IMP_Value_Should_Return_Minus_1()
        {
            //Arrange
            TypeOfCheck typeOfCheckWhichHasQualificationPlace =
                _unitOfWork.TypeOfCheckRepository.First(e => e.HasQualificationPlace());
            webpages_UserProfile screenerOffice =
                _unitOfWork.UserProfileRepository.First(
                    e => e.IsScreener() && e.ScreenerCategory == TypeOfCheckMeta.kOfficeCategory);
            QualificationPlace qualificayPlaceBelongsToTypeOfCheck = _unitOfWork.QualificationPlaceRepository.First(e =>
                e.GetTypeOfChecksCompatible()
                    .Any(f => Convert.ToByte(f) == typeOfCheckWhichHasQualificationPlace.TypeOfCheckCode));

            IEnumerable<AtomicCheckDTO> atomicChecksFromTypeOfCheck =
                _screeningService.GetAllAtomicChecksForScreening(_screeningDTO);
            AtomicCheckDTO atomicCheckRelated =
                atomicChecksFromTypeOfCheck.First(
                    e =>
                        e.QualificationPlace.QualificationPlaceId ==
                        qualificayPlaceBelongsToTypeOfCheck.QualificationPlaceId);

            int key = (SkillMatrix.kCategories.FirstOrDefault(f => f.Value == TypeOfCheckMeta.kOfficeCategory).Key);

            SkillMatrix skillMatrix =
                _unitOfWork.SkillMatrixRepository.First(e => e.webpages_UserProfile.UserId == screenerOffice.UserId &&
                                                             e.QualificationPlace.QualificationPlaceId ==
                                                             qualificayPlaceBelongsToTypeOfCheck.QualificationPlaceId);
            if (skillMatrix == null)
            {
                skillMatrix = new SkillMatrix
                {
                    QualificationPlace = qualificayPlaceBelongsToTypeOfCheck,
                    webpages_UserProfile = screenerOffice,
                    SkillValue = SkillMatrix.kImpossibleValue,
                    SkillMeta = key
                };
                _unitOfWork.SkillMatrixRepository.Add(skillMatrix);
             
            }
            else
            {
                skillMatrix.SkillValue = SkillMatrix.kImpossibleValue;
            }
            _unitOfWork.Commit();

            //Act
            IDictionary<int, float> dictionary =
                _dispatchingManagementService.GetScreenerSkillWeight(atomicCheckRelated);
            float actualValue = dictionary[screenerOffice.UserId];
            float expectedValue = Convert.ToSingle(SkillMatrix.kImpossibleValue);
            //Assert

            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}