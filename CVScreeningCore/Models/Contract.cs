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
    
    public partial class Contract:IEntity
    {
        public Contract()
        {
            this.ScreeningLevel = new HashSet<ScreeningLevel>();
            this.Permission = new HashSet<Permission>();
    		InitializeEntity();
        }
    
        public int ContractId { get; set; }
        public string ContractReference { get; set; }
        public string ContractYear { get; set; }
        public string ContractDescription { get; set; }
        public Nullable<bool> IsContractEnabled { get; set; }
        public byte ContractTenantId { get; set; }
    
        public virtual ClientCompany ClientCompany { get; set; }
        public virtual ICollection<ScreeningLevel> ScreeningLevel { get; set; }
        public virtual ICollection<Permission> Permission { get; set; }
    }
}
