using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningCore.Models.AtomicCheckValidationState;
using CVScreeningCore.Models.ScreeningState;
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
    public class Status
    {
        // 1. Declare global object
        [SetUp]
        public void RunOnceBeforeEachTest()
        {
            IEnumerable<CVScreeningCore.Models.Screening> screenings = _unitOfWork.ScreeningRepository.GetAll();
            foreach (CVScreeningCore.Models.Screening screening in screenings.Reverse())
            {
                _unitOfWork.ScreeningRepository.Delete(screening);
            }
        }

        [TearDown]
        public void RunOnceAfterEachTests()
        {
        }

        private IUnitOfWork _unitOfWork;
        private IQualificationPlaceFactory _factory;
        private ICommonService _commonService;
        private IScreeningService _screeningService;
        private IClientService _clientService;
        private IPermissionService _permissionService;
        private IQualificationService _qualificationService;
        private IWebSecurity _webSecurity;
        private IUserManagementService _userManagementService;
        private ILookUpDatabaseService<PoliceDTO> _policeService;
        private ILookUpDatabaseService<ImmigrationOfficeDTO> _immigrationOfficeService;
        private ILookUpDatabaseService<CompanyDTO> _companyService;
        private ILookUpDatabaseService<HighSchoolDTO> _highSchoolService;
        private ILookUpDatabaseService<FacultyDTO> _facultyService;
        private ILookUpDatabaseService<CertificationPlaceDTO> _certificationService;
        private ILookUpDatabaseService<CourtDTO> _courtLookUpDatabaseService;
        private IProfessionalQualificationService _professionalQualificationService;
        private ILookUpDatabaseService<PopulationOfficeDTO> _populationOfficeService;

        private UserProfileDTO _admin;
        private UserProfileDTO _accountManager;
        private UserProfileDTO _qualityControl;
        private UserProfileDTO _screenerOffice;
        private UserProfileDTO _screenerOnField;
        private ClientCompanyDTO _clientCompany;
        private ClientContractDTO _clientContract;
        private ScreeningLevelDTO _screeningLevelDTO;
        private ScreeningLevelVersionDTO _screeningLevelVersionDTO;
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
        private PopulationOfficeDTO _populationOfficeDTO;


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
            _policeService = new PoliceLookUpDatabaseService(_unitOfWork, _factory);
            _immigrationOfficeService = new ImmigrationOfficeLookUpDatabaseService(_unitOfWork, _factory);
            _companyService = new CompanyLookUpDatabaseService(_unitOfWork, _factory);
            _highSchoolService = new HighSchoolLookUpDatabaseService(_unitOfWork, _factory);
            _facultyService = new FacultyLookUpDatabaseService(_unitOfWork, _factory);
            _certificationService = new CertificationPlaceLookUpDatabaseService(_unitOfWork, _factory);
            _courtLookUpDatabaseService = new CourtLookUpDatabaseService(_unitOfWork, _factory);
            _qualificationService = new QualificationService(_unitOfWork, _commonService);
            _professionalQualificationService = new ProfessionalQualificationService(_unitOfWork);
            _populationOfficeService = new PopulationOfficeLookUpDatabaseService(_unitOfWork, _factory);

            Utilities.InitLocations(_commonService);
            Utilities.InitRoles(_userManagementService);

            _admin = Utilities.BuildAdminAccountSample();
            ErrorCode error = _userManagementService.CreateUserProfile(ref _admin,
                new List<string>(new List<string> {"Administrator"}),
                "123456", false);
            _permissionService = new PermissionService(_unitOfWork, _admin.UserName);
            _screeningService = new ScreeningService(
                _unitOfWork, _permissionService, _userManagementService, systemTime, _webSecurity, notificationService);

            InitializeTest();
        }

        // 3. Runs Twice; Once Before Test Case 1 and Once Before Test Case 2
        // Declare Objects Which Are Shared Among Tests, e.g. Shared Mocks


        private void InitializeTest()
        {
            Utilities.InitTypeOfCheck(_screeningService, _unitOfWork);

            // Create sample user account for testing
            _accountManager = Utilities.BuildAccountSample();
            ErrorCode error = _userManagementService.CreateUserProfile(ref _accountManager,
                new List<string>(new List<string> {"Account manager"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _qualityControl = Utilities.BuildAccountSample();
            _qualityControl.UserName = "quality@control.com";
            error = _userManagementService.CreateUserProfile(ref _qualityControl,
                new List<string>(new List<string> {"Quality control"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOffice = Utilities.BuildScreenerAccountOfficeDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOffice,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screenerOnField = Utilities.BuildScreenerAccountOnFieldDTO();
            error = _userManagementService.CreateUserProfile(ref _screenerOnField,
                new List<string>(new List<string> {"Screener"}),
                "123456", false);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientCompany = Utilities.BuildClientCompanySample();
            _clientCompany.AccountManagers = new List<UserProfileDTO> {_accountManager};
            error = _clientService.CreateClientCompany(ref _clientCompany);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _clientContract = Utilities.BuildClientContractDTO();
            error = _clientService.CreateClientContract(_clientCompany, ref _clientContract);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            _screeningLevelDTO = Utilities.BuildScreeningLevelDTO();

            _screeningLevelVersionDTO = Utilities.BuildScreeningLevelVersionDTO();
            _screeningLevelVersionDTO.TypeOfCheckScreeningLevelVersion =
                Utilities.BuildTypeOfCheckListForScreeningListDTO();
            _screeningLevelVersionDTO.ScreeningLevelVersionTurnaroundTime = 10;

            error = _clientService.CreateScreeningLevel(_clientContract, ref _screeningLevelDTO,
                ref _screeningLevelVersionDTO);
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

            _populationOfficeDTO = Utilities.BuildPopulationOfficeDTO();
            error = _populationOfficeService.CreateOrEditQualificationPlace(ref _populationOfficeDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
        }

        /// <summary>
        ///     Create a screening the application, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateScreening()
        {
            // Create screening
            ScreeningDTO screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            ErrorCode error = _screeningService.CreateScreening(screeningDTO.ScreeningLevelVersion, ref screeningDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Retrieve screening directly from repository
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);
            foreach (CVScreeningCore.Models.AtomicCheck atomicCheck in screening.AtomicCheck)
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
                _currentCompanyDTO,
                _police1DTO,
                _immigrationOfficeDTO,
                _highSchoolDTO,
                _facultyDTO,
                _commercialCourtDTO,
                _certificationPlaceDTO,
                _populationOfficeDTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            return screeningDTO;
        }

        /// <summary>
        ///     Create a screening the application, qualify it and process it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateQualifyAndAssignScreening()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateAndQualifyScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            IEnumerable<AtomicCheckDTO> atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);

            // Assign on field screener to office atomic check
            foreach (
                AtomicCheckDTO atomicCheckDTO in
                    atomicChecksDTO.Where(
                        u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                ErrorCode error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOffice);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            foreach (
                AtomicCheckDTO atomicCheckDTO in
                    atomicChecksDTO.Where(
                        u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOnFieldCategory))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                ErrorCode error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOnField);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            // Atomic checks are all in the status ON GOING
            Assert.AreEqual(0, screening.GetNewAtomicChecks().Count);
            return screeningDTO;
        }

        /// <summary>
        ///     Create a screening the application, qualify it and process it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateQualifyAssignAndProcessScreening()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateQualifyAndAssignScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            foreach (AtomicCheckDTO atomicCheckDTO in _screeningService.GetAllAtomicChecksForScreening(screeningDTO))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
                ErrorCode error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }
            return screeningDTO;
        }

        /// <summary>
        ///     Create a screening the application, qualify it, process it and validate it, used by test only
        /// </summary>
        /// <returns></returns>
        private ScreeningDTO CreateQualifyAssignProcessAndValidateScreening()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateQualifyAssignAndProcessScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            foreach (AtomicCheckDTO atomicCheckDTO in _screeningService.GetAllAtomicChecksForScreening(screeningDTO))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
                ErrorCode error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }
            return screeningDTO;
        }

        [TestFixtureTearDown]
        public void RunOnceAfterAll()
        {
        }


        [Test]
        public void AssignAtomicCheckScreeningTest()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateAndQualifyScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            // Atomic checks are all in the status NEW
            Assert.AreEqual(13, screening.GetNewAtomicChecks().Count);

            IEnumerable<AtomicCheckDTO> atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);

            // Assign on field screener to office atomic check
            AtomicCheckDTO atomicCheckOffice =
                atomicChecksDTO.First(u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory);
            ErrorCode error = _screeningService.AssignAtomicCheck(ref atomicCheckOffice, _screenerOnField);
            Assert.AreEqual(ErrorCode.ATOMIC_CHECK_CATEGORY_MISMATCH_ASSIGNEMENT_IMPOSSIBLE, error);

            foreach (
                AtomicCheckDTO atomicCheckDTO in
                    atomicChecksDTO.Where(
                        u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOffice);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            foreach (
                AtomicCheckDTO atomicCheckDTO in
                    atomicChecksDTO.Where(
                        u => u.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOnFieldCategory))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO, _screenerOnField);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            // Atomic checks are all in the status ON GOING
            Assert.AreEqual(0, screening.GetNewAtomicChecks().Count);
        }

        [Test]
        public void CreateScreeningAndQualifyTest()
        {
            // Create screening
            ScreeningDTO screeningDTO = Utilities.BuildScreeningDTO(_screeningLevelVersionDTO, _qualityControl);
            ErrorCode error = _screeningService.CreateScreening(screeningDTO.ScreeningLevelVersion, ref screeningDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Retrieve screening directly from repository
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);
            foreach (CVScreeningCore.Models.AtomicCheck atomicCheck in screening.AtomicCheck)
            {
                _unitOfWork.AtomicCheckRepository.Add(atomicCheck);
            }

            // Standard check when atomic check is created
            Assert.AreEqual(true, screening.IsNew());
            Assert.AreEqual((Byte) ScreeningStateType.NEW, screening.ScreeningState);
            Assert.AreEqual(screening.GetNewAtomicChecks().Count, screening.GetAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetWronflyQualifiedAtomicChecks().Count);
            Assert.AreEqual(screening.GetNewAtomicChecks().Count, screening.GetNotProcessedAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetProcessedAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetValidatedAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetRejectedAtomicChecks().Count);
            Assert.AreEqual(false, screening.IsQualificationStarted());
            Assert.AreEqual(false, screening.IsQualificationCompleted());
            Assert.AreEqual(1, screening.GetQualifiedAtomicChecks().Count);
            Assert.AreEqual(screening.GetNotYetQualifiedAtomicChecks().Count, screening.GetAtomicChecks().Count-1);

            // Qualification started
            ScreeningQualificationDTO qualificationBaseDTO = Utilities.BuildScreeningQualificationDTO();
            error = _qualificationService.SetQualificationBase(screeningDTO, ref qualificationBaseDTO);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // ID check and Neighborhood check are already qualified (3 neighborhood check are created + ID check)
            Assert.AreEqual(4, screening.GetQualifiedAtomicChecks().Count);
            Assert.AreEqual(true, screening.IsQualificationStarted());
            Assert.AreEqual(false, screening.IsQualificationCompleted());
            Assert.AreEqual(true, screening.IsOpen());
            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);

            // Qualification places started
            var qualifications = new List<BaseQualificationPlaceDTO> {_police1DTO, _immigrationOfficeDTO};
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            Assert.AreEqual(6, screening.GetQualifiedAtomicChecks().Count);

            IEnumerable<AtomicCheckDTO> atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO);
            Assert.AreEqual(0, screening.GetNotApplicableAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetValidatedAtomicChecks().Count);

            // Set employment check as not applicable, atomic check not applicable is automatically validated
            AtomicCheckDTO employmentAtomicCheck = atomicChecksDTO.Single(
                u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD
                && u.AtomicCheckType == CVScreeningCore.Models.AtomicCheck.kOtherCompanyType);
            IDictionary<AtomicCheckDTO, bool> notApplicableAtomicChecks = new Dictionary<AtomicCheckDTO, bool>();
            notApplicableAtomicChecks[employmentAtomicCheck] = true;
            error = _qualificationService.SetAtomicChecksAsNotApplicable(screeningDTO, notApplicableAtomicChecks);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(1, screening.GetNotApplicableAtomicChecks().Count);
            Assert.AreEqual(7, screening.GetQualifiedAtomicChecks().Count);
            Assert.AreEqual(1, screening.GetValidatedAtomicChecks().Count);

            // All the atomic check are now qualified
            qualifications = new List<BaseQualificationPlaceDTO>
            {
                _highSchoolDTO,
                _facultyDTO,
                _commercialCourtDTO,
                _certificationPlaceDTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(11, screening.GetQualifiedAtomicChecks().Count);
            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);
        }

        [Test]
        public void ProcessAtomicCheckNeighborhoodWrongQualifiedScreeningTest()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateQualifyAndAssignScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);
            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);


            // Atomic check is set ton wrongly qualified (neighborhood check for current address)
            AtomicCheckDTO atomicCheckToWronglyQualified =
                _screeningService.GetAllAtomicChecksForScreening(
                    screeningDTO)
                    .Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK
                                 && u.AtomicCheckType == CVScreeningCore.Models.AtomicCheck.kCurrentAddressType);

            atomicCheckToWronglyQualified.AtomicCheckState = (Byte) AtomicCheckStateType.WRONGLY_QUALIFIED;
            ErrorCode error = _screeningService.EditAtomicCheck(ref atomicCheckToWronglyQualified);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            foreach (AtomicCheckDTO atomicCheckDTO in
                _screeningService.GetAllAtomicChecksForScreening(screeningDTO)
                    .Where(u => u.AtomicCheckId != atomicCheckToWronglyQualified.AtomicCheckId))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
                error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);
            Assert.AreEqual(1, screening.GetWronflyQualifiedAtomicChecks().Count);
            Assert.AreEqual(screening.GetAtomicChecks().Count - 1, screening.GetProcessedAtomicChecks().Count);

            // Atomic check is set ton wrongly qualified, neighborhood check are not set to validated
            Assert.AreEqual(false, screening.AtomicCheck.Single(u => u.IsWronglyQualified()).IsValidated());
            Assert.AreEqual(13, screening.GetAtomicChecks().Count);

            // Requalify neighborhood check, no new atomic check are recreated
            ScreeningQualificationDTO qualification = Utilities.BuildScreeningQualificationDTO();
            error = _qualificationService.SetQualificationBase(screeningDTO, ref qualification);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(13, screening.GetAtomicChecks().Count);

            // Atomic check is set ton wrongly qualified
            AtomicCheckDTO currentAddressNewAtomicCheck =
                _screeningService.GetAllAtomicChecksForScreening(
                    screeningDTO).First(
                        u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK
                             && u.AtomicCheckType == CVScreeningCore.Models.AtomicCheck.kCurrentAddressType
                             && u.AtomicCheckState == (Byte) AtomicCheckStateType.NEW);

            error = _screeningService.AssignAtomicCheck(ref currentAddressNewAtomicCheck, _screenerOnField);
            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);
            Assert.AreEqual(0, screening.GetWronflyQualifiedAtomicChecks().Count);

            currentAddressNewAtomicCheck.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
            currentAddressNewAtomicCheck.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
            error = _screeningService.EditAtomicCheck(ref currentAddressNewAtomicCheck);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(screening.GetAtomicChecks().Count, screening.GetProcessedAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetWronflyQualifiedAtomicChecks().Count);

            foreach (AtomicCheckDTO atomicCheckDTO in
                _screeningService.GetAllAtomicChecksForScreening(screeningDTO).Where(
                    u => u.AtomicCheckId != atomicCheckToWronglyQualified.AtomicCheckId &&
                         u.AtomicCheckId != currentAddressNewAtomicCheck.AtomicCheckId))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
                error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            currentAddressNewAtomicCheck.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
            currentAddressNewAtomicCheck.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
            error = _screeningService.EditAtomicCheck(ref currentAddressNewAtomicCheck);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual((Byte) ScreeningStateType.VALIDATED, screening.ScreeningState);
        }


        [Test]
        public void ProcessAtomicCheckScreeningTest()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateQualifyAndAssignScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            Assert.AreEqual(0, screening.GetProcessedAtomicChecks().Count);

            foreach (AtomicCheckDTO atomicCheckDTO in _screeningService.GetAllAtomicChecksForScreening(screeningDTO))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
                ErrorCode error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            Assert.AreEqual(screening.GetAtomicChecks().Count, screening.GetProcessedAtomicChecks().Count);
        }

        [Test]
        public void ProcessAtomicCheckWrongQualifiedScreeningTest()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateQualifyAndAssignScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);

            // Atomic check is set ton wrongly qualified
            AtomicCheckDTO atomicCheckToWronglyQualified =
                _screeningService.GetAllAtomicChecksForScreening(
                    screeningDTO).Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.POLICE_CHECK);

            atomicCheckToWronglyQualified.AtomicCheckState = (Byte) AtomicCheckStateType.WRONGLY_QUALIFIED;
            ErrorCode error = _screeningService.EditAtomicCheck(ref atomicCheckToWronglyQualified);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            foreach (AtomicCheckDTO atomicCheckDTO in
                _screeningService.GetAllAtomicChecksForScreening(screeningDTO)
                    .Where(u => u.AtomicCheckId != atomicCheckToWronglyQualified.AtomicCheckId))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
                error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);
            Assert.AreEqual(1, screening.GetWronflyQualifiedAtomicChecks().Count);
            Assert.AreEqual(screening.GetAtomicChecks().Count - 1, screening.GetProcessedAtomicChecks().Count);

            // Atomic check is set to wrongly qualified, check that it is set to wrongly qualified also
            Assert.AreEqual(true, screening.AtomicCheck.Single(u => u.IsWronglyQualified()).IsValidated());

            // Requalify police check
            var qualifications = new List<BaseQualificationPlaceDTO>
            {
                _police2DTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Retrieve screening directly from repository
            _unitOfWork.AtomicCheckRepository.Add(
                screening.AtomicCheck.Single(
                    u =>
                        u.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckCode ==
                        (Byte) TypeOfCheckEnum.POLICE_CHECK
                        && u.AtomicCheckState == (Byte) AtomicCheckStateType.NEW));

            // Atomic check is set ton wrongly qualified
            AtomicCheckDTO policeNewAtomicCheck =
                _screeningService.GetAllAtomicChecksForScreening(
                    screeningDTO).First(u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.POLICE_CHECK
                                             && u.AtomicCheckState == (Byte) AtomicCheckStateType.NEW);

            error = _screeningService.AssignAtomicCheck(ref policeNewAtomicCheck, _screenerOnField);
            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);
            Assert.AreEqual(1, screening.GetWronflyQualifiedAtomicChecks().Count);

            policeNewAtomicCheck.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
            policeNewAtomicCheck.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
            error = _screeningService.EditAtomicCheck(ref policeNewAtomicCheck);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            Assert.AreEqual(screening.GetAtomicChecks().Count - 1, screening.GetProcessedAtomicChecks().Count);
            Assert.AreEqual(1, screening.GetWronflyQualifiedAtomicChecks().Count);

            foreach (AtomicCheckDTO atomicCheckDTO in
                _screeningService.GetAllAtomicChecksForScreening(screeningDTO).Where(
                    u => u.AtomicCheckId != atomicCheckToWronglyQualified.AtomicCheckId &&
                         u.AtomicCheckId != policeNewAtomicCheck.AtomicCheckId))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
                error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            policeNewAtomicCheck.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
            policeNewAtomicCheck.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
            error = _screeningService.EditAtomicCheck(ref policeNewAtomicCheck);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            IList<CVScreeningCore.Models.AtomicCheck> checks = screening.GetAtomicChecks();
            IList<CVScreeningCore.Models.AtomicCheck> validatedCheck = screening.GetValidatedAtomicChecks();

            Assert.AreEqual((Byte) ScreeningStateType.VALIDATED, screening.ScreeningState);
        }


        [Test]
        public void RejectAtomicCheckScreeningTest()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateQualifyAssignAndProcessScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            Assert.AreEqual(0, screening.GetValidatedAtomicChecks().Count);
            List<AtomicCheckDTO> atomicCheckToReject =
                _screeningService.GetAllAtomicChecksForScreening(screeningDTO).Where(u =>
                    u.AtomicCheckId%2 == 0).ToList();

            List<AtomicCheckDTO> atomicCheckToValidate =
                _screeningService.GetAllAtomicChecksForScreening(screeningDTO).Where(u =>
                    u.AtomicCheckId%2 != 0).ToList();

            foreach (AtomicCheckDTO atomicCheckDTO in atomicCheckToReject)
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.REJECTED;
                ErrorCode error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            foreach (AtomicCheckDTO atomicCheckDTO in atomicCheckToValidate)
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
                ErrorCode error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);
            Assert.AreEqual(atomicCheckToValidate.Count, screening.GetValidatedAtomicChecks().Count);
            Assert.AreEqual(atomicCheckToReject.Count, screening.GetRejectedAtomicChecks().Count);

            foreach (AtomicCheckDTO atomicCheckDTO in atomicCheckToReject)
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
                ErrorCode error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            foreach (AtomicCheckDTO atomicCheckDTO in atomicCheckToReject)
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
                ErrorCode error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }

            Assert.AreEqual((Byte) ScreeningStateType.VALIDATED, screening.ScreeningState);
            Assert.AreEqual(screening.GetValidatedAtomicChecks().Count, screening.GetValidatedAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetRejectedAtomicChecks().Count);
        }

        [Test]
        public void ValidateAndReopenAtomicCheckScreeningTest()
        {
            // Create screening and validate it, screening status => VALIDATED
            ScreeningDTO screeningDTO = CreateQualifyAssignProcessAndValidateScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);
            Assert.AreEqual((Byte) ScreeningStateType.VALIDATED, screening.ScreeningState);
            Assert.AreEqual(screening.GetAtomicChecks().Count, screening.GetValidatedAtomicChecks().Count);

            // Reject one atomic check, screening status => OPEN
            AtomicCheckDTO atomicCheckToReject =
                _screeningService.GetAllAtomicChecksForScreening(screeningDTO)
                    .Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.POLICE_CHECK);

            atomicCheckToReject.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.REJECTED;
            ErrorCode error = _screeningService.EditAtomicCheck(ref atomicCheckToReject);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(screening.GetAtomicChecks().Count - 1, screening.GetValidatedAtomicChecks().Count);
            Assert.AreEqual(1, screening.GetRejectedAtomicChecks().Count);
            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);

            // Reject one atomic check, screening status => PROCESSED
            atomicCheckToReject.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
            error = _screeningService.EditAtomicCheck(ref atomicCheckToReject);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);
            Assert.AreEqual(screening.GetAtomicChecks().Count - 1, screening.GetValidatedAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetRejectedAtomicChecks().Count);
            Assert.AreEqual(1, screening.GetProcessedAtomicChecks().Count);
            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);

            atomicCheckToReject.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
            error = _screeningService.EditAtomicCheck(ref atomicCheckToReject);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            // Validate one atomic check, screening status => VALIDATED
            Assert.AreEqual(screening.GetAtomicChecks().Count, screening.GetValidatedAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetRejectedAtomicChecks().Count);
            Assert.AreEqual(0, screening.GetProcessedAtomicChecks().Count);
            Assert.AreEqual((Byte) ScreeningStateType.VALIDATED, screening.ScreeningState);
        }

        [Test]
        public void ValidateAtomicCheckScreeningTest()
        {
            // Create screening
            ScreeningDTO screeningDTO = CreateQualifyAssignAndProcessScreening();
            CVScreeningCore.Models.Screening screening =
                _unitOfWork.ScreeningRepository.Single(u => u.ScreeningId == screeningDTO.ScreeningId);

            Assert.AreEqual(0, screening.GetValidatedAtomicChecks().Count);

            foreach (AtomicCheckDTO atomicCheckDTO in _screeningService.GetAllAtomicChecksForScreening(screeningDTO))
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
                ErrorCode error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }
            Assert.AreEqual(screening.GetAtomicChecks().Count, screening.GetValidatedAtomicChecks().Count);
            Assert.AreEqual((Byte) ScreeningStateType.VALIDATED, screening.ScreeningState);
        }

        [Test]
        public void ValidateNotApplicableAtomicCheckScreeningTest()
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
                _currentCompanyDTO,
                _immigrationOfficeDTO,
                _highSchoolDTO,
                _facultyDTO,
                _commercialCourtDTO,
                _certificationPlaceDTO,
                _populationOfficeDTO
            };
            error = _qualificationService.SetQualificationPlaces(screeningDTO, qualifications);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);

            AtomicCheckDTO policeAtomicCheck = _screeningService.GetAllAtomicChecksForScreening(screeningDTO)
                .Single(u => u.TypeOfCheck.TypeOfCheckCode == (Byte) TypeOfCheckEnum.POLICE_CHECK);

            IDictionary<AtomicCheckDTO, bool> notApplicableDictionnary = new Dictionary<AtomicCheckDTO, bool>();
            notApplicableDictionnary[policeAtomicCheck] = true;
            error = _qualificationService.SetAtomicChecksAsNotApplicable(screeningDTO, notApplicableDictionnary);
            Assert.AreEqual(ErrorCode.NO_ERROR, error);


            IEnumerable<AtomicCheckDTO> atomicChecksDTO = _screeningService.GetAllAtomicChecksForScreening(screeningDTO)
                .Where(
                    u => u.AtomicCheckId != policeAtomicCheck.AtomicCheckId);

            Assert.AreEqual((Byte) ScreeningStateType.OPEN, screening.ScreeningState);

            // Assign on field screener to office atomic check
            foreach (AtomicCheckDTO atomicCheckDTO in atomicChecksDTO)
            {
                AtomicCheckDTO refAtomicCheckTO = atomicCheckDTO;
                error = _screeningService.AssignAtomicCheck(ref refAtomicCheckTO,
                    refAtomicCheckTO.AtomicCheckCategory == CVScreeningCore.Models.AtomicCheck.kOfficeCategory
                        ? _screenerOffice
                        : _screenerOnField);

                Assert.AreEqual(ErrorCode.NO_ERROR, error);
                refAtomicCheckTO.AtomicCheckState = (Byte) AtomicCheckStateType.DONE_OK;
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.PROCESSED;
                error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
                refAtomicCheckTO.AtomicCheckValidationState = (Byte) AtomicCheckValidationStateType.VALIDATED;
                error = _screeningService.EditAtomicCheck(ref refAtomicCheckTO);
                Assert.AreEqual(ErrorCode.NO_ERROR, error);
            }
            IList<CVScreeningCore.Models.AtomicCheck> atomicChecks = screening.GetAtomicChecks();
            IList<CVScreeningCore.Models.AtomicCheck> validatedAtomicCheck = screening.GetValidatedAtomicChecks();

            Assert.AreEqual((Byte) ScreeningStateType.VALIDATED, screening.ScreeningState);
            Assert.AreEqual(screening.GetAtomicChecks().Count, screening.GetValidatedAtomicChecks().Count);
        }

        // 4. Runs Twice; Once after Test Case 1 and Once After Test Case 2
        // Dispose Objects Used in Each Test which are no longer required
    }
}