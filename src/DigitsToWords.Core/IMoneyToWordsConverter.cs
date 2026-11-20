namespace DigitsToWords.Core;

/// <summary>Turns a dollar amount into English words.</summary>
public interface IMoneyToWordsConverter
{
    /// <summary>Converts the amount to words and throws <see cref="ArgumentException"/> when the amount is invalid.</summary>
    string Convert(string amount);
}
