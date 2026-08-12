namespace DatabaseMastery.HotCoffeePostgreSQL.Configuration;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int AdminLoginPermitLimit { get; set; } = 10;

    public int AdminLoginWindowMinutes { get; set; } = 5;

    public int PublicReservationPermitLimit { get; set; } = 20;

    public int PublicReservationWindowMinutes { get; set; } = 10;
}
