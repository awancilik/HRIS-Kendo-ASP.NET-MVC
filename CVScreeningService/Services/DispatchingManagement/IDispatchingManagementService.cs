using System.Collections.Generic;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.Services.DispatchingManagement
{
    public interface IDispatchingManagementService
    {
        void DispatchAtomicChecks();
        int? GetDefaultMatrixValue(int screenerId, int typeOfCheckId);
        int? GetSkillMatrixValue(int screenerId, int qualificationPlaceId, string category);
        void UpdateDefaultMatrix(IEnumerable<IDictionary<string, string>> rows);
        void UpdateSkillMatrix(IEnumerable<IDictionary<string, string>> rows);
        UserProfileDTO GetScreenerForSpecificCase(AtomicCheckDTO atomicCheckDTO);

        #region Dispatching engine
        IDictionary<int, float> GetScreenerSkillWeight(AtomicCheckDTO atomicCheckDTO);
        IDictionary<int, float> GetScreenerWorkloadWeight();
        IDictionary<int, float> GetScreenerAvailabilityWeight(AtomicCheckDTO atomicCheckDTO);
        IDictionary<int, float> GetScreenerAtomicCheckAssignmentWeight(AtomicCheckDTO atomicCheckDTO);
        IDictionary<int, float> GetScreenerGeographicalWeight(AtomicCheckDTO atomicCheckDTO);
        IDictionary<int, float> GetAggregatedValues(AtomicCheckDTO atomicCheckDTO);
        #endregion


    }
}
