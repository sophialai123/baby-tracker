namespace BabyTrackerApi.Models;

public class Sleep : Activity
{
    public Sleep()
    {
        Type = "sleep";
    }

    public DateTime? BedTime { get; set; }
    public DateTime? WakeTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Quality { get; set; }
}
