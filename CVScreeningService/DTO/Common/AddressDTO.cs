namespace CVScreeningService.DTO.Common
{
    public class AddressDTO
    {
        public int AddressId { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public LocationDTO Location { get; set; }


        public override string ToString()
        {
            return string.Format("AddressDTO object: AddressId: {0}, Street: {1}, PostalCode: {2}, Location: {3}",
                AddressId, Street, PostalCode, Location.ToString());
        }
    }
}