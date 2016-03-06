using System;
using System.Collections.Generic;
using System.Linq;

namespace CVScreeningCore.Models
{
    public partial class SkillMatrix : IEntity
    {
        public const int kDefaultValue = -2;
        public const int kImpossibleValue = -1;

        public const string kDefaultValueString = "DEF";
        public const string kImpossibleValueString = "IMP";

        public static Dictionary<int, string> kCategories = new Dictionary<int, string>
        {
            {1, "Office"},
            {2, "On field"}
        };

        public void SetId(int id)
        {
            this.SkillMatrixId = id;
        }

        public int GetId()
        {
            return SkillMatrixId;
        }

        public void SetTenantId(byte id)
        {
            SkillMatrixTenantId = id;
        }

        public byte GetTenantId()
        {
            return SkillMatrixTenantId;
        }

        public void InitializeEntity()
        {
            throw new NotImplementedException();
        }
    }
}
