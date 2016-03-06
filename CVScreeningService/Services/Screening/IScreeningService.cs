using System.Collections.Generic;
using CVScreeningCore.Error;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.Services.Screening
{
    public interface IScreeningService
    {
        #region Type of check method

        List<TypeOfCheckDTO> GetAllTypeOfChecks();
        TypeOfCheckDTO GetTypeOfCheck(int id);
        ErrorCode CreateTypeOfCheck(ref TypeOfCheckDTO typeOfCheckDTO);
        ErrorCode CreateTypeOfCheckMetaValue(
            string typeOfCheckName, string typeOfCheckMetaKey, string typeOfCheckMetaValue, string category= null);
        string GetTypeOfCheckMetaValue(int typeOfCheckId, string typeOfCheckMetaKey);

        #endregion

        #region Screening

        bool AreAllAtomicChecksValidated(ScreeningBaseDTO screeningDTO);

        IEnumerable<ScreeningBaseDTO> GetAllScreenings();
        IEnumerable<ScreeningBaseDTO> GetAllScreeningsAssignedAsQualityControl();
        IEnumerable<ScreeningBaseDTO> SearchScreening(string name, string client, string startDateStr, string endDateStr, string status);

        ScreeningDTO GetScreening(int id);
        ScreeningBaseDTO GetBaseScreening(int id);



        ScreeningDTO GetScreeningForCover(int id);
        ErrorCode CreateScreening(ScreeningLevelVersionDTO screeningLevelVersionDTO, ref ScreeningDTO screeningDTO);
        ErrorCode EditScreening(ref ScreeningDTO screeningDTO);
        ErrorCode DeactivateScreening(int id);
        ErrorCode AssignScreening(ref ScreeningDTO screeningDTO, UserProfileDTO userProfileDTO);

        #endregion

        #region ScreeningLevelVersion
        IEnumerable<ScreeningLevelDTO> GetAllScreeningLevelsByContract(int contractId);
        IEnumerable<ScreeningLevelVersionDTO> GetAllScreeningLevelVersionsByScreeningLevel(int screeningLevelId);
        ScreeningLevelVersionDTO GetActiveScreeningLevelVersion(int screeningLevelId);

        ScreeningLevelDTO GetScreeningLevel(int screeningLevelId); 

        #endregion

        #region AtomicCheck

        IEnumerable<AtomicCheckDTO> GetAllAtomicChecks();
        IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksBase();


        IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksToAssign();


        /// <summary>
        /// Retrieve all the atomic checks that are not yet processed
        /// </summary>
        /// <returns></returns>
        IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksOnGoing();


        /// <summary>
        /// Retrieve all the atomic checks assigned that are pending validation
        /// </summary>
        /// <returns></returns>
        IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksPendingValidation();


        /// <summary>
        /// Retrieve all the atomic checks assigned to the current user that are not yet processed
        /// </summary>
        /// <returns></returns>
        IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksOnGoingAssignedAsScreener();


        /// <summary>
        /// Retrieve all the atomic checks assigned to the current user that are pending validation
        /// </summary>
        /// <returns></returns>
        IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksBasePendingValidationAssignedAsScreener();



        /// <summary>
        /// Retrieve all the atomic checks assigned to the current user that are pending validation
        /// </summary>
        /// <returns></returns>
        IEnumerable<AtomicCheckBaseDTO> GetAllAtomicChecksBasePendingValidationAssignedAsQualityControl();

        IEnumerable<AtomicCheckBaseDTO> GetAllBaseAtomicChecksForScreening(ScreeningBaseDTO screeningDTO);
        IEnumerable<AtomicCheckDTO> GetAllAtomicChecksForScreening(ScreeningBaseDTO screeningDTO);

        AtomicCheckDTO GetAtomicCheck(int id);
        ErrorCode EditAtomicCheck(ref AtomicCheckDTO atomicCheckDTO);
        ErrorCode AssignAtomicCheck(ref AtomicCheckDTO atomicCheckDTO, UserProfileDTO userProfileDTO);


        #endregion

        #region Report
        /// <summary>
        /// Create and upload a manual report
        /// </summary>
        /// <param name="screening"></param>
        /// <param name="screeningReportDTO"></param>
        /// <returns></returns>
        ErrorCode CreateManualReport(ScreeningBaseDTO screening, ref ScreeningReportDTO screeningReportDTO);

        /// <summary>
        /// Get report
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ScreeningReportDTO GetReport(int id);

        /// <summary>
        /// Submit general report to the client
        /// </summary>
        /// <param name="screeningReportDTO"></param>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        ErrorCode SubmitReport(ref ScreeningReportDTO screeningReportDTO, ref ScreeningDTO screeningDTO);
        #endregion
    }
}