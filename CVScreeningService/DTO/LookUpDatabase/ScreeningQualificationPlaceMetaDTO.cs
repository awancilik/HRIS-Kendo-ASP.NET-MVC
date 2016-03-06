using CVScreeningService.DTO.Screening;

namespace CVScreeningService.DTO.LookUpDatabase
{
    public class ScreeningQualificationPlaceMetaDTO
    {
        public int ScreeningId { get; set; }
        public int QualificationPlaceId { get; set; }
        public string ScreeningQualificationMetaKey { get; set; }
        public string ScreeningQualificationMetaValue { get; set; }
        public byte ScreeningQualificationMetaTenantId { get; set; }

        public virtual BaseQualificationPlaceDTO QualificationPlace { get; set; }
        public virtual ScreeningBaseDTO Screening { get; set; }
    }
}
