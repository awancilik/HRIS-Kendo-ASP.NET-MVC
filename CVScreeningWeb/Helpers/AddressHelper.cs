using System;
using CVScreeningService.DTO.Common;
using CVScreeningWeb.ViewModels.Common;
using Nalysa.Common.Log;

namespace CVScreeningWeb.Helpers
{
    //!TODO Remove duplicated code to handle mandatory and optionnal address
    public class AddressHelper
    {
        public static AddressDTO ExtractAddressViewModel(AddressViewModel iModel)
        {
            if (iModel == null)
                return null;
            return new AddressDTO
            {
                Street = iModel.Street != null && iModel.LocationViewModel != null && iModel.LocationViewModel.SubDistrictId != null
                    ? iModel.Street
                    : iModel.FullAddress,
                PostalCode = iModel.PostalCode,
                Location = iModel.LocationViewModel != null && iModel.LocationViewModel.SubDistrictId != null
                ? new LocationDTO { LocationId = Convert.ToInt32(iModel.LocationViewModel.SubDistrictId) }
                : new LocationDTO { LocationId = Convert.ToInt32(iModel.LocationViewModel.CountryId) }
            };
        }

        public static AddressDTO ExtractAddressOptionalViewModel(AddressOptionalViewModel iModel)
        {
            if (iModel == null)
                return null;
            return new AddressDTO
            {
                Street = iModel.Street != null && iModel.LocationViewModel != null && iModel.LocationViewModel.SubDistrictId != null
                    ? iModel.Street
                    : iModel.FullAddress,
                PostalCode = iModel.PostalCode,
                Location = iModel.LocationViewModel != null && iModel.LocationViewModel.SubDistrictId != null
                ? new LocationDTO { LocationId = Convert.ToInt32(iModel.LocationViewModel.SubDistrictId) }
                : new LocationDTO { LocationId = Convert.ToInt32(iModel.LocationViewModel.CountryId) }
            };
        }

        public static AddressViewModel BuildAddressViewModel(AddressDTO addressDTO = null, string name = null)
        {
            return new AddressViewModel
            {
                Name = name ?? "Address",
                Street = addressDTO != null ? addressDTO.Street : "",
                PostalCode = addressDTO != null ? addressDTO.PostalCode : "",
                // Full address is stored in the street field in databases if location is empty
                FullAddress = addressDTO != null 
                    && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY ? addressDTO.Street : "",
                LocationViewModel = new LocationViewModel
                {
                    SubDistrictId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationId.ToString()
                            : "0",
                    SubDistrictName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationName
                            : "",
                    DistrictId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationId.ToString()
                            : "0",
                    DistrictName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationName
                            : "",
                    CityId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationId.ToString()
                            : "0",
                    CityName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationName
                            : "",
                    ProvinceId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationParent.LocationId.ToString()
                            : "0",
                    ProvinceName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationParent.LocationName
                            : "",
                    CountryId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationParent.LocationParent.LocationId.ToString()
                            : addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY
                                ? addressDTO.Location.LocationId.ToString()
                                : "0",
                    CountryName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationParent.LocationParent.LocationName
                            : addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY
                                ? addressDTO.Location.LocationName
                                : "",
                }
            };
        }

        public static AddressOptionalViewModel BuildAddressOptionalViewModel(AddressDTO addressDTO = null, string name = null)
        {
            return new AddressOptionalViewModel
            {
                Name = name ?? "Address",
                Street = addressDTO != null ? addressDTO.Street : "",
                PostalCode = addressDTO != null ? addressDTO.PostalCode : "",
                // Full address is stored in the street field in databases if location is empty
                FullAddress = addressDTO != null
                    && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY ? addressDTO.Street : "",
                LocationViewModel = new LocationOptionalViewModel
                {
                    SubDistrictId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationId.ToString()
                            : "0",
                    SubDistrictName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationName
                            : "",
                    DistrictId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationId.ToString()
                            : "0",
                    DistrictName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationName
                            : "",
                    CityId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationId.ToString()
                            : "0",
                    CityName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationName
                            : "",
                    ProvinceId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationParent.LocationId.ToString()
                            : "0",
                    ProvinceName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationParent.LocationName
                            : "",
                    CountryId =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationParent.LocationParent.LocationId.ToString()
                            : addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY
                                ? addressDTO.Location.LocationId.ToString()
                                : "0",
                    CountryName =
                        addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_SUBDISTRICT
                            ? addressDTO.Location.LocationParent.LocationParent.LocationParent.LocationParent.LocationName
                            : addressDTO != null && addressDTO.Location != null && addressDTO.Location.LocationLevel == (int)LocationDTO.LocationLevelEnum.LOCATION_LEVEL_COUNTRY
                                ? addressDTO.Location.LocationName
                                : "",
                }
            };
        }

        /// <summary>
        /// Get address, postal, and location information from given AddressDTO and returns a string
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string GetAddressAsLabel(AddressDTO address)
        {
            string fullAddress = string.Empty;
            fullAddress += address.Street ?? string.Empty;
            fullAddress += address.PostalCode != null ? ", " + address.PostalCode : string.Empty;
            fullAddress += address.Location != null
                ? address.Location.LocationLevel == 1 //    if level is country
                    ? Environment.NewLine + address.Location.LocationName //    then concat country name directly
                    : Environment.NewLine + string.Format("{0}, {1}, {2}, {3}, {4}",
                        address.Location.LocationName,
                        address.Location.LocationParent.LocationName,
                        address.Location.LocationParent.LocationParent.LocationName,
                        address.Location.LocationParent.LocationParent.LocationParent.LocationName,
                        address.Location.LocationParent.LocationParent.LocationParent.LocationParent.LocationName
                        )
                : string.Empty;
            return fullAddress;
        }

        /// <summary>
        /// Get address, postal, and location information from given AddressDTO and returns a string
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string GetAddressAsTextArea(AddressDTO address)
        {
            string fullAddress = string.Empty;
            fullAddress += address.Street ?? string.Empty;
            fullAddress += address.PostalCode != null ? ", " + address.PostalCode : string.Empty;
            fullAddress += address.Location != null
                ? address.Location.LocationLevel == 1 //    if level is country
                    ? Environment.NewLine + address.Location.LocationName //    then concat country name directly
                    : Environment.NewLine + string.Format("{0}, {1}{2}{3}, {4}{5}{6}",
                        address.Location.LocationName,
                        address.Location.LocationParent.LocationName,
                        Environment.NewLine,
                        address.Location.LocationParent.LocationParent.LocationName,
                        address.Location.LocationParent.LocationParent.LocationParent.LocationName,
                        Environment.NewLine,
                        address.Location.LocationParent.LocationParent.LocationParent.LocationParent.LocationName
                        )
                : string.Empty;
            return fullAddress;
        }

        /// <summary>
        /// Get location information, address, and postal from given AddressDTO and returns a string
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string GetShortAddressAsString(AddressDTO address)
        {
            string shortAddress = string.Empty;
            shortAddress += address.Location != null
                ? address.Location.LocationLevel == 1
                    ? address.Location.LocationName + " "
                    : string.Format("{0}, {1} {2}, ", 
                        address.Location.LocationParent.LocationParent.LocationName,
                        address.Location.LocationParent.LocationName,
                        address.Location.LocationName)
                : string.Empty;
            shortAddress += address.Street ?? string.Empty;
            shortAddress += address.PostalCode != null ? ", " + address.PostalCode : string.Empty;
            shortAddress = shortAddress.Length<=40? shortAddress : shortAddress.Substring(0, 40)+"...";
            return shortAddress;
        }


    }
}