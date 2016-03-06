using CVScreeningService.DTO.Common;

namespace CVScreeningService.DTO.Client
{
    public class ClientDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Remarks { get; set; }
        public bool UserIsDeactivated { get; set; }

        public ContactInfoDTO ContactInfo { get; set; }
    }
}