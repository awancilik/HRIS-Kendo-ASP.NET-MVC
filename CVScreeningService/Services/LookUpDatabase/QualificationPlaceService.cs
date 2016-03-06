using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class QualificationPlaceService : LookUpDatabaseService<QualificationPlaceDTO>
    {
        private readonly IUnitOfWork _uow;

        public QualificationPlaceService(IUnitOfWork uow, IQualificationPlaceFactory factory)
            : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<QualificationPlace, QualificationPlaceDTO>()
                .ForMember(dto => dto.QualificationPlaceType, m => m.MapFrom(poco => poco.GetType().BaseType));

        }

        public override List<QualificationPlaceDTO> GetAllQualificationPlaces()
        {
            var qualificationPlaces = _uow.QualificationPlaceRepository.GetAll();
            return qualificationPlaces.Select(Mapper.Map<QualificationPlace, QualificationPlaceDTO>).ToList();
        }

        public override QualificationPlaceDTO GetQualificationPlace(int id)
        {
            var qualificationPlace = _uow.QualificationPlaceRepository.First(q => q.QualificationPlaceId == id);
            return Mapper.Map<QualificationPlace, QualificationPlaceDTO>(qualificationPlace);
        }
    }
}