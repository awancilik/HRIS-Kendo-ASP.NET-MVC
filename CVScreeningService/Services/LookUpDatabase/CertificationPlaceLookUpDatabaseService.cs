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
    public class CertificationPlaceLookUpDatabaseService : LookUpDatabaseService<CertificationPlaceDTO>
    {
        private readonly IUnitOfWork _uow;

        public CertificationPlaceLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory)
            : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<CertificationPlace, CertificationPlaceDTO>();
            Mapper.CreateMap<Address, AddressDTO>();
            Mapper.CreateMap<Location, LocationDTO>();
            Mapper.CreateMap<ProfessionalQualification, ProfessionalQualificationDTO>();
            
        }

        public override List<CertificationPlaceDTO> GetAllQualificationPlaces()
        {
            var certificationPlaces =
                _uow.QualificationPlaceRepository.AsQueryable<CertificationPlace>().ToList().Where(
                    q => !q.QualificationPlaceIsDeactivated);
            return certificationPlaces.Select(Mapper.Map<CertificationPlace, CertificationPlaceDTO>).ToList();
        }

        public override ErrorCode DeleteQualificationPlace(CertificationPlaceDTO qualificationPlaceDTO)
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
                // Clean and remove all the link with certification place
                var professionalQualifications = qualificationPlace.ProfessionalQualification;
                if (professionalQualifications != null)
                {
                    foreach (var qualification in professionalQualifications.Reverse())
                    {
                        qualificationPlace.ProfessionalQualification.Remove(qualification);
                    }
                }

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

        public override CertificationPlaceDTO GetQualificationPlace(int id)
        {
            var certificationPlaces = _uow.QualificationPlaceRepository.AsQueryable<CertificationPlace>().ToList()
                .Where(e => !e.QualificationPlaceIsDeactivated && e.QualificationPlaceId == id);
            var certificationPlace = certificationPlaces.FirstOrDefault();
            return Mapper.Map<CertificationPlace, CertificationPlaceDTO>(certificationPlace);
        }

        public override ErrorCode CreateOrEditQualificationPlace(ref CertificationPlaceDTO qualificationPlace)
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
                : new CertificationPlace();

            _qualificationPlace.QualificationPlaceName = qualificationPlace.QualificationPlaceName;
            _qualificationPlace.QualificationPlaceDescription = qualificationPlace.QualificationPlaceDescription;
            _qualificationPlace.QualificationPlaceWebSite = qualificationPlace.QualificationPlaceWebSite;
            _qualificationPlace.QualificationPlaceCategory = qualificationPlace.QualificationPlaceCategory;
            _qualificationPlace.Address = address;

            if (qualificationPlace.ProfessionalQualification != null)
            {
                _qualificationPlace.ProfessionalQualification.Clear();
                _qualificationPlace.ProfessionalQualification = GetSelectedProfessionalQualification(qualificationPlaceDTO);
            }
          
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

        private ICollection<ProfessionalQualification> GetSelectedProfessionalQualification(
            CertificationPlaceDTO certificationPlace)
        {
            var ids = certificationPlace.ProfessionalQualification.Select(e => e.ProfessionalQualificationId);
            return _uow.ProfessionalQualificationRepository.GetAll().Where(
                e => ids.Contains(e.ProfessionalQualificationId)).ToList();
        } 
    }
}