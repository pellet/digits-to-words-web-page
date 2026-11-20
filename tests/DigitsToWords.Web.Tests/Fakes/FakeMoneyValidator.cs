using DigitsToWords.Core;

namespace DigitsToWords.Web.Tests.Fakes;

/// <summary>A stand-in validator that accepts only the amounts it is told to accept and rejects everything else with a fixed message.</summary>
public sealed class FakeMoneyValidator(params string[] validAmounts) : IMoneyValidator
{
    /// <summary>The message returned for every rejected amount.</summary>
    public const string RejectionMessage = "Fake rejection: the amount is not valid.";

    /// <inheritdoc />
    public MoneyValidationResult Validate(string? input) =>
        input is not null && validAmounts.Contains(input)
            ? new MoneyValidationResult(true, null, input)
            : new MoneyValidationResult(false, RejectionMessage, null);
}
