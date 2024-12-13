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

        public string FileName { get; set; }
        public string NumberOfCompanies { get; set; }
        public string EconomicActivityCode { get; set; }
        public string EconomicActivityDescription { get; set; }
        public string GeographicAreaCode { get; set; }
        public string GeographicAreaDescription { get; set; }
        public string LegalFormCode { get; set; }
        public string LegalFormDescription { get; set; }
        public string ConvSignal { get; set; }
        public string ConvSignalDescription { get; set; }
        public string NumberOfVolumeOfBusinessForCompanies { get; set; }
        public string NumberOfPeopleWorkingForCompanies { get; set; }
        public string IncreasedValueForCompanies { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions);
        }
    }
}