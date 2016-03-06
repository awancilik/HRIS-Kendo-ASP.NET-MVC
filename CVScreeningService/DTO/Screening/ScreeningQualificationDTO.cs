using System;
using CVScreeningService.DTO.Common;

namespace CVScreeningService.DTO.Screening
{
    public class ScreeningQualificationDTO
    {
        public int ScreeningQualificationId { get; set; }
        public string ScreeningQualificationMaritalStatus { get; set; }
        public DateTime? ScreeningQualificationBirthDate { get; set; }
        public string ScreeningQualificationBirthPlace { get; set; }
        public string ScreeningQualificationGender { get; set; }
        public string ScreeningQualificationIDCardNumber { get; set; }
        public string ScreeningQualificationPassportNumber { get; set; }
        public string ScreeningQualificationRelationshipWithCandidate { get; set; }
        public bool? ScreeningQualificationIsDeactivated { get; set; }

        public bool IsCurrentAddressWronglyQualified { get; set; }
        public bool IsCVAddressWronglyQualified { get; set; }
        public bool IsIDCardAddressWronglyQualified { get; set; }

        public bool IsCurrentAddressReQualified { get; set; }
        public bool IsCVAddressReQualified { get; set; }
        public bool IsIDCardAddressReQualified { get; set; }

        public AddressDTO CurrentAddress { get; set; }
        public AddressDTO CVAddress { get; set; }
        public AddressDTO IDCardAddress { get; set; }
        public ContactPersonDTO EmergencyContactPerson { get; set; }
        public ScreeningDTO Screening { get; set; }
        public ContactInfoDTO PersonalContactInfo { get; set; }
    }
}
