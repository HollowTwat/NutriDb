namespace NutriDbService.PythModels
{
    public class NutriProps
    {
        public NutriProps() { }
        public NutriProps(decimal Fats, decimal Carbs, decimal Protein)
        {
            fats = Fats;
            carbs = Carbs;
            protein = Protein;
        }
        public decimal fats { get; set; }

        public decimal carbs { get; set; }

        public decimal protein { get; set; }
    }
}
