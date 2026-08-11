namespace DatabaseMastery.HotCoffeePostgreSQL.Validation
{
    public static class ValidationLimits
    {
        public const int CategoryNameMax = 200;
        public const int ImagePathMax = 500;
        public const int ProductNameMax = 200;
        public const int DescriptionMax = 4000;
        public const int PersonNameMax = 200;
        public const int PhoneMax = 30;
        public const int EmailMax = 254;
        public const int ReservationDescriptionMax = 2000;
        public const int ReservationStatusMax = 50;
        public const int CommentMax = 4000;
        public const int CustomerNameMax = 200;
        public const int GuestCountMax = 50;
        public const decimal PriceMin = 0.01m;
        public const decimal PriceMax = 999999.99m;
    }
}
