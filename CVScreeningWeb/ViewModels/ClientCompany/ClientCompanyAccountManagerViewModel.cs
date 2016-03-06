using System.Collections.Generic;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.ClientCompany
{
    public class AccountManagerViewModel
    {
        //Integer value of a checkbox
        [LocalizedDisplayName("Id", NameResourceType = typeof (Resources.Common))]
        public int Id { get; set; }

        //String name of a checkbox
        [LocalizedDisplayName("Name", NameResourceType = typeof (Resources.Common))]
        public string Name { get; set; }

        //Boolean value to select a checkbox
        //on the list
        public bool IsSelected { get; set; }
    }

    public class ClientCompanyAccountManagerViewModel
    {
        public IEnumerable<AccountManagerViewModel> AvailableAccountManagers { get; set; }
        public IEnumerable<AccountManagerViewModel> SelectedAccountManagers { get; set; }
        public PostedAccountManagers PostedAccountManagers { get; set; }
    }

    /// <summary>
    ///     for Helper class to make posting back selected values easier
    /// </summary>
    public class PostedAccountManagers
    {
        //this array will be used to POST values from the form to the controller
        public string[] AccountManagerIds { get; set; }
    }
}