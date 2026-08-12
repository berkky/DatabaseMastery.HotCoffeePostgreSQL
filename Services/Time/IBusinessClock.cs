namespace DatabaseMastery.HotCoffeePostgreSQL.Services.Time
{
    public interface IBusinessClock
    {
        DateOnly Today { get; }
        DateTimeOffset UtcNow { get; }
    }
}
