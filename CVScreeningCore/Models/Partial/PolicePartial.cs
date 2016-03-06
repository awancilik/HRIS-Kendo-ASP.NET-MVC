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
    
    public partial class Police:QualificationPlace , IEntity
    {
        /// <summary>
        /// Retrieve all the type of checks compatible with this qualification place
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<TypeOfCheckEnum> GetTypeOfChecksCompatible()
        {
            return new List<TypeOfCheckEnum>
            {
                TypeOfCheckEnum.POLICE_CHECK,
            };
        }

        public override QualificationCode GetQualificationCode()
        {
            return QualificationCode.Police;
        }
    }
}
