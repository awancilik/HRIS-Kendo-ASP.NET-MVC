using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class CourtLookUpDatabaseService : LookUpDatabaseService<CourtDTO>
    {
        private readonly IUnitOfWork _uow;

        public CourtLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory)
            : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<Court, CourtDTO>();
        }

        public override List<CourtDTO> GetAllQualificationPlaces()
        {
            var courts = _uow.QualificationPlaceRepository.AsQueryable<Court>().ToList()
                .Where(e => !e.QualificationPlaceIsDeactivated);
            return courts.Select(Mapper.Map<Court, CourtDTO>).ToList();
        }

        public override CourtDTO GetQualificationPlace(int id)
        {
            var court = _uow.QualificationPlaceRepository.AsQueryable<Court>().ToList()
                .FirstOrDefault(e => !e.QualificationPlaceIsDeactivated && e.QualificationPlaceId == id);    
            return Mapper.Map<Court, CourtDTO>(court);
        }
    }
}