namespace NutriDbService.IntegratorModels
{
    public class BotStartRequest : BaseRequest
    {
        public string start_text { get; set; }

        public override string Status => "A0";
    }
}
