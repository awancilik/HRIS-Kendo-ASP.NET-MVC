using System;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Common;

namespace CVScreeningService.DTO.UserManagement
{
    public class UserProfileDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Remarks { get; set; }
        public string UserType { get; set; }
        public byte[] UserPhoto { get; set; }
        public string UserPhotoContentType { get; set; }
        public string ScreenerCategory { get; set; }
        public byte? ScreenerCapabilities { get; set; }
        public bool UserIsDeactivated { get; set; }
        public Byte TenantId { get; set; }

        public AddressDTO Address { get; set; }
        public ContactInfoDTO ContactInfo { get; set; }
        public ContactPersonDTO ContactPerson { get; set; }
        public ClientCompanyDTO ClientCompanyForClientUserProfile { get; set; }
    }
}