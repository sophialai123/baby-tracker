namespace BabyTrackerApi.Models;

public class Nappy : Activity
{
    public Nappy()
    {
        Type = "nappy";
    }

    public string? ChangeType { get; set; }
    public bool IsWet { get; set; }
    public bool IsDirty { get; set; }
    public string? Consistency { get; set; }
}
