namespace NutriDbService.PythModels.Response
{
    public class CheckGPTResponse
    {
        public bool IsError { get; set; }

        public bool Done { get; set; }

        public GPTResponse Response { get; set; }
    }
}
