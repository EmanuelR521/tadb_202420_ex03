using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.DBContexts;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Exceptions;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Interfaces;
using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models;
using Dapper;
using Npgsql;
using System.Data;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Repositories
{
    public class ElementoRepository(PgsqlDbContext unContexto) : IElementoRepository
    {
        private readonly PgsqlDbContext contextoDB = unContexto;

        public async Task<List<Elemento>> GetAllAsync()
        {
            var conexion = contextoDB.CreateConnection();

            string sentenciaSQL =
                "SELECT elemento_uuid uuid, nombre, simbolo, numero_atomico, config_electronica" +
                "FROM core.elementos ORDER BY nombre";

            var resultadoElementos = await conexion
                .QueryAsync<Elemento>(sentenciaSQL, new DynamicParameters());

            return resultadoElementos.ToList();
        }

        public async Task<Elemento> GetByGuidAsync(Guid elemento_guid)
        {
            Elemento unElemento = new();

            var conexion = contextoDB.CreateConnection();

            DynamicParameters parametrosSentencia = new();
            parametrosSentencia.Add("@elemento_guid", elemento_guid,
                                    DbType.Guid, ParameterDirection.Input);

            string sentenciaSQL =
                "SELECT elemento_uuid uuid, nombre, simbolo, numero_atomico, config_electronica" +
                "FROM core.elementos " +
                "WHERE elemento_uuid = @elemento_guid ";


            var resultado = await conexion.QueryAsync<Elemento>(sentenciaSQL,
                parametrosSentencia);

            if (resultado.Any())
                unElemento = resultado.First();

            return unElemento;
        }

        public async Task<string> GetElementoByNameAsync(string elemento_nombre)
        {
            string nombreElemento = string.Empty;

            var conexion = contextoDB.CreateConnection();

            DynamicParameters parametrosSentencia = new();
            parametrosSentencia.Add("@elemento_nombre", elemento_nombre,
                                    DbType.String, ParameterDirection.Input);

            string sentenciaSQL =
                "SELECT distinct elementos FROM core.v_info_elementos WHERE LOWER(elemento) = LOWER(@elemento_nombre)";

            var resultado = await conexion.QueryAsync<string>(sentenciaSQL,
                parametrosSentencia);

            if (resultado.Any())
                nombreElemento = resultado.First();

            return nombreElemento;
        }


        public async Task<bool> CreateAsync(Elemento unElemento)
        {
            bool resultadoAccion = false;

            try
            {
                var conexion = contextoDB.CreateConnection();

                string procedimiento = "core.p_insertar_elemento";

                var parametros = new
                {
                    p_nombre = unElemento.Nombre,
                    p_simbolo = unElemento.Simbolo,
                    p_numero_atomico = unElemento.Numero_Atomico,
                    p_config_electronica = unElemento.Config_Electronica,

                };

                var cantidadFilas = await conexion
                    .ExecuteAsync(
                        procedimiento,
                        parametros,
                        commandType: CommandType.StoredProcedure);

                if (cantidadFilas != 0)
                    resultadoAccion = true;
            }
            catch (NpgsqlException error)
            {
                throw new DbOperationException(error.Message);
            }

            return resultadoAccion;
        }

        public async Task<bool> UpdateAsync(Elemento unElemento)
        {
            bool resultadoAccion = false;

            var elementoExistente = await GetByGuidAsync(unElemento.Uuid);

            if (elementoExistente.Uuid == Guid.Empty)
                throw new DbOperationException($"No se puede actualizar. No existe el elemento {unElemento.Nombre!}.");

            try
            {
                var conexion = contextoDB.CreateConnection();

                string procedimiento = "core.p_actualizar_elemento";
                var parametros = new
                {
                    p_uuid = unElemento.Uuid,
                    p_nombre = unElemento.Nombre,
                    p_simbolo = unElemento.Simbolo,
                    p_numero_atomico = unElemento.Numero_Atomico,
                    p_config_electronica = unElemento.Config_Electronica,
                };

                var cantidad_filas = await conexion.ExecuteAsync(
                    procedimiento,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                if (cantidad_filas != 0)
                    resultadoAccion = true;
            }
            catch (NpgsqlException error)
            {
                throw new DbOperationException(error.Message);
            }

            return resultadoAccion;
        }

        public async Task<bool> RemoveAsync(Guid elemento_guid)
        {
            bool resultadoAccion = false;

            try
            {

                var conexion = contextoDB.CreateConnection();

                string procedimiento = "core.p_eliminar_elemento";
                var parametros = new
                {
                    p_uuid = elemento_guid
                };

                var cantidad_filas = await conexion.ExecuteAsync(
                    procedimiento,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                if (cantidad_filas != 0)
                    resultadoAccion = true;
            }
            catch (NpgsqlException error)
            {
                throw new DbOperationException(error.Message);
            }

            return resultadoAccion;
        }
    }
}
