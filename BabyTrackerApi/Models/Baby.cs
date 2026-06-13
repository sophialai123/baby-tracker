namespace BabyTrackerApi.Models;

public class Baby
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BirthWeight { get; set; }
    public string? BirthLength { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
