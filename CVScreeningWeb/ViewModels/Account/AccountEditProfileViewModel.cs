using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using CVScreeningWeb.Filters;
using CVScreeningWeb.ViewModels.Common;
using CVScreeningWeb.ViewModels.Contact;

namespace CVScreeningWeb.ViewModels.Account
{
    public class AccountEditProfileViewModel
    {
        [LocalizedDisplayName("Id", NameResourceType = typeof(Resources.Common))]
        public int UserId { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("Email", NameResourceType = typeof(Resources.Common))]
        public string UserName { get; set; }

        [UIHint("StringTextBox")]
        [LocalizedDisplayName("FullName", NameResourceType = typeof(Resources.Common))]
        public string FullName { get; set; }

        public byte[] CurrentUserPhoto { get; set; }

        // User profile pictures
        [UIHint("PictureFileViewModel")]
        [LocalizedDisplayName("ProfilePicture", NameResourceType = typeof(Resources.Account))]
        public HttpPostedFileBase PictureFile { get; set; }

    }
}