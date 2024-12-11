using System.Text.Json;

namespace Extract.Data.SaveJson.dtos
{
    public class DataDto
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        public string? geocod { get; set; } = "";
        public string? geodsg { get; set; } = "";
        public string? dim_3 { get; set; } = "";
        public string? dim_3_t { get; set; } = "";
        public string? dim_4 { get; set; } = "";
        public string? dim_4_t { get; set; } = "";
        public string? ind_string { get; set; } = "";
        public string? valor { get; set; } = "";

        public string? sinal_conv { get; set; } = "";

        public string? sinal_conv_desc { get; set; } = "";

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions);
        }
    }
}