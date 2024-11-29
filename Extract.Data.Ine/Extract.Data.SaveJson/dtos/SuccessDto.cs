namespace Extract.Data.SaveJson.dtos
{
    public class SuccessDto
    {
        public List<SuccessMessageDto> Verdadeiro { get; set; } = [];
        public List<ErrorMessageDto> Falso { get; set; } = [];
    }
}