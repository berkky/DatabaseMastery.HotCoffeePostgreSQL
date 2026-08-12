using DatabaseMastery.HotCoffeePostgreSQL.Domain;

namespace DatabaseMastery.HotCoffeePostgreSQL.Entities
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateOnly ReservationDate { get; set; }
        public TimeOnly ReservationTime { get; set; }
        public int GuestCount { get; set; }
        public ReservationStatus Status { get; set; }
        public string? Description { get; set; }
    }
}
