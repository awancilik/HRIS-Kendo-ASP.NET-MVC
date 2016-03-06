using System;
using System.Collections.Generic;
using CVScreeningService.DTO.Screening;
using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Client
{
    public class ScreeningLevelVersionDTO
    {
        [ObjectId]
        public int ScreeningLevelVersionId { get; set; }
        public string ScreeningLevelVersionDescription { get; set; }
        public DateTime ScreeningLevelVersionStartDate { get; set; }
        public DateTime ScreeningLevelVersionEndDate { get; set; }
        public int ScreeningLevelVersionNumber { get; set; }
        public int ScreeningLevelVersionTurnaroundTime { get; set; }
        public bool ScreeningLevelVersionAllowedToContactCandidate { get; set; }
        public string ScreeningLevelVersionAllowedToContactCurrentCompany { get; set; }
        public string ScreeningLevelVersionLanguage { get; set; }

        public ScreeningLevelDTO ScreeningLevel { get; set; }
        public List<TypeOfCheckScreeningLevelVersionDTO> TypeOfCheckScreeningLevelVersion { get; set; }
    }
}