using System.Web;

namespace CVScreeningWeb.ViewModels.Shared
{
    public class AttachmentViewModel
    {
        public int  Id { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public int AtomicCheckId { get; set; }
    }
}