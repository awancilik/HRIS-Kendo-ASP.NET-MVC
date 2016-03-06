using System;
using System.Collections.Generic;
using CVScreeningService.DTO.Discussion;
using CVScreeningService.DTO.LookUpDatabase;
using CVScreeningService.DTO.UserManagement;
using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Screening
{
    public class AtomicCheckBaseDTO
    {
        public ScreeningBaseDTO Screening { get; set; }
        public string AssignedTo { get; set; }

        [ObjectId]
        public int AtomicCheckId { get; set; }
        public string AtomicCheckCategory { get; set; }
        public string AtomicCheckType { get; set; }
        public string AtomicCheckSummary { get; set; }
        public string AtomicCheckReport { get; set; }
        public string AtomicCheckRemarks { get; set; }
        public byte AtomicCheckTenantId { get; set; }
        public bool AtomicCheckDeactivated { get; set; }
        public byte AtomicCheckState { get; set; }
        public byte AtomicCheckValidationState { get; set; }
        public string State { get; set; }
        public string ValidationState { get; set; }
        public int InternalDiscussionId { get; set; }

        // Action that could be done and status information
        public bool CanBeAssigned { get; set; }
        public bool CanBeProcessed { get; set; }
        public bool CanBeValidated { get; set; }
        public bool CanBeRejected { get; set; }
        public bool CanBeWronglyQualified { get; set; }
        public bool IsQualified { get; set; }
        public bool IsValidated { get; set; }



        public Byte TypeOfCheckCode { get; set; }
        public string TypeOfCheckName { get; set; }
        public string TypeOfCheckComments { get; set; }
        public string AtomicCheckVerificationSummary { get; set; }



    }
}