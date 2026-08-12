using DatabaseMastery.HotCoffeePostgreSQL.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DatabaseMastery.HotCoffeePostgreSQL.Health;

public sealed class DatabaseReadyHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseReadyHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }
}
