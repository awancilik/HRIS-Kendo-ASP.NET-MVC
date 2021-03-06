//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Linq;

namespace CVScreeningCore.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ClientCompany:IEntity
    {
        #region IEntity interface method definition

        public void SetId(int id)
        {
            ClientCompanyId = id;
        }

        public int GetId()
        {
            return ClientCompanyId;
        }

        public void SetTenantId(byte id)
        {
            ClientCompanyTenantId = id;
        }

        public byte GetTenantId()
        {
            return ClientCompanyTenantId;
        }
        public void InitializeEntity()
        {

        }

        #endregion

        #region Getter and setter

        /// <summary>
        /// Clear account managers linked to this company and revoke their permission
        /// </summary>
        public void ClearAccountManagers()
        {
            RevokePermissionForAllAccountManagers();
            this.AccountManagers.Clear();
        }

        /// <summary>
        /// Add account manager to this company
        /// </summary>
        /// <param name="accountManager"></param>
        public void AddAccountManager(webpages_UserProfile accountManager)
        {
            this.AccountManagers.Add(accountManager);
            accountManager.GrantPermissionForAccountManager(this);
        }

        /// <summary>
        /// Add account manager to this company
        /// </summary>
        /// <param name="accountManager"></param>
        public void RemoveAccountManager(webpages_UserProfile accountManager)
        {
            accountManager.RevokePermissionForAccountManager(this);
            this.AccountManagers.Remove(accountManager);
        }

        #endregion

        #region Permission

        public void RevokePermissionForAllAccountManagers()
        {
            foreach (var accountManager in AccountManagers)
            {
                RevokePermissionForAccountManager(accountManager);
            }
        }

        public void RevokePermissionForAccountManager(webpages_UserProfile accountManager)
        {
            accountManager.RevokePermissionForAccountManager(this);
        }

        #endregion

    }
}
