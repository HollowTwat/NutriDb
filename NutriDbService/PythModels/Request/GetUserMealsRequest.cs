using System.ComponentModel.DataAnnotations;

namespace NutriDbService.PythModels.Request
{
    public class GetUserMealsRequest
    {
        [Required]
        public long userTgId { get; set; }
        public int? day { get; set; }
        public string dayStr { get; set; }
        public mealtype? typemeal { get; set; }
        [Required]
        public Periods period { get; set; }
    }
}
