using DigitsToWords.Web.Client.Services;

namespace DigitsToWords.Web.Tests.Fakes;

/// <summary>A stand-in API client that records each call and either returns fixed words or throws a chosen exception.</summary>
public sealed class FakeConversionApiClient : IConversionApiClient
{
    /// <summary>The words returned when no exception is set.</summary>
    public string Words { get; set; } = "FAKE WORDS";

    /// <summary>The exception to throw instead of returning words, or null to return words.</summary>
    public Exception? ExceptionToThrow { get; set; }

    /// <summary>The amounts the page sent, in order.</summary>
    public List<string> ReceivedAmounts { get; } = [];

    /// <inheritdoc />
    public Task<string> ConvertAsync(string amount, CancellationToken cancellationToken = default)
    {
        ReceivedAmounts.Add(amount);
        return ExceptionToThrow is null ? Task.FromResult(Words) : Task.FromException<string>(ExceptionToThrow);
    }
}
