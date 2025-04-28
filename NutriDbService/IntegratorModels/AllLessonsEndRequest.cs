namespace NutriDbService.IntegratorModels
{
    public class AllLessonsEndRequest : BaseRequest
    {
        public override string Status => "A6";
        public string ProfileName { get; set; }
        public int LessonsCompleted { get; set; }
    }
}
