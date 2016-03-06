using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CVScreeningCore.Models;
using CVScreeningWeb.ViewModels.Shared;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

namespace CVScreeningWeb.Helpers
{
    public class AccountHelper
    {
        public static RadioButtonViewModel BuildScreenerCategoryViewModel(string defaultValue = "")
        {
            return new RadioButtonViewModel
            {
                Data = new List<RadioButtonData>
                {
                    new RadioButtonData
                    {
                        Id = 1,
                        Text = AtomicCheck.kOfficeCategory,
                        Checked = defaultValue ==  AtomicCheck.kOfficeCategory ? true : false 
                    },
                    new RadioButtonData
                    {
                        Id = 2,
                        Text = AtomicCheck.kOnFieldCategory,
                        Checked = defaultValue ==  AtomicCheck.kOnFieldCategory ? true : false 
                    }
                },
            };
        }

        public static string ExtractScreenerCategoryViewModel(RadioButtonViewModel iModel)
        {
            if (iModel == null || String.IsNullOrEmpty(iModel.SelectedValue))
                return "";
            else switch (iModel.SelectedValue)
            {
                case "1":
                    return AtomicCheck.kOfficeCategory;
                case "2":
                    return AtomicCheck.kOnFieldCategory;
                default:
                    return "";
            }
        }

    }
}
