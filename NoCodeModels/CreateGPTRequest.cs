namespace NutriDbService.NoCodeModels
{
    public class CreateGPTRequest
    {
        public long UserTgId { get; set; }

        public string Question { get; set; }

        public string Oldmeal { get; set; }

        public string Type { get; set; }
    }
}
