namespace DigitsToWords.Core;

/// <summary>Checks that text is a dollar amount with at most two decimal places and a size below one undecillion.</summary>
public sealed class MoneyValidator : IMoneyValidator
{
    /// <summary>The message shown when nothing was entered.</summary>
    public const string EmptyMessage = "Please enter an amount.";

    /// <summary>The message shown when the text is not written as a plain number.</summary>
    public const string NotANumberMessage =
        "The amount must be a plain number such as 123.45 or -123.45, with no commas, spaces or symbols and with digits on both sides of any decimal point.";

    /// <summary>The message shown when there are more than two digits after the decimal point.</summary>
    public const string TooManyDecimalPlacesMessage = "The amount can have at most two decimal places.";

    /// <summary>The message shown when the amount is one undecillion dollars or more, ignoring its sign.</summary>
    public const string TooLargeMessage =
        "The amount is too large; it must be below one undecillion dollars, which allows at most 36 digits before the decimal point.";

    /// <summary>The longest whole-dollar part allowed, because one undecillion is a one followed by 36 zeros.</summary>
    private const int MaxDollarDigits = 36;

    private const int MaxCentDigits = 2;

    /// <summary>
    /// Validates the input and, when it is valid, returns it rewritten as dollars, a decimal point and exactly two cent digits.
    /// The rewritten value has no leading zeros and has a minus sign only when the amount is below zero.
    /// </summary>
    public MoneyValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Invalid(EmptyMessage);
        }

        string text = input.Trim();
        bool hasMinusSign = text[0] == '-';
        string unsignedText = hasMinusSign ? text[1..] : text;

        string[] parts = unsignedText.Split('.');
        if (parts.Length > 2 || !parts.All(IsOneOrMoreDigits))
        {
            return Invalid(NotANumberMessage);
        }

        string centDigits = parts.Length == 2 ? parts[1] : string.Empty;
        if (centDigits.Length > MaxCentDigits)
        {
            return Invalid(TooManyDecimalPlacesMessage);
        }

        string dollarDigits = parts[0].TrimStart('0');
        if (dollarDigits.Length > MaxDollarDigits)
        {
            return Invalid(TooLargeMessage);
        }

        string dollars = dollarDigits.Length == 0 ? "0" : dollarDigits;
        string cents = centDigits.PadRight(MaxCentDigits, '0');
        bool isZero = dollars == "0" && cents == "00";
        string sign = hasMinusSign && !isZero ? "-" : string.Empty;

        return new MoneyValidationResult(true, null, $"{sign}{dollars}.{cents}");
    }

    private static MoneyValidationResult Invalid(string message) => new(false, message, null);

    /// <summary>Returns true when the text is not empty and holds only the digits 0 to 9.</summary>
    private static bool IsOneOrMoreDigits(string text) => text.Length > 0 && text.All(IsDigit);

    /// <summary>Returns true for the digits 0 to 9 only, so digits from other writing systems are refused.</summary>
    private static bool IsDigit(char character) => character is >= '0' and <= '9';
}
