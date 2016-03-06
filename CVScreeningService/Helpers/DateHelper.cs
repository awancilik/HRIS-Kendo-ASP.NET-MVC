using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Settings;

namespace CVScreeningService.Helpers
{
    public class DateHelper
    {
        /// <summary>
        /// Static method used to add working days to a date
        /// </summary>
        /// <param name="currentDate"></param>
        /// <param name="daysToAdd"></param>
        /// <param name="publicHolidays"></param>
        /// <returns></returns>
        public static DateTime Add(DateTime currentDate, int daysToAdd, IEnumerable<PublicHoliday> publicHolidays)
        {
            for (var i = 1; i < daysToAdd + 1; i++)
            {
                var testDate = currentDate.AddDays(i).Date;
                
                // if the testDate is not a holiday and not a saturday and not a sunday, 
                if (!IsPublicHoliday(testDate, publicHolidays) &&
                    testDate.DayOfWeek != DayOfWeek.Saturday && 
                    testDate.DayOfWeek != DayOfWeek.Sunday) continue;
                
                //otherwise the daysToAdd is extended
                daysToAdd ++;
            }
            return currentDate.AddDays(daysToAdd);
        }


        private static bool IsPublicHoliday(DateTime currentDate, IEnumerable<PublicHoliday> publicHolidays)
        {
            return publicHolidays.Any(publicHoliday => currentDate.Date <= publicHoliday.PublicHolidayEndDate 
                    && currentDate.Date >= publicHoliday.PublicHolidayStartDate);
        }


        /// <summary>
        /// Retrieve how many working days there is between 2 dates
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="publicHolidays"></param>
        /// <returns></returns>
        public static int GetWorkingDaysDifference(DateTime startDate, DateTime endDate,
            IEnumerable<PublicHolidayDTO> publicHolidays)
        {
            startDate = startDate.Date;
            endDate = endDate.Date;

            if (endDate <= startDate)
                return 0;

            int total = 0;
            int diff = endDate.Subtract(startDate).Days;

            for (var i = 1; i < diff + 1; i++)
            {
                var testDate = startDate.AddDays(i).Date;

                // if the testDate is a holiday or a saturday or a sunday this is not a working days
                if (IsPublicHoliday(testDate, publicHolidays) ||
                    testDate.DayOfWeek == DayOfWeek.Saturday ||
                    testDate.DayOfWeek == DayOfWeek.Sunday) continue;

                //otherwise the daysToAdd is extended
                total++;
            }
            return total;
        }

        private static bool IsPublicHoliday(DateTime currentDate, IEnumerable<PublicHolidayDTO> publicHolidays)
        {
            return publicHolidays.Any(publicHoliday => currentDate.Date <= publicHoliday.PublicHolidayEndDate
                    && currentDate.Date >= publicHoliday.PublicHolidayStartDate);
        }
    }
}