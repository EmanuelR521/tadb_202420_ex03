using System.Text.Json.Serialization;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models
{
    public class Elemento_Por_Compuesto
    {
        [JsonPropertyName("elemento_uuid")]
        public Guid Elemento_Uuid { get; set; }

        [JsonPropertyName("compuesto_uuid")]
        public Guid Compuesto_Uuid { get; set; }

        [JsonPropertyName("cantidad_atomos")]
        public int Cantidad_Atomos { get; set; }
    }
}
