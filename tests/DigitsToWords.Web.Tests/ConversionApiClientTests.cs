using System.Net;
using System.Net.Http.Json;
using DigitsToWords.Core;
using DigitsToWords.Web.Client.Services;

namespace DigitsToWords.Web.Tests;

/// <summary>Tests that the API client turns each kind of server reply into the right result or exception.</summary>
public sealed class ConversionApiClientTests
{
    [Fact]
    public async Task Success_reply_returns_the_words_and_posts_the_amount()
    {
        var handler = new StubHandler(HttpStatusCode.OK, new ConvertResponse("TWELVE DOLLARS"));
        var client = CreateClient(handler);

        var words = await client.ConvertAsync("12");

        Assert.Equal("TWELVE DOLLARS", words);
        Assert.Equal(HttpMethod.Post, handler.Request?.Method);
        Assert.Equal("http://localhost/api/convert", handler.Request?.RequestUri?.ToString());
        Assert.Equal(new ConvertRequest("12"), handler.RequestBody);
    }

    [Fact]
    public async Task Bad_request_reply_throws_with_the_server_message()
    {
        var client = CreateClient(new StubHandler(HttpStatusCode.BadRequest, new ConvertError("Too many decimal places.")));

        var error = await Assert.ThrowsAsync<ConversionRejectedException>(() => client.ConvertAsync("12.505"));

        Assert.Equal("Too many decimal places.", error.Message);
    }

    [Fact]
    public async Task Server_error_reply_throws_an_http_request_exception()
    {
        var client = CreateClient(new StubHandler(HttpStatusCode.InternalServerError, body: null));

        await Assert.ThrowsAsync<HttpRequestException>(() => client.ConvertAsync("12"));
    }

    private static ConversionApiClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

    /// <summary>Returns one fixed reply and keeps the request it received.</summary>
    private sealed class StubHandler(HttpStatusCode status, object? body) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public ConvertRequest? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            if (request.Content is not null)
            {
                RequestBody = await request.Content.ReadFromJsonAsync<ConvertRequest>(cancellationToken);
            }

            return new HttpResponseMessage(status) { Content = body is null ? null : JsonContent.Create(body) };
        }
    }
}
