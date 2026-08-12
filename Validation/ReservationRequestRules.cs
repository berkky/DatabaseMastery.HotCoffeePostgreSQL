using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatabaseMastery.HotCoffeePostgreSQL.Validation
{
    public static class ReservationRequestRules
    {
        /// <summary>
        /// Rejects reservation dates before today's restaurant business calendar date (Europe/Istanbul).
        /// </summary>
        public static void ValidateDateNotInPast(
            ModelStateDictionary modelState,
            DateOnly reservationDate,
            DateOnly businessToday,
            string fieldName = "ReservationDate")
        {
            if (reservationDate < businessToday)
            {
                modelState.AddModelError(
                    fieldName,
                    "Rezervasyon tarihi geçmiş bir gün olamaz.");
            }
        }
    }
}
