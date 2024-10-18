namespace NutriDbService.PythModels.Response
{
    public class GPTAnswerResponse
    {
        public bool IsError { get; set; }

        public GPTResponse Answer { get; set; }

    }
}
