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
            var conexion = contextoDB.CreateConnection();

            string sentenciaSQL = 
                "SELECT compuesto_uuid AS Uuid, nombre, formula " +
                "FROM core.compuestos " +
                "ORDER BY nombre";

            var resultadoCompuestos = await conexion
                .QueryAsync<Compuesto>(sentenciaSQL, new DynamicParameters());

            return resultadoCompuestos.ToList();
        }

        public async Task<Compuesto> GetByGuidAsync(Guid compuestoGuid)
        {
            Compuesto unCompuesto = new();

            var conexion = contextoDB.CreateConnection();

            DynamicParameters parametrosSentencia = new();
            parametrosSentencia.Add("@compuesto_guid", compuestoGuid,
                DbType.Guid, ParameterDirection.Input);

            string sentenciaSQL = 
                "SELECT compuesto_uuid AS Uuid, nombre, formula " +
                "FROM core.compuestos " +
                "WHERE compuesto_uuid = @compuesto_guid";

            var resultado = await conexion.QueryAsync<Compuesto>(sentenciaSQL, parametrosSentencia);

            if(resultado.Any())
                unCompuesto = resultado.First();

            return unCompuesto;
        }

        public async Task<string> GetCompuestoByNameAsync(string compuesto_nombre)
        {
            string nombreCompuesto = string.Empty;

            var conexion = contextoDB.CreateConnection();

            DynamicParameters parametrosSentencia = new();
            parametrosSentencia.Add("@compuesto_nombre", compuesto_nombre,
                                    DbType.String, ParameterDirection.Input);

            string sentenciaSQL =
                "SELECT distinct compuestos " +
                "FROM core.v_info_compuestos " +
                "WHERE LOWER(compuesto) = LOWER(@compuesto_nombre)";

            var resultado = await conexion.QueryAsync<string>(sentenciaSQL,
                parametrosSentencia);

            if (resultado.Any())
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

                var parametros = new
                {
                    p_nombre = compuesto.Nombre,
                    p_simbolo = compuesto.Simbolo,
                    p_numero_atomico = compuesto.Numero_Atomico,
                    p_config_electronica = compuesto.Config_Electronica

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
                throw new DbOperationException(ex.Message);
            }

            return resultadoAccion;
        }

        public async Task<bool> UpdateAsync(Compuesto unCompuesto)
        {
            bool resultadoAccion = false;

            var compuestoExistente = await GetByGuidAsync(unCompuesto.Uuid);

            if (compuestoExistente.Uuid == Guid.Empty)
                throw new DbOperationException($"No se puede actualizar. No existe  {unCompuesto.Nombre!}.");

            try
            {
                var conexion = contextoDB.CreateConnection();
                string procedimiento = "core.p_actualizar_compuesto";
                var parametros = new
                {
                    p_uuid = unCompuesto.Uuid,
                    p_nombre = unCompuesto.Nombre,
                    p_simbolo = unCompuesto.Simbolo,
                    p_numero_atomico = unCompuesto.Numero_Atomico,
                    p_config_electronica = unCompuesto.Config_Electronica
                };

                var cantidad_filas = await conexion.ExecuteAsync(
                    procedimiento, 
                    parametros, 
                    commandType: CommandType.StoredProcedure);


                if (cantidad_filas != 0)
                    resultadoAccion = true;
            }
            catch (NpgsqlException ex)
            {
                throw new DbOperationException(ex.Message);
            }

            return resultadoAccion;

        }

        public async Task<bool> RemoveAsync(Guid compuestoGuid)
        {
            bool resultadoAccion = false;

            try
            {
                var conexion = contextoDB.CreateConnection();

                string procedimiento = "core.p_eliminar_compuesto";

                var parametros = new 
                { 
                    p_uuid = compuestoGuid 
                };

                var cantidad_filas = await conexion.ExecuteAsync(
                    procedimiento, 
                    parametros, 
                    commandType: CommandType.StoredProcedure);


                if (cantidad_filas != 0)
                    resultadoAccion = true;
            }
            catch (NpgsqlException ex)
            {
                throw new DbOperationException(ex.Message);
            }
            return resultadoAccion;
        }
    }
}

