namespace DatabaseMastery.HotCoffeePostgreSQL.Services.Time
{
    public sealed class BusinessClock : IBusinessClock
    {
        private readonly TimeProvider _timeProvider;

        public BusinessClock(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();

        public DateOnly Today
        {
            get
            {
                var local = TimeZoneInfo.ConvertTime(UtcNow, BusinessTimeZones.Restaurant);
                return DateOnly.FromDateTime(local.DateTime);
            }
        }
    }
}
