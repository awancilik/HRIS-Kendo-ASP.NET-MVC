using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.Helpers
{
    public class FormHelper
    {
        public static DropDownListViewModel BuildDropDownListViewModel(IDictionary<int, string> sources,
            int? selectedKey = null, string selectedValue = null)
        {
            return (selectedKey == null && selectedValue == null)
                ? new DropDownListViewModel
                {   Sources = sources.Select(
                        s => new SelectListItem
                        {
                            Text = s.Value,
                            Value = s.Key+""
                        }).ToList()
                }: new DropDownListViewModel
                {   Sources = sources.Select(
                        s => new SelectListItem
                        {
                            Text = s.Value,
                            Value = s.Key+"",
                            Selected = selectedKey == null ? s.Value.Equals(selectedValue):s.Key.Equals(selectedKey)                           
                        }).ToList()
                };
        }

        public static int ExtractDropDownListViewModel(DropDownListViewModel iModel)
        {
            return Convert.ToInt32(iModel.PostData);
        }

        public static SelectListItemViewModel BuildSelectListViewModel(IDictionary<int, string> sources,
            IEnumerable<int> selectedKeys = null, IEnumerable<string> selectedValues = null)
        {
            return (selectedKeys == null && selectedValues == null)
                ? new SelectListItemViewModel
                {
                    SelectListItems = sources.Select(
                        item => new SelectListItem
                        {
                            Text = item.Value,
                            Value = item.Key+"",
                        })
                }: new SelectListItemViewModel
                {
                    SelectListItems = sources.Select(
                        item => new SelectListItem
                        {
                            Text = item.Value,
                            Value = item.Key+"",
                            Selected = selectedKeys == null ? 
                            selectedValues.Contains(item.Value):
                            selectedKeys.Contains(item.Key)
                        })
                };
        }

        public static ICollection<string> ExtractSelectListViewModel(SelectListItemViewModel iModel)
        {           
            return iModel != null ? iModel.ItemIds.ToList() : null;
        }

    }
}