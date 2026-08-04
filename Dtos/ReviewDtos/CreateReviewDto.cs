namespace DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos
{
    public class CreateReviewDto
    {
        public string CustomerName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }
        public int ProductId { get; set; }
    }
}
