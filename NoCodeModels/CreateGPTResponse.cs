namespace NutriDbService.NoCodeModels
{
    public class CreateGPTResponse
    {
        public CreateGPTResponse()
        {

        }
        public CreateGPTResponse(int requestId)
        {
            RequestId = requestId;
            isError = false;
        }

        public bool isError { get; set; }
        public int RequestId { get; set; }
    }
}
