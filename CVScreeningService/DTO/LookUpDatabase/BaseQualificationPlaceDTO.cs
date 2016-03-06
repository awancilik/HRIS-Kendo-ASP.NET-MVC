using System.Collections.Generic;
using CVScreeningCore.Models;
using CVScreeningService.DTO.Common;

namespace CVScreeningService.DTO.LookUpDatabase
{
    public abstract class BaseQualificationPlaceDTO
    {
        public int QualificationPlaceId { get; set; }
        public string QualificationPlaceName { get; set; }
        public string QualificationPlaceCategory { get; set; }
        public string QualificationPlaceDescription { get; set; }
        public string QualificationPlaceWebSite { get; set; }
        public AddressDTO Address { get; set; }
        public bool QualificationReQualified { get; set; }
        public TypeOfCheckEnum TypeOfCheckType { get; set; }
        public ICollection<ScreeningQualificationPlaceMetaDTO> ScreeningQualificationPlaceMeta { get; set; }
    }
}