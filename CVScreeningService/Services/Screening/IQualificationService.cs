using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.Screening;

namespace CVScreeningService.Services.Screening
{
    public interface IQualificationService
    {
        #region Qualification method

        /// <summary>
        /// Retrieve the qualification base data of a screening (martial status, gender ...)
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        ScreeningQualificationDTO GetQualificationBase(ScreeningBaseDTO screeningDTO);

        /// <summary>
        /// Set the qualification base data of a screening (martial status, gender ...)
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="screeningQualificationDTO"></param>
        /// <returns></returns>
        ErrorCode SetQualificationBase(ScreeningBaseDTO screeningDTO, ref ScreeningQualificationDTO screeningQualificationDTO);

        /// <summary>
        /// Retrieve the list of qualifications places of a screening (police, court, company ...) 
        /// that are not linked to an atomic check wrongly qualified
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        IEnumerable<BaseQualificationPlaceDTO> GetQualificationPlaces(ScreeningBaseDTO screeningDTO);


        /// <summary>
        /// Retrieve the list of qualifications places of a screening (police, court, company ...)
        /// that are linked to an atomic check wrongly qualified
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        IEnumerable<BaseQualificationPlaceDTO> GetWronglyQualifiedQualificationPlaces(ScreeningBaseDTO screeningDTO);

        /// <summary>
        /// Set the list of qualifications places of a screening (police, court, company ...)
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="qualificationPlacesDTO"></param>
        /// <param name="requalifiedQualifications"></param>
        /// <returns></returns>
        ErrorCode SetQualificationPlaces(ScreeningBaseDTO screeningDTO, 
            IEnumerable<BaseQualificationPlaceDTO> qualificationPlacesDTO,
            IEnumerable<int> requalifiedQualifications = null);


        /// <summary>
        /// Set atomic checks of a screening as not applicable,
        /// </summary>
        /// <param name="screeningDTO">Screening</param>
        /// <param name="atomicCheckDTO">Contain typeOfCheck and category information needed to retrieve atomic checks</param>
        /// <returns></returns>
        ErrorCode SetAtomicChecksAsNotApplicable(ScreeningBaseDTO screeningDTO, IDictionary<AtomicCheckDTO, bool> atomicCheckDTO);


        #endregion
    }
}