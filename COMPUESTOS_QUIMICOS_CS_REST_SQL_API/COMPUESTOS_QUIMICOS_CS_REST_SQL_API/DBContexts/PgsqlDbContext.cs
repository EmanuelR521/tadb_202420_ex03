using Npgsql;
using System.Data;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.DBContexts
{
    public class PgsqlDbContext
    {
        private readonly string cadenaConexion;

        public PgsqlDbContext(IConfiguration unaConfiguracion)
        {
            cadenaConexion = unaConfiguracion.GetConnectionString("CompuestoQuimico")!;
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(cadenaConexion);
        }
    }
}