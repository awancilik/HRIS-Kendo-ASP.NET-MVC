using System.Collections.Generic;

namespace CVScreeningWeb.ViewModels.Dispatching
{
    public class MatrixRowViewModel
    {
        public int RowId { get; set; }
        public string RowName { get; set; }
        public IEnumerable<MatrixColumnViewModel> Columns { get; set; }
    }
}