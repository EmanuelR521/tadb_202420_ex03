using Dapper;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.DBContexts;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Exceptions;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;
using Npgsql;
using System.Data;
using System.Text.Json;
using NpgsqlTypes;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Repositories
{
    public class CompuestoRepository(PgsqlDbContext contexto) : ICompuestoRepository
    {
        private readonly PgsqlDbContext contextoDB = contexto;

        public async Task<List<CompuestoSimplificado>> GetAllAsync()
        {
            using var conexion = contextoDB.CreateConnection();

            string sentenciaSQL = "SELECT id_uuid uuid, nombre, formula_quimica, masa_molar, estado_agregacion FROM core.compuestos ORDER BY nombre";

            var resultadoCompuestos = await conexion.QueryAsync<CompuestoSimplificado>(sentenciaSQL, new DynamicParameters());
            return resultadoCompuestos.ToList();
        }


        public async Task<Compuesto> GetByGuidAsync(Guid compuestoGuid)
        {
            Compuesto unCompuesto = new();

            using var conexion = contextoDB.CreateConnection();

            DynamicParameters parametrosSentencia = new();
            parametrosSentencia.Add("@uuid", compuestoGuid,
                                    DbType.Guid, ParameterDirection.Input);

            string sentenciaSQL = "SELECT id_uuid uuid, nombre, formula_quimica, masa_molar, estado_agregacion FROM core.compuestos WHERE id_uuid = @uuid";

            var resultado = await conexion.QueryAsync<Compuesto>(sentenciaSQL, parametrosSentencia);

            if(resultado.Any())
                unCompuesto = resultado.First();

            return unCompuesto;
        }


        public async Task<Compuesto> GetCompuestoByNameAsync(string compuestoNombre)
        {
            Compuesto nombreCompuesto = new();
           
            var conexion = contextoDB.CreateConnection();

            DynamicParameters parametrosSentencia = new();
            parametrosSentencia.Add("@compuestoNombre", compuestoNombre,
                                    DbType.String, ParameterDirection.Input);

            string sentenciaSQL = "SELECT id_uuid uuid,nombre,formula_quimica,masa_molar,estado_agregacion FROM core.compuestos WHERE LOWER(nombre) = LOWER(@compuestoNombre)";

            var resultado = await conexion.QueryAsync<Compuesto>(sentenciaSQL, parametrosSentencia);

            if(resultado.Any())
                nombreCompuesto = resultado.First();

            return nombreCompuesto;
        }
        public async Task<bool> CreateAsync(Compuesto compuesto)
        {
            bool resultadoAccion = false;

            try
            {
                var conexion = contextoDB.CreateConnection();
                string procedimiento = "core.p_insertar_compuesto";






                var elementosJson = JsonSerializer.Serialize(compuesto.Elementos);




                var parametros = new
                {
                    p_nombre = compuesto.Nombre,
                    p_formula_quimica = compuesto.Formula_Quimica,
                    p_masa_molar = compuesto.Masa_Molar,
                    p_estado_agregacion = compuesto.Estado_Agregacion,
                    p_elementos = elementosJson
                };

                var cantidadFilas = await conexion.ExecuteAsync(
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
                var elementosJson = JsonSerializer.Serialize(compuesto.Elementos);

                var parametros = new
                {
                    p_compuesto_uuid = compuesto.Uuid,
                    p_nombre = compuesto.Nombre,
                    p_formula_quimica = compuesto.Formula_Quimica,
                    p_masa_molar = compuesto.Masa_Molar,
                    p_estado_agregacion = compuesto.Estado_Agregacion,
                    p_elementos = elementosJson
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


            }
            catch (NpgsqlException ex)
            {
                throw new DbOperationException($"Error al eliminar el compuesto: {ex.Message}");
            }

            return resultadoAccion;
        }
    }
}

