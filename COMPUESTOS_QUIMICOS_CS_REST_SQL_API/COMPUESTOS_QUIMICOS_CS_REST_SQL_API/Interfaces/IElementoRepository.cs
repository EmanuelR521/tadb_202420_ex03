﻿using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces
{
    public interface IElementoRepository
    {
        public Task<List<Elemento>> GetAllAsync();
        public Task<Elemento> GetByGuidAsync(Guid elemento_guid);
        public Task<string> GetElementoByNameAsync(string elemento_nombre);
        public Task<bool> CreateAsync(Elemento unElemento);
        public Task<bool> UpdateAsync(Elemento unElemento);
        public Task<bool> RemoveAsync(Guid elemento_guid);
    }
}
