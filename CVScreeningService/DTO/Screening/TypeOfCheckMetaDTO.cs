using System;

namespace CVScreeningService.DTO.Screening
{
    [Serializable]
    public class TypeOfCheckMetaDTO
    {
        public TypeOfCheckDTO TypeOfCheck { get; set; }

        public int TypeOfCheckMetaId { get; set; }
        public string TypeOfCheckMetaKey { get; set; }
        public string TypeOfCheckMetaValue { get; set; }
        public string TypeOfCheckMetaCategory { get; set; }
    }
}