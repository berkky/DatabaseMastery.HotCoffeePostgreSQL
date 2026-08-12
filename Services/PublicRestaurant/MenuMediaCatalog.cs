namespace DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;

/// <summary>
/// Application-owned menu media paths and hero asset selection.
/// </summary>
public static class MenuMediaCatalog
{
    public const string MenuImagesRoot = "/images/menu/";

    /// <summary>
    /// Decorative hero background owned by the application (not a product-specific claim).
    /// </summary>
    public const string HeroStageImagePath = "/images/menu/menu-hero-3d.jpg";

    public static bool IsSafePublicImageScheme(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (url.StartsWith("/", StringComparison.Ordinal))
        {
            return true;
        }

        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsBlockedScheme(string url)
    {
        return url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("file:", StringComparison.OrdinalIgnoreCase);
    }

    public static string? MapLegacyStoredPathToOwnedPath(string storedUrl)
    {
        if (string.IsNullOrWhiteSpace(storedUrl))
        {
            return null;
        }

        if (storedUrl.StartsWith("/dina-html/images/menu/", StringComparison.OrdinalIgnoreCase)
            || storedUrl.StartsWith("/dina-html/", StringComparison.OrdinalIgnoreCase))
        {
            return $"{MenuImagesRoot}{Path.GetFileName(storedUrl)}";
        }

        if (storedUrl.StartsWith(MenuImagesRoot, StringComparison.OrdinalIgnoreCase))
        {
            return storedUrl;
        }

        return null;
    }
}
