using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces
{
    public interface ICompuestoRepository
    {
        public Task<List<Compuesto>> GetAllAsync();
        public Task<Compuesto> GetByGuidAsync(Guid compuesto_guid);
        public Task<bool> CreateAsync(Compuesto compuesto);
        public Task<bool> UpdateAsync(Compuesto compuesto);
        public Task<bool> RemoveAsync(Guid compuesto_guid);
        public Task<string> GetCompuestoByNameAsync(string compuesto_nombre);
    }
}
