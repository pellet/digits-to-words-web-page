using DigitsToWords.Core;

namespace DigitsToWords.Web.Tests.Fakes;

/// <summary>A stand-in converter that returns a predictable phrase built from the amount it receives.</summary>
public sealed class FakeMoneyToWordsConverter : IMoneyToWordsConverter
{
    /// <summary>Returns the words the fake produces for the given amount.</summary>
    public static string WordsFor(string amount) => $"FAKE WORDS FOR {amount}";

    /// <inheritdoc />
    public string Convert(string amount) => WordsFor(amount);
}
