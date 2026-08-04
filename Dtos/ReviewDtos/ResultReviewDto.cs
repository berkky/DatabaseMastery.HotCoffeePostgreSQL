using DatabaseMastery.HotCoffeePostgreSQL.Entities;

namespace DatabaseMastery.HotCoffeePostgreSQL.Dtos.ReviewDtos
{
    public class ResultReviewDto
    {
        public int ReviewId { get; set; }
        public string CustomerName { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }     
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }      
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
