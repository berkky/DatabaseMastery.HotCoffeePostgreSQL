using DatabaseMastery.HotCoffeePostgreSQL.Configuration;

namespace DatabaseMastery.HotCoffeePostgreSQL.Infrastructure;

public static class ApplicationConfigurationValidator
{
    public static void ValidateRequiredConfiguration(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required. Configure it via User Secrets or environment variables.");
        }

        var adminAuth = configuration.GetSection(AdminAuthOptions.SectionName).Get<AdminAuthOptions>();
        if (string.IsNullOrWhiteSpace(adminAuth?.Username))
        {
            throw new InvalidOperationException(
                "AdminAuth:Username is required. Configure it via User Secrets or environment variables.");
        }

        if (string.IsNullOrWhiteSpace(adminAuth.Password))
        {
            throw new InvalidOperationException(
                "AdminAuth:Password is required. Configure it via User Secrets or environment variables.");
        }
    }
}
