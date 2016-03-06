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
    
    public partial class ContactInfo:IEntity
    {
        public ContactInfo()
        {
            this.ClientCompany = new HashSet<ClientCompany>();
            this.webpages_UserProfile = new HashSet<webpages_UserProfile>();
            this.ContactPerson = new HashSet<ContactPerson>();
    		InitializeEntity();
        }
    
        public int ContactInfoId { get; set; }
        public string HomePhoneNumber { get; set; }
        public string WorkPhoneNumber { get; set; }
        public string MobilePhoneNumber { get; set; }
        public string WebSite { get; set; }
        public string OfficialEmail { get; set; }
        public byte ContactInfoTenantId { get; set; }
        public string Position { get; set; }
    
        public virtual ICollection<ClientCompany> ClientCompany { get; set; }
        public virtual ICollection<webpages_UserProfile> webpages_UserProfile { get; set; }
        public virtual ScreeningQualification ScreeningQualification { get; set; }
        public virtual ICollection<ContactPerson> ContactPerson { get; set; }
    }
}
