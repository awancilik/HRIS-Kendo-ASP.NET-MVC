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
    
    public partial class Discussion:IEntity
    {
        public Discussion()
        {
            this.Message = new HashSet<Message>();
            this.Permission = new HashSet<Permission>();
    		InitializeEntity();
        }
    
        public int DiscussionId { get; set; }
        public string DiscussionTitle { get; set; }
        public string DiscussionType { get; set; }
        public System.DateTime DiscussionCreatedDate { get; set; }
        public byte DiscussionTenantId { get; set; }
    
        public virtual AtomicCheck AtomicCheck { get; set; }
        public virtual Screening Screening { get; set; }
        public virtual ICollection<Message> Message { get; set; }
        public virtual ICollection<Permission> Permission { get; set; }
    }
}
