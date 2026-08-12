namespace DatabaseMastery.HotCoffeePostgreSQL.Services.PublicRestaurant;

/// <summary>
/// Verified product photograph mappings keyed by stable <see cref="Entities.Product.ProductId"/>.
/// DB <c>ImageUrl</c> values are never mutated; this catalog is the application-owned source of truth.
/// Template placeholders (e.g. gray "Menu IMG 400x400") are never eligible for mapping.
/// </summary>
public static class ProductMediaCatalog
{
    public const string ProductImagesRoot = "/images/menu/products/";

    /// <summary>
    /// Future application-owned generated product photos (STEP 10.5+).
    /// Resolver emits these only when the file exists and an explicit mapping is registered.
    /// </summary>
    public const string GeneratedProductsRoot = "/images/products/generated/";

    /// <summary>
    /// ProductId → owned public path. Only genuine food photographs belong here.
    /// All prior STEP 10.3 mappings pointed at identical Dina template placeholders and were removed.
    /// </summary>
    private static readonly IReadOnlyDictionary<int, string> VerifiedProductImages =
        new Dictionary<int, string>();

    public static IReadOnlyDictionary<int, string> VerifiedMappings => VerifiedProductImages;

    public static bool TryGetVerifiedPath(int productId, out string path)
    {
        return VerifiedProductImages.TryGetValue(productId, out path!);
    }

    /// <summary>
    /// Known SHA-256 of the Dina template gray "Menu IMG 400x400" placeholder (9250 bytes).
    /// All recovered menu product JPGs except the cinematic hero share this fingerprint.
    /// </summary>
    public const string TemplatePlaceholderSha256 =
        "635C5AAB4823661E7830278D9558076DBA1FF2F7BD314A1F25FF8162A516B4EE";

    public static bool IsTemplatePlaceholderAsset(string physicalPath)
    {
        if (!File.Exists(physicalPath))
        {
            return false;
        }

        var info = new FileInfo(physicalPath);
        if (info.Length != 9250)
        {
            return false;
        }

        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(physicalPath)));
        return hash.Equals(TemplatePlaceholderSha256, StringComparison.OrdinalIgnoreCase);
    }
}
