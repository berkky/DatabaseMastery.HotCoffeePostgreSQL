namespace DatabaseMastery.HotCoffeePostgreSQL.Services;

/// <summary>
/// Npgsql 10 rejects binary comparisons between timestamptz columns and
/// DateTime values with Kind=Unspecified (e.g. DateTime.Today / UtcNow.Date).
/// This helper preserves existing calendar-day intent without schema changes.
/// </summary>
internal static class NpgsqlDateTimeCompatibility
{
    public static DateTime AsUtcCalendarDate(DateTime value) =>
        DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
