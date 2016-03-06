using System.ComponentModel.DataAnnotations;

namespace CVScreeningWeb.ViewModels.Dispatching
{
    public class MatrixColumnViewModel
    {
        public int ColumnId { get; set; }
        public string ColumnName { get; set; }

        [Range(0, 100)]
        public int Value { get; set; }
    }
}