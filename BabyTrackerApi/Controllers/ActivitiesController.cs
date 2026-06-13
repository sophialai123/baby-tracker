using Microsoft.AspNetCore.Mvc;
using BabyTrackerApi.Models;
using BabyTrackerApi.Services;

namespace BabyTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly DataStore _dataStore;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(DataStore dataStore, ILogger<ActivitiesController> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Activity>>> GetActivities([FromQuery] string babyId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(babyId))
                return BadRequest("Baby ID is required");

            var activities = await _dataStore.GetActivitiesAsync(babyId);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for baby {BabyId}", babyId);
            return StatusCode(500, "Error retrieving activities");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> GetActivity(string id)
    {
        try
        {
            var activity = await _dataStore.GetActivityAsync(id);
            if (activity == null)
                return NotFound();

            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity {ActivityId}", id);
            return StatusCode(500, "Error retrieving activity");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Activity>> CreateActivity([FromBody] Dictionary<string, object> activityData)
    {
        try
        {
            if (!activityData.ContainsKey("babyId") || string.IsNullOrWhiteSpace(activityData["babyId"].ToString()))
                return BadRequest("Baby ID is required");

            if (!activityData.ContainsKey("type") || string.IsNullOrWhiteSpace(activityData["type"].ToString()))
                return BadRequest("Activity type is required");

            var babyId = activityData["babyId"].ToString();
            var type = activityData["type"].ToString()?.ToLower();

            var baby = await _dataStore.GetBabyAsync(babyId!);
            if (baby == null)
                return BadRequest("Baby not found");

            Activity activity = type switch
            {
                "sleep" => CreateSleepSessionActivity(activityData),
                "growth" => CreateGrowthActivity(activityData),
                "nappy" => CreateNappyActivity(activityData),
                "feeding" => CreateFeedingActivity(activityData),
                _ => throw new InvalidOperationException($"Unknown activity type: {type}")
            };

            activity.BabyId = babyId!;

            if (activityData.ContainsKey("timestamp") && DateTime.TryParse(activityData["timestamp"].ToString(), out var timestamp))
                activity.Timestamp = timestamp;

            if (activityData.ContainsKey("notes"))
                activity.Notes = activityData["notes"].ToString();

            var createdActivity = await _dataStore.AddActivityAsync(activity);
            return CreatedAtAction(nameof(GetActivity), new { id = createdActivity.Id }, createdActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating activity");
            return StatusCode(500, "Error creating activity");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateActivity(string id, [FromBody] Dictionary<string, object> activityData)
    {
        try
        {
            var existingActivity = await _dataStore.GetActivityAsync(id);
            if (existingActivity == null)
                return NotFound();

            var type = existingActivity.Type.ToLower();

            Activity updatedActivity = type switch
            {
                "sleep" => UpdateSleepSessionActivity((SleepSession)existingActivity, activityData),
                "growth" => UpdateGrowthActivity((Growth)existingActivity, activityData),
                "nappy" => UpdateNappyActivity((Nappy)existingActivity, activityData),
                "feeding" => UpdateFeedingActivity((Feeding)existingActivity, activityData),
                _ => throw new InvalidOperationException($"Unknown activity type: {type}")
            };

            if (activityData.ContainsKey("timestamp") && DateTime.TryParse(activityData["timestamp"].ToString(), out var timestamp))
                updatedActivity.Timestamp = timestamp;

            if (activityData.ContainsKey("notes"))
                updatedActivity.Notes = activityData["notes"].ToString();

            var success = await _dataStore.UpdateActivityAsync(id, updatedActivity);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity {ActivityId}", id);
            return StatusCode(500, "Error updating activity");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(string id)
    {
        try
        {
            var success = await _dataStore.DeleteActivityAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity {ActivityId}", id);
            return StatusCode(500, "Error deleting activity");
        }
    }

    [HttpGet("sleep-summary")]
    public async Task<ActionResult<DailySleepSummary>> GetDailySleepSummary([FromQuery] string babyId, [FromQuery] DateTime date)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(babyId))
                return BadRequest("Baby ID is required");

            var activities = await _dataStore.GetActivitiesAsync(babyId);
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var sleepActivities = activities
                .Where(a => a.Type == "sleep" && a.Timestamp >= startOfDay && a.Timestamp < endOfDay)
                .OfType<SleepSession>()
                .ToList();

            var totalMinutes = sleepActivities
                .Where(s => s.DurationMinutes.HasValue && s.DurationMinutes > 0)
                .Sum(s => s.DurationMinutes ?? 0);

            var summary = new DailySleepSummary
            {
                Date = startOfDay,
                TotalMinutes = totalMinutes,
                TotalHours = decimal.Divide(totalMinutes, 60),
                SessionCount = sleepActivities.Count,
                Sessions = sleepActivities
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating daily sleep summary for baby {BabyId}", babyId);
            return StatusCode(500, "Error calculating sleep summary");
        }
    }

    private SleepSession CreateSleepSessionActivity(Dictionary<string, object> data)
    {
        var sleep = new SleepSession();
        if (data.ContainsKey("startTime") && DateTime.TryParse(data["startTime"].ToString(), out var startTime))
            sleep.StartTime = startTime;
        if (data.ContainsKey("endTime") && DateTime.TryParse(data["endTime"].ToString(), out var endTime))
            sleep.EndTime = endTime;
        if (data.ContainsKey("durationMinutes") && int.TryParse(data["durationMinutes"].ToString(), out var duration))
            sleep.DurationMinutes = duration;
        if (data.ContainsKey("quality"))
            sleep.Quality = data["quality"].ToString();
        return sleep;
    }

    private SleepSession UpdateSleepSessionActivity(SleepSession sleep, Dictionary<string, object> data)
    {
        if (data.ContainsKey("startTime") && DateTime.TryParse(data["startTime"].ToString(), out var startTime))
            sleep.StartTime = startTime;
        if (data.ContainsKey("endTime") && DateTime.TryParse(data["endTime"].ToString(), out var endTime))
            sleep.EndTime = endTime;
        if (data.ContainsKey("durationMinutes") && int.TryParse(data["durationMinutes"].ToString(), out var duration))
            sleep.DurationMinutes = duration;
        if (data.ContainsKey("quality"))
            sleep.Quality = data["quality"].ToString();
        return sleep;
    }

    private Growth CreateGrowthActivity(Dictionary<string, object> data)
    {
        var growth = new Growth();
        if (data.ContainsKey("weight") && decimal.TryParse(data["weight"].ToString(), out var weight))
            growth.Weight = weight;
        if (data.ContainsKey("length") && decimal.TryParse(data["length"].ToString(), out var length))
            growth.Length = length;
        if (data.ContainsKey("headCircumference") && decimal.TryParse(data["headCircumference"].ToString(), out var headCirc))
            growth.HeadCircumference = headCirc;
        if (data.ContainsKey("weightUnit"))
            growth.WeightUnit = data["weightUnit"].ToString();
        if (data.ContainsKey("lengthUnit"))
            growth.LengthUnit = data["lengthUnit"].ToString();
        return growth;
    }

    private Growth UpdateGrowthActivity(Growth growth, Dictionary<string, object> data)
    {
        if (data.ContainsKey("weight") && decimal.TryParse(data["weight"].ToString(), out var weight))
            growth.Weight = weight;
        if (data.ContainsKey("length") && decimal.TryParse(data["length"].ToString(), out var length))
            growth.Length = length;
        if (data.ContainsKey("headCircumference") && decimal.TryParse(data["headCircumference"].ToString(), out var headCirc))
            growth.HeadCircumference = headCirc;
        if (data.ContainsKey("weightUnit"))
            growth.WeightUnit = data["weightUnit"].ToString();
        if (data.ContainsKey("lengthUnit"))
            growth.LengthUnit = data["lengthUnit"].ToString();
        return growth;
    }

    private Nappy CreateNappyActivity(Dictionary<string, object> data)
    {
        var nappy = new Nappy();
        if (data.ContainsKey("changeType"))
            nappy.ChangeType = data["changeType"].ToString();
        if (data.ContainsKey("isWet") && bool.TryParse(data["isWet"].ToString(), out var isWet))
            nappy.IsWet = isWet;
        if (data.ContainsKey("isDirty") && bool.TryParse(data["isDirty"].ToString(), out var isDirty))
            nappy.IsDirty = isDirty;
        if (data.ContainsKey("consistency"))
            nappy.Consistency = data["consistency"].ToString();
        return nappy;
    }

    private Nappy UpdateNappyActivity(Nappy nappy, Dictionary<string, object> data)
    {
        if (data.ContainsKey("changeType"))
            nappy.ChangeType = data["changeType"].ToString();
        if (data.ContainsKey("isWet") && bool.TryParse(data["isWet"].ToString(), out var isWet))
            nappy.IsWet = isWet;
        if (data.ContainsKey("isDirty") && bool.TryParse(data["isDirty"].ToString(), out var isDirty))
            nappy.IsDirty = isDirty;
        if (data.ContainsKey("consistency"))
            nappy.Consistency = data["consistency"].ToString();
        return nappy;
    }

    private Feeding CreateFeedingActivity(Dictionary<string, object> data)
    {
        var feeding = new Feeding();
        if (data.ContainsKey("feedingType"))
            feeding.FeedingType = data["feedingType"].ToString();
        if (data.ContainsKey("amount") && decimal.TryParse(data["amount"].ToString(), out var amount))
            feeding.Amount = amount;
        if (data.ContainsKey("amountUnit"))
            feeding.AmountUnit = data["amountUnit"].ToString();
        if (data.ContainsKey("durationMinutes") && int.TryParse(data["durationMinutes"].ToString(), out var duration))
            feeding.DurationMinutes = duration;
        if (data.ContainsKey("side"))
            feeding.Side = data["side"].ToString();
        return feeding;
    }

    private Feeding UpdateFeedingActivity(Feeding feeding, Dictionary<string, object> data)
    {
        if (data.ContainsKey("feedingType"))
            feeding.FeedingType = data["feedingType"].ToString();
        if (data.ContainsKey("amount") && decimal.TryParse(data["amount"].ToString(), out var amount))
            feeding.Amount = amount;
        if (data.ContainsKey("amountUnit"))
            feeding.AmountUnit = data["amountUnit"].ToString();
        if (data.ContainsKey("durationMinutes") && int.TryParse(data["durationMinutes"].ToString(), out var duration))
            feeding.DurationMinutes = duration;
        if (data.ContainsKey("side"))
            feeding.Side = data["side"].ToString();
        return feeding;
    }
}
