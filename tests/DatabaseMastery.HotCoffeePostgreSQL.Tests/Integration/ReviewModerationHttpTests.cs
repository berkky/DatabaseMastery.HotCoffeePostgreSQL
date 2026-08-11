using System.Net;
using System.Net.Http;
using Xunit;

namespace DatabaseMastery.HotCoffeePostgreSQL.Tests.Integration;

public class ReviewModerationHttpTests : IClassFixture<HotCoffeeWebApplicationFactory>
{
    private readonly HotCoffeeWebApplicationFactory _factory;

    public ReviewModerationHttpTests(HotCoffeeWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateReview_Get_Valid_Returns200()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync($"/Review/UpdateReview/{_factory.HiddenReviewId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("name=\"ReviewId\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"CustomerName\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"Comment\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"Rating\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("name=\"ProductId\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"Status\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("name=\"CreatedAt\"", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateReview_Get_Missing_Returns404()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/Review/UpdateReview/2147483647");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateReview_Get_Zero_Returns400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/Review/UpdateReview/0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
