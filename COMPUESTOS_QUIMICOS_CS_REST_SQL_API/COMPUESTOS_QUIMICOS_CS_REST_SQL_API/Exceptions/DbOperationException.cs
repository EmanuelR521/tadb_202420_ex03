/*
DbOperationException:
Excepcion creada para enviar mensajes relacionados 
con la ejecución de operaciones CRUD en la base de datos
*/
using System;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Exceptions
{
    public class DbOperationException : Exception
    {
        public DbOperationException(string message) : base(message)
        {
        }
    }
}
