using System.ComponentModel;

namespace NutriDbService.PythModels
{
    public enum mealtype
    {
        [Description("Завтрак")]
        breakfast = 0,
        [Description("Полдник")]
        afternoon = 1,
        [Description("Обед")]
        dinner = 2,
        [Description("Вечерний перекус")]
        evening = 3,
        [Description("Ужин")]
        supper = 4,
        [Description("Перекус")]
        nightsnack = 5
    }

}
