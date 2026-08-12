using System.Globalization;

namespace DatabaseMastery.HotCoffeePostgreSQL.Domain;

public static class TurkishDatePresentation
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string FormatDate(DateOnly date, string format = "dd MMM yyyy") =>
        date.ToString(format, Turkish);

    public static string FormatDateShort(DateOnly date) =>
        date.ToString("dd MMM", Turkish);

    public static string FormatDateTime(DateTime dateTime, string format = "dd MMM yyyy") =>
        dateTime.ToString(format, Turkish);

    public static string FormatDayName(DateOnly date) =>
        date.ToString("dddd", Turkish);
}
