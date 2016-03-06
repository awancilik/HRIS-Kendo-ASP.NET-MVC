using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;
using Nalysa.Common.Log;

namespace CVScreeningService.Services.LookUpDatabase
{
    public abstract class LookUpDatabaseService<T> : ILookUpDatabaseService<T>
        where T : BaseQualificationPlaceDTO
    {
        private readonly IQualificationPlaceFactory _factory;
        private readonly IUnitOfWork _uow;

        protected LookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory)
        {
            _uow = uow;
            _factory = factory;
            Mapper.CreateMap<QualificationPlace, BaseQualificationPlaceDTO>()
                .ForMember(dto => dto.TypeOfCheckType,
                    m => m.MapFrom(poco => poco.GetTypeOfChecksCompatible().FirstOrDefault()))
                .Include<QualificationPlace, QualificationPlaceDTO>()
                .Include<Court, CourtDTO>()
                .Include<Police, PoliceDTO>()
                .Include<ImmigrationOffice, ImmigrationOfficeDTO>()
                .Include<Faculty, FacultyDTO>()
                .Include<Company, CompanyDTO>()
                .Include<HighSchool, HighSchoolDTO>()
                .Include<DrivingLicenseCheckingOffice, DrivingLicenseOfficeDTO>()
                .Include<CertificationPlace, CertificationPlaceDTO>()
                .Include<PopulationOffice, PopulationOfficeDTO>();
            Mapper.CreateMap<ContactPerson, ContactPersonDTO>();
            Mapper.CreateMap<ContactInfo, ContactInfoDTO>();
            Mapper.CreateMap<Address, AddressDTO>();
            Mapper.CreateMap<ScreeningQualificationPlaceMeta, ScreeningQualificationPlaceMetaDTO>();
        }

        public virtual T GetQualificationPlace(int id)
        {
            throw new NotImplementedException();
        }

        public virtual ErrorCode CreateOrEditQualificationPlace(ref T qualificationPlace)
        {
            var addressId = qualificationPlace.Address.Location.LocationId;
            var address = new Address
            {
                Street = qualificationPlace.Address.Street,
                PostalCode = qualificationPlace.Address.PostalCode,
                Location = _uow.LocationRepository.First(l => l.LocationId == addressId)
            };
            address = _uow.AddressRepository.Add(address);

            var qualificationPlaceId = qualificationPlace.QualificationPlaceId;

            //check if the id exist, then do edit
            var isExist = _uow.QualificationPlaceRepository
                .Exist(p => p.QualificationPlaceId == qualificationPlaceId);

            //check if exist based on name
            var qualificationPlaceDTO = qualificationPlace;
            if (!isExist && !ValidateExistingObject(qualificationPlaceDTO))
                return ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_ALREADY_EXIST;

            var _qualificationPlace = isExist
                ? _uow.QualificationPlaceRepository
                    .First(p => p.QualificationPlaceId == qualificationPlaceId)
                : _factory.Create(qualificationPlace);

            _qualificationPlace.QualificationPlaceName = qualificationPlace.QualificationPlaceName;
            _qualificationPlace.QualificationPlaceDescription = qualificationPlace.QualificationPlaceDescription;
            _qualificationPlace.QualificationPlaceWebSite = qualificationPlace.QualificationPlaceWebSite;
            _qualificationPlace.QualificationPlaceCategory = qualificationPlace.QualificationPlaceCategory;
            _qualificationPlace.Address = address;

            try
            {
                _qualificationPlace = _uow.QualificationPlaceRepository.Add(_qualificationPlace);
                _uow.Commit();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        _qualificationPlace.QualificationPlaceName, ex.Message));
                return ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_INSERT_ERROR;
            }

            //set the id of DTO from the inserted object
            qualificationPlace.QualificationPlaceId = _qualificationPlace.QualificationPlaceId;
            return ErrorCode.NO_ERROR;
        }

        protected bool ValidateExistingObject(BaseQualificationPlaceDTO qualificationPlaceDTO)
        {
            //search the existing object with the same name
            var existingQualificationPlace = _uow.QualificationPlaceRepository.First(
                e => e.QualificationPlaceName.ToLower().Equals(qualificationPlaceDTO.QualificationPlaceName.ToLower()));
            //if there is existing name but not active, then it will be valid to have the same name
            return existingQualificationPlace == null || existingQualificationPlace.QualificationPlaceIsDeactivated;
        }

        public virtual List<T> GetAllQualificationPlaces()
        {
            return null;
        }

        public virtual ErrorCode DeleteQualificationPlace(T qualificationPlaceDTO)
        {
            var id = qualificationPlaceDTO.QualificationPlaceId;
            var qualificationPlace = _uow.QualificationPlaceRepository.First(
                q => q.QualificationPlaceId == id);
            if (qualificationPlace == null)
                return ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_NOT_FOUND;
            if (qualificationPlace.QualificationPlaceIsDeactivated)
                return ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_ALREADY_DEACTIVATED;

            try
            {
                qualificationPlace.QualificationPlaceIsDeactivated = true;
                _uow.Commit();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        id, ex.Message));
                return ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_DELETE_ERROR;
            }
            return ErrorCode.NO_ERROR;
        }

        public List<ContactPersonDTO> GetContactPersons(int id)
        {
            var qualificationPlaceBO = _uow.QualificationPlaceRepository
                .First(q => q.QualificationPlaceId == id);
            return qualificationPlaceBO.ContactPerson.Select(
                Mapper.Map<ContactPerson, ContactPersonDTO>).ToList();
        }

        public IEnumerable<string> GetTypeOfCheckCategory(int qualificationPlaceId)
        {
            var categories = new List<string>();
            var typeOfChecks = _uow.TypeOfCheckRepository.GetAll();
            var qualificationPlace = _uow.QualificationPlaceRepository
                .First(e => e.QualificationPlaceId == qualificationPlaceId);

            foreach (var typeOfCheck in typeOfChecks)
            {
                if (!typeOfCheck.TypeOfCheckCode.Equals(
                    Convert.ToByte(qualificationPlace.GetTypeOfChecksCompatible().FirstOrDefault()))) continue;

                var typeOfCheckMeta = typeOfCheck.TypeOfCheckMeta;
                var firstInvestigationPlace =
                    typeOfCheckMeta.FirstOrDefault(e => e.TypeOfCheckMetaKey == "FIRST_INVESTIGATION_PLACE");
                var secondInvestigationPlace =
                    typeOfCheckMeta.FirstOrDefault(e => e.TypeOfCheckMetaKey == "SECOND_INVESTIGATION_PLACE");
                    
                if(firstInvestigationPlace!=null)
                    categories.Add(firstInvestigationPlace.TypeOfCheckMetaValue);
                if(secondInvestigationPlace!=null)
                    categories.Add(secondInvestigationPlace.TypeOfCheckMetaValue);
            }
            return categories;
        }

        public IEnumerable<BaseQualificationPlaceDTO> GetQualificationPlaceByScreenerCategory(string screenerCategory)
        {
            var results = new List<BaseQualificationPlaceDTO>();
            foreach (var qualificationPlace in _uow.QualificationPlaceRepository.GetAll())
            {
                QualificationPlace place = qualificationPlace;
                var code = Convert.ToByte(place.GetTypeOfChecksCompatible().FirstOrDefault());
                var typeOfCheck = 
                    _uow.TypeOfCheckRepository.First(e =>
                    e.TypeOfCheckCode == code);

                if (typeOfCheck.HasSkillCriterion(screenerCategory) && (screenerCategory == TypeOfCheckMeta.kOfficeCategory ? 
                    typeOfCheck.IsOfficeTypeOfCheck() : typeOfCheck.IsOnFieldTypeOfCheck()))
                {
                    results.Add(Mapper.Map<QualificationPlace, BaseQualificationPlaceDTO>(qualificationPlace));
                }
            }
            return results;
        }
    }
}