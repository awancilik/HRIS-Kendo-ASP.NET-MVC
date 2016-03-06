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
    public class ProfessionalQualificationService : IProfessionalQualificationService
    {
        private readonly IUnitOfWork _uow;

        public ProfessionalQualificationService(IUnitOfWork uow)
        {
            _uow = uow;
            Mapper.CreateMap<ProfessionalQualification, ProfessionalQualificationDTO>();
        }

        public List<ProfessionalQualificationDTO> GetAllProfessionalQualifications()
        {
            var professionalQualificationBo =
                _uow.ProfessionalQualificationRepository.GetAll();
            return
                professionalQualificationBo.Select(Mapper.Map<ProfessionalQualification, ProfessionalQualificationDTO>)
                    .ToList();
        }

        public ProfessionalQualificationDTO GetProfessionalQualification(int id)
        {
            var professionalQualificationBo = _uow.ProfessionalQualificationRepository.First(
                p => p.ProfessionalQualificationId == id);
            return Mapper.Map<ProfessionalQualification, ProfessionalQualificationDTO>(professionalQualificationBo);
        }

        public ErrorCode CreateOrUpdateProfessionalQualification(
            ref ProfessionalQualificationDTO professionalQualification)
        {
            var id = professionalQualification.ProfessionalQualificationId;

            var existingProfessionalQualification =
                _uow.ProfessionalQualificationRepository.First(
                    p => p.ProfessionalQualificationId == id);
        

            var professionalQualificationBo = existingProfessionalQualification ?? new ProfessionalQualification();

            //check if the same name is exist
            var professionalQualificationDTO = professionalQualification;
            var isExist = existingProfessionalQualification != null;
            if (!isExist &&
                _uow.ProfessionalQualificationRepository.Exist(e => e.ProfessionalQualificationName.ToLower()
                    .Equals(professionalQualificationDTO.ProfessionalQualificationName.ToLower())))
                return ErrorCode.DBLOOKUP_PROFESSIONAL_QUALIFICATION_IS_EXIST;


            professionalQualificationBo.ProfessionalQualificationName =
                professionalQualification.ProfessionalQualificationName;
            professionalQualificationBo.ProfessionalQualificationDescription =
                professionalQualification.ProfessionalQualificationDescription;
            professionalQualificationBo.ProfessionalQualificationCode = 
                professionalQualification.ProfessionalQualificationCode;
            professionalQualificationBo.QualificationPlace = professionalQualificationBo.QualificationPlace 
                ?? new List<QualificationPlace>();

            if (professionalQualification.QualificationPlace != null)
            {
                professionalQualificationBo.QualificationPlace.Clear();
                foreach (var center in professionalQualification.QualificationPlace)
                {
                    var clone = center;
                    professionalQualificationBo.QualificationPlace.Add(
                        _uow.QualificationPlaceRepository.First(qp => qp.QualificationPlaceId == clone.QualificationPlaceId));
                }
            }

            try
            {
                _uow.ProfessionalQualificationRepository.Add(professionalQualificationBo);
                _uow.Commit();
                professionalQualification =  
                    Mapper.Map<ProfessionalQualification, ProfessionalQualificationDTO>(professionalQualificationBo);

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. Name: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        professionalQualification.ProfessionalQualificationName, ex.Message));
                return ErrorCode.DBLOOKUP_PROFESSIONAL_QUALIFICATION_INSERT_ERROR;
            }
            return ErrorCode.NO_ERROR;
        }

        public ErrorCode DeleteProfessionalQualification(int id)
        {
            var professionalQualificationBo = _uow.ProfessionalQualificationRepository.First(
                p => p.ProfessionalQualificationId == id);

            if (professionalQualificationBo == null)
                return ErrorCode.DBLOOKUP_PROFESSIONAL_QUALIFICATION_NOT_FOUND;

            try
            {
                // Clean and remove all the link with certification place
                var certificationPlaces = professionalQualificationBo.QualificationPlace;
                if (certificationPlaces != null)
                {
                    foreach (var certificationPlace in certificationPlaces.Reverse())
                    {
                        professionalQualificationBo.QualificationPlace.Remove(certificationPlace);
                    }
                }

                _uow.ProfessionalQualificationRepository.Delete(professionalQualificationBo);
                _uow.Commit();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(
                    string.Format("Function: {0}. ID: {1}. Error: {2}",
                        MethodBase.GetCurrentMethod().Name,
                        id, ex.Message));
                return ErrorCode.DBLOOKUP_PROFESSIONAL_QUALIFICATION_DELETE_ERROR;
            }

            return ErrorCode.NO_ERROR;
        }

        public List<CertificationPlaceDTO> GetCertificationPlaces(int id)
        {
            var professionalQualification = _uow.ProfessionalQualificationRepository
                .First(p => p.ProfessionalQualificationId == id);
            var certificationPlaces = professionalQualification.QualificationPlace.Select(
                e => e as CertificationPlace).ToList();
            return certificationPlaces.Select(
                Mapper.Map<CertificationPlace, CertificationPlaceDTO>).ToList();
        }
    }
}