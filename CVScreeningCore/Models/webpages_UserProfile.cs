//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CVScreeningCore.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class webpages_UserProfile:IEntity
    {
        public webpages_UserProfile()
        {
            this.Screening = new HashSet<Screening>();
            this.UserLeave = new HashSet<UserLeave>();
            this.webpages_Roles = new HashSet<webpages_Roles>();
            this.ClientCompaniesForAM = new HashSet<ClientCompany>();
            this.AtomicCheck = new HashSet<AtomicCheck>();
            this.History = new HashSet<History>();
            this.Message = new HashSet<Message>();
            this.DefaultMatrix = new HashSet<DefaultMatrix>();
            this.Permission = new HashSet<Permission>();
            this.SkillMatrix = new HashSet<SkillMatrix>();
            this.NotificationOfUser = new HashSet<NotificationOfUser>();
    		InitializeEntity();
        }
    
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Remarks { get; set; }
        public byte[] UserPhoto { get; set; }
        public string ScreenerCategory { get; set; }
        public bool UserIsDeactivated { get; set; }
        public byte UserProfileTenantId { get; set; }
        public string UserPhotoContentType { get; set; }
        public Nullable<byte> ScreenerCapabilities { get; set; }
    
        public virtual Address Address { get; set; }
        public virtual ContactInfo ContactInfo { get; set; }
        public virtual ICollection<Screening> Screening { get; set; }
        public virtual ICollection<UserLeave> UserLeave { get; set; }
        public virtual ICollection<webpages_Roles> webpages_Roles { get; set; }
        public virtual ClientCompany ClientCompanyForClientUserProfile { get; set; }
        public virtual ICollection<ClientCompany> ClientCompaniesForAM { get; set; }
        public virtual ICollection<AtomicCheck> AtomicCheck { get; set; }
        public virtual ICollection<History> History { get; set; }
        public virtual ICollection<Message> Message { get; set; }
        public virtual ContactPerson ContactPerson { get; set; }
        public virtual ICollection<DefaultMatrix> DefaultMatrix { get; set; }
        public virtual ICollection<Permission> Permission { get; set; }
        public virtual ICollection<SkillMatrix> SkillMatrix { get; set; }
        public virtual ICollection<NotificationOfUser> NotificationOfUser { get; set; }
    }
}
