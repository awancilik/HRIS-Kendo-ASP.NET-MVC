using CVScreeningService.Filters;

namespace CVScreeningService.DTO.Client
{
    public class ClientContractDTO
    {
        [ObjectId]
        public int ContractId { get; set; }
        public string ContractReference { get; set; }
        public string ContractYear { get; set; }
        public string ContractDescription { get; set; }
        public bool IsContractEnabled { get; set; }
        public virtual ClientCompanyDTO ClientCompany { get; set; }
    }
}