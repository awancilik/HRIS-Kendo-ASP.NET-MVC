using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.LookUpDatabase;

namespace CVScreeningService.Services.LookUpDatabase
{
    public interface ILookUpDatabaseService<T>
    {
        /// <summary>
        /// Retrieve
        /// </summary>
        /// <returns></returns>
        List<T> GetAllQualificationPlaces();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetQualificationPlace(int id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qualificationPlace"></param>
        /// <returns></returns>
        ErrorCode CreateOrEditQualificationPlace(ref T qualificationPlace);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="qualificationType"></param>
        /// <returns></returns>
        ErrorCode DeleteQualificationPlace(T qualificationPlaceDTO);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<ContactPersonDTO> GetContactPersons(int id);

        /// <summary>
        /// Get corresponding type of check screener categories (on field / office)
        /// </summary>
        /// <param name="qualificationPlaceId"></param>
        /// <returns></returns>
        IEnumerable<string> GetTypeOfCheckCategory(int qualificationPlaceId);

        /// <summary>
        /// Get list of qualification places with given screener category
        /// </summary>
        /// <param name="screenerCategory"></param>
        /// <returns></returns>
        IEnumerable<BaseQualificationPlaceDTO> GetQualificationPlaceByScreenerCategory(string screenerCategory);
    }
}