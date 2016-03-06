using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CVScreeningWeb.ViewModels.Discussion
{
    public class MessageDetailsViewModel
    {
        public int MessageId { get; set; }

        public string Message { get; set; }

        public string CreatedByFullName { get; set; }

        public string CreatedByUserName { get; set; }

        public DateTime CreatedDate { get; set; }

    }

    public class DiscussionDetailsViewModel
    {
        [Required]
        public int DiscussionId { get; set; }

        public string DiscussionTitle { get; set; }

        public IEnumerable<MessageDetailsViewModel> Messages { get; set; }
    }
}