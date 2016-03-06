using System.Collections.Generic;
using CVScreeningService.DTO.Common;
using CVScreeningService.DTO.UserManagement;

namespace CVScreeningService.DTO.Client
{
    public class ClientCompanyDTO
    {
        public int ClientCompanyId { get; set; }
        public string ClientCompanyName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool ClientCompanyIsDeactivated { get; set; }

        public virtual AddressDTO Address { get; set; }
        public virtual ContactInfoDTO ContactInfo { get; set; }
        public virtual ICollection<ContactPersonDTO> ContactPerson { get; set; }
        public virtual ICollection<ClientContractDTO> Contract { get; set; }
        public virtual ICollection<UserProfileDTO> AccountManagers { get; set; }
    }
}