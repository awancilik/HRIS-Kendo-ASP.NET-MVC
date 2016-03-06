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
    public class AddressMandatoryAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(validationContext.DisplayName);

            var model = (AddressViewModel) value;
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
        private ValidationResult CheckIndonesianAddress(AddressViewModel viewModel, ValidationContext validationContext)
        {
            if (String.IsNullOrEmpty(viewModel.LocationViewModel.CountryId)
                || String.IsNullOrEmpty(viewModel.LocationViewModel.ProvinceId)
                || String.IsNullOrEmpty(viewModel.LocationViewModel.CityId)
                || String.IsNullOrEmpty(viewModel.LocationViewModel.DistrictId)
                || String.IsNullOrEmpty(viewModel.LocationViewModel.SubDistrictId)
                || String.IsNullOrEmpty(viewModel.PostalCode)
                || String.IsNullOrEmpty(viewModel.Street))
            {
                return new ValidationResult(Address.AddressMandatory);
            }
            return ValidationResult.Success;
        }

        /// <summary>
        /// Check if address (address is not an indonesian address) has been filled completely
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        private ValidationResult CheckOthersAddress(AddressViewModel viewModel, ValidationContext validationContext)
        {
            if (String.IsNullOrEmpty(viewModel.FullAddress) 
                ||String.IsNullOrEmpty(viewModel.LocationViewModel.CountryId))
            {
                return new ValidationResult(Address.AddressMandatory);
            }
            return ValidationResult.Success;
        }

    }

}