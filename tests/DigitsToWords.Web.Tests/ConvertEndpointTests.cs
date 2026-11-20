using System.Net;
using System.Net.Http.Json;
using DigitsToWords.Core;
using DigitsToWords.Web.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DigitsToWords.Web.Tests;

/// <summary>Integration tests for <c>POST /api/convert</c>, run against the real web host with fake validation and conversion.</summary>
public sealed class ConvertEndpointTests(ConvertEndpointTests.FakeServicesFactory factory)
    : IClassFixture<ConvertEndpointTests.FakeServicesFactory>
{
    private const string ValidAmount = "123.45";

    [Fact]
    public async Task Valid_amount_returns_200_with_the_words()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/convert", new ConvertRequest(ValidAmount));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ConvertResponse>();
        Assert.Equal(FakeMoneyToWordsConverter.WordsFor(ValidAmount), body?.Words);
    }

    [Theory]
    [InlineData("12.505")]
    [InlineData("")]
    public async Task Invalid_amount_returns_400_with_the_validator_message(string amount)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/convert", new ConvertRequest(amount));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ConvertError>();
        Assert.Equal(FakeMoneyValidator.RejectionMessage, body?.Error);
    }

    [Fact]
    public async Task Request_without_an_amount_returns_400_with_the_validator_message()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/convert", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ConvertError>();
        Assert.Equal(FakeMoneyValidator.RejectionMessage, body?.Error);
    }

    /// <summary>Starts the web host with the real validator and converter replaced by fakes.</summary>
    public sealed class FakeServicesFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMoneyValidator>();
                services.RemoveAll<IMoneyToWordsConverter>();
                services.AddSingleton<IMoneyValidator>(new FakeMoneyValidator(ValidAmount));
                services.AddSingleton<IMoneyToWordsConverter>(new FakeMoneyToWordsConverter());
            });
        }
    }
}
