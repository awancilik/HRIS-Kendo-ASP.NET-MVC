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
    
    public partial class ScreeningReport:IEntity
    {
        #region IEntity interface method definition

        public void SetId(int id)
        {
            ScreeningReportId = id;
        }

        public int GetId()
        {
            return ScreeningReportId;
        }

        public void SetTenantId(byte id)
        {
            ScreeningReportTenantId = id;
        }

        public byte GetTenantId()
        {
            return ScreeningReportTenantId;
        }

        public void InitializeEntity()
        {

        }

        #endregion

        #region Constant screening report

        /// <summary>
        /// Atomic check category: office
        /// </summary>
        public const string kAutomaticGenerationType = "Automatic";

        /// <summary>
        /// Atomic check category: on field
        /// </summary>
        public const string kManualGenerationType = "Manual";

        #endregion

        #region Permission

        /// <summary>
        /// Grantt permission to the report
        /// </summary>
        public void GrantPermission()
        {
            this.GrantPermissionForQualityControl(this.Screening.QualityControl);
            this.Screening.GetClients().ToList().ForEach(c => GrantPermissionForClient(c));
            this.Screening.GetAccountManagers().ToList().ForEach(a => GrantPermissionForClient(a));
        }

        /// <summary>
        /// Add permission for this report
        /// </summary>
        /// <param name="userProfile"></param>
        /// <param name="permissionName"></param>
        private void GrantPermission(webpages_UserProfile userProfile, string permissionName)
        {
            if (!this.Permission.Any(p => p.ScreeningReport == this && p.UserProfile == userProfile
                                          && p.PermissionName == permissionName && p.PermissionIsGranted == true))
            {
                this.Permission.Add(new Permission
                {
                    ScreeningReport = this,
                    UserProfile = userProfile,
                    PermissionName = permissionName,
                    PermissionIsGranted = true
                });
            }
        }

        /// <summary>
        /// Revoke permission for this report
        /// </summary>
        /// <param name="userProfile"></param>
        /// <param name="permissionName"></param>
        private void RevokePermission(webpages_UserProfile userProfile, string permissionName)
        {
            this.Permission.Reverse().Where(p => p.ScreeningReport == this && p.UserProfile == userProfile
                && p.PermissionName == permissionName).ToList().ForEach(z => this.Permission.Remove(z));
        }

        /// <summary>
        /// Setup report view permission
        /// </summary>
        /// <param name="userProfile"></param>
        public void GrantReportViewPermission(webpages_UserProfile userProfile)
        {
            GrantPermission(userProfile, Models.Permission.kReportViewPermission);
        }

        /// <summary>
        /// Revoke report view permission
        /// </summary>
        /// <param name="userProfile"></param>
        public void RevokeReportViewPermission(webpages_UserProfile userProfile)
        {
            RevokePermission(userProfile, Models.Permission.kReportViewPermission);
        }

        /// <summary>
        /// Setup permission to qc on this report
        /// </summary>
        /// <param name="userProfile"></param>
        public void GrantPermissionForQualityControl(webpages_UserProfile userProfile)
        {
            // Setup report permission
            new List<string>
            {
                Models.Permission.kReportManagePermission,
                Models.Permission.kReportViewPermission,
            }.ForEach(p => GrantPermission(userProfile, p));
        }

        /// <summary>
        /// Clear permission to qc on this report
        /// </summary>
        /// <param name="userProfile"></param>
        public void RevokePermissionForQualityControl(webpages_UserProfile userProfile)
        {
            // Setup report permission
            new List<string>
                {
                    Models.Permission.kReportManagePermission,
                    Models.Permission.kReportViewPermission,
                }.ForEach(p => RevokePermission(userProfile,p));
        }

        /// <summary>
        /// Setup permission to am on this report
        /// </summary>
        /// <param name="userProfile"></param>
        public void GrantPermissionForAccountManager(webpages_UserProfile userProfile)
        {
            // Setup report permission
            new List<string>
            {
                Models.Permission.kReportViewPermission,
            }.ForEach(p => GrantPermission(userProfile, p));
        }

        /// <summary>
        /// Clear permission to am on this report
        /// </summary>
        /// <param name="userProfile"></param>
        public void RevokePermissionForAccountManager(webpages_UserProfile userProfile)
        {
            // Setup report permission
            new List<string>
                {
                    Models.Permission.kReportViewPermission,
                }.ForEach(p => RevokePermission(userProfile, p));
        }

        /// <summary>
        /// Setup permission to client on this report
        /// </summary>
        /// <param name="userProfile"></param>
        public void GrantPermissionForClient(webpages_UserProfile userProfile)
        {
            // Setup report permission
            new List<string>
            {
                Models.Permission.kReportViewPermission,
            }.ForEach(p => GrantPermission(userProfile, p));
        }

        /// <summary>
        /// Clear permission to client on this report
        /// </summary>
        /// <param name="userProfile"></param>
        public void RevokePermissionForClient(webpages_UserProfile userProfile)
        {
            // Setup report permission
            new List<string>
                {
                    Models.Permission.kReportViewPermission,
                }.ForEach(p => RevokePermission(userProfile, p));
        }

        #endregion
    }
}
