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
    
    public partial class Notification:IEntity
    {
        #region IEntity interface method definition

        public void SetId(int id)
        {
            NotificationId = id;
        }

        public int GetId()
        {
            return NotificationId;
        }

        public void SetTenantId(byte id)
        {
            NotificationTenantId = id;
        }

        public byte GetTenantId()
        {
            return NotificationTenantId;
        }

        public void InitializeEntity()
        {

        }

        #endregion
    }
}