using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services
{
    public static class DatabaseExceptionHelper
    {
        public static bool IsForeignKeyViolation(DbUpdateException exception)
        {
            return exception.InnerException is PostgresException postgresException
                && postgresException.SqlState == PostgresErrorCodes.ForeignKeyViolation;
        }
    }
}
