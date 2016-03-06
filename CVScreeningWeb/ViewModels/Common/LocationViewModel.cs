using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CVScreeningWeb.Filters;

namespace CVScreeningWeb.ViewModels.Common
{
    public class LocationViewModel
    {
        [Required]
        [LocalizedDisplayName("Country", NameResourceType = typeof(Resources.Common))]
        public string CountryId { get; set; }
        public string CountryName { get; set; }

        [LocalizedDisplayName("Province", NameResourceType = typeof(Resources.Common))]
        public string ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        
        [LocalizedDisplayName("City", NameResourceType = typeof(Resources.Common))]
        public string CityId { get; set; }
        public string CityName { get; set; }

        [LocalizedDisplayName("District", NameResourceType = typeof(Resources.Common))]
        public string DistrictId { get; set; }
        public string DistrictName { get; set; }

        [LocalizedDisplayName("SubDistrict", NameResourceType = typeof(Resources.Common))]
        public string SubDistrictId { get; set; }
        public string SubDistrictName { get; set; }

    }
}