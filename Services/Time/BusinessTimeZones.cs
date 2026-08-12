namespace DatabaseMastery.HotCoffeePostgreSQL.Services.Time
{
    public static class BusinessTimeZones
    {
        public const string RestaurantTimeZoneId = "Europe/Istanbul";

        public static TimeZoneInfo Restaurant { get; } =
            TimeZoneInfo.FindSystemTimeZoneById(RestaurantTimeZoneId);
    }
}
