using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class ImmigrationOfficeLookUpDatabaseService : LookUpDatabaseService<ImmigrationOfficeDTO>
    {
        private readonly IUnitOfWork _uow;

        public ImmigrationOfficeLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory)
            : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<ImmigrationOffice, ImmigrationOfficeDTO>();
        }

        public override List<ImmigrationOfficeDTO> GetAllQualificationPlaces()
        {
            var immigrationOffices =
                _uow.QualificationPlaceRepository.AsQueryable<ImmigrationOffice>().ToList()
                    .Where(e => !e.QualificationPlaceIsDeactivated);
            return immigrationOffices.Select(Mapper.Map<ImmigrationOffice, ImmigrationOfficeDTO>).ToList();
        }

        public override ImmigrationOfficeDTO GetQualificationPlace(int id)
        {
            var immigrationOffice = _uow.QualificationPlaceRepository.AsQueryable<ImmigrationOffice>().ToList()
                .FirstOrDefault(e => !e.QualificationPlaceIsDeactivated && e.QualificationPlaceId == id);
            return Mapper.Map<ImmigrationOffice, ImmigrationOfficeDTO>(immigrationOffice);
        }
    }
}