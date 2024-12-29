using System.Text.Json;

namespace Extract.Data.SaveJson.dtos
{
    public class DataDto
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public string? NumberOfCompanies { get; set; } = null;
        public string EconomicActivityCode { get; set; } = null!;
        public string EconomicActivityDescription { get; set; } = null!;
        public string GeographicAreaCode { get; set; } = null!;
        public string GeographicAreaDescription { get; set; } = null!;
        public string? LegalFormCode { get; set; } = null;
        public string? LegalFormDescription { get; set; } = null;
        public string? ConvSignal { get; set; } = null;
        public string? ConvSignalDescription { get; set; } = null;
        public string? NumberOfVolumeOfBusinessForCompanies { get; set; } = null;
        public string? NumberOfPeopleWorkingForCompanies { get; set; } = null;
        public string? IncreasedValueForCompanies { get; set; } = null;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions);
        }
    }
}