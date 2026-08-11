using System.Net;
using System.Net.Http;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class HttpSemanticsTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public HttpSemanticsTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/Category/DeleteCategory/1")]
    [InlineData("/Product/DeleteProduct/1")]
    [InlineData("/Reservation/DeleteReservation/1")]
    [InlineData("/Reservation/ApproveReservation/1")]
    [InlineData("/Reservation/PendingReservation/1")]
    [InlineData("/Reservation/CancelReservation/1")]
    [InlineData("/Review/PublishReview/1")]
    [InlineData("/Review/HideReview/1")]
    [InlineData("/Review/DeleteReview/1")]
    public async Task MutationRoutes_RejectGet(string path)
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Theory]
    [InlineData("/Category/DeleteCategory/1")]
    [InlineData("/Reservation/ApproveReservation/1")]
    [InlineData("/Review/HideReview/1")]
    public async Task AuthenticatedPost_WithoutAntiforgery_Returns400(string path)
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.PostAsync(path, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = "1"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublicCreateReservation_WithoutAntiforgery_Returns400()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.PostAsync("/Reservation/CreateReservation", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Test",
            ["Phone"] = "+905551112233",
            ["Email"] = "a@b.com",
            ["GuestCount"] = "2"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublicCreateReservation_Html_HasToken_AndNoStatusField()
    {
        var client = _factory.CreateClient();
        var html = await client.GetStringAsync("/Reservation/CreateReservation");
        Assert.Contains("__RequestVerificationToken", html, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Status\"", html, StringComparison.OrdinalIgnoreCase);
    }
}
