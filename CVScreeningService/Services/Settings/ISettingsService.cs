using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.Settings;

namespace CVScreeningService.Services.Settings
{
    public interface ISettingsService
    {

        #region Public holiday

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<PublicHolidayDTO> GetAllPublicHolidays();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicHolidayDTO"></param>
        /// <param name="commit"></param>
        ErrorCode CreatePublicHoliday(ref PublicHolidayDTO publicHolidayDTO, bool commit = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicHolidayDTO"></param>
        /// <param name="commit"></param>
        /// <returns></returns>
        ErrorCode EditPublicHoliday(PublicHolidayDTO publicHolidayDTO, bool commit = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicHolidayId"></param>
        /// <returns></returns>
        PublicHolidayDTO GetPublicHoliday(int publicHolidayId);

        ErrorCode DeletePublicHoliday(int publicHolidayId, bool commit = true);

        #endregion


        #region Type of check settings

        /// <summary>
        /// Retrieve all the type of check meta value for all the type of checks associated to this key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<TypeOfCheckMetaDTO> GetTypeOfCheckMeta(string key);

        /// <summary>
        /// Set all the type of check meta value for all the type of checks associated to this key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="typeOfChecksMetaDTO"></param>
        /// <returns></returns>
        ErrorCode SetTypeOfCheckMeta(string key, IEnumerable<TypeOfCheckMetaDTO> typeOfChecksMetaDTO);

        #endregion
    }
}