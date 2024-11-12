using System.Text.Json.Serialization;

namespace COMPUESTOS_QUIMICOS_CS_REST_SQL_API.Models
{
    public class CompuestoSimplificado
    {
        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; } = Guid.Empty;

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; } = string.Empty;

        [JsonPropertyName("formula_quimica")]
        public string? Formula_Quimica { get; set; } = string.Empty;

        [JsonPropertyName("masa_molar")]
        public decimal Masa_Molar { get; set; } = 0;

        [JsonPropertyName("estado_agregacion")]
        public string? Estado_Agregacion { get; set; } = string.Empty;
    }
}
