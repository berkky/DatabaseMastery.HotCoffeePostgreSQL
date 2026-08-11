using System.Security.Cryptography;
using System.Text;
using DatabaseMastery.HotCoffeePostgreSQL.Configuration;
using Microsoft.Extensions.Options;

namespace DatabaseMastery.HotCoffeePostgreSQL.Services.AdminAuth
{
    public class AdminCredentialValidator : IAdminCredentialValidator
    {
        private readonly AdminAuthOptions _options;

        public AdminCredentialValidator(IOptions<AdminAuthOptions> options)
        {
            _options = options.Value;
        }

        public bool IsConfigurationUsable()
        {
            return !string.IsNullOrWhiteSpace(_options.Username)
                && !string.IsNullOrWhiteSpace(_options.Password);
        }

        public bool Validate(string username, string password)
        {
            if (!IsConfigurationUsable())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var normalizedInput = username.Trim();
            var configuredUsername = _options.Username!.Trim();

            if (!string.Equals(normalizedInput, configuredUsername, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return FixedTimeEquals(password, _options.Password!);
        }

        private static bool FixedTimeEquals(string supplied, string configured)
        {
            var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
            var configuredBytes = Encoding.UTF8.GetBytes(configured);

            if (suppliedBytes.Length != configuredBytes.Length)
            {
                CryptographicOperations.FixedTimeEquals(suppliedBytes, suppliedBytes);
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes);
        }
    }
}
