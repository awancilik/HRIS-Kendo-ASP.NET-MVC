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
    
    public partial class Message:IEntity
    {
        public int MessageId { get; set; }
        public string MessageContent { get; set; }
        public System.DateTime MessageCreatedDate { get; set; }
        public byte MessageTenantId { get; set; }
    
        public virtual Discussion Discussion { get; set; }
        public virtual webpages_UserProfile MessageCreatedBy { get; set; }
    }
}
