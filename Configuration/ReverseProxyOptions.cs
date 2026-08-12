namespace DatabaseMastery.HotCoffeePostgreSQL.Configuration;

public sealed class ReverseProxyOptions
{
    public const string SectionName = "ReverseProxy";

    /// <summary>
    /// When false or when no trusted proxies/networks are configured, forwarded headers are not enabled.
    /// </summary>
    public bool Enabled { get; set; }

    public string[] KnownProxies { get; set; } = [];

    public string[] KnownNetworks { get; set; } = [];
}
