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
    
    public partial class ProfessionalQualification:IEntity
    {
        public ProfessionalQualification()
        {
            this.QualificationPlace = new HashSet<QualificationPlace>();
    		InitializeEntity();
        }
    
        public int ProfessionalQualificationId { get; set; }
        public string ProfessionalQualificationName { get; set; }
        public string ProfessionalQualificationCode { get; set; }
        public byte ProfessionalQualificationTenantId { get; set; }
        public string ProfessionalQualificationDescription { get; set; }
    
        public virtual ICollection<QualificationPlace> QualificationPlace { get; set; }
    }
}
