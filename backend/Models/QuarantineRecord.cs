
using System.Text.Json.Serialization;

namespace Commerce.Api.Models;

public class QuarantineRecord
{
    public int Id { get; set; }

    [JsonPropertyName("pc_processdate")]
    public string? ProcessDate { get; set; }

    [JsonPropertyName("pc_nomcomred")]
    public string? Nombre { get; set; }

    [JsonPropertyName("pc_numdoc")]
    public string? Documento { get; set; }

    [JsonPropertyName("motivo")]
    public string? Motivo { get; set; }
}
