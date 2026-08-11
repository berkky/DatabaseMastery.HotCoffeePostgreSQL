namespace DatabaseMastery.HotCoffeePostgreSQL.Configuration
{
    public class AdminAuthOptions
    {
        public const string SectionName = "AdminAuth";

        public string? Username { get; set; }

        public string? Password { get; set; }
    }
}
