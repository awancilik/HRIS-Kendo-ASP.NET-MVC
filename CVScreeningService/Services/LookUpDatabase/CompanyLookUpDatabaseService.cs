using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class CompanyLookUpDatabaseService : LookUpDatabaseService<CompanyDTO>
    {
        private readonly IUnitOfWork _uow;

        public CompanyLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory) : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<Company, CompanyDTO>();
        }

        public override List<CompanyDTO> GetAllQualificationPlaces()
        {
            var companies = _uow.QualificationPlaceRepository.AsQueryable<Company>().ToList()
                .Where(e => !e.QualificationPlaceIsDeactivated);
            return companies.Select(Mapper.Map<Company, CompanyDTO>).ToList();
        }

        public override CompanyDTO GetQualificationPlace(int id)
        {
            var court = _uow.QualificationPlaceRepository.AsQueryable<Company>().ToList().
                FirstOrDefault(e => !e.QualificationPlaceIsDeactivated && e.QualificationPlaceId == id);
            return Mapper.Map<Company, CompanyDTO>(court);
        }
    }
}