using System.Web.Hosting;

namespace CVScreeningCore.Models
{
    public interface IEntity
    {
        /// <summary>
        /// Set identifier to the entity
        /// </summary>
        /// <param name="id"></param>
        void SetId(int id);

        /// <summary>
        /// Get identifier from the entity
        /// </summary>
        /// <returns></returns>
        int GetId();

        /// <summary>
        /// Set tenant Id to the entity
        /// </summary>
        /// <param name="id"></param>
        void SetTenantId(byte id);

        /// <summary>
        /// Get tenant Id from the entity
        /// </summary>
        /// <returns></returns>
        byte GetTenantId();

        /// <summary>
        /// Called 
        /// </summary>
        void InitializeEntity();

    }
}
