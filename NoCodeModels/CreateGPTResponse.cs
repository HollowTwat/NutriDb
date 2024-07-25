namespace NutriDbService.NoCodeModels
{
    public class CreateGPTResponse
    {
        public CreateGPTResponse(int requestId)
        {
            RequestId = requestId;
        }

        public int RequestId { get; set; }
    }
}
