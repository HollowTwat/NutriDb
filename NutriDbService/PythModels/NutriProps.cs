namespace NutriDbService.PythModels
{
    public class NutriProps
    {
        public NutriProps() { }
        public NutriProps(decimal Fats, decimal Carbs, decimal Protein, decimal Kcal)
        {
            fats = Fats;
            carbs = Carbs;
            protein = Protein;
            kcal = Kcal;
        }
        public decimal fats { get; set; }

        public decimal carbs { get; set; }

        public decimal protein { get; set; }

        public decimal kcal { get; set; }
    }
}
