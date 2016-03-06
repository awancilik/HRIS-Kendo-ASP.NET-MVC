using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CVScreeningCore.Models;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningCore.Models.AtomicCheckValidationState;
using CVScreeningCore.Models.ScreeningState;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.Settings;
using CVScreeningWeb.ViewModels.AtomicCheck;
using CVScreeningWeb.ViewModels.Home;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.Helpers
{
    public class DashboardHelper
    {

        private static bool IsOnGoing(string status)
        {
            return !IsCompleted(status) && !IsValidated(status) && !IsDeactivated(status);
        }

        private static bool IsCompleted(string status)
        {
            return status == ScreeningStateSubmitted.kScreeningStateSubmitted;
        }

        private static bool IsValidated(string status)
        {
            return status == ScreeningStateValidated.kScreeningStateValidated;
        }

        private static bool IsDeactivated(string status)
        {
            return status == ScreeningStateDeactivated.kScreeningStateDeactivated;
        }

        public static DashboardAdministratorViewModel BuildDashboardAdministratorViewModel(
                IEnumerable<ScreeningBaseDTO> screeningsDTO,
                IEnumerable<ScreeningBaseDTO> screeningToQualifyDTO,
                IEnumerable<ScreeningBaseDTO> screeningsToRequalifyDTO,
                IEnumerable<AtomicCheckBaseDTO> atomicChecksToAssignDTO,
                IEnumerable<AtomicCheckBaseDTO> atomicChecksOnGoingDTO,
                IEnumerable<AtomicCheckBaseDTO> atomicChecksPendingValidationDTO,
                IEnumerable<PublicHolidayDTO> publicHolidayDTO)
        {
            return new DashboardAdministratorViewModel
            {
                AtomicChecksToAssign = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(atomicChecksToAssignDTO, publicHolidayDTO),
                    Type = "AssignTo"
                },
                AtomicChecksPendingValidation = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(
                        atomicChecksPendingValidationDTO, publicHolidayDTO),
                    Type = "PendingValidation"
                },
                AtomicChecksOnGoing = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(
                        atomicChecksOnGoingDTO, publicHolidayDTO),
                    Type = "OnGoing"
                },
                ScreeningToQualify = new ScreeningGridViewModel
                {
                    Screenings =
                        ScreeningHelper.BuildScreeningManageViewModels(screeningToQualifyDTO, publicHolidayDTO, "TO QUALIFY")
                    .Union(
                        ScreeningHelper.BuildScreeningManageViewModels(screeningsToRequalifyDTO, publicHolidayDTO, "WRONGLY QUALIFIED")),
                },
                ScreeningOnGoing = new ScreeningGridViewModel
                {
                    Screenings = ScreeningHelper.BuildScreeningManageViewModels(screeningsDTO.Where(v => IsOnGoing(v.State)), publicHolidayDTO),
                    Type = "OnGoing"
                },
                ScreeningToSubmit = new ScreeningGridViewModel
                {
                    Screenings = ScreeningHelper.BuildScreeningManageViewModels(screeningsDTO.Where(v => IsValidated(v.State)), publicHolidayDTO),
                    Type = "ToSubmit"
                },
            };
        }




        public static DashboardClientViewModel BuildDashboardClientViewModel(
            IEnumerable<ScreeningBaseDTO> screeningsDTO,
            IEnumerable<PublicHolidayDTO> publicHolidayDTO)
        {
            var dashboardClientViewModel = new DashboardClientViewModel
            {
                ScreeningOnGoing = new ScreeningGridViewModel
                {
                  Type = "OnGoing",
                  Screenings = ScreeningHelper.BuildScreeningManageViewModels(
                    screeningsDTO.Where(v => IsOnGoing(v.State)), publicHolidayDTO)
                },
                ScreeningCompleted = new ScreeningGridViewModel
                {
                    Type = "Completed",
                    Screenings = ScreeningHelper.BuildScreeningManageViewModels(
                        screeningsDTO.Where(v => IsCompleted(v.State)), publicHolidayDTO)
                }
            };
            return dashboardClientViewModel;
        }

        public static DashboardClientViewModel BuildDashboardAccountManagerViewModel(
            IEnumerable<ScreeningBaseDTO> screeningsDTO,
            IEnumerable<PublicHolidayDTO> publicHolidayDTO)
        {
            var dashboardAccountManagerViewModel = new DashboardClientViewModel
            {
                ScreeningOnGoing = new ScreeningGridViewModel
                {
                    Type = "OnGoing",
                    Screenings = ScreeningHelper.BuildScreeningManageViewModels(
                        screeningsDTO.Where(v => IsOnGoing(v.State)), publicHolidayDTO)
                },
                ScreeningCompleted = new ScreeningGridViewModel
                {
                    Type = "Completed",
                    Screenings = ScreeningHelper.BuildScreeningManageViewModels(
                        screeningsDTO.Where(v => IsCompleted(v.State)), publicHolidayDTO)
                }
            };
            return dashboardAccountManagerViewModel;
        }


        public static DashboardQualifierViewModel BuildDashboardQualifierViewModel(
            IEnumerable<ScreeningBaseDTO> screeningToQualifyDTO,
            IEnumerable<ScreeningBaseDTO> screeningsToRequalifyDTO,
            IEnumerable<PublicHolidayDTO> publicHolidayDTO)
        {
            var dashboardClientViewModel = new DashboardQualifierViewModel
            {
                ScreeningToQualify = new ScreeningGridViewModel
                {
                    Screenings = 
                        ScreeningHelper.BuildScreeningManageViewModels(screeningToQualifyDTO, publicHolidayDTO, "TO QUALIFY")
                    .Union(
                        ScreeningHelper.BuildScreeningManageViewModels(screeningsToRequalifyDTO, publicHolidayDTO, "WRONGLY QUALIFIED")),
                },
            };
            return dashboardClientViewModel;
        }

        public static DashboardProductionManagerViewModel BuildDashboardProductionManagerViewModel(
            IEnumerable<AtomicCheckBaseDTO> atomicChecksToAssignDTO,
            IEnumerable<AtomicCheckBaseDTO> atomicChecksOnGoingDTO,
            IEnumerable<AtomicCheckBaseDTO> atomicChecksPendingValidationDTO,
            IEnumerable<PublicHolidayDTO> publicHolidayDTO)
        {
            return new DashboardProductionManagerViewModel
            {
                AtomicChecksToAssign = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(atomicChecksToAssignDTO, publicHolidayDTO),
                    Type = "AssignTo"
                },
                AtomicChecksPendingValidation = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(atomicChecksPendingValidationDTO, publicHolidayDTO),
                    Type = "PendingValidation"
                },
                AtomicChecksOnGoing = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(atomicChecksOnGoingDTO, publicHolidayDTO),
                    Type = "OnGoing"
                }
            };
        }


        public static DashboardScreenerViewModel BuildDashboardScreenerViewModel(
            IEnumerable<AtomicCheckBaseDTO> atomicChecksOnGoingDTO,
            IEnumerable<AtomicCheckBaseDTO> atomicChecksPendingValidationDTO,
            IEnumerable<PublicHolidayDTO> publicHolidayDTO)
        {
            return new DashboardScreenerViewModel
            {
                AtomicChecksPendingValidation = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(
                        atomicChecksPendingValidationDTO, publicHolidayDTO),
                    Type = "PendingValidation"
                },
                AtomicChecksOnGoing = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(atomicChecksOnGoingDTO,
                        publicHolidayDTO),
                    Type = "OnGoing"
                }
            };
        }

        public static DashboardQualityControlViewModel BuildDashboardQualityControlViewModel(
            IEnumerable<AtomicCheckBaseDTO> atomicChecksPendingValidationDTO,
            IEnumerable<ScreeningBaseDTO> screeningsDTO,
            IEnumerable<PublicHolidayDTO> publicHolidayDTO)
        {
            return new DashboardQualityControlViewModel
            {
                AtomicChecksPendingValidation = new AtomicCheckGridViewModel
                {
                    AtomicChecks = AtomicCheckHelper.BuildAtomicCheckManageViewModels(atomicChecksPendingValidationDTO, publicHolidayDTO),
                    Type = "PendingValidation"
                },
                ScreeningOnGoing = new ScreeningGridViewModel
                {
                    Screenings = ScreeningHelper.BuildScreeningManageViewModels(
                        screeningsDTO.Where(v => IsOnGoing(v.State)), publicHolidayDTO),
                    Type = "OnGoing"
                },
                ScreeningToSubmit = new ScreeningGridViewModel
                {
                    Screenings = ScreeningHelper.BuildScreeningManageViewModels(
                        screeningsDTO.Where(v => IsValidated(v.State)), publicHolidayDTO),
                    Type = "ToSubmit"
                },
            };
        }


    }
}