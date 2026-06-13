namespace BabyTrackerApi.Models;

public class DailySleepSummary
{
    public DateTime Date { get; set; }
    public int TotalMinutes { get; set; }
    public decimal TotalHours { get; set; }
    public int SessionCount { get; set; }
    public List<SleepSession> Sessions { get; set; } = new();
}
