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

   
    public partial class History:IEntity
    {

        #region IEntity interface method definition

        public void SetId(int id)
        {
            HistoryId = id;
        }

        public int GetId()
        {
            return HistoryId;
        }

        public void SetTenantId(byte id)
        {
            HistoryTenantId = id;
        }

        public byte GetTenantId()
        {
            return HistoryTenantId;
        }

        public void InitializeEntity()
        {

        }

        #endregion

        #region Constant atomic check

        /// <summary>
        /// History action: screening creation
        /// </summary>
        public const string kScreeningCreationAction = "SCREENING_CREATION";

        /// <summary>
        /// History action: screening status update
        /// </summary>
        public const string kScreeningStatusUpdateAction = "SCREENING_STATUS_UPDATE";

        /// <summary>
        /// History action: atomic check creation
        /// </summary>
        public const string kAtomicCheckCreationAction = "ATOMIC_CHECK_CREATION";

        /// <summary>
        /// History action: atomic check status update
        /// </summary>
        public const string kAtomicCheckStatusUpdateAction = "ATOMIC_CHECK_STATUS_UPDATE";

        /// <summary>
        /// History action: atomic check validation status update
        /// </summary>
        public const string kAtomicCheckValidationStatusUpdateAction = "ATOMIC_CHECK_VALIDATION_STATUS_UPDATE";

        #endregion
    }
}
