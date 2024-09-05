namespace NutriDbService.PythModels
{
    public class PythFood
    {
        public string description { get; set; }
        public decimal weight { get; set; }

        public NutriProps nutritional_value { get; set; }

    }
}
