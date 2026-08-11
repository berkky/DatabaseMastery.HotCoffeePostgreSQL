using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatabaseMastery.HotCoffeePostgreSQL.Validation
{
    public static class ReservationRequestRules
    {
        /// <summary>
        /// Rejects reservation dates before today's local calendar date (Turkey business context).
        /// </summary>
        public static void ValidateDateNotInPast(
            ModelStateDictionary modelState,
            DateTime reservationDate,
            string fieldName = "ReservationDate")
        {
            if (reservationDate.Date < DateTime.Today)
            {
                modelState.AddModelError(
                    fieldName,
                    "Rezervasyon tarihi geçmiş bir gün olamaz.");
            }
        }
    }
}
