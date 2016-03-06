using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Services.Common;
using CVScreeningService.Services.Screening;
using CVScreeningService.Services.UserManagement;

namespace CVScreeningService.Tests.UnitTest
{
    public class Utilities
    {
        public static void InitRoles(IUserManagementService userManagementService)
        {
            userManagementService.CreateRole(webpages_Roles.kAccountManagerRole);
            userManagementService.CreateRole(webpages_Roles.kAdministratorRole);
            userManagementService.CreateRole(webpages_Roles.kClientRole);
            userManagementService.CreateRole(webpages_Roles.kHrRole);
            userManagementService.CreateRole(webpages_Roles.kProductionManagerRole);
            userManagementService.CreateRole(webpages_Roles.kQualityControlRole);
            userManagementService.CreateRole(webpages_Roles.kScreenerRole);
            userManagementService.CreateRole(webpages_Roles.kQualifierRole);
        }

        public static void InitPermissionForProductionManager(IUnitOfWork unifOfWork)
        {
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 1,
                Roles = new webpages_Roles
                {
                    RoleId = 5,
                    RoleName = webpages_Roles.kProductionManagerRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kScreeningViewPermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 2,
                Roles = new webpages_Roles
                {
                    RoleId = 5,
                    RoleName = webpages_Roles.kProductionManagerRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kAtomicCheckAssignPermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 3,
                Roles = new webpages_Roles
                {
                    RoleId = 5,
                    RoleName = webpages_Roles.kProductionManagerRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                PermissionIsGranted = true
            });
        }


        public static void InitPermissionForQualifier(IUnitOfWork unifOfWork)
        {
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 4,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kScreeningCreatePermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 5,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kScreeningManagePermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 6,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kScreeningViewPermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 7,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kContractViewPermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 8,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kScreeningLevelViewPermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 9,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kScreeningLevelVersionViewPermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 10,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kAtomicCheckViewPermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 11,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kInternalScreeningDiscussionManagePermission,
                PermissionIsGranted = true
            });
            unifOfWork.PermissionRepository.Add(new CVScreeningCore.Models.Permission
            {
                PermissionId = 12,
                Roles = new webpages_Roles
                {
                    RoleId = 8,
                    RoleName = webpages_Roles.kQualifierRole
                },
                PermissionName = CVScreeningCore.Models.Permission.kInternalAtomicCheckDiscussionManagePermission,
                PermissionIsGranted = true
            });
        }


        /// <summary>
        ///     Create fictive location for testing
        /// </summary>
        /// <param name="commonService"></param>
        public static void InitLocations(ICommonService commonService)
        {
            // Create country location INDONESIA
            var countryDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY,
                LocationName = "Indonesia",
                LocationParent = null,
                LocationParentLocationId = null
            };
            ErrorCode error = commonService.CreateLocation(ref countryDTO);

            // Create province location DKI Jakarta
            var provinceJakartaDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_PROVINCE,
                LocationName = "DKI Jakarta",
                LocationParent = countryDTO,
                LocationParentLocationId = countryDTO.LocationId
            };
            error = commonService.CreateLocation(ref provinceJakartaDTO);

            var provinceJawaTimurDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_PROVINCE,
                LocationName = "Jawa Timur",
                LocationParent = countryDTO,
                LocationParentLocationId = countryDTO.LocationId
            };
            error = commonService.CreateLocation(ref provinceJawaTimurDTO);

            var provinceJawaTengahDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_PROVINCE,
                LocationName = "Jawa Tengah",
                LocationParent = countryDTO,
                LocationParentLocationId = countryDTO.LocationId
            };
            error = commonService.CreateLocation(ref provinceJawaTengahDTO);

            // Create city location Jakarta Selatan
            var cityJakartaSelatanDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY,
                LocationName = "Jakarta Selatan",
                LocationParent = provinceJakartaDTO,
                LocationParentLocationId = provinceJakartaDTO.LocationId
            };
            error = commonService.CreateLocation(ref cityJakartaSelatanDTO);

            // Create city location Jakarta Timur
            var cityJakartaTimurDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY,
                LocationName = "Jakarta Timur",
                LocationParent = provinceJakartaDTO,
                LocationParentLocationId = provinceJakartaDTO.LocationId
            };
            error = commonService.CreateLocation(ref cityJakartaTimurDTO);

            // Create city location Jakarta Pusat
            var cityJakartaPusatDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY,
                LocationName = "Jakarta Pusat",
                LocationParent = provinceJakartaDTO,
                LocationParentLocationId = provinceJakartaDTO.LocationId
            };
            error = commonService.CreateLocation(ref cityJakartaPusatDTO);

            var districtKemayoranDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_DISTRICT,
                LocationName = "Kemayoran",
                LocationParent = cityJakartaPusatDTO,
                LocationParentLocationId = cityJakartaPusatDTO.LocationId
            };
            commonService.CreateLocation(ref districtKemayoranDTO);

            var subDistrictKemayoranBaratDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT,
                LocationName = "Kemayoran Barat",
                LocationParent = districtKemayoranDTO,
                LocationParentLocationId = districtKemayoranDTO.LocationId
            };
            commonService.CreateLocation(ref subDistrictKemayoranBaratDTO);

            // Create city location Jakarta Barat
            var cityJakartaBaratDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY,
                LocationName = "Jakarta Barat",
                LocationParent = provinceJakartaDTO,
                LocationParentLocationId = provinceJakartaDTO.LocationId
            };
            error = commonService.CreateLocation(ref cityJakartaBaratDTO);

            // Create city location Surabaya
            var citySurabayaDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY,
                LocationName = "Surabaya",
                LocationParent = provinceJawaTimurDTO,
                LocationParentLocationId = provinceJawaTimurDTO.LocationId
            };
            error = commonService.CreateLocation(ref citySurabayaDTO);

            // Create city location Malang
            var cityMalangDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_CITY,
                LocationName = "Malang",
                LocationParent = provinceJawaTimurDTO,
                LocationParentLocationId = provinceJawaTimurDTO.LocationId
            };
            error = commonService.CreateLocation(ref cityMalangDTO);

            // Create district location Cilandak
            var districtCilandakDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_DISTRICT,
                LocationName = "Cilandak",
                LocationParent = cityJakartaSelatanDTO,
                LocationParentLocationId = cityJakartaSelatanDTO.LocationId
            };
            error = commonService.CreateLocation(ref districtCilandakDTO);

            // Create subdistrict location Cilandak Utara
            var districtCilandakUtaraDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT,
                LocationName = "Cilandak Utara",
                LocationParent = districtCilandakDTO,
                LocationParentLocationId = districtCilandakDTO.LocationId
            };
            error = commonService.CreateLocation(ref districtCilandakUtaraDTO);


            // Create subdistrict location Cipete
            var districtCipeteDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT,
                LocationName = "Cipete",
                LocationParent = districtCilandakDTO,
                LocationParentLocationId = districtCilandakDTO.LocationId
            };
            error = commonService.CreateLocation(ref districtCipeteDTO);


            // Create district location Pasar Minggu
            var districtPasarMingguDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_DISTRICT,
                LocationName = "Pasar Minggu",
                LocationParent = cityJakartaSelatanDTO,
                LocationParentLocationId = cityJakartaSelatanDTO.LocationId
            };
            error = commonService.CreateLocation(ref districtPasarMingguDTO);

            // Create subdistrict location Cilandak Utara
            var subDistrictJatipadangDTO = new LocationDTO
            {
                LocationLevel = (int) LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT,
                LocationName = "Jati Padang",
                LocationParent = districtPasarMingguDTO,
                LocationParentLocationId = districtPasarMingguDTO.LocationId
            };
            commonService.CreateLocation(ref subDistrictJatipadangDTO);
        }

        public static void InitTypeOfCheck(
            IScreeningService screeningService, IUnitOfWork unifOfWork)
        {
            foreach (var dispatchingSkill in BuildDispatchingSkills())
            {
                unifOfWork.DispatchingSettingsRepository.Add(
                    new DispatchingSettings
                    {
                        DispatchingSettingsKey = dispatchingSkill.Key,
                        DispatchingSettingsValue = dispatchingSkill.Value
                    });
            }

            var typeOfCheckDTO = new TypeOfCheckDTO();

            typeOfCheckDTO.CheckName = "Education check standard";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.EDUCATION_CHECK_STANDARD;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");

            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "10", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kSkillCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kSkillCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);

            typeOfCheckDTO.CheckName = "Education check with evidence";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.EDUCATION_CHECK_WITH_EVIDENCE;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "10", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kSkillCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOfficeCategory);

            typeOfCheckDTO.CheckName = "Employment check standard";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "10", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "5", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Employment check performance";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_PERFORMANCE;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "10", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "5", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Police check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.POLICE_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No cases filed");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOnFieldCategory);
            
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kSkillCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kSkillCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);
            


            typeOfCheckDTO.CheckName = "Litigation check civil";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.LITIGATION_CHECK_CIVIL;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No cases filed");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);

            typeOfCheckDTO.CheckName = "Litigation check criminal";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.LITIGATION_CHECK_CRIMINAL;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                "REPORT_SUMMARY_VALUE_DONE_OK", "No cases filed");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Litigation check civil and criminal";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.LITIGATION_CHECK_CIVIL_CRIMINAL;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No cases filed");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE",
                "On field");

            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Bankruptcy check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.BANKRUPTY_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No cases filed");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "1", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "2", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Industrial court";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.INDUSTRIAL_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No cases filed");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Media check simplified";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.MEDIA_CHECK_SIMPLIFIED;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No adverse information");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "No");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "4", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "2", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);

            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            typeOfCheckDTO.CheckName = "Media check comprehensive";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.MEDIA_CHECK_COMPREHENSIVE;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No adverse information");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "No");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "6", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Professional qualification check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.PROFESSIONNAL_QUALIFICATION_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "ID check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.ID_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "No");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "4", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Passport check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.PASSPORT_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "2", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);

            typeOfCheckDTO.CheckName = "Driving license check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.DRIVING_LICENSE_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Neighbourhood check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE",
                "On field");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "Yes");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "7", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Reference check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.REFERENCES_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No adverse information");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "4", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "2", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Reverse directorship";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.REVERSE_DIRECTORSHIP;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No information found");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "No");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "4", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "2", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Group sanctions check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.GROUP_SANCTIONS_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No adverse information");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "No");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "4", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "2", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Contact number verifications check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.CONTACT_NUMBER_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "Verified");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Invalid");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "No");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "4", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "2", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Medical check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.MEDICAL_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK", "Done");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "No");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);


            typeOfCheckDTO.CheckName = "Credit check";
            typeOfCheckDTO.TypeOfCheckCode = (Byte) TypeOfCheckEnum.CREDIT_CHECK;
            screeningService.CreateTypeOfCheck(ref typeOfCheckDTO);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_OK",
                "No adverse information");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "REPORT_SUMMARY_VALUE_DONE_KO",
                "Mandatory");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "FIRST_INVESTIGATION_PLACE", "Office");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "SECOND_INVESTIGATION_PLACE", "N/A");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, "CAN_BE_WRONGLY_QUALIFIED", "No");
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOfficeCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kAverageCompletionRateKey, "5", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName,
                TypeOfCheckMeta.kCompletionMinimumWorkingDays, "3", AtomicCheck.kOnFieldCategory);
            screeningService.CreateTypeOfCheckMetaValue(typeOfCheckDTO.CheckName, TypeOfCheckMeta.kGeographicalCriterion,
                TypeOfCheckMeta.kNoValue, AtomicCheck.kOnFieldCategory);

            //AVAILABILITY_CRITERION && WORKLOAD_CRITERION -> ALL
            
            //ALREADY_ASSIGNED_CRITERION 
            var blackListedAlreadyAssignedCriterion = new List<TypeOfCheckEnum>
            {
                TypeOfCheckEnum.BANKRUPTY_CHECK,
                TypeOfCheckEnum.MEDICAL_CHECK,
                TypeOfCheckEnum.MEDIA_CHECK_SIMPLIFIED,
                TypeOfCheckEnum.MEDIA_CHECK_COMPREHENSIVE,
                TypeOfCheckEnum.ID_CHECK,
                TypeOfCheckEnum.PASSPORT_CHECK,
                TypeOfCheckEnum.REFERENCES_CHECK,
                TypeOfCheckEnum.REVERSE_DIRECTORSHIP,
                TypeOfCheckEnum.GROUP_SANCTIONS_CHECK,
                TypeOfCheckEnum.CONTACT_NUMBER_CHECK,
                TypeOfCheckEnum.MEDICAL_CHECK
            };

            foreach (var typeOfCheck in unifOfWork.TypeOfCheckRepository.GetAll())
            {
                //AVAILABILITY_CRITERION
                screeningService.CreateTypeOfCheckMetaValue(typeOfCheck.CheckName, TypeOfCheckMeta.kAvailabilityCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);
                screeningService.CreateTypeOfCheckMetaValue(typeOfCheck.CheckName, TypeOfCheckMeta.kAvailabilityCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOfficeCategory);

                //WORKLOAD_CRITERION
                screeningService.CreateTypeOfCheckMetaValue(typeOfCheck.CheckName, TypeOfCheckMeta.kWorkLoadCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);
                screeningService.CreateTypeOfCheckMetaValue(typeOfCheck.CheckName, TypeOfCheckMeta.kWorkLoadCriterion,
                TypeOfCheckMeta.kYesValue, AtomicCheck.kOfficeCategory);

                //ALREADY_ASSIGNED_CRITERION
                if (blackListedAlreadyAssignedCriterion.Contains((TypeOfCheckEnum) typeOfCheck.TypeOfCheckCode))
                {
                    screeningService.CreateTypeOfCheckMetaValue(typeOfCheck.CheckName,
                        TypeOfCheckMeta.kAlreadyAssignedCriterion,
                        TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);
                    screeningService.CreateTypeOfCheckMetaValue(typeOfCheck.CheckName,
                        TypeOfCheckMeta.kAlreadyAssignedCriterion,
                        TypeOfCheckMeta.kYesValue, AtomicCheck.kOfficeCategory);
                }
                else
                {
                    screeningService.CreateTypeOfCheckMetaValue(typeOfCheck.CheckName,
                        TypeOfCheckMeta.kAlreadyAssignedCriterion,
                        TypeOfCheckMeta.kYesValue, AtomicCheck.kOnFieldCategory);
                    screeningService.CreateTypeOfCheckMetaValue(typeOfCheck.CheckName,
                        TypeOfCheckMeta.kAlreadyAssignedCriterion,
                        TypeOfCheckMeta.kYesValue, AtomicCheck.kOfficeCategory);
                }
            }
        }

        /// <summary>
        ///     Build DTO to create location
        /// </summary>
        /// <returns></returns>
        public static AddressDTO BuildAddressDTO()
        {
            return new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 14
                }
            };
        }

        /// <summary>
        ///     Build DTO to create screener account in office
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildScreenerAccountOfficeDTO()
        {
            UserProfileDTO screener = BuildAccountSample();
            screener.ScreenerCategory = AtomicCheck.kOfficeCategory;
            screener.UserName = "screener@office.com";
            return screener;
        }


        /// <summary>
        ///     Build DTO to create screener account on field
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildScreenerAccountOnFieldDTO()
        {
            UserProfileDTO screener = BuildAccountSample();
            screener.ScreenerCategory = AtomicCheck.kOnFieldCategory;
            screener.UserName = "screener@onfield.com";
            return screener;
        }

        /// <summary>
        ///     Build DTO to create account
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildAccountSample()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999"
            };

            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 13
                }
            };

            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact to retrieve"
            };

            var userProfileDTO = new UserProfileDTO
            {
                FullName = "My fullname test",
                UserName = "justfortest2@mytest.com",
                Address = addressDTO,
                Remarks = "My remarks",
                ContactInfo = contactInfoDTO,
                ContactPerson = contactPersonDTO,
                TenantId = 1
            };
            return userProfileDTO;
        }

        /// <summary>
        ///     Build DTO to create account
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildAdminAccountSample()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999"
            };
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 13
                }
            };
            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact to retrieve"
            };
            var userProfileDTO = new UserProfileDTO
            {
                FullName = "Administrator",
                UserName = "admin@admin.com",
                Address = addressDTO,
                Remarks = "My remarks",
                ContactInfo = contactInfoDTO,
                ContactPerson = contactPersonDTO,
                TenantId = 1
            };
            return userProfileDTO;
        }

        /// <summary>
        ///     Build DTO to create account
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildQualifierAccountSample()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999"
            };
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 13
                }
            };
            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact to retrieve"
            };
            var userProfileDTO = new UserProfileDTO
            {
                FullName = "Qualifier",
                UserName = "qualifier@qualifier.com",
                Address = addressDTO,
                Remarks = "My remarks",
                ContactInfo = contactInfoDTO,
                ContactPerson = contactPersonDTO,
                TenantId = 1
            };
            return userProfileDTO;
        }


        /// <summary>
        ///     Build DTO to create account
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildQualityControlAccountSample()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999"
            };
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 13
                }
            };

            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact to retrieve"
            };

            var userProfileDTO = new UserProfileDTO
            {
                FullName = "QC",
                UserName = "qc@qc.com",
                Address = addressDTO,
                Remarks = "My remarks",
                ContactInfo = contactInfoDTO,
                ContactPerson = contactPersonDTO,
                TenantId = 1
            };
            return userProfileDTO;
        }

        /// <summary>
        ///     Build DTO to create account
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildAccountManagerAccountSample()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999"
            };
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 13
                }
            };
            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact to retrieve"
            };
            var userProfileDTO = new UserProfileDTO
            {
                FullName = "AM",
                UserName = "am@am.com",
                Address = addressDTO,
                Remarks = "My remarks",
                ContactInfo = contactInfoDTO,
                ContactPerson = contactPersonDTO,
                TenantId = 1
            };
            return userProfileDTO;
        }

        /// <summary>
        ///     Build DTO to create account
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildProductionManagerAccountSample()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999"
            };
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 13
                }
            };
            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact to retrieve"
            };
            var userProfileDTO = new UserProfileDTO
            {
                FullName = "PM",
                UserName = "pm@pm.com",
                Address = addressDTO,
                Remarks = "My remarks",
                ContactInfo = contactInfoDTO,
                ContactPerson = contactPersonDTO,
                TenantId = 1
            };
            return userProfileDTO;
        }

        /// <summary>
        ///     Build DTO to create account
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildHrAccountSample()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                HomePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999"
            };
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 1",
                PostalCode = "12790",
                Location = new LocationDTO
                {
                    LocationId = 13
                }
            };
            var contactPersonDTO = new ContactPersonDTO
            {
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
                ContactPersonName = "My contact to retrieve"
            };
            var userProfileDTO = new UserProfileDTO
            {
                FullName = "Hr",
                UserName = "hr@hr.com",
                Address = addressDTO,
                Remarks = "My remarks",
                ContactInfo = contactInfoDTO,
                ContactPerson = contactPersonDTO,
                TenantId = 1
            };
            return userProfileDTO;
        }


        /// <summary>
        ///     Build DTO to create client account
        /// </summary>
        /// <returns></returns>
        public static UserProfileDTO BuildClientAccountSample()
        {
            var contactInfoDTO = new ContactInfoDTO
            {
                MobilePhoneNumber = "622199988888",
                WorkPhoneNumber = "622199988999"
            };


            var userProfileDTO = new UserProfileDTO
            {
                FullName = "My client account",
                UserName = "myclient@mytest.com",
                Remarks = "My client account remarks",
                ContactInfo = contactInfoDTO,
                TenantId = 1
            };

            return userProfileDTO;
        }


        /// <summary>
        ///     Build DTO to create account
        /// </summary>
        /// <returns></returns>
        public static ClientCompanyDTO BuildClientCompanySample()
        {
            var addressDTO = new AddressDTO
            {
                Street = "Jalan Cipete, 4",
                PostalCode = "12780",
                Location = new LocationDTO
                {
                    LocationId = 14
                }
            };

            var contactInfoDTO = new ContactInfoDTO
            {
                WebSite = "www.mycompany.com",
            };

            // Public holiday from today to today + 1
            var clientCompanyDTO = new ClientCompanyDTO
            {
                ClientCompanyName = "My client company",
                Category = "IT",
                Description = "My description",
                Address = addressDTO,
                ContactInfo = contactInfoDTO,
            };

            return clientCompanyDTO;
        }

        /// <summary>
        ///     Build DTO to create client contract
        /// </summary>
        /// <returns></returns>
        public static ClientContractDTO BuildClientContractDTO()
        {
            // Public holiday from today to today + 1
            ClientCompanyDTO clientCompanyDTO = BuildClientCompanySample();

            var contractDTO = new ClientContractDTO
            {
                ContractReference = "INT-BC",
                ContractDescription = "Contract description",
                ContractYear = "2014",
                ClientCompany = clientCompanyDTO,
                IsContractEnabled = true
            };
            return contractDTO;
        }

        /// <summary>
        ///     Build DTO to create a screening level
        /// </summary>
        /// <returns></returns>
        public static ScreeningLevelDTO BuildScreeningLevelDTO()
        {
            var screeningLevelDTO = new ScreeningLevelDTO
            {
                ScreeningLevelName = "Employee",
                Contract = BuildClientContractDTO()
            };
            return screeningLevelDTO;
        }

        /// <summary>
        ///     Build DTO to create a screening level version
        /// </summary>
        /// <returns></returns>
        public static ScreeningLevelVersionDTO BuildScreeningLevelVersionDTO()
        {
            var screeningLevelVersionDTO = new ScreeningLevelVersionDTO
            {
                ScreeningLevelVersionDescription = "Employee description",
                ScreeningLevelVersionLanguage = "Bahasa Indonesia",
                ScreeningLevelVersionAllowedToContactCurrentCompany = ScreeningLevelVersion.kHR,
                ScreeningLevelVersionAllowedToContactCandidate = true,
                ScreeningLevelVersionStartDate = DateTime.Now,
                ScreeningLevelVersionEndDate = DateTime.Now.AddDays(10),
                ScreeningLevelVersionTurnaroundTime = 5
            };
            return screeningLevelVersionDTO;
        }

        /// <summary>
        ///     Build DTO to create type of check set to create a screening level
        /// </summary>
        /// <returns></returns>
        public static List<TypeOfCheckScreeningLevelVersionDTO> BuildTypeOfCheckListForScreeningListDTO()
        {
            var list = new List<TypeOfCheckScreeningLevelVersionDTO>
            {
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "Professional qualification check",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.PROFESSIONNAL_QUALIFICATION_CHECK
                    },
                    TypeOfCheckScreeningComments = "Professional qualification check comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "Employment check standard",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.EMPLOYMENT_CHECK_STANDARD
                    },
                    TypeOfCheckScreeningComments = "Employement check comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "Passport check",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.PASSPORT_CHECK
                    },
                    TypeOfCheckScreeningComments = "Passport check comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "Police check",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.POLICE_CHECK
                    },
                    TypeOfCheckScreeningComments = "Police check comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "Education check standard",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.EDUCATION_CHECK_STANDARD
                    },
                    TypeOfCheckScreeningComments = "Education check standard comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "ID check",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.ID_CHECK
                    },
                    TypeOfCheckScreeningComments = "ID check comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "Bankruptcy check",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.BANKRUPTY_CHECK
                    },
                    TypeOfCheckScreeningComments = "Bankruptcy check comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "Neighbourhood check",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.NEIGHBOURHOOD_CHECK
                    },
                    TypeOfCheckScreeningComments = "Neighbourhood check comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        CheckName = "Media check simplified",
                        TypeOfCheckCode = (Byte) TypeOfCheckEnum.MEDIA_CHECK_SIMPLIFIED
                    },
                    TypeOfCheckScreeningComments = "Media check simplified"
                }
            };
            return list;
        }


        /// <summary>
        ///     Build DTO to create type of check set to create a screening level
        /// </summary>
        /// <returns></returns>
        public static List<TypeOfCheckScreeningLevelVersionDTO> BuildTypeOfCheckList2ForScreeningListDTO()
        {
            var list = new List<TypeOfCheckScreeningLevelVersionDTO>
            {
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO {CheckName = "Media check comprehensive"},
                    TypeOfCheckScreeningComments = "Media check comprehensive comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO {CheckName = "Litigation check civil"},
                    TypeOfCheckScreeningComments = "Litigation check civil comments"
                },
                new TypeOfCheckScreeningLevelVersionDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO {CheckName = "Reference check"},
                    TypeOfCheckScreeningComments = "Reference check comments"
                },
            };
            return list;
        }


        public static void BuildTypeOfCheckMetaRepository(ref IUnitOfWork unifOfWork)
        {
            unifOfWork.TypeOfCheckMetaRepository.Add(new TypeOfCheckMeta
            {
                TypeOfCheckMetaId = 1,
                TypeOfCheckMetaKey = "",
                TypeOfCheckMetaValue = "",
                TypeOfCheckMetaTenantId = 1
            });
        }


        /// <summary>
        ///     Build DTO to create screening
        /// </summary>
        /// <returns></returns>
        public static ScreeningDTO BuildScreeningDTO(
            ScreeningLevelVersionDTO screeningLevelVersionDTO,
            UserProfileDTO qualityControl)
        {
            return new ScreeningDTO
            {
                ScreeningFullName = "Screening full name",
                ScreeningLevelVersion = screeningLevelVersionDTO,
                QualityControl = qualityControl,
                Attachment = new Collection<AttachmentDTO>
                {
                    new AttachmentDTO
                    {
                        AttachmentId = 1,
                        AttachmentName = "My File 1",
                        AttachmentFileType = "PDF"
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to create screening
        /// </summary>
        /// <returns></returns>
        public static ScreeningQualificationDTO BuildScreeningQualificationDTO()
        {
            return new ScreeningQualificationDTO
            {
                ScreeningQualificationMaritalStatus = "Single",
                ScreeningQualificationBirthDate = new DateTime(1985, 12, 28),
                ScreeningQualificationBirthPlace = "Jakarta",
                ScreeningQualificationGender = "M",
                ScreeningQualificationIDCardNumber = "11CK3030",
                ScreeningQualificationRelationshipWithCandidate = "Mother",
                ScreeningQualificationPassportNumber = "111KKOD",
                CurrentAddress = BuildAddressDTO(),
                IDCardAddress = BuildAddressDTO(),
                CVAddress = BuildAddressDTO(),
                PersonalContactInfo = new ContactInfoDTO
                {
                    MobilePhoneNumber = "62-2199988888",
                    HomePhoneNumber = "62-2199988999"
                },
                EmergencyContactPerson = new ContactPersonDTO
                {
                    ContactPersonName = "My mother",
                    ContactInfo = new ContactInfoDTO
                    {
                        HomePhoneNumber = "62-2199988888"
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to creation an immigration office
        /// </summary>
        /// <returns></returns>
        public static ImmigrationOfficeDTO BuildImmigrationOfficeDTO()
        {
            return new ImmigrationOfficeDTO
            {
                QualificationPlaceName = "ImmigrationOffice New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description ImmigrationOffice New",
                QualificationPlaceWebSite = "http://immigrationOfficenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to creation a police office
        /// </summary>
        /// <returns></returns>
        public static PoliceDTO BuildPoliceOfficeDTO()
        {
            return new PoliceDTO
            {
                QualificationPlaceName = "Police New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description Police New",
                QualificationPlaceWebSite = "http://policenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to creation a district court
        /// </summary>
        /// <returns></returns>
        public static CourtDTO BuildDistrictCourtDTO()
        {
            return new CourtDTO
            {
                QualificationPlaceName = "District court New",
                QualificationPlaceCategory = "District",
                QualificationPlaceDescription = "District court New New",
                QualificationPlaceWebSite = "http://policenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to creation an industrial court
        /// </summary>
        /// <returns></returns>
        public static CourtDTO BuildIndustrialCourtDTO()
        {
            return new CourtDTO
            {
                QualificationPlaceName = "District industrial New",
                QualificationPlaceCategory = "Industrial",
                QualificationPlaceDescription = "District industrial New",
                QualificationPlaceWebSite = "http://policenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to creation a commercial court
        /// </summary>
        /// <returns></returns>
        public static CourtDTO BuildCommercialCourtDTO()
        {
            return new CourtDTO
            {
                QualificationPlaceName = "District commercial New",
                QualificationPlaceCategory = "Commercial",
                QualificationPlaceDescription = "District commercial New",
                QualificationPlaceWebSite = "http://policenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                }
            };
        }

        public static IDictionary<string, string> BuildDispatchingSkills()
        {
            return new Dictionary<string, string>()
            {
                {"DefaultScreenerCapabilities", "5"},
                {"GeographicalOutsideIndonesiaValue", "100"},
                {"GeographicalSameCityValue", "50"},
                {"GeographicalSameCountryValue", "16"},
                {"GeographicalSameDistrictValue", "66"},
                {"GeographicalSameProvinceValue", "33"},
                {"GeographicalSameSubDistrictValue", "83"}, 
                {"SkillCoefficient", "1"},
                {"WorkloadCoefficient", "2"},
                {"AvailibilityCoefficient", "4"},
                {"AlreadyCoefficient", "8"},
                {"GeographicalCoefficient", "1"}
            };
        } 


        /// <summary>
        ///     Build DTO to create a certification place
        /// </summary>
        /// <returns></returns>
        public static CertificationPlaceDTO BuildCertificationPlaceDTO()
        {
            return new CertificationPlaceDTO
            {
                QualificationPlaceName = "Iverson",
                QualificationPlaceDescription = "Iverson training",
                QualificationPlaceWebSite = "http://iversion.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to create a professionnal qualification
        /// </summary>
        /// <returns></returns>
        public static ProfessionalQualificationDTO BuildProfessionalQualificationDTO()
        {
            return new ProfessionalQualificationDTO
            {
                ProfessionalQualificationName = ".Net",
                ProfessionalQualificationCode = "N001"
            };
        }

        /// <summary>
        ///     Build DTO to creation a police office
        /// </summary>
        /// <returns></returns>
        public static CompanyDTO BuildCurrentCompanyDTO()
        {
            return new CompanyDTO
            {
                QualificationPlaceName = "Current Company New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description Company New",
                QualificationPlaceWebSite = "http://companynew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                },
                ScreeningQualificationPlaceMeta = new Collection<ScreeningQualificationPlaceMetaDTO>
                {
                    new ScreeningQualificationPlaceMetaDTO
                    {
                        ScreeningQualificationMetaKey = ScreeningQualificationPlaceMeta.kIsCurrentCompany,
                        ScreeningQualificationMetaValue = ScreeningQualificationPlaceMeta.kYesValue
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to creation a police office
        /// </summary>
        /// <returns></returns>
        public static CompanyDTO BuildCompanyDTO()
        {
            return new CompanyDTO
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
                        LocationId = 14
                    }
                },
                ScreeningQualificationPlaceMeta = new Collection<ScreeningQualificationPlaceMetaDTO>
                {
                    new ScreeningQualificationPlaceMetaDTO
                    {
                        ScreeningQualificationMetaKey = ScreeningQualificationPlaceMeta.kIsCurrentCompany,
                        ScreeningQualificationMetaValue = ScreeningQualificationPlaceMeta.kNoValue
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to creation an high school
        /// </summary>
        /// <returns></returns>
        public static HighSchoolDTO BuildHighSchoolDTO()
        {
            return new HighSchoolDTO
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
        }

        /// <summary>
        ///     Build DTO to create a faculty
        /// </summary>
        /// <returns></returns>
        public static FacultyDTO BuildFaculyDTO()
        {
            return new FacultyDTO
            {
                QualificationPlaceName = "Faculty New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description Faculty New",
                QualificationPlaceWebSite = "http://facultynew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                },
                University = new UniversityDTO
                {
                    UniversityId = 1
                }
            };
        }

        /// <summary>
        ///     Build DTO to create a faculty
        /// </summary>
        /// <returns></returns>
        public static DrivingLicenseOfficeDTO BuildDrivingLicenseOfficeDTO()
        {
            return new DrivingLicenseOfficeDTO
            {
                QualificationPlaceName = "DrivingLicenseOffice New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description DrivingLicenseOffice New",
                QualificationPlaceWebSite = "http://drivingLicenseOfficenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                }
            };
        }

        /// <summary>
        ///     Build DTO to creation an population office
        /// </summary>
        /// <returns></returns>
        public static PopulationOfficeDTO BuildPopulationOfficeDTO()
        {
            return new PopulationOfficeDTO
            {
                QualificationPlaceName = "PopulationOffice New",
                QualificationPlaceCategory = "Public",
                QualificationPlaceDescription = "Description PopulationOffice New",
                QualificationPlaceWebSite = "http://populationOfficenew.com",
                Address = new AddressDTO
                {
                    Street = "Street",
                    Location = new LocationDTO
                    {
                        LocationId = 14
                    }
                }
            };
        }

        public static void InitScreeners(IUnitOfWork unitOfWork)
        {
            //best for skills
            var screenerA = new webpages_UserProfile
            {
                FullName = "screenerA",
                ScreenerCategory = TypeOfCheckMeta.kOfficeCategory
            };
            
            //best for geographical
            var screenerB = new webpages_UserProfile()
            {
                FullName = "screenerB",
                ScreenerCategory = TypeOfCheckMeta.kOnFieldCategory
            };

            //best for workload 
            var screenerC = new webpages_UserProfile()
            {
                FullName = "screenerC",
                ScreenerCategory = TypeOfCheckMeta.kOfficeCategory
            };

            //best for availibility
            var screenerD = new webpages_UserProfile()
            {
                FullName = "screenerD",
                ScreenerCategory = TypeOfCheckMeta.kOnFieldCategory
            };

            //best for already assigned
            var screenerE = new webpages_UserProfile()
            {
                FullName = "screenerE",
                ScreenerCategory = TypeOfCheckMeta.kOnFieldCategory
            };

            var screenerF = new webpages_UserProfile()
            {
                FullName = "screenerF",
                ScreenerCategory = TypeOfCheckMeta.kOfficeCategory
            };

            //remove all users and their leaves
            foreach (var userLeave in unitOfWork.UserLeaveRepository.GetAll().Reverse())
            {
                unitOfWork.UserLeaveRepository.Delete(userLeave);
            }
            foreach (var userProfile in unitOfWork.UserProfileRepository.GetAll().Reverse())
            {
                unitOfWork.UserProfileRepository.Delete(userProfile);
            }
            unitOfWork.Commit();

            unitOfWork.UserProfileRepository.Add(screenerA);
            unitOfWork.UserProfileRepository.Add(screenerB);
            unitOfWork.UserProfileRepository.Add(screenerC);
            unitOfWork.UserProfileRepository.Add(screenerD);
            unitOfWork.UserProfileRepository.Add(screenerE);
            unitOfWork.UserProfileRepository.Add(screenerF);

            //set as Screener
            var roleAsScreener = unitOfWork.RoleRepository.First(e => e.RoleName == webpages_Roles.kScreenerRole);

            foreach (var userProfile in unitOfWork.UserProfileRepository.GetAll().Where(e => e.webpages_Roles.Count == 0))
            {
                userProfile.webpages_Roles = new Collection<webpages_Roles>
                {
                    roleAsScreener
                };
                userProfile.Address = new Address
                {
                    Location = unitOfWork.LocationRepository.First(e => e.LocationName == "Jati Padang")
                };
                userProfile.ScreenerCapabilities = 5;
            }
            unitOfWork.Commit();
        }

        public static void InitSkillMatrix(IUnitOfWork unitOfWork)
        {
            var allQualificationPlaces = unitOfWork.QualificationPlaceRepository.GetAll();
            var screeners =
                unitOfWork.UserProfileRepository.GetAll()
                    .Where(e => e.webpages_Roles.Any(f => f.RoleName == webpages_Roles.kScreenerRole));


            foreach (var skillMatrix in unitOfWork.SkillMatrixRepository.GetAll().Reverse())
            {
                unitOfWork.SkillMatrixRepository.Delete(skillMatrix);
            }

            foreach (var qualificationPlace in allQualificationPlaces)
            {
                foreach (var screener in screeners)
                {
                    unitOfWork.SkillMatrixRepository.Add(new SkillMatrix
                    {
                        webpages_UserProfile = screener,
                        QualificationPlace = qualificationPlace,
                        SkillMeta = 1,
                        SkillValue = 50
                    });
                    unitOfWork.SkillMatrixRepository.Add(new SkillMatrix
                    {
                        webpages_UserProfile = screener,
                        QualificationPlace = qualificationPlace,
                        SkillMeta = 2,
                        SkillValue = 50
                    });
                }
            }
            unitOfWork.Commit();
        }

        public static void InitAtomicChecks(IUnitOfWork unitOfWork)
        {
            foreach (var atomicCheck in unitOfWork.AtomicCheckRepository.GetAll())
            {
                atomicCheck.setState(AtomicCheckStateType.NEW);
                atomicCheck.Screener = null;
            }
        }
    }
}