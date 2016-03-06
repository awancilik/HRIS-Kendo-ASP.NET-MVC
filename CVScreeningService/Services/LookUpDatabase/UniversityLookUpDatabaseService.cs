using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;
using Nalysa.Common.Log;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class UniversityLookUpDatabaseService : IUniversityLookUpDatabaseService
    {
        private readonly IUnitOfWork _uow;

        public UniversityLookUpDatabaseService(IUnitOfWork uow)
        {
            _uow = uow;
            Mapper.CreateMap<Faculty, FacultyDTO>();
            Mapper.CreateMap<University, UniversityDTO>();
        }

        public List<UniversityDTO> GetAllUniversities()
        {
            var universities = _uow.UniversityRepository.GetAll();
            return universities.Select(Mapper.Map<University, UniversityDTO>).ToList();
        }

        public UniversityDTO GetUniversity(int id)
        {
            var university = _uow.UniversityRepository.First(u => u.UniversityId == id);
            return Mapper.Map<University, UniversityDTO>(university);
        }

        public ErrorCode CreateOrUpdateUniversity(ref UniversityDTO university)
        {
            var universityId = university.UniversityId;
            var isExist = _uow.UniversityRepository.Exist(u => u.UniversityId == universityId);

            var _university = isExist
                ? _uow.UniversityRepository.First(u => u.UniversityId == universityId)
                : new University();
            
            var universityDTO = university;

            if (!isExist && _uow.UniversityRepository.Exist(e => e.UniversityName.ToLower()
                .Equals(universityDTO.UniversityName.ToLower())))
                return ErrorCode.DBLOOKUP_UNIVERSITY_IS_EXIST;

            _university.UniversityName = university.UniversityName;
            _university.UniversityWebSite = university.UniversityWebSite;

            try
            {
                _uow.UniversityRepository.Add(_university);
                _uow.Commit();
                university.UniversityId = _university.UniversityId;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(string.Format
                    ("CreateOrUpdateUniversity: {0}. Error: {1}", university.UniversityId, ex.Message));
                return ErrorCode.DBLOOKUP_UNIVERSITY_INSERT_ERROR;
            }
            return ErrorCode.NO_ERROR;
        }

        public ErrorCode DeleteUniversity(int id)
        {
            var university = _uow.UniversityRepository.First(u => u.UniversityId == id);
            if (university == null)
                return ErrorCode.DBLOOKUP_UNIVERSITY_NOT_FOUND;

            try
            {
                _uow.UniversityRepository.Delete(university);
                _uow.Commit();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(string.Format(
                    "DeleteUniversity: {0}. Error: {1}", id, ex.Message));
                return ErrorCode.DBLOOKUP_UNIVERSITY_DELETE_ERROR;
            }

            return ErrorCode.NO_ERROR;
        }
    }
}