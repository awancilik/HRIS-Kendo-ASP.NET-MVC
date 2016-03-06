using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public class QualificationPlaceFactory : IQualificationPlaceFactory
    {
        private readonly IUnitOfWork _unitOfWork;

        public QualificationPlaceFactory(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public QualificationPlace Create(BaseQualificationPlaceDTO qualificationPlaceDTO)
        {
            if (qualificationPlaceDTO is PoliceDTO)
                return new Police();
            if (qualificationPlaceDTO is CourtDTO)
                return new Court();
            if (qualificationPlaceDTO is DrivingLicenseOfficeDTO)
                return new DrivingLicenseCheckingOffice();
            if (qualificationPlaceDTO is CertificationPlaceDTO)
                return new CertificationPlace();
            if (qualificationPlaceDTO is FacultyDTO)
                return new Faculty();
            if (qualificationPlaceDTO is CompanyDTO)
                return new Company();
            if (qualificationPlaceDTO is HighSchoolDTO)
                return new HighSchool();
            if (qualificationPlaceDTO is ImmigrationOfficeDTO)
                return new ImmigrationOffice();
            if (qualificationPlaceDTO is PopulationOfficeDTO)
                return new PopulationOffice();
            return null;
        }

        public QualificationCode GetType(QualificationPlace qualificationPlace)
        {
            return qualificationPlace.GetQualificationCode();
        }

        public QualificationCode GetType(BaseQualificationPlaceDTO qualificationPlaceDTO)
        {
            var qualificationPlace =
                _unitOfWork.QualificationPlaceRepository.First(
                    e => e.QualificationPlaceId == qualificationPlaceDTO.QualificationPlaceId);
            return GetType(qualificationPlace);
        }
    }
}