using System.Text.Json;

namespace Extract.Data.SaveJson.models
{
    public class CompanyData
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        public string? NumberOfCompanies { get; set; } = null!;

        public string? EconomicActivityCode { get; set; } = null!;

        public string? EconomicActivityDescription { get; set; } = null!;

        public string? GeographicAreaCode { get; set; } = null!;

        public string? GeographicAreaDescription { get; set; } = null!;

        public string? LegalFormCode { get; set; } = null!;

        public string? LegalFormDescription { get; set; } = null!;

        public string? ConvSignal { get; set; } = null!;

        public string? ConvSignalDescription { get; set; } = null!;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions);
        }
    }
}