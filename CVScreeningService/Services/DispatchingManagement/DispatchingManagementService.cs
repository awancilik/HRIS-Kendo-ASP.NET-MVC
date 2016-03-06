using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Exception;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.Settings;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;
using CVScreeningService.Helpers;
using CVScreeningService.Interceptor;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.SystemTime;
using CVScreeningService.Services.UserManagement;
using Microsoft.Ajax.Utilities;
using Contract = System.Diagnostics.Contracts.Contract;
using Nalysa.Common.Log;

namespace CVScreeningService.Services.DispatchingManagement
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class DispatchingManagementService : IDispatchingManagementService
    {
        private readonly IScreeningService _screeningService;
        private readonly ISystemTimeService _systemTimeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManagementService _userManagementService;


        public DispatchingManagementService(
            IUnitOfWork unitOfWork, IUserManagementService userManagementService,
            ISystemTimeService systemTimeService,
            IScreeningService screeningService)
        {
            _unitOfWork = unitOfWork;
            _userManagementService = userManagementService;
            _systemTimeService = systemTimeService;
            _screeningService = screeningService;
        }

        #region Dispatching engine

        public virtual void DispatchAtomicChecks()
        {
            // Get All atomic checks which are qualified and not assigned 
            var atomicChecks = _unitOfWork.AtomicCheckRepository.GetAll();
            atomicChecks = atomicChecks.Where(a =>  !a.IsAssigned() && a.IsQualified() && !a.IsNotApplicable() && !a.IsDeactivated());
            var atomicChecksShouldBeAssigned = atomicChecks.Select(a => _screeningService.GetAtomicCheck(a.AtomicCheckId));

            // Main logic
            foreach (AtomicCheckDTO atomicCheckDTO in atomicChecksShouldBeAssigned)
            {
                try
                {
                    //show atomic check information 
                    LogManager.Instance.Info(string.Format("Dispatching job: id {0}; type of check {1}; category {2}",
                        atomicCheckDTO.AtomicCheckId, atomicCheckDTO.TypeOfCheck.CheckName, atomicCheckDTO.AtomicCheckCategory));

                    UserProfileDTO screenerToAssign = null;
                    if (IsSpecificCase(atomicCheckDTO))
                    {
                        screenerToAssign = GetScreenerForSpecificCase(atomicCheckDTO);
                    }
                    if (screenerToAssign == null)
                    {
                        var aggregatedDictionary = GetAggregatedValues(atomicCheckDTO);
                        //show atomic check aggregated value information
                        foreach (var aggregatedValue in aggregatedDictionary)
                        {
                            LogManager.Instance.Info(string.Format("Aggregated Value: screener id {0}, value {1}", aggregatedValue.Key, aggregatedValue.Value));
                        }

                        screenerToAssign = GetScreenerWithMaximumValue(aggregatedDictionary);
                    }

                    if (screenerToAssign == null)
                        continue;

                    var copyAtomicCheckDTO = atomicCheckDTO;
                    _screeningService.AssignAtomicCheck(ref copyAtomicCheckDTO, screenerToAssign);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error(
                        string.Format("Exception thrown when dispatching atomic check: {0}",  
                        ExceptionInterceptor.DisplayObjectProperties(atomicCheckDTO)));
                    LogManager.Instance.Error(
                        string.Format("Stack exception: {0}", ExceptionInterceptor.FlattenException(ex)));
                }
            }
        }

        public virtual IDictionary<int, float> GetAggregatedValues(AtomicCheckDTO atomicCheckDTO)
        {
            float skillCoefficient =
                Convert.ToSingle(
                    _unitOfWork.DispatchingSettingsRepository.First(
                        e => e.DispatchingSettingsKey == DispatchingSettings.kSkillCoefficient).DispatchingSettingsValue);

            float geographicalCoefficient = Convert.ToSingle(
                _unitOfWork.DispatchingSettingsRepository.First(
                    e => e.DispatchingSettingsKey == DispatchingSettings.kGeographicalCoefficient)
                    .DispatchingSettingsValue);

            float workloadCoefficient = Convert.ToSingle(
                _unitOfWork.DispatchingSettingsRepository.First(
                    e => e.DispatchingSettingsKey == DispatchingSettings.kWorkloadCoefficient)
                    .DispatchingSettingsValue);

            float availabilityCoefficient = Convert.ToSingle(
                _unitOfWork.DispatchingSettingsRepository.First(
                    e => e.DispatchingSettingsKey == DispatchingSettings.kAvailibilityCoefficient)
                    .DispatchingSettingsValue);

            float alreadyCoefficient = Convert.ToSingle(
                _unitOfWork.DispatchingSettingsRepository.First(
                    e => e.DispatchingSettingsKey == DispatchingSettings.kAlreadyCoefficient)
                    .DispatchingSettingsValue);

            LogManager.Instance.Info(string.Format("Skill coefficient {0}, Geographical coefficient {1}, Workload coefficient {2}, Availibility coefficient {3}, Already coefficient {4}",
                skillCoefficient, geographicalCoefficient, workloadCoefficient, availabilityCoefficient, alreadyCoefficient));


            // Instantiate dictionary with 0 value
            IDictionary<int, float> aggregatedDictionary =
                _userManagementService.GetAllUserScreeners().ToDictionary(e => e.UserId, f => Convert.ToSingle(0));

            TypeOfCheck typeOfCheck =
                _unitOfWork.TypeOfCheckRepository.First(e => e.TypeOfCheckId == atomicCheckDTO.TypeOfCheck.TypeOfCheckId);

            // Calculate Skill Weight
            var impossibleUserIds = new List<int>();
            
            TypeOfCheckMeta meta =
                typeOfCheck.TypeOfCheckMeta.FirstOrDefault(e => e.TypeOfCheckMetaKey == TypeOfCheckMeta.kSkillCriterion
                                                                &&
                                                                e.TypeOfCheckMetaCategory ==
                                                                atomicCheckDTO.AtomicCheckCategory);
            if (meta != null && meta.TypeOfCheckMetaValue == TypeOfCheckMeta.kYesValue)
            {
                foreach (var skillWeight in GetScreenerSkillWeight(atomicCheckDTO))
                {
                    if(skillWeight.Value == SkillMatrix.kImpossibleValue)
                        impossibleUserIds.Add(skillWeight.Key);

                    aggregatedDictionary[skillWeight.Key] += (skillCoefficient * skillWeight.Value);
                    
                    //show information skill aggregated value 
                    LogManager.Instance.Info(string.Format("Skill Aggregated Value: screener id {0}, value {1}, value * coefficient {2}", skillWeight.Key, skillWeight.Value, (skillCoefficient * skillWeight.Value)));
                }
            }


            // Calculate Geographical Weight
            meta =
                typeOfCheck.TypeOfCheckMeta.FirstOrDefault(
                    e => e.TypeOfCheckMetaKey == TypeOfCheckMeta.kGeographicalCriterion
                         && e.TypeOfCheckMetaCategory == atomicCheckDTO.AtomicCheckCategory);
            if (meta != null && meta.TypeOfCheckMetaValue == TypeOfCheckMeta.kYesValue)
                foreach (var geographicalWeight in GetScreenerGeographicalWeight(atomicCheckDTO))
                {
                    aggregatedDictionary[geographicalWeight.Key] += (geographicalCoefficient*geographicalWeight.Value);

                    //show information geographical aggregated value 
                    LogManager.Instance.Info(string.Format("Geographic Aggregated Value: screener id {0}, value {1}, value * coefficient {2}", geographicalWeight.Key, geographicalWeight.Value, (geographicalCoefficient * geographicalWeight.Value)));
                }

            // Calculate Workload Weight
            meta =
                typeOfCheck.TypeOfCheckMeta.FirstOrDefault(
                    e => e.TypeOfCheckMetaKey == TypeOfCheckMeta.kWorkLoadCriterion
                         &&
                         e.TypeOfCheckMetaCategory ==
                         atomicCheckDTO.AtomicCheckCategory);

            if (meta != null && meta.TypeOfCheckMetaValue == TypeOfCheckMeta.kYesValue)
                foreach (var workloadWeight in GetScreenerWorkloadWeight())
                {
                    aggregatedDictionary[workloadWeight.Key] += (workloadCoefficient*workloadWeight.Value);
                     
                    //show information workload aggregated value 
                    LogManager.Instance.Info(string.Format("Workload Aggregated Value: screener id {0}, value {1}, value * coefficient {2}", workloadWeight.Key, workloadWeight.Value, (workloadCoefficient * workloadWeight.Value)));
                }

            // Calculate Availability Weight
            meta =
                typeOfCheck.TypeOfCheckMeta.FirstOrDefault(
                    e => e.TypeOfCheckMetaKey == TypeOfCheckMeta.kAvailabilityCriterion
                         &&
                         e.TypeOfCheckMetaCategory ==
                         atomicCheckDTO.AtomicCheckCategory);
            if (meta != null && meta.TypeOfCheckMetaValue == TypeOfCheckMeta.kYesValue)
                foreach (var availabilityWeight in GetScreenerAvailabilityWeight(atomicCheckDTO))
                {
                    aggregatedDictionary[availabilityWeight.Key] += (availabilityCoefficient*availabilityWeight.Value);
                    
                    //show information availability aggregated value 
                    LogManager.Instance.Info(string.Format("Availability Aggregated Value: screener id {0}, value {1}, value * coefficient {2}", availabilityWeight.Key, availabilityWeight.Value, (availabilityCoefficient * availabilityWeight.Value)));
                }

            // Calculate Already Weight
            meta =
                typeOfCheck.TypeOfCheckMeta.FirstOrDefault(
                    e => e.TypeOfCheckMetaKey == TypeOfCheckMeta.kAlreadyAssignedCriterion
                         && e.TypeOfCheckMetaCategory == atomicCheckDTO.AtomicCheckCategory);
            if (meta != null && meta.TypeOfCheckMetaValue == TypeOfCheckMeta.kYesValue)
                foreach (var alreadyWeight in GetScreenerAtomicCheckAssignmentWeight(atomicCheckDTO))
                {
                    aggregatedDictionary[alreadyWeight.Key] += (alreadyCoefficient*alreadyWeight.Value);

                    //show information already aggregated value 
                    LogManager.Instance.Info(string.Format("Already Aggregated Value: screener id {0}, value {1}, value {1}, value * coefficient {2}", alreadyWeight.Key, alreadyWeight.Value, (alreadyCoefficient * alreadyWeight.Value)));
                }

            var differentCategoryScreeners = aggregatedDictionary.Where(
                value => _unitOfWork.UserProfileRepository.First(u => u.UserId == value.Key).ScreenerCategory !=
                         atomicCheckDTO.AtomicCheckCategory).Select(u => u.Key).ToList();

            // Set impossible for users with different category 
            foreach (var value in differentCategoryScreeners)
            {
                aggregatedDictionary[value] = -1;
            }

            // Set impossible for IMP users in skill
            foreach (var impossibleUserId in impossibleUserIds)
            {
                aggregatedDictionary[impossibleUserId] = -1;
            }
            return aggregatedDictionary;
        }

        /// <summary>
        ///     Compute screener workload weight
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<int, float> GetScreenerWorkloadWeight()
        {
            var dictionnary = new Dictionary<int, float>();
            List<UserProfileDTO> userProfilesDTO =
                _userManagementService.GetUserProfilesByRoles(webpages_Roles.kScreenerRole);

            foreach (UserProfileDTO userProfileDTO in userProfilesDTO)
            {
                webpages_UserProfile userProfile =
                    _unitOfWork.UserProfileRepository.First(u => u.UserId == userProfileDTO.UserId);
                // Screener capabilities: how many check of level 5 that a screener can handle before to be fully booked
                var screenerAverageDiminution = (int) (100/(userProfile.ScreenerCapabilities*5));

                int result = userProfile.GetAtomicChecks().Aggregate(0, (current, check) => current + (check.GetWorkloadWeight()*screenerAverageDiminution));
                int userWorkload = 100 - result;

                userWorkload = (userWorkload >= 0) ? userWorkload : 0;
                dictionnary[userProfileDTO.UserId] = userWorkload;
            }
            return dictionnary;
        }

        /// <summary>
        ///     Compute screener availability weight
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        public virtual IDictionary<int, float> GetScreenerAvailabilityWeight(AtomicCheckDTO atomicCheckDTO)
        {
            var dictionnary = new Dictionary<int, float>();
            AtomicCheck atomicCheck =
                _unitOfWork.AtomicCheckRepository.First(e => e.AtomicCheckId == atomicCheckDTO.AtomicCheckId);

            Contract.Assume(atomicCheck.IsQualified(), "Atomic check should be qualified");
            Contract.Assume(!atomicCheck.IsAssigned(),
                "Atomic check should be not assigned");

            // Turnaround time correspond to the time between today and the deadline of the screening
            int keepingWorkingDays = DateHelper.GetWorkingDaysDifference(_systemTimeService.GetCurrentDateTime(),
                (DateTime) atomicCheck.Screening.ScreeningDeadlineDate, new List<PublicHolidayDTO>());

            int minimumWorkingDays = atomicCheck.GetCompletionMinimumWorkingDays();

            // Not enough time to process the atomic check: URGENT !!!
/*            if (keepingWorkingDays + 1 - minimumWorkingDays > 0)
            {
                keepingWorkingDays = minimumWorkingDays + 1;
            }*/

/*            Contract.Assume(keepingWorkingDays + 1 - minimumWorkingDays > 0,
                "Turnaround time should be greater than minimum working days");*/

            float holidayUnit = 100/(float) (keepingWorkingDays - minimumWorkingDays);
            List<UserProfileDTO> userProfilesDTO =
                _userManagementService.GetUserProfilesByRoles(webpages_Roles.kScreenerRole);

            foreach (UserProfileDTO userProfileDTO in userProfilesDTO)
            {
                float userWorkload = 0;
                int workingDays = _userManagementService.RetrieveNumberOfWorkingDaysToNextLeave(userProfileDTO);
                if (workingDays >= keepingWorkingDays)
                    // Next leave after the duration needed to process the type of check
                {
                    userWorkload = 100;
                }
                else if (workingDays < minimumWorkingDays)
                    // Next leave before the duration needed to process the type of check
                {
                    userWorkload = 0;
                }
                else
                {
                    userWorkload = 100 - (holidayUnit*(keepingWorkingDays - workingDays - 1));
                }
                dictionnary[userProfileDTO.UserId] = userWorkload;
            }
            return dictionnary;
        }

        public virtual IDictionary<int, float> GetScreenerSkillWeight(AtomicCheckDTO atomicCheckDTO)
        {
            AtomicCheck atomicCheck =
                _unitOfWork.AtomicCheckRepository.First(e => e.AtomicCheckId == atomicCheckDTO.AtomicCheckId);
            Contract.Assume(atomicCheck.IsQualified(), "Atomic check should be qualified");
            Contract.Assume(!atomicCheck.IsAssigned(),
                "Atomic check should be not assigned");

            List<UserProfileDTO> screeners = _userManagementService.GetUserProfilesByRoles(webpages_Roles.kScreenerRole);

            var dictionary = new Dictionary<int, float>();
            foreach (UserProfileDTO screenerDTO in screeners)
            {
                int? value;
                // Atomic check with qualification place
                if (atomicCheck.QualificationPlace != null)
                {
                    value =
                        GetSkillMatrixValue(screenerDTO.UserId,
                            atomicCheck.QualificationPlace.QualificationPlaceId, atomicCheck.AtomicCheckCategory);

                    if (value == SkillMatrix.kDefaultValue)
                    {
                        value = GetDefaultMatrixValue(screenerDTO.UserId,
                            atomicCheck.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckId);
                    }
                }
                else
                {
                    value = GetDefaultMatrixValue(screenerDTO.UserId,
                        atomicCheck.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckId);
                }


                dictionary.Add(screenerDTO.UserId,
                    Convert.ToSingle(value));
            }

            return dictionary;
        }

        /// <summary>
        ///     Compute screeners already assigned weight
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        public virtual IDictionary<int, float> GetScreenerAtomicCheckAssignmentWeight(
            AtomicCheckDTO atomicCheckDTO)
        {
            var dictionnary = new Dictionary<int, float>();
            AtomicCheck atomicCheck =
                _unitOfWork.AtomicCheckRepository.First(e =>
                    e.AtomicCheckId == atomicCheckDTO.AtomicCheckId);

            Contract.Assume(atomicCheck.IsQualified(),
                "Atomic check should be qualified");
            Contract.Assume(!atomicCheck.IsAssigned(),
                "Atomic check should be not assigned");

            // Atomic check is not linked to any qualification place
            if (!atomicCheck.GetTypeOfCheck().IsQualificationNeeded())
                throw new ExceptionDispatchingInvalidAtomicCheck();

            List<UserProfileDTO> userProfilesDTO =
                _userManagementService.GetUserProfilesByRoles(webpages_Roles.kScreenerRole);

            foreach (UserProfileDTO userProfileDTO in userProfilesDTO)
            {
                float userWorkload = 0;
                webpages_UserProfile userProfile =
                    _unitOfWork.UserProfileRepository.First(u =>
                        u.UserId == userProfileDTO.UserId);

                // Screener has already an atomic check assigned to him for the same qualification place that is on going
                if (userProfile.AtomicCheck.Any(u =>
                    u.IsNotProcessed() && u.IsOnGoing()
                    && u.AtomicCheckCategory == atomicCheck.AtomicCheckCategory
                    && u.QualificationPlace == atomicCheck.QualificationPlace))
                    userWorkload = 100;
                dictionnary[userProfileDTO.UserId] = userWorkload;
            }
            return dictionnary;
        }

        /// <summary>
        ///     Compute screener geographical weight
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        public virtual IDictionary<int, float> GetScreenerGeographicalWeight(AtomicCheckDTO atomicCheckDTO)
        {
            try
            {
                AtomicCheck atomicCheck =
                _unitOfWork.AtomicCheckRepository.First(e => e.AtomicCheckId == atomicCheckDTO.AtomicCheckId);
                Contract.Assume(atomicCheck.IsQualified(), "Atomic check should be qualified");
                Contract.Assume(!atomicCheck.IsAssigned(),
                    "Atomic check should be not assigned");

                if (!atomicCheck.GetTypeOfCheck().HasGeographicalCriterion())
                    throw new ExceptionDispatchingInvalidAtomicCheck();

                var dictionary = new Dictionary<int, float>();
                Address address;
                if (atomicCheck.GetTypeOfCheck().HasQualificationPlace())
                    address = atomicCheck.QualificationPlace.Address;
                else if (atomicCheck.IsNeighborhoodCVAddress())
                    address = atomicCheck.Screening.ScreeningQualification.CVAddress;
                else if (atomicCheck.IsNeighborhoodCurrentAddress())
                    address = atomicCheck.Screening.ScreeningQualification.CurrentAddress;
                else
                    address = atomicCheck.Screening.ScreeningQualification.IDCardAddress;

                List<UserProfileDTO> userProfilesDTO =
                    _userManagementService.GetUserProfilesByRoles(webpages_Roles.kScreenerRole);
                Dictionary<string, string> dispatchingSettings =
                    _unitOfWork.DispatchingSettingsRepository.GetAll()
                        .ToDictionary(e => e.DispatchingSettingsKey, f => f.DispatchingSettingsValue);
                foreach (UserProfileDTO userProfileDTO in userProfilesDTO.Where(e => e.Address != null))
                {
                    webpages_UserProfile userProfile =
                        _unitOfWork.UserProfileRepository.First(u => u.UserId == userProfileDTO.UserId);
                
                    string weight = dispatchingSettings[DispatchingSettings.kGeographicalSameCountryValue];

                    if (address.Location.IsCountry()) //if the location is outside from Indonesia
                        weight = dispatchingSettings[DispatchingSettings.kGeographicalOutsideIndonesiaValue];
                    else if (AreLocatedInTheSameSubDistrict(userProfile.Address, address))
                        weight = dispatchingSettings[DispatchingSettings.kGeographicalSameSubDistrictValue];
                    else if (AreLocatedInTheSameDistrict(userProfile.Address, address))
                        weight = dispatchingSettings[DispatchingSettings.kGeographicalSameDistrictValue];
                    else if (AreLocatedInTheSameCity(userProfile.Address, address))
                        weight = dispatchingSettings[DispatchingSettings.kGeographicalSameCityValue];
                    else if (AreLocationInTheSameProvince(userProfile.Address, address))
                        weight = dispatchingSettings[DispatchingSettings.kGeographicalSameProvinceValue];
                    else if (userProfile.Address.Location.IsCountry())  // Screener is outside from Indonesia
                        weight = "0";

                    dictionary[userProfileDTO.UserId] = Convert.ToSingle(weight);
                }
                return dictionary;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        ///     Get best screener to
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        public virtual UserProfileDTO GetScreenerForSpecificCase(AtomicCheckDTO atomicCheckDTO)
        {
            AtomicCheck atomicCheck =
                _unitOfWork.AtomicCheckRepository.First(e => e.AtomicCheckId == atomicCheckDTO.AtomicCheckId);
            TypeOfCheck typeOfChecks = atomicCheck.GetTypeOfCheck();
            UserProfileDTO screenerToAssign;

            switch ((TypeOfCheckEnum) typeOfChecks.TypeOfCheckCode)
            {
                case TypeOfCheckEnum.MEDIA_CHECK_COMPREHENSIVE:
                case TypeOfCheckEnum.MEDIA_CHECK_SIMPLIFIED:
                case TypeOfCheckEnum.CONTACT_NUMBER_CHECK:
                case TypeOfCheckEnum.REFERENCES_CHECK:
                case TypeOfCheckEnum.MEDICAL_CHECK:
                    screenerToAssign = GetScreenerAssignedInEmploymentCheck(atomicCheck);
                    break;
                case TypeOfCheckEnum.NEIGHBOURHOOD_CHECK:
                    screenerToAssign = GetScreenerAssignedInPoliceCheck(atomicCheck);
                    break;
                case TypeOfCheckEnum.ID_CHECK:
                    screenerToAssign = GetScreenerAssignedInPopulationOffice(atomicCheck);
                    break;
                default:
                    screenerToAssign = null;
                    break;
            }
            return screenerToAssign;
        }

        private UserProfileDTO GetScreenerAssignedInPopulationOffice(AtomicCheck atomicCheck)
        {
            var atomicCheckPopulationOffice =
               atomicCheck.Screening.AtomicCheck.FirstOrDefault(
               atomCheck => atomCheck.QualificationPlace is PopulationOffice);
            if (atomicCheckPopulationOffice == null || atomicCheckPopulationOffice.Screener == null)
                return null;

            var screener = atomicCheckPopulationOffice.Screener;
            return _userManagementService.GetUserProfileById(screener.UserId);
        }

        private UserProfileDTO GetScreenerAssignedInPoliceCheck(AtomicCheck atomicCheck)
        {
            var atomicCheckPolice =
                atomicCheck.Screening.AtomicCheck.FirstOrDefault(
                atomCheck => atomCheck.QualificationPlace is Police);
            if (atomicCheckPolice == null || atomicCheckPolice.Screener == null) 
                return null;

            var screener = atomicCheckPolice.Screener;
            return _userManagementService.GetUserProfileById(screener.UserId);
        }

        private UserProfileDTO GetScreenerAssignedInEmploymentCheck(AtomicCheck atomicCheck)
        {
            var atomicCheckEmployment =
                atomicCheck.Screening.AtomicCheck.FirstOrDefault(
                    atomCheck => atomCheck.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckCode
                                 == (byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_PERFORMANCE ||
                                 atomCheck.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckCode ==
                                 (byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD);

            if (atomicCheckEmployment == null || atomicCheckEmployment.Screener == null) 
                return null;

            var screener = atomicCheckEmployment.Screener;
            return _userManagementService.GetUserProfileById(screener.UserId);
        }

        /// <summary>
        ///     Check if the atomic check has specific cases
        /// </summary>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        private bool IsSpecificCase(AtomicCheckDTO atomicCheckDTO)
        {
            AtomicCheck atomicCheck =
                _unitOfWork.AtomicCheckRepository.First(e => e.AtomicCheckId == atomicCheckDTO.AtomicCheckId);
            TypeOfCheck typeOfChecks = atomicCheck.GetTypeOfCheck();

            // type of check which require special approach
            var specificCaseChecks = new List<TypeOfCheckEnum>
            {
                TypeOfCheckEnum.MEDIA_CHECK_COMPREHENSIVE,
                TypeOfCheckEnum.MEDICAL_CHECK,
                TypeOfCheckEnum.MEDIA_CHECK_SIMPLIFIED,
                TypeOfCheckEnum.ID_CHECK,
                TypeOfCheckEnum.NEIGHBOURHOOD_CHECK,
                TypeOfCheckEnum.REFERENCES_CHECK,
                TypeOfCheckEnum.CONTACT_NUMBER_CHECK,
                TypeOfCheckEnum.MEDICAL_CHECK
            };

            return (specificCaseChecks.Any(e => e == (TypeOfCheckEnum) typeOfChecks.TypeOfCheckCode));
        }


        private UserProfileDTO GetScreenerWithMaximumValue(IDictionary<int, float> dictionary)
        {
            float maxValue = dictionary.Max(e => e.Value);

            // If maximum value is impossible, we does not
            if (maxValue == -1)
                return null;

            int screenerId = dictionary.FirstOrDefault(e => e.Value.Equals(maxValue)).Key;
            return _userManagementService.GetUserProfileById(screenerId);
        }

        private bool AreLocationInTheSameProvince(Address screenerAddress, Address contentAddress)
        {
            // Screener outside Indonesia
            if (screenerAddress.Location.LocationParent == null)
                return false;
            return screenerAddress.Location.LocationParent.LocationParent.LocationParent.LocationId ==
                   contentAddress.Location.LocationParent.LocationParent.LocationParent.LocationId;
        }

        private bool AreLocatedInTheSameCity(Address screenerAddress, Address contentAddress)
        {
            // Screener outside Indonesia
            if (screenerAddress.Location.LocationParent == null)
                return false;
            return screenerAddress.Location.LocationParent.LocationParent.LocationId ==
                   contentAddress.Location.LocationParent.LocationParent.LocationId;
        }

        private bool AreLocatedInTheSameDistrict(Address screenerAddress, Address contentAddress)
        {
            // Screener outside Indonesia
            if (screenerAddress.Location.LocationParent == null)
                return false;
            return screenerAddress.Location.LocationParent.LocationId ==
                   contentAddress.Location.LocationParent.LocationId;
        }

        private bool AreLocatedInTheSameSubDistrict(Address screenerAddress, Address contentAddress)
        {
            // Screener outside Indonesia
            if (screenerAddress.Location.LocationParent == null)
                return false;
            return screenerAddress.Location.LocationId == contentAddress.Location.LocationId;
        }

        #endregion

        #region Skills and default matrix

        /// <summary>
        /// </summary>
        /// <param name="screenerId"></param>
        /// <param name="typeOfCheckId"></param>
        /// <returns></returns>
        public virtual int? GetDefaultMatrixValue(int screenerId, int typeOfCheckId)
        {
            DefaultMatrix matrix = _unitOfWork.DefaultMatrixRepository.First(
                e => e.TypeOfCheck.TypeOfCheckId == typeOfCheckId &&
                     e.webpages_UserProfile.UserId == screenerId);

            return matrix != null ? matrix.DefaultValue : 50;
        }

        /// <summary>
        ///     Get specific skill value for given screener,
        ///     qualification place, and category
        /// </summary>
        /// <param name="screenerId"></param>
        /// <param name="qualificationPlaceId"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public virtual int? GetSkillMatrixValue(int screenerId, int qualificationPlaceId, string category)
        {
            int key = (SkillMatrix.kCategories.FirstOrDefault(f => f.Value == category).Key);

            SkillMatrix matrix = _unitOfWork.SkillMatrixRepository.First(
                e => e.QualificationPlace.QualificationPlaceId == qualificationPlaceId
                     && e.webpages_UserProfile.UserId == screenerId
                     && e.SkillMeta == key);

            return matrix != null
                ? matrix.SkillValue
                : SkillMatrix.kDefaultValue;
        }

        /// <summary>
        /// </summary>
        /// <param name="rows"></param>
        public virtual void UpdateDefaultMatrix(IEnumerable<IDictionary<string, string>> rows)
        {
            foreach (var typeOfCheck in rows)
            {
                // ColumnNames indicate screener full names
                // delimiter is always '|'
                foreach (string screener in typeOfCheck["ColumnNames"].Split('|'))
                {
                    if (screener.IsNullOrWhiteSpace()) continue;

                    // get POCO of default matrix which belongs to specific typeOfCheck 
                    // and screener
                    int typeOfCheckId = int.Parse(typeOfCheck["RowId"]);
                    int screenerId = _unitOfWork.UserProfileRepository.First(e =>
                        e.FullName.Equals(screener, StringComparison.OrdinalIgnoreCase)).UserId;

                    DefaultMatrix defaultMatrix = _unitOfWork.DefaultMatrixRepository.First(
                        e => e.TypeOfCheck.TypeOfCheckId == typeOfCheckId &&
                             e.webpages_UserProfile.FullName.Contains(screener)
                        );

                    int value = Convert.ToInt32(typeOfCheck[screener.Replace(" ", string.Empty)]);

                    if (defaultMatrix == null)
                    {
                        defaultMatrix = new DefaultMatrix
                        {
                            TypeOfCheck =
                                _unitOfWork.TypeOfCheckRepository.First(
                                    e => e.TypeOfCheckId == typeOfCheckId),
                            webpages_UserProfile = _unitOfWork.UserProfileRepository.First(e => e.UserId == screenerId),
                            DefaultValue = value
                        };
                        _unitOfWork.DefaultMatrixRepository.Add(defaultMatrix);
                    }
                    else
                    {
                        defaultMatrix.DefaultValue = value;
                    }
                }
            }
            _unitOfWork.Commit();
        }

        /// <summary>
        /// </summary>
        /// <param name="rows"></param>
        public virtual void UpdateSkillMatrix(IEnumerable<IDictionary<string, string>> rows)
        {
            foreach (var qualificationPlace in rows)
            {
                // ColumnNames indicate screener full names
                // delimiter is always '|'
                foreach (string screener in qualificationPlace["ColumnNames"].Split('|'))
                {
                    if (screener.IsNullOrWhiteSpace())
                        continue;

                    // get POCO of skill matrix which belongs to specific 
                    // qualification place and screener

                    // Given RowId format is "qualificationId|categoryString"
                    int qualificationPlaceId = int.Parse(qualificationPlace["RowId"].Split('|')[0]);

                    int screenerId = _unitOfWork.UserProfileRepository.First(e =>
                        e.FullName.Equals(screener, StringComparison.OrdinalIgnoreCase)).UserId;

                    // Take the dictionary key of given value
                    int categoryKey = SkillMatrix.kCategories
                        .FirstOrDefault(kCategory => kCategory.Value
                                                     == qualificationPlace["Category"]).Key;

                    SkillMatrix skillMatrix = _unitOfWork.SkillMatrixRepository.First(
                        e => e.QualificationPlace.QualificationPlaceId == qualificationPlaceId &&
                             e.webpages_UserProfile.UserId == screenerId &&
                             e.SkillMeta == categoryKey);

                    // Convert DEF and IMP to constant value
                    int value = qualificationPlace[screener.Replace(" ", string.Empty)]
                        .Equals(SkillMatrix.kDefaultValueString)
                        ? SkillMatrix.kDefaultValue
                        : qualificationPlace[screener.Replace(" ", string.Empty)]
                            .Equals(SkillMatrix.kImpossibleValueString)
                            ? SkillMatrix.kImpossibleValue
                            : Convert.ToInt32(
                                qualificationPlace[screener.Replace(" ", string.Empty)]);

                    if (skillMatrix == null)
                    {
                        skillMatrix = new SkillMatrix
                        {
                            QualificationPlace =
                                _unitOfWork.QualificationPlaceRepository.First(
                                    e => e.QualificationPlaceId == qualificationPlaceId),
                            webpages_UserProfile =
                                _unitOfWork.UserProfileRepository.First(e => e.UserId == screenerId),
                            SkillValue = value,
                            SkillMeta = categoryKey
                        };
                        _unitOfWork.SkillMatrixRepository.Add(skillMatrix);
                    }
                    else
                    {
                        skillMatrix.SkillValue = value;
                    }
                }
            }
            _unitOfWork.Commit();
        }

        #endregion
    }
}