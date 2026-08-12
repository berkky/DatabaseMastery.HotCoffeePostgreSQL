namespace DatabaseMastery.HotCoffeePostgreSQL.Configuration
{
    public sealed class RestaurantBrandOptions
    {
        public const string SectionName = "RestaurantBrand";

        public string BrandName { get; set; } = "HotCoffee";

        public string ShortDescription { get; set; } =
            "Özenle hazırlanan kahve ve lezzetlerle sakin bir gastronomi deneyimi.";
    }
}
