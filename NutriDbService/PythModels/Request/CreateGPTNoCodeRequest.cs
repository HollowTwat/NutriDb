﻿namespace NutriDbService.PythModels.Request
{
    public class CreateGPTNoCodeRequest
    {
        public long UserTgId { get; set; }

        public string Question { get; set; }

        public string Oldmeal { get; set; }

        public string Type { get; set; }

        public string AssistantType { get; set; }

        public string OutputType { get; set; }
    }
}