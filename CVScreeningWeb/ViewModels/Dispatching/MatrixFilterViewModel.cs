namespace CVScreeningWeb.ViewModels.Dispatching
{
    public class MatrixFilterViewModel
    {
        public int[] RowIds { get; set; }
        public int[] ColIds { get; set; }
        public int[] CatIds { get; set; }
        public string ScreenerCategory { get; set; }
    }
}