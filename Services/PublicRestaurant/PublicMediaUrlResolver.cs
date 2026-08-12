namespace DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;

public interface IPublicMediaUrlResolver
{
    string? ResolveProductImageUrl(int productId, string? storedUrl);

    string? ResolveHeroStageImageUrl();
}

public sealed class PublicMediaUrlResolver : IPublicMediaUrlResolver
{
    private readonly IWebHostEnvironment _environment;

    public PublicMediaUrlResolver(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string? ResolveProductImageUrl(int productId, string? storedUrl)
    {
        if (ProductMediaCatalog.TryGetVerifiedPath(productId, out var catalogPath)
            && IsEligibleProductImage(catalogPath))
        {
            return catalogPath;
        }

        if (string.IsNullOrWhiteSpace(storedUrl))
        {
            return null;
        }

        var trimmed = storedUrl.Trim();
        if (MenuMediaCatalog.IsBlockedScheme(trimmed))
        {
            return null;
        }

        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        foreach (var candidate in BuildCandidates(trimmed))
        {
            if (candidate.StartsWith('/') && IsEligibleProductImage(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    public string? ResolveHeroStageImageUrl()
    {
        return LocalFileExists(MenuMediaCatalog.HeroStageImagePath)
            ? MenuMediaCatalog.HeroStageImagePath
            : null;
    }

    private IEnumerable<string> BuildCandidates(string storedUrl)
    {
        var fileName = Path.GetFileName(storedUrl);
        var ownedCandidate = string.IsNullOrWhiteSpace(fileName)
            ? null
            : $"{MenuMediaCatalog.MenuImagesRoot}{fileName}";

        yield return storedUrl;

        var mapped = MenuMediaCatalog.MapLegacyStoredPathToOwnedPath(storedUrl);
        if (!string.IsNullOrWhiteSpace(mapped))
        {
            yield return mapped;
        }

        if (!string.IsNullOrWhiteSpace(ownedCandidate)
            && !ownedCandidate.Equals(storedUrl, StringComparison.OrdinalIgnoreCase)
            && (mapped is null || !ownedCandidate.Equals(mapped, StringComparison.OrdinalIgnoreCase)))
        {
            yield return ownedCandidate;
        }
    }

    private bool IsEligibleProductImage(string webPath)
    {
        if (!LocalFileExists(webPath))
        {
            return false;
        }

        var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physical = Path.Combine(_environment.WebRootPath, relative);
        return !ProductMediaCatalog.IsTemplatePlaceholderAsset(physical);
    }

    private bool LocalFileExists(string webPath)
    {
        var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physical = Path.Combine(_environment.WebRootPath, relative);
        return File.Exists(physical) && new FileInfo(physical).Length > 0;
    }
}
