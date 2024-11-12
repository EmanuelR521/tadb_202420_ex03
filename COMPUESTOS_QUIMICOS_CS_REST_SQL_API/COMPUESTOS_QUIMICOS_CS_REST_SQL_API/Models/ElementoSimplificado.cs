using System.Text.Json.Serialization;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models
{
    public class ElementoSimplificado
    {
        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; } = Guid.Empty;

        [JsonPropertyName("cantidad_atomos")]
        public int Cantidad_Atomos { get; set; } = 0;
    }
}
