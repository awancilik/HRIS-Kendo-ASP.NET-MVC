using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using CVScreeningWeb.Resources;
using CVScreeningWeb.ViewModels.Common;

namespace CVScreeningWeb.Filters
{
    public class AddressOptionalAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var model = (AddressOptionalViewModel) value;
            // Indonesian address
            if (String.IsNullOrEmpty(model.FullAddress))
            {
                return this.CheckIndonesianAddress(model, validationContext);
            }
            // Other type of address
            return this.CheckOthersAddress(model, validationContext);

        }


        /// <summary>
        /// Check if indonesian address has been filled completely
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        private ValidationResult CheckIndonesianAddress(AddressOptionalViewModel viewModel, ValidationContext validationContext)
        {
            if (String.IsNullOrEmpty(viewModel.LocationViewModel.CountryId)
                && String.IsNullOrEmpty(viewModel.LocationViewModel.ProvinceId)
                && String.IsNullOrEmpty(viewModel.LocationViewModel.CityId)
                && String.IsNullOrEmpty(viewModel.LocationViewModel.DistrictId)
                && String.IsNullOrEmpty(viewModel.LocationViewModel.SubDistrictId)
                && String.IsNullOrEmpty(viewModel.PostalCode)
                && String.IsNullOrEmpty(viewModel.Street))
            {
                return ValidationResult.Success;
            }

            if (!String.IsNullOrEmpty(viewModel.LocationViewModel.CountryId)
                    && !String.IsNullOrEmpty(viewModel.LocationViewModel.ProvinceId)
                    && !String.IsNullOrEmpty(viewModel.LocationViewModel.CityId)
                    && !String.IsNullOrEmpty(viewModel.LocationViewModel.DistrictId)
                    && !String.IsNullOrEmpty(viewModel.LocationViewModel.SubDistrictId)
                    && !String.IsNullOrEmpty(viewModel.PostalCode)
                    && !String.IsNullOrEmpty(viewModel.Street))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(Address.AddressMandatory);
        }

        /// <summary>
        /// Check if address (address is not an indonesian address) has been filled completely
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        private ValidationResult CheckOthersAddress(AddressOptionalViewModel viewModel, ValidationContext validationContext)
        {
            if (String.IsNullOrEmpty(viewModel.FullAddress) 
                && String.IsNullOrEmpty(viewModel.LocationViewModel.CountryId))
            {
                return ValidationResult.Success;
            }

            if (!String.IsNullOrEmpty(viewModel.FullAddress)
                && !String.IsNullOrEmpty(viewModel.LocationViewModel.CountryId))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(Address.AddressMandatory);
        }

    }

}