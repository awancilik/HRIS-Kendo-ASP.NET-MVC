using System;

namespace CVScreeningCore.Models
{
    public partial class DispatchingSettings : IEntity
    {

        public void SetId(int id)
        {
            this.DispatchingSettingsId = id;
        }

        public int GetId()
        {
            return DispatchingSettingsId;
        }

        public void SetTenantId(byte id)
        {
            this.DispatchingSettingsTenantId = id;
        }

        public byte GetTenantId()
        {
            return DispatchingSettingsTenantId;
        }

        public void InitializeEntity()
        {
            throw new NotImplementedException();
        }

        public const string kDefaultScreenerCapabilities = "DefaultScreenerCapabilities";
        public const string kGeographicalOutsideIndonesiaValue = "GeographicalOutsideIndonesiaValue";
        public const string kGeographicalSameCityValue = "GeographicalSameCityValue";
        public const string kGeographicalSameCountryValue = "GeographicalSameCountryValue";
        public const string kGeographicalSameDistrictValue = "GeographicalSameDistrictValue";
        public const string kGeographicalSameProvinceValue = "GeographicalSameProvinceValue";
        public const string kGeographicalSameSubDistrictValue = "GeographicalSameSubDistrictValue";
        public const string kSkillCoefficient = "SkillCoefficient";
        public const string kWorkloadCoefficient = "WorkloadCoefficient";
        public const string kAvailibilityCoefficient = "AvailibilityCoefficient";
        public const string kAlreadyCoefficient = "AlreadyCoefficient";
        public const string kGeographicalCoefficient = "GeographicalCoefficient";

    }
}
