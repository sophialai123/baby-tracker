namespace BabyTrackerApi.Models;

public class Feeding : Activity
{
    public Feeding()
    {
        Type = "feeding";
    }

    public string? FeedingType { get; set; }
    public decimal? Amount { get; set; }
    public string? AmountUnit { get; set; } = "ml";
    public int? DurationMinutes { get; set; }
    public string? Side { get; set; }
}
