using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CVScreeningCore.Models;
using CVScreeningCore.Models.ScreeningState;
using CVScreeningService.DTO.Screening;
using CVScreeningService.DTO.Settings;
using CVScreeningService.Helpers;
using CVScreeningWeb.ViewModels.Screening;

namespace CVScreeningWeb.Helpers
{
    public class ScreeningHelper
    {

        public static IEnumerable<ScreeningManageViewModel> BuildScreeningManageViewModels(
               IEnumerable<ScreeningBaseDTO> screeningDTO,
               IEnumerable<PublicHolidayDTO> publicHolidaysDTO,
               string status = null)
        {
            return screeningDTO.Select(e => new ScreeningManageViewModel
            {
                Id = e.ScreeningId,
                Reference = e.ScreeningReference,
                Name = e.ScreeningFullName,
                DayPending = LayoutHelper.GetPendingDaysAsString(DateHelper.GetWorkingDaysDifference(
                    DateTime.Now,
                    (DateTime)e.ScreeningDeadlineDate,
                    publicHolidaysDTO)),
                DayPendingInt = DateHelper.GetWorkingDaysDifference(
                    DateTime.Now,
                    (DateTime)e.ScreeningDeadlineDate,
                    publicHolidaysDTO),
                Deadline = Convert.ToDateTime(e.ScreeningDeadlineDate).ToShortDateString(),
                DeliveryDate = e.ScreeningDeliveryDate != null ? Convert.ToDateTime(e.ScreeningDeliveryDate).ToShortDateString() : "",
                ScreeningLevel = e.ScreeningLevelName,
                Status = String.IsNullOrEmpty(status)
                        ? ScreeningStateFactory.GetStateAsString((ScreeningStateType)e.ScreeningState)
                        : status,
                ExternalDiscussionId = e.ExternalDiscussionId,
                InternalDiscussionId = e.InternalDiscussionId
            });
        }

    }
}