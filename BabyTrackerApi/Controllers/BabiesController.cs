using Microsoft.AspNetCore.Mvc;
using BabyTrackerApi.Models;
using BabyTrackerApi.Services;

namespace BabyTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BabiesController : ControllerBase
{
    private readonly DataStore _dataStore;
    private readonly ILogger<BabiesController> _logger;

    public BabiesController(DataStore dataStore, ILogger<BabiesController> logger)
    {
        _dataStore = dataStore;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Baby>>> GetBabies()
    {
        try
        {
            var babies = await _dataStore.GetBabiesAsync();
            return Ok(babies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving babies");
            return StatusCode(500, "Error retrieving babies");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Baby>> GetBaby(string id)
    {
        try
        {
            var baby = await _dataStore.GetBabyAsync(id);
            if (baby == null)
                return NotFound();

            return Ok(baby);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving baby {BabyId}", id);
            return StatusCode(500, "Error retrieving baby");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Baby>> CreateBaby(Baby baby)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(baby.Name))
                return BadRequest("Baby name is required");

            if (baby.DateOfBirth == default)
                return BadRequest("Date of birth is required");

            if (baby.DateOfBirth > DateTime.UtcNow)
                return BadRequest("Date of birth cannot be in the future");

            var createdBaby = await _dataStore.AddBabyAsync(baby);
            return CreatedAtAction(nameof(GetBaby), new { id = createdBaby.Id }, createdBaby);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating baby");
            return StatusCode(500, "Error creating baby");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBaby(string id, Baby baby)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(baby.Name))
                return BadRequest("Baby name is required");

            if (baby.DateOfBirth > DateTime.UtcNow)
                return BadRequest("Date of birth cannot be in the future");

            var success = await _dataStore.UpdateBabyAsync(id, baby);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating baby {BabyId}", id);
            return StatusCode(500, "Error updating baby");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBaby(string id)
    {
        try
        {
            var success = await _dataStore.DeleteBabyAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting baby {BabyId}", id);
            return StatusCode(500, "Error deleting baby");
        }
    }
}
