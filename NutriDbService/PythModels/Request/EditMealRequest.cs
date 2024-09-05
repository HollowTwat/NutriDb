using System;

namespace NutriDbService.PythModels.Request
{
    public class EditMealRequest : CreateMealRequest
    {
        public int? mealId { get; set; }
    }
}
