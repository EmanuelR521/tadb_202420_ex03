using System.Text.Json.Serialization;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models
{
    public class Elemento
    {
        [JsonPropertyName("id_uuid")]
        public Guid Uuid { get; set; } = Guid.Empty;

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; } = string.Empty;

        [JsonPropertyName("simbolo")]
        public string? Simbolo { get; set; } = string.Empty;

        [JsonPropertyName("numero_atomico")]
        public int Numero_Atomico { get; set; } = 0;

        [JsonPropertyName("config_electronica")]
        public string? Config_Electronica { get; set; } = string.Empty;

    }
}
