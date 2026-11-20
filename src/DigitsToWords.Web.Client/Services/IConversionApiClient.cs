namespace DigitsToWords.Web.Client.Services;

/// <summary>Asks the server to convert a dollar amount into words.</summary>
public interface IConversionApiClient
{
    /// <summary>Sends the amount to the server and returns the amount written in words.</summary>
    /// <exception cref="ConversionRejectedException">The server said the amount is not valid.</exception>
    /// <exception cref="HttpRequestException">The server could not be reached or gave an unexpected reply.</exception>
    Task<string> ConvertAsync(string amount, CancellationToken cancellationToken = default);
}
