namespace BabyTrackerApi.Models;

public class Growth : Activity
{
    public Growth()
    {
        Type = "growth";
    }

    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? HeadCircumference { get; set; }
    public string? WeightUnit { get; set; } = "kg";
    public string? LengthUnit { get; set; } = "cm";
}
