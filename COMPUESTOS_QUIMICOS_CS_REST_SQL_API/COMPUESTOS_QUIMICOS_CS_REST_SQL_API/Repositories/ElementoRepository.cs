using COMPUESTOS_QUIMICOS_CS_REST_SQL_API.DBContexts;
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
                "SELECT elemento_uuid uuid, nombre, formula_quimica, masa_molar, estado_agregacion" +
                "FROM core.elementos ORDER BY nombre";

            var resultadoElementos = await conexion
                .QueryAsync<Elemento>(sentenciaSQL, new DynamicParameters());

            return resultadoElementos.ToList();
        }

        //public async Task<Pais> GetByGuidAsync(Guid pais_guid)
        //{
        //    Pais unPais = new();

        //    var conexion = contextoDB.CreateConnection();

        //    DynamicParameters parametrosSentencia = new();
        //    parametrosSentencia.Add("@pais_guid", pais_guid,
        //                            DbType.Guid, ParameterDirection.Input);

        //    string sentenciaSQL =
        //        "SELECT pais_uuid uuid, nombre, continente " +
        //        "FROM core.paises " +
        //        "WHERE pais_uuid = @pais_guid ";


        //    var resultado = await conexion.QueryAsync<Pais>(sentenciaSQL,
        //        parametrosSentencia);

        //    if (resultado.Any())
        //        unPais = resultado.First();

        //    return unPais;
        //}

        //public async Task<bool> CreateAsync(Pais unPais)
        //{
        //    bool resultadoAccion = false;

        //    try
        //    {
        //        var conexion = contextoDB.CreateConnection();

        //        string procedimiento = "core.p_insertar_pais";

        //        var parametros = new
        //        {
        //            p_nombre = unPais.Nombre,
        //            p_continente = unPais.Continente
        //        };

        //        var cantidadFilas = await conexion
        //            .ExecuteAsync(
        //                procedimiento,
        //                parametros,
        //                commandType: CommandType.StoredProcedure);

        //        if (cantidadFilas != 0)
        //            resultadoAccion = true;
        //    }
        //    catch (NpgsqlException error)
        //    {
        //        throw new DbOperationException(error.Message);
        //    }

        //    return resultadoAccion;
        //}

        //public async Task<bool> UpdateAsync(Pais unPais)
        //{
        //    bool resultadoAccion = false;

        //    var paisExistente = await GetByGuidAsync(unPais.Uuid);

        //    if (paisExistente.Uuid == Guid.Empty)
        //        throw new DbOperationException($"No se puede actualizar. No existe la fruta {unPais.Nombre!}.");

        //    try
        //    {
        //        var conexion = contextoDB.CreateConnection();

        //        string procedimiento = "core.p_actualizar_pais";
        //        var parametros = new
        //        {
        //            p_uuid = unPais.Uuid,
        //            p_nombre = unPais.Nombre,
        //            p_continente = unPais.Continente
        //        };

        //        var cantidad_filas = await conexion.ExecuteAsync(
        //            procedimiento,
        //            parametros,
        //            commandType: CommandType.StoredProcedure);

        //        if (cantidad_filas != 0)
        //            resultadoAccion = true;
        //    }
        //    catch (NpgsqlException error)
        //    {
        //        throw new DbOperationException(error.Message);
        //    }

        //    return resultadoAccion;
        //}

        //public async Task<bool> RemoveAsync(Guid pais_guid)
        //{
        //    bool resultadoAccion = false;

        //    try
        //    {

        //        var conexion = contextoDB.CreateConnection();

        //        string procedimiento = "core.p_eliminar_pais";
        //        var parametros = new
        //        {
        //            p_uuid = pais_guid
        //        };

        //        var cantidad_filas = await conexion.ExecuteAsync(
        //            procedimiento,
        //            parametros,
        //            commandType: CommandType.StoredProcedure);

        //        if (cantidad_filas != 0)
        //            resultadoAccion = true;
        //    }
        //    catch (NpgsqlException error)
        //    {
        //        throw new DbOperationException(error.Message);
        //    }

        //    return resultadoAccion;
        //}
    }
}
