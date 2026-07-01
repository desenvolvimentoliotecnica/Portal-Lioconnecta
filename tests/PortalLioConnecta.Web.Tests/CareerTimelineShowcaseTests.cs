using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PortalLioConnecta.Web.Tests;

public class CareerTimelineShowcaseTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly HttpClient _client;

    public CareerTimelineShowcaseTests(WebApplicationFactory<global::Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ShowcasePage_ReturnsOk()
    {
        var response = await _client.GetAsync("/Home/CareerTimelineShowcase");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
