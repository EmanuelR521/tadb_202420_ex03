using Dapper;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.DBContexts;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Exceptions;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;
using Npgsql;
using System.Data;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Repositories
{
    public class CompuestoRepository(PgsqlDbContext contexto) : ICompuestoRepository
    {
        private readonly PgsqlDbContext contextoDB = contexto;



        public async Task<List<Compuesto>> GetAllAsync()
        {
            using var conexion = contextoDB.CreateConnection();
            string sentenciaSQL = "SELECT compuesto_uuid AS Uuid, nombre, formula_quimica, masa_molar, estado_agregacion FROM core.compuestos";

            var resultadoCompuestos = await conexion.QueryAsync<Compuesto>(sentenciaSQL);
            return resultadoCompuestos.ToList();
        }


        public async Task<Compuesto> GetByGuidAsync(Guid compuestoGuid)
        {
            using var conexion = contextoDB.CreateConnection();
            string sentenciaSQL = "SELECT compuesto_uuid AS Uuid, nombre, formula_quimica, masa_molar, estado_agregacion FROM core.compuestos WHERE compuesto_uuid = @compuestoGuid";

            var parametros = new DynamicParameters();
            parametros.Add("@compuestoGuid", compuestoGuid, DbType.Guid, ParameterDirection.Input);

            var resultado = await conexion.QueryAsync<Compuesto>(sentenciaSQL, parametros);
            return resultado.FirstOrDefault() ?? throw new DbOperationException($"Compuesto con GUID {compuestoGuid} no encontrado.");
        }


        public async Task<string> GetCompuestoByNameAsync(string compuestoNombre)
        {
            using var conexion = contextoDB.CreateConnection();
            string sentenciaSQL = "SELECT nombre FROM core.compuestos WHERE LOWER(nombre) = LOWER(@compuestoNombre)";

            var parametros = new DynamicParameters();
            parametros.Add("@compuestoNombre", compuestoNombre, DbType.String, ParameterDirection.Input);

            var resultado = await conexion.QueryAsync<string>(sentenciaSQL, parametros);
            return resultado.FirstOrDefault() ?? string.Empty;
        }


        public async Task<bool> CreateAsync(Compuesto compuesto)
        {
            bool resultadoAccion = false;

            try
            {
                var conexion = contextoDB.CreateConnection();
                string procedimiento = "core.p_insertar_compuesto";

                var parametros = new
                {
                    p_nombre = compuesto.Nombre,
                    p_formula_quimica = compuesto.Formula_Quimica,
                    p_masa_molar = compuesto.Masa_Molar,
                    p_estado_agregacion = compuesto.Estado_Agregacion
                };

                var cantidadFilas = await conexion
                    .ExecuteAsync(
                        procedimiento, 
                        parametros, 
                        commandType: CommandType.StoredProcedure);

                if (cantidadFilas != 0)
                    resultadoAccion = true;

            }
            catch (NpgsqlException ex)
            {
                throw new DbOperationException($"Error al crear el compuesto: {ex.Message}");
            }
            return resultadoAccion;
        }


        public async Task<bool> UpdateAsync(Compuesto compuesto)
        {
            bool resultadoAccion = false;

            var compuestoExistente = await GetByGuidAsync(compuesto.Uuid);

            if (compuestoExistente == null || compuestoExistente.Uuid == Guid.Empty)
                throw new DbOperationException($"No se puede actualizar. No existe el compuesto {compuesto.Nombre}.");

            try
            {
                var conexion = contextoDB.CreateConnection();
                string procedimiento = "core.p_actualizar_compuesto";

                var parametros = new
                {
                    p_compuesto_uuid = compuesto.Uuid,
                    p_nombre = compuesto.Nombre,
                    p_formula_quimica = compuesto.Formula_Quimica,
                    p_masa_molar = compuesto.Masa_Molar,
                    p_estado_agregacion = compuesto.Estado_Agregacion
                };

                var cantidad_filas = await conexion.ExecuteAsync(procedimiento, parametros, commandType: CommandType.StoredProcedure);

                if (cantidad_filas != 0)
                    resultadoAccion = true;

            }
            catch (NpgsqlException ex)
            {
                throw new DbOperationException($"Error al actualizar el compuesto: {ex.Message}");
            }
            return resultadoAccion;
        }


        public async Task<bool> RemoveAsync(Guid compuestoGuid)
        {
            bool resultadoAccion = false;
            try
            {
                using var conexion = contextoDB.CreateConnection();
                string procedimiento = "core.p_eliminar_compuesto";

                var parametros = new { p_compuesto_uuid = compuestoGuid };


                var cantidad_filas = await conexion.ExecuteAsync(
                    procedimiento, 
                    parametros, 
                    commandType: CommandType.StoredProcedure);

                if (cantidad_filas != 0)
                    resultadoAccion = true;

                if (cantidad_filas == 0)
                {
                    throw new AppValidationException("No se encontró ningún compuesto para eliminar con el GUID proporcionado.");
                }

            }
            catch (NpgsqlException ex)
            {
                throw new DbOperationException($"Error al eliminar el compuesto: {ex.Message}");
            }

            return resultadoAccion;
        }
    }
}

