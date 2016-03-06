using System.Collections.Generic;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class InlineListViewModel
    {
        public IEnumerable<string> List { get; set; }
        public string Saparation { get; set; } 
    }
}