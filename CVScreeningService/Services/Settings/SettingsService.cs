using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CVScreeningCore.Error;
using CVScreeningCore.Models;
using CVScreeningDAL.UnitOfWork;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.Settings;
using CVScreeningService.Filters;

namespace CVScreeningService.Services.Settings
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class SettingsService : ISettingsService
    {
        private readonly IUnitOfWork _uow;

        public SettingsService(IUnitOfWork uow)
        {
            _uow = uow;
            Mapper.CreateMap<PublicHoliday, PublicHolidayDTO>();
            Mapper.CreateMap<TypeOfCheckMeta, TypeOfCheckMetaDTO>();
            Mapper.CreateMap<TypeOfCheck, TypeOfCheckDTO>();

        }

        #region Public Holiday

        public virtual IEnumerable<PublicHolidayDTO> GetAllPublicHolidays()
        {
            var publicHolidays =
                _uow.PublicHolidayRepository.GetAll();
            publicHolidays = publicHolidays.OrderBy(u => u.PublicHolidayStartDate);
            return publicHolidays.Select(Mapper.Map<PublicHoliday, PublicHolidayDTO>).ToList();
        }

        /// <summary>
        /// Service - Create a public holiday
        /// </summary>
        /// <param name="publicHolidayDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode CreatePublicHoliday(ref PublicHolidayDTO publicHolidayDTO, bool commit = true)
        {
            var publicHoliday = new PublicHoliday
            {
                PublicHolidayName = publicHolidayDTO.PublicHolidayName,
                PublicHolidayRemarks = publicHolidayDTO.PublicHolidayRemarks,
                PublicHolidayStartDate = publicHolidayDTO.PublicHolidayStartDate.Date,
                PublicHolidayEndDate = publicHolidayDTO.PublicHolidayEndDate.Date
            };

            var publicHolidayAll = _uow.PublicHolidayRepository.GetAll();
            var endDate = publicHoliday.PublicHolidayEndDate.Date;
            var startDate = publicHolidayDTO.PublicHolidayStartDate.Date;


            var publicHolidayOverlap =
                publicHolidayAll.Where(
                    u => u.PublicHolidayStartDate <= endDate
                            &&
                            u.PublicHolidayEndDate >= startDate);

            if (publicHolidayOverlap.Any())
            {
                return ErrorCode.PUBLIC_HOLIDAY_DATE_OVERLAPPING;
            }

            _uow.PublicHolidayRepository.Add(publicHoliday);

            if (!commit)
                return ErrorCode.NO_ERROR;

            _uow.Commit();
            publicHolidayDTO.PublicHolidayId = publicHoliday.PublicHolidayId;

            return ErrorCode.NO_ERROR;
        }

        /// <summary>
        /// Service - Edit a public holiday
        /// </summary>
        /// <param name="publicHolidayDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        public virtual ErrorCode EditPublicHoliday(PublicHolidayDTO publicHolidayDTO, bool commit = true)
        {

            if (!_uow.PublicHolidayRepository.Exist(u => u.PublicHolidayId == publicHolidayDTO.PublicHolidayId))
            {
                return ErrorCode.PUBLIC_HOLIDAY_NOT_FOUND;
            }

            publicHolidayDTO.PublicHolidayStartDate = publicHolidayDTO.PublicHolidayStartDate.Date;
            publicHolidayDTO.PublicHolidayEndDate = publicHolidayDTO.PublicHolidayEndDate.Date;

            var publicHoliday =
                _uow.PublicHolidayRepository.Single(u => u.PublicHolidayId == publicHolidayDTO.PublicHolidayId);

            var publicHolidayAll = _uow.PublicHolidayRepository.GetAll();

            var publicHolidayOverlap =
                publicHolidayAll.Where(
                    u => u.PublicHolidayStartDate <= publicHolidayDTO.PublicHolidayEndDate
                            &&
                            u.PublicHolidayEndDate >= publicHolidayDTO.PublicHolidayStartDate
                            &&
                            u.PublicHolidayId != publicHoliday.PublicHolidayId
                    );

            if (publicHolidayOverlap.Any())
            {
                return ErrorCode.PUBLIC_HOLIDAY_DATE_OVERLAPPING;
            }

            publicHoliday.PublicHolidayName = publicHolidayDTO.PublicHolidayName;
            publicHoliday.PublicHolidayRemarks = publicHolidayDTO.PublicHolidayRemarks;
            publicHoliday.PublicHolidayStartDate = publicHolidayDTO.PublicHolidayStartDate;
            publicHoliday.PublicHolidayEndDate = publicHolidayDTO.PublicHolidayEndDate;

            if (commit) _uow.Commit();

            return ErrorCode.NO_ERROR;
        }


        public virtual PublicHolidayDTO GetPublicHoliday(int publicHolidayId)
        {
            if (!_uow.PublicHolidayRepository.Exist(u => u.PublicHolidayId == publicHolidayId))
            {
                return null;
            }

            var publicHoliday = _uow.PublicHolidayRepository.Single(u => u.PublicHolidayId == publicHolidayId);
            return Mapper.Map<PublicHoliday, PublicHolidayDTO>(publicHoliday);
        }


        public virtual ErrorCode DeletePublicHoliday(int publicHolidayId, bool commit = true)
        {
            if (!_uow.PublicHolidayRepository.Exist(u => u.PublicHolidayId == publicHolidayId))
            {
                return ErrorCode.PUBLIC_HOLIDAY_NOT_FOUND;
            }

            var publicHoliday = _uow.PublicHolidayRepository.Single(u => u.PublicHolidayId == publicHolidayId);
            _uow.PublicHolidayRepository.Delete(publicHoliday);

            if (commit)
                _uow.Commit();

            return ErrorCode.NO_ERROR;
        }

        #endregion

        #region TypeOfCheck Meta


        public virtual IEnumerable<TypeOfCheckMetaDTO> GetTypeOfCheckMeta(string key)
        {
            if (!_uow.TypeOfCheckMetaRepository.Exist(u => u.TypeOfCheckMetaKey == key))
                return null;

            var typeOfCheckMeta = _uow.TypeOfCheckMetaRepository.Find(u => u.TypeOfCheckMetaKey == key);
            return typeOfCheckMeta.Select(Mapper.Map<TypeOfCheckMeta, TypeOfCheckMetaDTO>).ToList();
        }

        public virtual ErrorCode SetTypeOfCheckMeta(string key, IEnumerable<TypeOfCheckMetaDTO> typeOfChecksMetaDTO)
        {
            if (!_uow.TypeOfCheckMetaRepository.Exist(u => u.TypeOfCheckMetaKey == key))
                return ErrorCode.TYPE_OF_CHECK_META_NOT_FOUND;

            foreach (var typeOfCheckMetaDTO in typeOfChecksMetaDTO)
            {
                var typeOfCheckMeta = _uow.TypeOfCheckMetaRepository.Single(
                    u =>
                        u.TypeOfCheckMetaKey == key &&
                        u.TypeOfCheckMetaCategory == typeOfCheckMetaDTO.TypeOfCheckMetaCategory
                        && u.TypeOfCheck.TypeOfCheckId == typeOfCheckMetaDTO.TypeOfCheck.TypeOfCheckId);

                typeOfCheckMeta.TypeOfCheckMetaValue = typeOfCheckMetaDTO.TypeOfCheckMetaValue;
            }

            _uow.Commit();
            return ErrorCode.NO_ERROR;
        }


        #endregion
    }
}