namespace NutriDbService.IntegratorModels
{
    public class LessonEndRequest : BaseRequest
    {
        public override string Status => "A5";
        public string ProfileName { get; set; }
        public int LessonsCurrent { get; set; }
        public int LessonsCompleted { get; set; }
    }
}
