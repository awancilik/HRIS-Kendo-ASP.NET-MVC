using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;
using Nalysa.Common.Log;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class FacultyLookUpDatabaseService : LookUpDatabaseService<FacultyDTO>
    {
        private readonly IUnitOfWork _uow;

        public FacultyLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory) : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<Faculty, FacultyDTO>();
            Mapper.CreateMap<University, UniversityDTO>();
        }

        public override List<FacultyDTO> GetAllQualificationPlaces()
        {
            var faculties = _uow.QualificationPlaceRepository.AsQueryable<Faculty>().ToList()
                .Where(e => !e.QualificationPlaceIsDeactivated);
            return faculties.Select(Mapper.Map<Faculty, FacultyDTO>).ToList();
        }

        public override FacultyDTO GetQualificationPlace(int id)
        {
            var faculty = _uow.QualificationPlaceRepository.AsQueryable<Faculty>().ToList()
                .FirstOrDefault(e => !e.QualificationPlaceIsDeactivated);
            return Mapper.Map<Faculty, FacultyDTO>(faculty);
        }

        public override ErrorCode CreateOrEditQualificationPlace(ref FacultyDTO qualificationPlace)
        {
            var addressId = qualificationPlace.Address.Location.LocationId;
            var address = new Address
            {
                Street = qualificationPlace.Address.Street,
                PostalCode = qualificationPlace.Address.PostalCode,
                Location =
                    _uow.LocationRepository.First(l => l.LocationId == addressId)
            };
            address = _uow.AddressRepository.Add(address);

            var qualificationPlaceId = qualificationPlace.QualificationPlaceId;
            var isExist = _uow.QualificationPlaceRepository
                .Exist(p => p.QualificationPlaceId == qualificationPlaceId);

            //check if exist based on name
            var qualificationPlaceDTO = qualificationPlace;
            if (!isExist && !ValidateExistingObject(qualificationPlaceDTO))
                return ErrorCode.DBLOOKUP_QUALIFICATION_PLACE_ALREADY_EXIST;

            var _qualificationPlace = isExist
                ? _uow.QualificationPlaceRepository
                    .First(p => p.QualificationPlaceId == qualificationPlaceId)
                : new Faculty();

            _qualificationPlace.QualificationPlaceName = qualificationPlace.QualificationPlaceName;
            _qualificationPlace.QualificationPlaceDescription = qualificationPlace.QualificationPlaceDescription;
            _qualificationPlace.QualificationPlaceWebSite = qualificationPlace.QualificationPlaceWebSite;
            _qualificationPlace.QualificationPlaceCategory = qualificationPlace.QualificationPlaceCategory;
            _qualificationPlace.QualificationPlaceAlumniWebSite = qualificationPlace.QualificationPlaceAlumniWebSite;
            _qualificationPlace.Address = address;

            if (qualificationPlaceDTO.University == null ||
                _uow.UniversityRepository.First(e => e.UniversityId == qualificationPlaceDTO.University.UniversityId) == null)
            {
                return ErrorCode.DBLOOKUP_UNIVERSITY_NOT_FOUND;
            }

            _qualificationPlace.University =
                _uow.UniversityRepository.First(e => e.UniversityId == qualificationPlaceDTO.University.UniversityId);

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
    }
}