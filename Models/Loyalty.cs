namespace NutriDbService.Models;

public partial class Loyalty
{
    public int Id { get; set; }

    public decimal? Balance { get; set; }

    public short? TargetCount { get; set; }

    public short? CurrentCount { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; }
}
