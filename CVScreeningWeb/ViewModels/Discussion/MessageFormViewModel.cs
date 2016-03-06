using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CVScreeningWeb.ViewModels.Discussion
{
    public class MessageFormViewModel
    {
        [Required]
        public int DiscussionId { get; set; }

        [Required]
        public string Message { get; set; }
    }
}