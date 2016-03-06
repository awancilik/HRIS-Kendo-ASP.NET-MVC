using System.Collections.Generic;
using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Client
{
    public class ScreeningLevelDTO
    {
        [ObjectId]
        public int ScreeningLevelId { get; set; }
        public string ScreeningLevelName { get; set; }

        public ClientContractDTO Contract { get; set; }
        public List<ScreeningLevelVersionDTO> ScreeningLevelVersion { get; set; }
    }
}