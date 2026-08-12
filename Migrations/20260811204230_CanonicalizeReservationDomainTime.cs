using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseMastery.HotCoffeePostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class CanonicalizeReservationDomainTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deterministic legacy status normalization (exact spellings only).
            // PostgreSQL U& escapes keep Turkish letters ASCII-safe in source.
            migrationBuilder.Sql(
                "UPDATE \"Reservations\" SET \"Status\" = 'Pending' " +
                "WHERE \"Status\" IN ('Beklemede', 'Pending');");

            migrationBuilder.Sql(
                "UPDATE \"Reservations\" SET \"Status\" = 'Confirmed' " +
                "WHERE \"Status\" IN ('Confirmed', 'Onaylandi') " +
                "OR \"Status\" = U&'Onayland\\0131';");

            migrationBuilder.Sql(
                "UPDATE \"Reservations\" SET \"Status\" = 'Cancelled' " +
                "WHERE \"Status\" IN ('Cancelled', 'Iptal Edildi') " +
                "OR \"Status\" = U&'\\0130ptal Edildi';");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            // Calendar date: historical values are Istanbul midnights stored as timestamptz
            // (typically 21:00 UTC). Convert using Europe/Istanbul, not session TimeZone.
            migrationBuilder.Sql(
                "ALTER TABLE \"Reservations\" " +
                "ALTER COLUMN \"ReservationDate\" TYPE date " +
                "USING ((\"ReservationDate\" AT TIME ZONE 'Europe/Istanbul')::date);");

            // Clock time: validated intervals 0 <= t < 24h -> time without time zone.
            migrationBuilder.Sql(
                "ALTER TABLE \"Reservations\" " +
                "ALTER COLUMN \"ReservationTime\" TYPE time without time zone " +
                "USING ((TIME '00:00:00' + \"ReservationTime\")::time without time zone);");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reservations_Status",
                table: "Reservations",
                sql: "\"Status\" IN ('Pending', 'Confirmed', 'Cancelled')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reservations_Status",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.Sql(
                "ALTER TABLE \"Reservations\" " +
                "ALTER COLUMN \"ReservationTime\" TYPE interval " +
                "USING (\"ReservationTime\" - TIME '00:00:00');");

            // Restore date as midnight UTC timestamptz of the stored calendar date.
            migrationBuilder.Sql(
                "ALTER TABLE \"Reservations\" " +
                "ALTER COLUMN \"ReservationDate\" TYPE timestamp with time zone " +
                "USING ((\"ReservationDate\"::timestamp without time zone AT TIME ZONE 'UTC'));");
        }
    }
}
