using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class HighSchoolLookUpDatabaseService : LookUpDatabaseService<HighSchoolDTO>
    {
        private readonly IUnitOfWork _uow;


        public HighSchoolLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory) : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<HighSchool, HighSchoolDTO>();
        }

        public override List<HighSchoolDTO> GetAllQualificationPlaces()
        {
            var highSchools = _uow.QualificationPlaceRepository.AsQueryable<HighSchool>().ToList()
                .Where(e => !e.QualificationPlaceIsDeactivated);
            return highSchools.Select(Mapper.Map<HighSchool, HighSchoolDTO>).ToList();
        }

        public override HighSchoolDTO GetQualificationPlace(int id)
        {
            var highSchool = _uow.QualificationPlaceRepository.AsQueryable<HighSchool>().ToList()
                .FirstOrDefault(e => !e.QualificationPlaceIsDeactivated && e.QualificationPlaceId == id);
            return Mapper.Map<HighSchool, HighSchoolDTO>(highSchool);
        }
    }
}