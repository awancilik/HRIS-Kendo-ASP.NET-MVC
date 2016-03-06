using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class DrivingLicenseOfficeLookUpDatabaseService : LookUpDatabaseService<DrivingLicenseOfficeDTO>
    {
        private readonly IUnitOfWork _uow;

        public DrivingLicenseOfficeLookUpDatabaseService(IUnitOfWork uow, IQualificationPlaceFactory factory)
            : base(uow, factory)
        {
            _uow = uow;
            Mapper.CreateMap<DrivingLicenseCheckingOffice, DrivingLicenseOfficeDTO>();
        }

        public override List<DrivingLicenseOfficeDTO> GetAllQualificationPlaces()
        {
            var drivingOffices = _uow.QualificationPlaceRepository.AsQueryable<DrivingLicenseCheckingOffice>().ToList()
                .Where(e => !e.QualificationPlaceIsDeactivated);
            return drivingOffices.Select(Mapper.Map<DrivingLicenseCheckingOffice, DrivingLicenseOfficeDTO>).ToList();
        }

        public override DrivingLicenseOfficeDTO GetQualificationPlace(int id)
        {
            var drivingOffice =
                _uow.QualificationPlaceRepository.AsQueryable<DrivingLicenseCheckingOffice>().ToList()
                    .FirstOrDefault(e => !e.QualificationPlaceIsDeactivated && e.QualificationPlaceId == id);
            return Mapper.Map<DrivingLicenseCheckingOffice, DrivingLicenseOfficeDTO>(drivingOffice);
        }
    }
}