using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Shared;

namespace CVScreeningWeb.ViewModels.Dispatching
{
    public class MatrixViewModel
    {
        [UIHint("MultiSelectViewModel")]
        [LocalizedDisplayName("ColumnFilter", NameResourceType = typeof (Resources.Dispatching))]
        public MultiSelectViewModel ColumnFilter { get; set; }

        [UIHint("MultiSelectViewModel")]
        [LocalizedDisplayName("RowFilter", NameResourceType = typeof (Resources.Dispatching))]
        public MultiSelectViewModel RowFilter { get; set; }

        [UIHint("MultiSelectViewModel")]
        [LocalizedDisplayName("CategoryFilter", NameResourceType = typeof(Resources.Dispatching))]
        public MultiSelectViewModel CategoryFilter { get; set; }

        [UIHint("DropDownListViewModel")]
        [LocalizedDisplayName("ScreenerCategoryFilter", NameResourceType = typeof (Resources.Dispatching))]
        public DropDownListViewModel ScreenerCategoryFilter { get; set; }

        public List<Dictionary<string, string>> Matrix { get; set; }

        public int[] ColumnToHide { get; set; }
    }
}