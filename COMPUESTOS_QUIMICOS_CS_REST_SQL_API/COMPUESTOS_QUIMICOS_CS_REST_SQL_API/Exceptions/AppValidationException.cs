using System;

/*
AppValidationException:
Excepcion creada para enviar mensajes relacionados 
con la validación de datos en las capas de servicio
*/

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Exceptions
{
    public class AppValidationException : Exception
    {
        public AppValidationException(string message) : base(message)
        {
        }
    }
}
