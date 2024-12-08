namespace Extract.Data.SaveJson.dtos
{
    public class ResponseDto
    {
        public string? IndicadorCod { get; set; } = "";
        public string? IndicadorDsg { get; set; } = "";
        public string? MetaInfUrl { get; set; } = "";
        public DateTime? DataExtracao { get; set; } = DateTime.Now;
        public DateTime? DataUltimoAtualizacao { get; set; } = null;
        public string? UltimoPref { get; set; } = "";
        public Dictionary<string, List<DataDto>>? Dados { get; set; } = [];

        public SuccessDto Sucesso { get; set; } = new SuccessDto();
    }
}