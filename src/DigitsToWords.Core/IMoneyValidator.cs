namespace DigitsToWords.Core;

/// <summary>Checks whether text typed by a user is an acceptable dollar amount.</summary>
public interface IMoneyValidator
{
    /// <summary>Validates the input and returns the outcome, including a normalised value when valid.</summary>
    MoneyValidationResult Validate(string? input);
}
