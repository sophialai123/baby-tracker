namespace BabyTrackerApi.Models;

public class SleepSession : Activity
{
    public SleepSession()
    {
        Type = "sleep";
    }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Quality { get; set; }
}
