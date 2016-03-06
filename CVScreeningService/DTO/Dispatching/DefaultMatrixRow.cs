using System.Collections.Generic;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.DTO.Dispatching
{
    public class DefaultMatrixRow : TypeOfCheckDTO
    {
        public IEnumerable<UserProfileDTO> Columns { get; set; }
    }
}
