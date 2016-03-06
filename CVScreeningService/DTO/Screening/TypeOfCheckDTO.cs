using System;

namespace CVScreeningService.DTO.Screening
{
    [Serializable]
    public class TypeOfCheckDTO
    {
        public int TypeOfCheckId { get; set; }
        public string CheckName { get; set; }
        public byte TypeOfCheckCode { get; set; }
    }
}