namespace NutriDbService.IntegratorModels
{
    public class LessonStartRequest : BaseRequest
    {
        public override string Status => "A4";
        public string ProfileName { get; set; }
        public int LessonsCurrent { get; set; }
        public int LessonsCompleted { get; set; }
    }
}
