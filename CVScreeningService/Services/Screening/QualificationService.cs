using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Exception;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;
using CVScreeningService.Filters;
using CVScreeningService.Services.Common;
using Nalysa.Common.Log;
using WebGrease.Css.Extensions;

namespace CVScreeningService.Services.Screening
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class QualificationService : IQualificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ICommonService _commonService;

        public QualificationService(IUnitOfWork uow, ICommonService commonService)
        {
            _uow = uow;
            _commonService = commonService;

            Mapper.CreateMap<ScreeningQualification, ScreeningQualificationDTO>();
            Mapper.CreateMap<CVScreeningCore.Models.Screening, ScreeningDTO>()
                .ForMember(dto => dto.ClientCompanyName, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyName))
                .ForMember(dto => dto.ScreeningLevelName, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelName))
                .ForMember(dto => dto.ClientCompanyId, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyId))
                .ForMember(dto => dto.ExternalDiscussionId, m => m.MapFrom(poco => poco.Discussion.First(u => u.DiscussionType == CVScreeningCore.Models.Discussion.kExternalScreeningType).DiscussionId))
                .ForMember(dto => dto.InternalDiscussionId, m => m.MapFrom(poco => poco.Discussion.First(u => u.DiscussionType == CVScreeningCore.Models.Discussion.kInternalScreeningType).DiscussionId))
                .ForMember(dto => dto.State, m => m.MapFrom(poco => poco.State.Name));

            Mapper.CreateMap<CVScreeningCore.Models.Screening, ScreeningBaseDTO>()
                .ForMember(dto => dto.ClientCompanyName, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyName))
                .ForMember(dto => dto.ScreeningLevelName, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.ScreeningLevelName))
                .ForMember(dto => dto.ClientCompanyId, m => m.MapFrom(poco => poco.ScreeningLevelVersion.ScreeningLevel.Contract.ClientCompany.ClientCompanyId))
                .ForMember(dto => dto.ExternalDiscussionId, m => m.MapFrom(poco => poco.Discussion.First(u => u.DiscussionType == CVScreeningCore.Models.Discussion.kExternalScreeningType).DiscussionId))
                .ForMember(dto => dto.InternalDiscussionId, m => m.MapFrom(poco => poco.Discussion.First(u => u.DiscussionType == CVScreeningCore.Models.Discussion.kInternalScreeningType).DiscussionId))
                .ForMember(dto => dto.State, m => m.MapFrom(poco => poco.State.Name));
            
            Mapper.CreateMap<ContactPerson, ContactPersonDTO>();
            Mapper.CreateMap<Address, AddressDTO>();
            Mapper.CreateMap<ContactInfo, ContactInfoDTO>();
            Mapper.CreateMap<QualificationPlace, BaseQualificationPlaceDTO>()
                .Include<QualificationPlace, QualificationPlaceDTO>()
                .Include<Court, CourtDTO>()
                .Include<Police, PoliceDTO>()
                .Include<Faculty, FacultyDTO>()
                .Include<Company, CompanyDTO>()
                .Include<HighSchool, HighSchoolDTO>()
                .Include<DrivingLicenseCheckingOffice, DrivingLicenseOfficeDTO>()
                .Include<CertificationPlace, CertificationPlaceDTO>();

            Mapper.CreateMap<ProfessionalQualification, ProfessionalQualificationDTO>();
            Mapper.CreateMap<ScreeningQualificationPlaceMeta, ScreeningQualificationPlaceMetaDTO>();

        }


        #region Qualification method

        /// <summary>
        /// Retrieve the qualification base data of a screening (martial status, gender ...)
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public virtual ScreeningQualificationDTO GetQualificationBase(ScreeningBaseDTO screeningDTO)
        {
            int id = screeningDTO.ScreeningId;

            // Screening does not exist
            if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
            {
                return null;
            }

            var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
            var qualification = screening.ScreeningQualification;

            var screeningQualificationDTO = Mapper.Map<ScreeningQualification, 
                ScreeningQualificationDTO>(qualification);
            
            // Neighborhood check should be check to see if any address are wrongly qualified
            if (screening.HasNeighbourhoodCheck())
            {
                screeningQualificationDTO.IsCVAddressWronglyQualified =
                    screening.GetCVAddressAtomicCheck().IsWronglyQualified();
                screeningQualificationDTO.IsCurrentAddressWronglyQualified =
                    screening.GetCurrentAddressAtomicCheck().IsWronglyQualified();
                screeningQualificationDTO.IsIDCardAddressWronglyQualified =
                    screening.GetIDCardAddressAtomicCheck().IsWronglyQualified();
            }
            return screeningQualificationDTO;
        }

        /// <summary>
        /// Set the qualification base data of a screening (martial status, gender ...)
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="screeningQualificationDTO"></param>
        /// <returns></returns>
        public virtual ErrorCode SetQualificationBase(ScreeningBaseDTO screeningDTO, ref ScreeningQualificationDTO screeningQualificationDTO)
        {
            int? id = screeningDTO.ScreeningId;
            var currentAddressDTO = screeningQualificationDTO.CurrentAddress;
            int? currentAddressLocationId = (currentAddressDTO != null && currentAddressDTO.Location != null) ? (int?)currentAddressDTO.Location.LocationId : null;
            var idCardAddressDTO = screeningQualificationDTO.IDCardAddress;
            var idCardAddressLocationId = (idCardAddressDTO != null && idCardAddressDTO.Location != null) ? (int?)idCardAddressDTO.Location.LocationId : null;
            var cvAddressDTO = screeningQualificationDTO.CVAddress;
            var cvAddressLocationId = (cvAddressDTO != null && cvAddressDTO.Location != null) ? (int?)cvAddressDTO.Location.LocationId : null;
            var personalContactInfoDTO = screeningQualificationDTO.PersonalContactInfo;
            var emergencyContactPersonDTO = screeningQualificationDTO.EmergencyContactPerson;

            try
            {
                // Screening does not exist
                if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
                {
                    return ErrorCode.SCREENING_NOT_FOUND;
                }

                var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
                var qualification = screening.ScreeningQualification;

                qualification.ScreeningQualificationMaritalStatus = screeningQualificationDTO.ScreeningQualificationMaritalStatus;
                qualification.ScreeningQualificationBirthDate = screeningQualificationDTO.ScreeningQualificationBirthDate;
                qualification.ScreeningQualificationBirthPlace = screeningQualificationDTO.ScreeningQualificationBirthPlace;
                qualification.ScreeningQualificationGender = screeningQualificationDTO.ScreeningQualificationGender;
                qualification.ScreeningQualificationIDCardNumber = screeningQualificationDTO.ScreeningQualificationIDCardNumber;
                qualification.ScreeningQualificationPassportNumber = screeningQualificationDTO.ScreeningQualificationPassportNumber;
                qualification.ScreeningQualificationRelationshipWithCandidate = screeningQualificationDTO.ScreeningQualificationRelationshipWithCandidate;
                qualification.ScreeningQualificationIsDeactivated = screeningQualificationDTO.ScreeningQualificationIsDeactivated;

                if (currentAddressDTO != null && currentAddressLocationId != null && currentAddressLocationId != 0)
                {
                    qualification.CurrentAddress = qualification.CurrentAddress ?? _uow.AddressRepository.Add(new Address());
                    qualification.CurrentAddress.Street = currentAddressDTO.Street;
                    qualification.CurrentAddress.PostalCode = currentAddressDTO.PostalCode;
                    qualification.CurrentAddress.Location = _uow.LocationRepository
                        .Single(l => l.LocationId == currentAddressLocationId);
                    if (screeningQualificationDTO.IsCurrentAddressReQualified)
                        screening.CurrentAddressHasBeenRequalified();
                }

                if (idCardAddressDTO != null && idCardAddressLocationId != null && idCardAddressLocationId != 0)
                {
                    qualification.IDCardAddress = qualification.IDCardAddress ?? _uow.AddressRepository.Add(new Address());
                    qualification.IDCardAddress.Street = idCardAddressDTO.Street;
                    qualification.IDCardAddress.PostalCode = idCardAddressDTO.PostalCode;
                    qualification.IDCardAddress.Location =
                        _uow.LocationRepository.Single(l => l.LocationId == idCardAddressLocationId);
                    if (screeningQualificationDTO.IsIDCardAddressReQualified)
                        screening.IDCardAddressHasBeenRequalified();
                }

                if (cvAddressDTO != null && cvAddressLocationId != null && cvAddressLocationId != 0)
                {
                    qualification.CVAddress = qualification.CVAddress ?? _uow.AddressRepository.Add(new Address());
                    qualification.CVAddress.Street = cvAddressDTO.Street;
                    qualification.CVAddress.PostalCode = cvAddressDTO.PostalCode;
                    qualification.CVAddress.Location = _uow.LocationRepository.Single(l => l.LocationId == cvAddressLocationId);
                    if (screeningQualificationDTO.IsCVAddressReQualified)
                        screening.CVAddressHasBeenRequalified();
                }

                if (personalContactInfoDTO != null)
                {
                    qualification.PersonalContactInfo = qualification.PersonalContactInfo ?? _uow.ContactInfoRepository.Add(new ContactInfo());
                    qualification.PersonalContactInfo.HomePhoneNumber = personalContactInfoDTO.HomePhoneNumber;
                    qualification.PersonalContactInfo.MobilePhoneNumber = personalContactInfoDTO.MobilePhoneNumber;
                }

                if (emergencyContactPersonDTO != null)
                {
                    qualification.EmergencyContactPerson = qualification.EmergencyContactPerson
                        ?? _uow.ContactPersonRepository.Add(
                                new ContactPerson
                                {
                                    ContactInfo = _uow.ContactInfoRepository.Add(new ContactInfo())
                                });
                    qualification.EmergencyContactPerson.ContactPersonName = emergencyContactPersonDTO.ContactPersonName;
                    if (emergencyContactPersonDTO.ContactInfo != null)
                        qualification.EmergencyContactPerson.ContactInfo.HomePhoneNumber = emergencyContactPersonDTO.ContactInfo.HomePhoneNumber;
                }

                screening.ComputeScreeningState();
                _uow.Commit();
                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: " +
                                  "{0}. screeningDTO: " +
                                  "{1}. screeningQualificationDTO: " +
                                  "{2}. Exception:{3}", 
                                  MethodBase.GetCurrentMethod().Name,
                    screeningDTO, screeningQualificationDTO, ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }

        /// <summary>
        /// Retrieve the list of qualifications places of a screening (police, court, company ...) 
        /// that are not linked to an atomic check wrongly qualified
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public virtual IEnumerable<BaseQualificationPlaceDTO> GetQualificationPlaces(ScreeningBaseDTO screeningDTO)
        {
            int? id = screeningDTO.ScreeningId;

            // Screening does not exist
            if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
            {
                return null;
            }

            var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
            var qualificationPlaces = screening.AtomicCheck
                .Where(u => u.QualificationPlace != null && !u.State.IsWronglyQualified())
                .Select(u => u.QualificationPlace);

            return qualificationPlaces.Select(Mapper.Map<QualificationPlace, BaseQualificationPlaceDTO>).ToList();
        }

        /// <summary>
        /// Retrieve the list of qualifications places of a screening (police, court, company ...) 
        /// that are linked to an atomic check wrongly qualified
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public virtual IEnumerable<BaseQualificationPlaceDTO> GetWronglyQualifiedQualificationPlaces(ScreeningBaseDTO screeningDTO)
        {
            int? id = screeningDTO.ScreeningId;

            // Screening does not exist
            if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
            {
                return null;
            }

            var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
            var qualificationPlaces = screening.AtomicCheck
                .Where(u => u.QualificationPlace != null && u.State.IsWronglyQualified()).Select(u => u.QualificationPlace);

            return qualificationPlaces.Select(Mapper.Map<QualificationPlace, BaseQualificationPlaceDTO>).ToList();
        }

        /// <summary>
        /// Set the list of qualifications places of a screening (police, court, company ...)
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="qualificationPlacesDTO"></param>
        /// <param name="requalifiedQualification"></param>
        /// <returns></returns>
        public virtual ErrorCode SetQualificationPlaces(ScreeningBaseDTO screeningDTO,
            IEnumerable<BaseQualificationPlaceDTO> qualificationPlacesDTO,
            IEnumerable<int> requalifiedQualification = null)
        {
            int? id = screeningDTO.ScreeningId;
            try
            {
                // Screening does not exist
                if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
                {
                    return ErrorCode.SCREENING_NOT_FOUND;
                }
                var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
                var qualificationPlaces = screening.QualificationPlace;
                var newQualifications = new List<QualificationPlace>();

                if (requalifiedQualification != null)
                {
                    requalifiedQualification.ForEach(q => screening.RequalificationCompleted(
                        _uow.QualificationPlaceRepository.Single(u => u.QualificationPlaceId == q)));
                }

                // Insert qualification places
                foreach (var qualificationPlaceDTO in qualificationPlacesDTO)
                {
                    var qualificationPlaceId = qualificationPlaceDTO.QualificationPlaceId;
                    
                    // Qualification place does not exist
                    if (!_uow.QualificationPlaceRepository.Exist(u => u.QualificationPlaceId == qualificationPlaceId))
                    {
                        return ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND;
                    }

                    // Screening already qualified with this qualification place
                    if (qualificationPlaces.Any(u => u.QualificationPlaceId == qualificationPlaceId))
                        continue;

                    var qualificationPlace =
                        _uow.QualificationPlaceRepository.Single(
                            u => u.QualificationPlaceId == qualificationPlaceId);

                    // If Qualification Place is Company then add the meta 
                    if (qualificationPlace is Company)
                    {
                        qualificationPlace.ScreeningQualificationPlaceMeta =
                            qualificationPlaceDTO.ScreeningQualificationPlaceMeta.Select(
                                e => new ScreeningQualificationPlaceMeta
                                {
                                    ScreeningQualificationMetaKey = e.ScreeningQualificationMetaKey,
                                    ScreeningQualificationMetaValue = e.ScreeningQualificationMetaValue,
                                    Screening = screening
                                }).ToList();
                    }
                    screening.QualificationPlace.Add(qualificationPlace);

                    // Add qualification to dictionnary of new qualification
                    newQualifications.Add(qualificationPlace);
                    LogManager.Instance.Info(string.Format("Function: {0}. Qualification added: {1}",
                        MethodBase.GetCurrentMethod().Name, qualificationPlace.ToString()));
                }
                
                foreach (var qualificationPlace in newQualifications)
                {
                    AtomicCheck atomicCheck = screening.SetAtomicCheck(qualificationPlace);
                    LogManager.Instance.Info(string.Format("Function: {0}. Qualification: {1}. Atomic check set: {2}",
                        MethodBase.GetCurrentMethod().Name, qualificationPlace.ToString(), atomicCheck.ToString()));
                }
                
                screening.ComputeScreeningState();
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (ExceptionQualificationNotCompatible ex)
            {
                LogManager.Instance.Error(
                string.Format("Function: {0}. screeningDTO: {1}. Exception:{2}",
                    MethodBase.GetCurrentMethod().Name,
                    screeningDTO.ToString(), ex.Message));
                return ErrorCode.DBLOOKUP_QUALIFICATION_NOT_COMPATIBLE_WITH_SCREENING;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                string.Format("Function: {0}. screeningDTO: {1}. Exception:{2}",
                    MethodBase.GetCurrentMethod().Name,
                    screeningDTO.ToString(), ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }
        }


        /// <summary>
        /// Set atomic checks of a screening as not applicable,
        /// </summary>
        /// <param name="screeningDTO">Screening</param>
        /// <param name="atomicCheckDTO">Contain typeOfCheck and category information needed to retrieve atomic checks with
        /// a boolean value to tell if the typeOfCheck is applicable or not</param>
        /// <returns></returns>
        public virtual ErrorCode SetAtomicChecksAsNotApplicable(ScreeningBaseDTO screeningDTO, IDictionary<AtomicCheckDTO, bool> atomicCheckDTO)
        {
            int? id = screeningDTO.ScreeningId;
            try
            {
                // Screening does not exist
                if (!_uow.ScreeningRepository.Exist(u => u.ScreeningId == id))
                {
                    return ErrorCode.SCREENING_NOT_FOUND;
                }
                var screening = _uow.ScreeningRepository.Single(u => u.ScreeningId == id);
                foreach (var checkDTO in atomicCheckDTO)
                {
                    var atomicCheck = screening.AtomicCheck.Where(
                        u => u.AtomicCheckType == checkDTO.Key.AtomicCheckType && 
                            u.TypeOfCheckScreeningLevelVersion.TypeOfCheck.TypeOfCheckCode == checkDTO.Key.TypeOfCheck.TypeOfCheckCode);

                    foreach (var check in atomicCheck)
                    {
                        // Type of check not applicable
                        if (checkDTO.Value)
                        {
                            // Atomic check already qualified cannot be not applicable
                            if (check.IsNotApplicable() || !check.IsQualified())
                            {
                                if (!check.IsNotApplicable())
                                {
                                    check.State.ToNotApplicable();
                                }
                            }
                            else
                            {
                                LogManager.Instance.Info(
                                    string.Format(
                                        "Function: {0}. Atomic check already qualified cannot be not applicable." +
                                        "Screening:{1}, AtomicCheck {2}",
                                        MethodBase.GetCurrentMethod().Name, screeningDTO.ToString(), check.ToString()));
                                return ErrorCode.ATOMIC_CHECK_NOT_APPLICABLE_UPDATE_ERROR;
                            }
                        }
                        // Type of check applicable
                        else
                        {
                            // Only type of check not applicable can become applicable
                            if (check.IsNotApplicable())
                            {
                                check.State.ToNew();
                            }
                        }
                    }
                }

                screening.ComputeScreeningState();
                _uow.Commit();

                return ErrorCode.NO_ERROR;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                string.Format("Function: {0}. screeningDTO: {1}. Exception:{2}",
                    MethodBase.GetCurrentMethod().Name,
                    screeningDTO.ToString(), ex.Message));
                return ErrorCode.UNKNOWN_ERROR;
            }

        }

        #endregion
    }
}