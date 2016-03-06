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
    
    public partial class ScreeningQualification:IEntity
    {
        public void SetId(int id)
        {
            ScreeningQualificationId = id;
        }


        public int GetId()
        {
            return ScreeningQualificationId;
        }

        public void SetTenantId(byte id)
        {
            ScreeningQualificationTenantId = id;
        }

        public byte GetTenantId()
        {
            return ScreeningQualificationTenantId;
        }

        public void InitializeEntity()
        {

        }

        /// <summary>
        /// Reset current address of the qualification to null value
        /// </summary>
        public void ResetCurrentAddress()
        {
            this.CurrentAddress.CurrentAddressScreeningQualification.Remove(this);
            this.CurrentAddress = null;
        }

        /// <summary>
        /// Reset CV address of the qualification to null value
        /// </summary>
        public void ResetCVAddress()
        {
            this.CVAddress.CVAddressScreeningQualification.Remove(this);
            this.CVAddress = null;
        }

        /// <summary>
        /// Reset id card address of the qualification to null value
        /// </summary>
        public void ResetIDCardAddress()
        {
            this.IDCardAddress.IDCardAddressScreeningQualification.Remove(this);
            this.IDCardAddress = null;
        }
    }
}