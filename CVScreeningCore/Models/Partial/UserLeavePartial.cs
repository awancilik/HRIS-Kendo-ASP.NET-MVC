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
    
    public partial class UserLeave:IEntity
    {
        public void SetId(int id)
        {
            UserLeaveId = id;
        }

        public int GetId()
        {
            return UserLeaveId;
        }

        public void SetTenantId(byte id)
        {
            UserLeaveTenantId = id;
        }

        public byte GetTenantId()
        {
            return UserLeaveTenantId;
        }

        public void InitializeEntity()
        {

        }
    }
}
