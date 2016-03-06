using System;

namespace CVScreeningCore.Models
{
    public partial class DefaultMatrix : IEntity
    {

        public const int kInitialDefaultValue = 50;

        public void SetId(int id)
        {
            this.DefaultMatrixId = id;
        }

        public int GetId()
        {
            return DefaultMatrixId;
        }

        public void SetTenantId(byte id)
        {
            this.DefaultMatrixTenantId = id;
        }

        public byte GetTenantId()
        {
            return DefaultMatrixTenantId;
        }

        public void InitializeEntity()
        {
            throw new NotImplementedException();
        }
    }
}
