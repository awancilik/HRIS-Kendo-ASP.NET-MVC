namespace CVScreeningService.DTO.Common
{
    public class LocationDTO
    {
        public enum LocationLevelEnum
        {
            LOCATION_LEVEL_COUNTRY = 1,
            LOCATION_LEVEL_PROVINCE = 2,
            LOCATION_LEVEL_CITY = 3,
            LOCATION_LEVEL_DISTRICT = 4,
            LOCATION_LEVEL_SUBDISTRICT = 5,
        }

        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public int LocationLevel { get; set; }
        public string LocationParentLocationName { get; set; }
        public int? LocationParentLocationId { get; set; }

        public LocationDTO LocationParent { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "LocationDTO object: LocationId: {0}, LocationName: {1}, LocationLevel: {2}, LocationParentLocationId: {3}",
                    LocationId, LocationName, LocationLevel, LocationParentLocationId);
        }
    }
}