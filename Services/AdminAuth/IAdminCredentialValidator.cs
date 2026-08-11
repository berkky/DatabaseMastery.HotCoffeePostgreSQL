namespace DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAuth
{
    public interface IAdminCredentialValidator
    {
        bool IsConfigurationUsable();

        bool Validate(string username, string password);
    }
}
