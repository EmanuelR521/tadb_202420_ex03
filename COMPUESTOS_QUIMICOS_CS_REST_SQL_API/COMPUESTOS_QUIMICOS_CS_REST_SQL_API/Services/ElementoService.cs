using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Exceptions;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Services
{
    public class ElementoService(IElementoRepository elementoRepository)
    {
        private readonly IElementoRepository _elementoRepository = elementoRepository;

        public async Task<List<Elemento>> GetAllAsync()
        {
            return await _elementoRepository.GetAllAsync();
        }

        public async Task<Elemento> GetByGuidAsync(Guid elemento_guid)
        {
            var unElemento = await _elementoRepository.GetByGuidAsync(elemento_guid);

            if (unElemento.Uuid == Guid.Empty)
                throw new AppValidationException($"Elemento no encontrado con el GUID {elemento_guid}");

            return unElemento;
        }

        public async Task<Elemento> CreateAsync(Elemento unElemento)
        {
            Elemento elementoCreado = new();
            string resultadoValidacionDatos = ValidaDatos(unElemento);

            if (!string.IsNullOrEmpty(resultadoValidacionDatos))
                throw new AppValidationException(resultadoValidacionDatos);

            var elementoExistente = await _elementoRepository.GetElementoByNameAsync(unElemento.Nombre!);

            if (!string.IsNullOrEmpty(elementoExistente.Nombre))
                throw new AppValidationException($"Ya existe un elemento registrado con el nombre {unElemento.Nombre}");



            try
            {
                bool resultado = await _elementoRepository.CreateAsync(unElemento);

                if (!resultado)
                    throw new AppValidationException("Operación ejecutada, pero no generó cambios");

                elementoCreado = await _elementoRepository.GetElementoByNameAsync(unElemento.Nombre!);
                Console.WriteLine(elementoCreado.Uuid);

            }
            catch (DbOperationException ex)
            {
                throw new AppValidationException("Error en la base de datos: " + ex.Message);
            }

            return elementoCreado;
        }

        public async Task<Elemento> UpdateAsync(Elemento unElemento)
        {
            string resultadoValidacionDatos = ValidaDatos(unElemento);

            if (!string.IsNullOrEmpty(resultadoValidacionDatos))
                throw new AppValidationException(resultadoValidacionDatos);

            var elementoExistente = await _elementoRepository.GetByGuidAsync(unElemento.Uuid);

            if (elementoExistente.Uuid == Guid.Empty)
                throw new AppValidationException($"Elemento no encontrado con el GUID {unElemento.Uuid}");

            try
            {

                bool resultado = await _elementoRepository.UpdateAsync(unElemento);

                if (!resultado)
                    throw new AppValidationException("Operación ejecutada, pero no generó cambios");

            }
            catch (DbOperationException ex)
            {
                throw new AppValidationException("Error en la base de datos: " + ex.Message);
            }

            return unElemento;
        }

        public async Task<Elemento> RemoveAsync(Guid elemento_guid)
        {
            var elementoExitente = await _elementoRepository.GetByGuidAsync(elemento_guid);

            if (elementoExitente.Uuid == Guid.Empty)
                throw new AppValidationException($"No existe un compuesto identificado con el GUID {elemento_guid} para eliminar.");

            try
            {
                bool resultadoAccion = await _elementoRepository.RemoveAsync(elemento_guid);

                if (!resultadoAccion)
                    throw new AppValidationException("Operación ejecutada, pero no generó cambios en la DB");
            }
            catch (DbOperationException ex)
            {
                throw new AppValidationException("Error en la base de datos: " + ex.Message);
            }

            return elementoExitente;
        }



        private static string ValidaDatos(Elemento unElemento)
        {
            if (string.IsNullOrEmpty(unElemento.Nombre))
                return ("El nombre del elemento no puede estar vacio");

            if (string.IsNullOrEmpty(unElemento.Simbolo))
                return ("El simbolo del elemento no puede estar vacio");

            if (unElemento.Numero_Atomico <= 0)
                return "el numero atomico no puede ser menor o igual a cero.";

            if (string.IsNullOrEmpty(unElemento.Config_Electronica))
                return ("La configuracion electronica no puede estar vacia");

            return string.Empty;
        }
    }
}
