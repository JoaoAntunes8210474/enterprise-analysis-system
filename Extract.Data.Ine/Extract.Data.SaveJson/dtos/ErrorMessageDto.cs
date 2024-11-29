namespace Extract.Data.SaveJson.dtos
{
    public class ErrorMessageDto
    {
        public string IndicadorCod { get; set; } = "";
        public string Lingua { get; set; } = "";
        public DateTime DataExtracao { get; set; } = DateTime.Now;
        public string Msg { get; set; } = "";
        public string Cod { get; set; } = "";
    }
}