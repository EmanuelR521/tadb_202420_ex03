using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Exceptions;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Services
{
    public class CompuestoService(ICompuestoRepository compuestoRepository)
    {
        private readonly ICompuestoRepository _compuestoRepository = compuestoRepository;

        
        public async Task<List<Compuesto>> GetAllAsync()
        {
            return await _compuestoRepository.GetAllAsync();
        }

        
        public async Task<Compuesto> GetByGuidAsync(Guid compuestoGuid)
        {
            var unCompuesto = await _compuestoRepository.GetByGuidAsync(compuestoGuid);

            if (unCompuesto.Uuid == Guid.Empty)
                throw new AppValidationException($"Compuesto no encontrado con el GUID {compuestoGuid}");

            return unCompuesto;
        }


        public async Task<Compuesto> CreateAsync(Compuesto unCompuesto)
        {
            string resultadoValidacionDatos = ValidaDatos(unCompuesto);

            if (!string.IsNullOrEmpty(resultadoValidacionDatos))
                throw new AppValidationException(resultadoValidacionDatos);

            var compuestoExistente = await _compuestoRepository.GetCompuestoByNameAsync(unCompuesto.Nombre!);

            if (!string.IsNullOrEmpty(compuestoExistente))
                throw new AppValidationException($"Ya existe un compuesto registrado con el nombre {unCompuesto.Nombre}");

            try
            {
                bool resultado = await _compuestoRepository.CreateAsync(unCompuesto);

                if (!resultado)
                    throw new AppValidationException("Operación ejecutada, pero no generó cambios");

            }
            catch (DbOperationException ex)
            {
                throw new AppValidationException("Error en la base de datos: " + ex.Message);
            }

            return unCompuesto;
        }


        public async Task<Compuesto> UpdateAsync(Compuesto unCompuesto)
        {
            string resultadoValidacionDatos = ValidaDatos(unCompuesto);

            if (!string.IsNullOrEmpty(resultadoValidacionDatos))
                throw new AppValidationException(resultadoValidacionDatos);

            var compuestoExistente = await _compuestoRepository.GetByGuidAsync(unCompuesto.Uuid);

            if (compuestoExistente.Uuid == Guid.Empty)
                throw new AppValidationException($"Compuesto con GUID {unCompuesto.Uuid} no encontrado para actualizar.");

            try
            {
                bool resultadoAccion = await _compuestoRepository.UpdateAsync(unCompuesto);

                if (!resultadoAccion)
                    throw new AppValidationException("Operación ejecutada, pero no generó cambios en la DB");

                compuestoExistente = await _compuestoRepository.GetByGuidAsync(unCompuesto.Uuid);
            }
            catch (DbOperationException ex)
            {
                throw new AppValidationException("Error en la base de datos: " + ex.Message);
            }

            return compuestoExistente;
        }


        public async Task<Compuesto> RemoveAsync(Guid compuestoGuid)
        {
            var compuestoExistente = await _compuestoRepository.GetByGuidAsync(compuestoGuid);

            if (compuestoExistente.Uuid == Guid.Empty)
                throw new AppValidationException($"No existe un compuesto identificado con el GUID {compuestoGuid} para eliminar.");

            try
            {
                bool resultadoAccion = await _compuestoRepository.RemoveAsync(compuestoGuid);

                if (!resultadoAccion)
                    throw new AppValidationException("Operación ejecutada, pero no generó cambios en la DB");
            }
            catch (DbOperationException ex)
            {
                throw new AppValidationException("Error en la base de datos: " + ex.Message);
            }

            return compuestoExistente;
        }

        
        private static string ValidaDatos(Compuesto unCompuesto)
        {
            if (string.IsNullOrEmpty(unCompuesto.Nombre))
                return "El nombre del compuesto no puede estar vacío.";

            if (string.IsNullOrEmpty(unCompuesto.Formula_Quimica))
                return "La fórmula química del compuesto no puede estar vacía.";

            if (unCompuesto.Masa_Molar <= 0)
                return "La masa molar del compuesto no puede ser menor o igual a cero.";

            if (string.IsNullOrEmpty(unCompuesto.Estado_Agregacion))
                return "El estado de agregación del compuesto no puede estar vacío.";

            return string.Empty;
        }
    }
}