using System.Collections.Generic;
using CVScreeningWeb.Filters;
using Foolproof;
using System;
using System.ComponentModel.DataAnnotations;

namespace CVScreeningWeb.ViewModels.ScreeningLevel
{
    public class ScreeningLevelsManageViewModel
    {
        /// <summary>
        /// Contract identifier
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Contract reference
        /// </summary>
        public string ContractReference { get; set; }

        /// <summary>
        /// Contract year
        /// </summary>
        public string ContractYear { get; set; }

        /// <summary>
        /// Screening levels
        /// </summary>
        public List<ScreeningLevelManageViewModel> ScreeningLevels { get; set; }
    }

    public class ScreeningLevelManageViewModel
    {
        /// <summary>
        /// Screening level identifier
        /// </summary>
        public int ScreeningLevelId { get; set; }

        public string ScreeningLevelName { get; set; }

        /// <summary>
        /// Screening level version
        /// </summary>
        public ScreeningLevelVersionFormViewModel ScreeningLevelVersion { get; set; }
    }

    public class ScreeningLevelVersionFormViewModel
    {
        /// <summary>
        /// Screening level version identifier
        /// </summary>
        public int ScreeningLevelVersionId { get; set; }

        public string Description { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public bool AllowedToContact { get; set; }

        public int VersionNumber { get; set; }

        public int TurnaroundTime { get; set; }

        public string Language { get; set; }

    }


}