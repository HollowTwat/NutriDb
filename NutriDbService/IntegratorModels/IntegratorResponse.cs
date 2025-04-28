namespace NutriDbService.IntegratorModels
{
    public class IntegratorResponse
    {
        public bool Status { get; set; }
        public ResponseData Response { get; set; }
    }
    public class ResponseData
    {
        public long Time { get; set; }
    }
}
