using System.Net;
using System.Net.Http;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class AuthenticationTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public AuthenticationTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Menu/Index")]
    [InlineData("/Reservation/CreateReservation")]
    public async Task Anonymous_PublicRoutes_Return200(string path)
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/Dashboard/Index")]
    [InlineData("/Category/CategoryList")]
    [InlineData("/Product/ProductList")]
    [InlineData("/Reservation/ReservationList")]
    [InlineData("/Review/ReviewList")]
    [InlineData("/Statistics/Index")]
    public async Task Anonymous_AdminRoutes_ChallengeToLogin(string path)
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("/Account/Login", location, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvalidLogin_DoesNotAuthenticate()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = HotCoffeeWebApplicationFactory.ExtractAntiforgeryToken(html)!;

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = "wrong-user",
            ["Password"] = "wrong-password",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            body.Contains("Geçersiz", StringComparison.Ordinal)
            || body.Contains("Ge&#xE7;ersiz", StringComparison.OrdinalIgnoreCase)
            || body.Contains("alert-errors", StringComparison.OrdinalIgnoreCase),
            "Expected a generic invalid-login error message.");

        var dash = await client.GetAsync("/Dashboard/Index");
        Assert.Equal(HttpStatusCode.Redirect, dash.StatusCode);
    }

    [Fact]
    public async Task ValidLogin_AllowsDashboard()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/Dashboard/Index");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExternalReturnUrl_IsRejected()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = HotCoffeeWebApplicationFactory.ExtractAntiforgeryToken(html)!;

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = _factory.TestUsername,
            ["Password"] = _factory.TestPassword,
            ["ReturnUrl"] = "https://evil.example/phish",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.DoesNotContain("evil.example", location, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/Dashboard", location, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalReturnUrl_IsHonored()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = HotCoffeeWebApplicationFactory.ExtractAntiforgeryToken(html)!;

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = _factory.TestUsername,
            ["Password"] = _factory.TestPassword,
            ["ReturnUrl"] = "/Product/ProductList",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Product/ProductList", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Logout_ClearsSession_AndGetLogoutIsUnavailable()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var getLogout = await client.GetAsync("/Account/Logout");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, getLogout.StatusCode);

        var loginOrDash = await client.GetAsync("/Dashboard/Index");
        Assert.Equal(HttpStatusCode.OK, loginOrDash.StatusCode);

        var page = await client.GetAsync("/Dashboard/Index");
        var html = await page.Content.ReadAsStringAsync();
        var token = HotCoffeeWebApplicationFactory.ExtractAntiforgeryToken(html)
            ?? HotCoffeeWebApplicationFactory.ExtractAntiforgeryToken(
                await (await client.GetAsync("/Category/CategoryList")).Content.ReadAsStringAsync());

        Assert.False(string.IsNullOrEmpty(token));

        var logout = await client.PostAsync("/Account/Logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token!
        }));
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        var after = await client.GetAsync("/Dashboard/Index");
        Assert.Equal(HttpStatusCode.Redirect, after.StatusCode);
        Assert.Contains("/Account/Login", after.Headers.Location?.ToString() ?? string.Empty);
    }
}
