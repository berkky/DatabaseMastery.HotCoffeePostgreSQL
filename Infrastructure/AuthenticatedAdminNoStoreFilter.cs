using DatabaseMastery.HotCoffeePostgreSQL.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatabaseMastery.HotCoffeePostgreSQL.Infrastructure;

public sealed class AuthenticatedAdminNoStoreFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!ShouldApplyNoStore(context))
        {
            return;
        }

        var headers = context.HttpContext.Response.Headers;
        headers.CacheControl = "no-store, no-cache";
        headers.Pragma = "no-cache";
        headers.Expires = "0";
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static bool ShouldApplyNoStore(FilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (user.IsInRole(AdminRoles.Admin))
        {
            return true;
        }

        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        return path.StartsWith("/Account/", StringComparison.OrdinalIgnoreCase);
    }
}
