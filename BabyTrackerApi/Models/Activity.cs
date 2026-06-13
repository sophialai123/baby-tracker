using System.Text.Json.Serialization;
using BabyTrackerApi.Services;

namespace BabyTrackerApi.Models;

[JsonConverter(typeof(ActivityConverter))]
public abstract class Activity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BabyId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
