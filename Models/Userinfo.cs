namespace NutriDbService.Models;

public partial class Userinfo
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public short? Age { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public float? Weight { get; set; }

    public float? Height { get; set; }

    public string Gender { get; set; }

    public virtual User User { get; set; }
}
