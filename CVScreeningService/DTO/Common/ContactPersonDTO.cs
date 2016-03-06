namespace CVScreeningService.DTO.Common
{
    public class ContactPersonDTO
    {
        public int ContactPersonId { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactComments { get; set; }
        public ContactInfoDTO ContactInfo { get; set; }
        public AddressDTO Address { get; set; }
        public int QualificationPlaceId { get; set; }
        public int ClientCompanyId { get; set; }
        public bool IsContactDeactivated { get; set; }
    }
}