namespace NutriDbService.PythModels.Request
{
    public class CreateGPTNoCodeRequest
    {
        public long UserTgId { get; set; }

        public string Question { get; set; }

        public string Extra { get; set; }

        public string Type { get; set; }

        public string AssistantType { get; set; }

        public string OutputType { get; set; }
        public string DeleteThread { get; set; }
    }
}
