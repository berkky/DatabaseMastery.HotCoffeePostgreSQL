using DatabaseMastery.HotCoffeePostgreSQL.Domain;

namespace DatabaseMastery.HotCoffeePostgreSQL.Domain
{
    public static class ReservationStatusPresentation
    {
        public static string GetDisplayName(ReservationStatus status) => status switch
        {
            ReservationStatus.Pending => "Beklemede",
            ReservationStatus.Confirmed => "Onaylandı",
            ReservationStatus.Cancelled => "İptal Edildi",
            _ => status.ToString()
        };

        public static string GetBadgeCssClass(ReservationStatus status) => status switch
        {
            ReservationStatus.Pending => "sb-yellow",
            ReservationStatus.Confirmed => "sb-green",
            ReservationStatus.Cancelled => "sb-red",
            _ => "sb-yellow"
        };
    }
}
