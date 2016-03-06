using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Screening;
using CVScreeningWeb.ViewModels.Settings;
using DotNetOpenAuth.AspNet.Clients;

namespace CVScreeningWeb.Helpers
{
    public class SettingsHelper
    {

        public static IEnumerable<TypeOfCheckSettingsViewModel> BuildTypeOfChecksMetaViewModels(
            IEnumerable<TypeOfCheckMetaDTO> averageCompletionRateMeta,
            IEnumerable<TypeOfCheckMetaDTO> completionMinimumWorkingDaysMeta)
        {
            return averageCompletionRateMeta.Select(e => new TypeOfCheckSettingsViewModel
            {
                TypeOfCheckMetaId = e.TypeOfCheckMetaId,
                TypeOfCheckId = e.TypeOfCheck.TypeOfCheckId,
                TypeOfCheckCategory = e.TypeOfCheckMetaCategory,
                TypeOfCheckName = e.TypeOfCheck.CheckName,
                AverageCompletionRate = int.Parse(e.TypeOfCheckMetaValue),
                CompletionMinimunWorkingDays = 
                    int.Parse(completionMinimumWorkingDaysMeta.First(
                        u => u.TypeOfCheck.TypeOfCheckId == e.TypeOfCheck.TypeOfCheckId 
                                && u.TypeOfCheckMetaCategory == e.TypeOfCheckMetaCategory)
                            .TypeOfCheckMetaValue)
            });
        }


        public static IEnumerable<TypeOfCheckMetaDTO> ExtractTypeOfChecksMetaAverageCompletionRate(
            IEnumerable<TypeOfCheckSettingsViewModel> model)
        {
            return model.Select(e => new TypeOfCheckMetaDTO
                {
                    TypeOfCheck = new TypeOfCheckDTO
                    {
                        TypeOfCheckId = e.TypeOfCheckId
                    },
                    TypeOfCheckMetaKey = TypeOfCheckMeta.kAverageCompletionRateKey,
                    TypeOfCheckMetaValue = e.AverageCompletionRate.ToString(),
                    TypeOfCheckMetaCategory = e.TypeOfCheckCategory,
                    TypeOfCheckMetaId = e.TypeOfCheckMetaId
                });
        }

        public static IEnumerable<TypeOfCheckMetaDTO> ExtractTypeOfChecksMetaCompletionMinimumWorkingDays(
            IEnumerable<TypeOfCheckSettingsViewModel> model)
        {
            return model.Select(e => new TypeOfCheckMetaDTO
            {
                TypeOfCheck = new TypeOfCheckDTO
                {
                    TypeOfCheckId = e.TypeOfCheckId
                },
                TypeOfCheckMetaKey = TypeOfCheckMeta.kCompletionMinimumWorkingDays,
                TypeOfCheckMetaValue = e.CompletionMinimunWorkingDays.ToString(),
                TypeOfCheckMetaCategory = e.TypeOfCheckCategory,
                TypeOfCheckMetaId = e.TypeOfCheckMetaId
            });
        }


    }
}