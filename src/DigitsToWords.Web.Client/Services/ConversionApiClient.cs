using System.Net;
using System.Net.Http.Json;
using DigitsToWords.Core;

namespace DigitsToWords.Web.Client.Services;

/// <summary>Calls the server's <c>POST api/convert</c> endpoint over HTTP.</summary>
public sealed class ConversionApiClient(HttpClient httpClient) : IConversionApiClient
{
    /// <inheritdoc />
    public async Task<string> ConvertAsync(string amount, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/convert", new ConvertRequest(amount), cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadFromJsonAsync<ConvertError>(cancellationToken);
            throw new ConversionRejectedException(error?.Error ?? "The amount is not valid.");
        }

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ConvertResponse>(cancellationToken);
        return body?.Words ?? throw new HttpRequestException("The server returned an empty reply.");
    }
}
