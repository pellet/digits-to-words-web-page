namespace DigitsToWords.Core;

/// <summary>The outcome of validating an amount, with an error message when invalid or a cleaned-up value when valid.</summary>
public sealed record MoneyValidationResult(bool IsValid, string? ErrorMessage, string? NormalisedValue);
