using DigitsToWords.Core;

namespace DigitsToWords.Core.Tests;

public class MoneyValidatorTests
{
    private const string EmptyMessage = "Please enter an amount.";

    private const string NotANumberMessage =
        "The amount must be a plain number such as 123.45 or -123.45, with no commas, spaces or symbols and with digits on both sides of any decimal point.";

    private const string TooManyDecimalPlacesMessage = "The amount can have at most two decimal places.";

    private const string TooLargeMessage =
        "The amount is too large; it must be below one undecillion dollars, which allows at most 36 digits before the decimal point.";

    private static readonly string ThirtySixNines = new('9', 36);

    private static readonly string OneUndecillion = "1" + new string('0', 36);

    private readonly MoneyValidator _validator = new();

    [Theory]
    [InlineData("123.45", "123.45")]
    [InlineData("12", "12.00")]
    [InlineData("12.5", "12.50")]
    [InlineData("12.50", "12.50")]
    [InlineData("12.05", "12.05")]
    [InlineData("0", "0.00")]
    [InlineData("0.0", "0.00")]
    [InlineData("0.00", "0.00")]
    [InlineData("0.45", "0.45")]
    [InlineData("1000000", "1000000.00")]
    public void Validate_accepts_plain_amounts_and_writes_them_with_two_decimal_places(string input, string expected)
    {
        AssertValid(input, expected);
    }

    [Theory]
    [InlineData("-5", "-5.00")]
    [InlineData("-123.45", "-123.45")]
    [InlineData("-0.01", "-0.01")]
    [InlineData("-0.5", "-0.50")]
    public void Validate_accepts_negative_amounts_and_keeps_the_minus_sign(string input, string expected)
    {
        AssertValid(input, expected);
    }

    [Theory]
    [InlineData("-0")]
    [InlineData("-0.0")]
    [InlineData("-0.00")]
    [InlineData("-000")]
    public void Validate_drops_the_minus_sign_from_zero(string input)
    {
        AssertValid(input, "0.00");
    }

    [Theory]
    [InlineData("007", "7.00")]
    [InlineData("000123.45", "123.45")]
    [InlineData("00", "0.00")]
    [InlineData("00.50", "0.50")]
    [InlineData("-007", "-7.00")]
    public void Validate_ignores_leading_zeros(string input, string expected)
    {
        AssertValid(input, expected);
    }

    [Theory]
    [InlineData(" 123.45", "123.45")]
    [InlineData("123.45 ", "123.45")]
    [InlineData("  123.45  ", "123.45")]
    [InlineData("\t12\r\n", "12.00")]
    [InlineData(" -5 ", "-5.00")]
    public void Validate_trims_whitespace_around_the_amount(string input, string expected)
    {
        AssertValid(input, expected);
    }

    [Fact]
    public void Validate_accepts_the_largest_allowed_amount()
    {
        AssertValid(ThirtySixNines + ".99", ThirtySixNines + ".99");
    }

    [Fact]
    public void Validate_accepts_the_largest_allowed_negative_amount()
    {
        AssertValid("-" + ThirtySixNines + ".99", "-" + ThirtySixNines + ".99");
    }

    [Fact]
    public void Validate_does_not_count_leading_zeros_towards_the_size_limit()
    {
        AssertValid("0000" + ThirtySixNines, ThirtySixNines + ".00");
    }

    [Fact]
    public void Validate_accepts_a_long_run_of_zeros_as_zero()
    {
        AssertValid(new string('0', 100), "0.00");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Validate_rejects_empty_input(string? input)
    {
        AssertInvalid(input, EmptyMessage);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12a")]
    [InlineData("a12")]
    [InlineData("1,000")]
    [InlineData("1,000.00")]
    [InlineData("$5")]
    [InlineData("5$")]
    [InlineData("1 000")]
    [InlineData("12 .50")]
    [InlineData("- 5")]
    [InlineData("+5")]
    [InlineData("--5")]
    [InlineData("5-")]
    [InlineData("1-2")]
    [InlineData("-")]
    [InlineData(".")]
    [InlineData("-.")]
    [InlineData("12.")]
    [InlineData(".5")]
    [InlineData("-.5")]
    [InlineData("-12.")]
    [InlineData("1.2.3")]
    [InlineData("1..2")]
    [InlineData("1e5")]
    [InlineData("1E5")]
    [InlineData("0x1F")]
    [InlineData("(5)")]
    [InlineData("12.5a")]
    [InlineData("12.a5")]
    [InlineData("12.-5")]
    [InlineData("١٢٣")]
    [InlineData("１２３")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void Validate_rejects_text_that_is_not_a_plain_number(string input)
    {
        AssertInvalid(input, NotANumberMessage);
    }

    [Theory]
    [InlineData("12.505")]
    [InlineData("12.500")]
    [InlineData("0.001")]
    [InlineData("-0.000")]
    [InlineData("1.23456789")]
    public void Validate_rejects_more_than_two_decimal_places(string input)
    {
        AssertInvalid(input, TooManyDecimalPlacesMessage);
    }

    [Fact]
    public void Validate_rejects_one_undecillion()
    {
        AssertInvalid(OneUndecillion, TooLargeMessage);
    }

    [Fact]
    public void Validate_rejects_minus_one_undecillion()
    {
        AssertInvalid("-" + OneUndecillion, TooLargeMessage);
    }

    [Fact]
    public void Validate_rejects_one_undecillion_written_with_cents_or_leading_zeros()
    {
        AssertInvalid(OneUndecillion + ".00", TooLargeMessage);
        AssertInvalid("00" + OneUndecillion, TooLargeMessage);
    }

    [Fact]
    public void Validate_rejects_amounts_far_above_the_limit()
    {
        AssertInvalid(new string('9', 200), TooLargeMessage);
    }

    [Fact]
    public void Validate_reports_the_decimal_places_before_the_size_when_both_are_wrong()
    {
        AssertInvalid(OneUndecillion + ".505", TooManyDecimalPlacesMessage);
    }

    [Fact]
    public void The_public_message_constants_hold_the_messages_that_are_returned()
    {
        Assert.Equal(EmptyMessage, MoneyValidator.EmptyMessage);
        Assert.Equal(NotANumberMessage, MoneyValidator.NotANumberMessage);
        Assert.Equal(TooManyDecimalPlacesMessage, MoneyValidator.TooManyDecimalPlacesMessage);
        Assert.Equal(TooLargeMessage, MoneyValidator.TooLargeMessage);
    }

    private void AssertValid(string input, string expectedNormalisedValue)
    {
        MoneyValidationResult result = _validator.Validate(input);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(expectedNormalisedValue, result.NormalisedValue);
    }

    private void AssertInvalid(string? input, string expectedMessage)
    {
        MoneyValidationResult result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Equal(expectedMessage, result.ErrorMessage);
        Assert.Null(result.NormalisedValue);
    }
}
