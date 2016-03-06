using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Models;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningDAL.UnitOfWork;
using AutoMapper;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class PopulationOfficeLookUpDatabaseService : LookUpDatabaseService<PopulationOfficeDTO>
    {
        private readonly IUnitOfWork _uow;

        public PopulationOfficeLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory)
            : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<PopulationOffice, PopulationOfficeDTO>();
        }

        public override List<PopulationOfficeDTO> GetAllQualificationPlaces()
        {
            var populationOffices =
                _uow.QualificationPlaceRepository.AsQueryable<PopulationOffice>().ToList()
                    .Where(e => !e.QualificationPlaceIsDeactivated);
            return populationOffices.Select(Mapper.Map<PopulationOffice, PopulationOfficeDTO>).ToList();
        }

        public override PopulationOfficeDTO GetQualificationPlace(int id)
        {
            var populationOffice = _uow.QualificationPlaceRepository.AsQueryable<PopulationOffice>().ToList()
                .FirstOrDefault(e => !e.QualificationPlaceIsDeactivated && e.QualificationPlaceId == id);
            return Mapper.Map<PopulationOffice, PopulationOfficeDTO>(populationOffice);
        }
    }
}
