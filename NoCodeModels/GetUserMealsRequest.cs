namespace NutriDbService.NoCodeModels
{
    public class GetUserMealsRequest
    {
        public long userTgId { get; set; }
        public int? day { get; set; }
        public string dayStr { get; set; }
        public mealtype? typemeal { get; set; }
    }
}
