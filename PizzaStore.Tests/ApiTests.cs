using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PizzaStore.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ReturnsHelloWorld()
    {
        var response = await _client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Hello World!", body);
    }

    [Fact]
    public async Task GetPizzas_ReturnsOk()
    {
        var response = await _client.GetAsync("/pizzas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
