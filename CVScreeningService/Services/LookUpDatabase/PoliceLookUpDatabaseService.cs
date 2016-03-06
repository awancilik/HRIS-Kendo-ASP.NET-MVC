using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class PoliceLookUpDatabaseService : LookUpDatabaseService<PoliceDTO>
    {
        private readonly IUnitOfWork _uow;

        public PoliceLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory)
            : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<Police, PoliceDTO>();
        }

        public override List<PoliceDTO> GetAllQualificationPlaces()
        {
            var polices = _uow.QualificationPlaceRepository.AsQueryable<Police>().ToList()
                .Where(e => !e.QualificationPlaceIsDeactivated);
            return polices.Select(Mapper.Map<Police, PoliceDTO>).ToList();
        }

        public override PoliceDTO GetQualificationPlace(int id)
        {
            var police = _uow.QualificationPlaceRepository.AsQueryable<Police>().ToList()
                .FirstOrDefault(e => !e.QualificationPlaceIsDeactivated && e.QualificationPlaceId == id);
            return Mapper.Map<Police, PoliceDTO>(police);
        }
    }
}