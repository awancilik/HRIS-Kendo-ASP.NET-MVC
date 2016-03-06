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
    
    public partial class ContactPerson:IEntity
    {
        public ContactPerson()
        {
            this.ScreeningQualification = new HashSet<ScreeningQualification>();
            this.webpages_UserProfile = new HashSet<webpages_UserProfile>();
            this.QualificationPlace = new HashSet<QualificationPlace>();
    		InitializeEntity();
        }
    
        public int ContactPersonId { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactComments { get; set; }
        public byte ContactPersonTenantId { get; set; }
        public Nullable<bool> IsContactDeactivated { get; set; }
    
        public virtual Address Address { get; set; }
        public virtual ClientCompany ClientCompany { get; set; }
        public virtual ContactInfo ContactInfo { get; set; }
        public virtual ICollection<ScreeningQualification> ScreeningQualification { get; set; }
        public virtual ICollection<webpages_UserProfile> webpages_UserProfile { get; set; }
        public virtual ICollection<QualificationPlace> QualificationPlace { get; set; }
    }
}
