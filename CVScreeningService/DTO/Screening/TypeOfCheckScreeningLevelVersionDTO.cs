using System;

namespace CVScreeningService.DTO.Screening
{
    [Serializable]
    public class TypeOfCheckScreeningLevelVersionDTO
    {
        public int TypeOfCheckId { get; set; }
        public string TypeOfCheckScreeningComments { get; set; }
        public TypeOfCheckDTO TypeOfCheck { get; set; }
    }
}