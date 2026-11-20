using DigitsToWords.Core;

namespace DigitsToWords.Core.Tests;

public class MoneyToWordsConverterTests
{
    private static readonly string ThirtySixNines = new('9', 36);

    private static readonly string OneUndecillion = "1" + new string('0', 36);

    private readonly MoneyToWordsConverter _converter = new();

    [Fact]
    public void Convert_gives_the_wording_from_the_original_request()
    {
        Assert.Equal(
            "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS",
            _converter.Convert("123.45"));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.0")]
    [InlineData("0.00")]
    [InlineData("000")]
    [InlineData("000.00")]
    public void Convert_writes_zero_as_zero_dollars(string amount)
    {
        Assert.Equal("ZERO DOLLARS", _converter.Convert(amount));
    }

    [Theory]
    [InlineData("1", "ONE DOLLAR")]
    [InlineData("1.00", "ONE DOLLAR")]
    [InlineData("0.01", "ZERO DOLLARS AND ONE CENT")]
    [InlineData("1.01", "ONE DOLLAR AND ONE CENT")]
    [InlineData("1.02", "ONE DOLLAR AND TWO CENTS")]
    [InlineData("2.01", "TWO DOLLARS AND ONE CENT")]
    [InlineData("1.10", "ONE DOLLAR AND TEN CENTS")]
    [InlineData("1000001", "ONE MILLION AND ONE DOLLARS")]
    [InlineData("21.21", "TWENTY-ONE DOLLARS AND TWENTY-ONE CENTS")]
    public void Convert_uses_the_singular_only_for_exactly_one_dollar_or_one_cent(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("0.45", "ZERO DOLLARS AND FORTY-FIVE CENTS")]
    [InlineData("0.02", "ZERO DOLLARS AND TWO CENTS")]
    [InlineData("0.10", "ZERO DOLLARS AND TEN CENTS")]
    [InlineData("0.11", "ZERO DOLLARS AND ELEVEN CENTS")]
    [InlineData("0.99", "ZERO DOLLARS AND NINETY-NINE CENTS")]
    [InlineData("00.45", "ZERO DOLLARS AND FORTY-FIVE CENTS")]
    public void Convert_writes_zero_dollars_before_an_amount_of_cents_only(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("12.5", "TWELVE DOLLARS AND FIFTY CENTS")]
    [InlineData("12.50", "TWELVE DOLLARS AND FIFTY CENTS")]
    [InlineData("12.05", "TWELVE DOLLARS AND FIVE CENTS")]
    [InlineData("0.1", "ZERO DOLLARS AND TEN CENTS")]
    [InlineData("0.9", "ZERO DOLLARS AND NINETY CENTS")]
    public void Convert_reads_one_decimal_place_as_tens_of_cents(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12.0")]
    [InlineData("12.00")]
    public void Convert_leaves_out_the_cents_when_they_are_zero(string amount)
    {
        Assert.Equal("TWELVE DOLLARS", _converter.Convert(amount));
    }

    [Theory]
    [InlineData("2", "TWO DOLLARS")]
    [InlineData("3", "THREE DOLLARS")]
    [InlineData("4", "FOUR DOLLARS")]
    [InlineData("5", "FIVE DOLLARS")]
    [InlineData("6", "SIX DOLLARS")]
    [InlineData("7", "SEVEN DOLLARS")]
    [InlineData("8", "EIGHT DOLLARS")]
    [InlineData("9", "NINE DOLLARS")]
    public void Convert_writes_single_digits(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("10", "TEN DOLLARS")]
    [InlineData("11", "ELEVEN DOLLARS")]
    [InlineData("12", "TWELVE DOLLARS")]
    [InlineData("13", "THIRTEEN DOLLARS")]
    [InlineData("14", "FOURTEEN DOLLARS")]
    [InlineData("15", "FIFTEEN DOLLARS")]
    [InlineData("16", "SIXTEEN DOLLARS")]
    [InlineData("17", "SEVENTEEN DOLLARS")]
    [InlineData("18", "EIGHTEEN DOLLARS")]
    [InlineData("19", "NINETEEN DOLLARS")]
    public void Convert_writes_ten_and_the_teens(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("20", "TWENTY DOLLARS")]
    [InlineData("30", "THIRTY DOLLARS")]
    [InlineData("40", "FORTY DOLLARS")]
    [InlineData("50", "FIFTY DOLLARS")]
    [InlineData("60", "SIXTY DOLLARS")]
    [InlineData("70", "SEVENTY DOLLARS")]
    [InlineData("80", "EIGHTY DOLLARS")]
    [InlineData("90", "NINETY DOLLARS")]
    public void Convert_writes_exact_tens_without_a_hyphen(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("21", "TWENTY-ONE DOLLARS")]
    [InlineData("32", "THIRTY-TWO DOLLARS")]
    [InlineData("43", "FORTY-THREE DOLLARS")]
    [InlineData("54", "FIFTY-FOUR DOLLARS")]
    [InlineData("65", "SIXTY-FIVE DOLLARS")]
    [InlineData("76", "SEVENTY-SIX DOLLARS")]
    [InlineData("87", "EIGHTY-SEVEN DOLLARS")]
    [InlineData("98", "NINETY-EIGHT DOLLARS")]
    [InlineData("99", "NINETY-NINE DOLLARS")]
    public void Convert_joins_tens_and_units_with_a_hyphen(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("100", "ONE HUNDRED DOLLARS")]
    [InlineData("200", "TWO HUNDRED DOLLARS")]
    [InlineData("500", "FIVE HUNDRED DOLLARS")]
    [InlineData("900", "NINE HUNDRED DOLLARS")]
    public void Convert_writes_exact_hundreds_without_and(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("101", "ONE HUNDRED AND ONE DOLLARS")]
    [InlineData("105", "ONE HUNDRED AND FIVE DOLLARS")]
    [InlineData("110", "ONE HUNDRED AND TEN DOLLARS")]
    [InlineData("119", "ONE HUNDRED AND NINETEEN DOLLARS")]
    [InlineData("120", "ONE HUNDRED AND TWENTY DOLLARS")]
    [InlineData("342", "THREE HUNDRED AND FORTY-TWO DOLLARS")]
    [InlineData("999", "NINE HUNDRED AND NINETY-NINE DOLLARS")]
    public void Convert_writes_and_after_a_hundred_that_is_followed_by_tens_or_units(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("1000", "ONE THOUSAND DOLLARS")]
    [InlineData("10000", "TEN THOUSAND DOLLARS")]
    [InlineData("100000", "ONE HUNDRED THOUSAND DOLLARS")]
    [InlineData("1100", "ONE THOUSAND ONE HUNDRED DOLLARS")]
    [InlineData("1000100", "ONE MILLION ONE HUNDRED DOLLARS")]
    [InlineData("1005000", "ONE MILLION FIVE THOUSAND DOLLARS")]
    [InlineData("2000000", "TWO MILLION DOLLARS")]
    [InlineData("1000000000", "ONE BILLION DOLLARS")]
    public void Convert_writes_no_and_between_groups_when_the_last_group_is_zero_or_at_least_one_hundred(
        string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("1001", "ONE THOUSAND AND ONE DOLLARS")]
    [InlineData("1005", "ONE THOUSAND AND FIVE DOLLARS")]
    [InlineData("1010", "ONE THOUSAND AND TEN DOLLARS")]
    [InlineData("1099", "ONE THOUSAND AND NINETY-NINE DOLLARS")]
    [InlineData("1000005", "ONE MILLION AND FIVE DOLLARS")]
    [InlineData("1005005", "ONE MILLION FIVE THOUSAND AND FIVE DOLLARS")]
    [InlineData("1000000012", "ONE BILLION AND TWELVE DOLLARS")]
    [InlineData("1005.05", "ONE THOUSAND AND FIVE DOLLARS AND FIVE CENTS")]
    public void Convert_writes_and_before_a_last_group_below_one_hundred_that_follows_larger_groups(
        string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("1105", "ONE THOUSAND ONE HUNDRED AND FIVE DOLLARS")]
    [InlineData("12345", "TWELVE THOUSAND THREE HUNDRED AND FORTY-FIVE DOLLARS")]
    [InlineData("123456", "ONE HUNDRED AND TWENTY-THREE THOUSAND FOUR HUNDRED AND FIFTY-SIX DOLLARS")]
    [InlineData("105000", "ONE HUNDRED AND FIVE THOUSAND DOLLARS")]
    [InlineData(
        "1234567.89",
        "ONE MILLION TWO HUNDRED AND THIRTY-FOUR THOUSAND FIVE HUNDRED AND SIXTY-SEVEN DOLLARS AND EIGHTY-NINE CENTS")]
    [InlineData(
        "987654321",
        "NINE HUNDRED AND EIGHTY-SEVEN MILLION SIX HUNDRED AND FIFTY-FOUR THOUSAND THREE HUNDRED AND TWENTY-ONE DOLLARS")]
    public void Convert_writes_each_group_of_three_digits_with_its_own_and(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData(3, "ONE THOUSAND DOLLARS")]
    [InlineData(6, "ONE MILLION DOLLARS")]
    [InlineData(9, "ONE BILLION DOLLARS")]
    [InlineData(12, "ONE TRILLION DOLLARS")]
    [InlineData(15, "ONE QUADRILLION DOLLARS")]
    [InlineData(18, "ONE QUINTILLION DOLLARS")]
    [InlineData(21, "ONE SEXTILLION DOLLARS")]
    [InlineData(24, "ONE SEPTILLION DOLLARS")]
    [InlineData(27, "ONE OCTILLION DOLLARS")]
    [InlineData(30, "ONE NONILLION DOLLARS")]
    [InlineData(33, "ONE DECILLION DOLLARS")]
    public void Convert_writes_the_scale_word_for_a_one_followed_by_zeros(int numberOfZeros, string expected)
    {
        string amount = "1" + new string('0', numberOfZeros);

        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData(4, "TEN THOUSAND DOLLARS")]
    [InlineData(5, "ONE HUNDRED THOUSAND DOLLARS")]
    [InlineData(34, "TEN DECILLION DOLLARS")]
    [InlineData(35, "ONE HUNDRED DECILLION DOLLARS")]
    public void Convert_writes_tens_and_hundreds_of_a_scale_word(int numberOfZeros, string expected)
    {
        string amount = "1" + new string('0', numberOfZeros);

        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Fact]
    public void Convert_writes_every_scale_word_in_order_in_one_amount()
    {
        string amount = string.Concat(
            "1", "002", "003", "004", "005", "006", "007", "008", "009", "010", "011", "012");

        string expected = string.Join(
            ' ',
            "ONE DECILLION",
            "TWO NONILLION",
            "THREE OCTILLION",
            "FOUR SEPTILLION",
            "FIVE SEXTILLION",
            "SIX QUINTILLION",
            "SEVEN QUADRILLION",
            "EIGHT TRILLION",
            "NINE BILLION",
            "TEN MILLION",
            "ELEVEN THOUSAND",
            "AND TWELVE DOLLARS");

        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Fact]
    public void Convert_writes_the_largest_allowed_amount()
    {
        string expected = string.Join(
            ' ',
            "NINE HUNDRED AND NINETY-NINE DECILLION",
            "NINE HUNDRED AND NINETY-NINE NONILLION",
            "NINE HUNDRED AND NINETY-NINE OCTILLION",
            "NINE HUNDRED AND NINETY-NINE SEPTILLION",
            "NINE HUNDRED AND NINETY-NINE SEXTILLION",
            "NINE HUNDRED AND NINETY-NINE QUINTILLION",
            "NINE HUNDRED AND NINETY-NINE QUADRILLION",
            "NINE HUNDRED AND NINETY-NINE TRILLION",
            "NINE HUNDRED AND NINETY-NINE BILLION",
            "NINE HUNDRED AND NINETY-NINE MILLION",
            "NINE HUNDRED AND NINETY-NINE THOUSAND",
            "NINE HUNDRED AND NINETY-NINE DOLLARS",
            "AND NINETY-NINE CENTS");

        Assert.Equal(expected, _converter.Convert(ThirtySixNines + ".99"));
    }

    [Fact]
    public void Convert_writes_the_largest_allowed_negative_amount_with_minus()
    {
        string words = _converter.Convert("-" + ThirtySixNines + ".99");

        Assert.StartsWith("MINUS NINE HUNDRED AND NINETY-NINE DECILLION ", words);
        Assert.EndsWith(" NINE HUNDRED AND NINETY-NINE DOLLARS AND NINETY-NINE CENTS", words);
    }

    [Theory]
    [InlineData("-5", "MINUS FIVE DOLLARS")]
    [InlineData("-1", "MINUS ONE DOLLAR")]
    [InlineData("-123.45", "MINUS ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS")]
    [InlineData("-0.45", "MINUS ZERO DOLLARS AND FORTY-FIVE CENTS")]
    [InlineData("-0.01", "MINUS ZERO DOLLARS AND ONE CENT")]
    [InlineData("-1005", "MINUS ONE THOUSAND AND FIVE DOLLARS")]
    public void Convert_starts_negative_amounts_with_minus(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("-0")]
    [InlineData("-0.0")]
    [InlineData("-0.00")]
    [InlineData("-000")]
    public void Convert_writes_minus_zero_without_minus(string amount)
    {
        Assert.Equal("ZERO DOLLARS", _converter.Convert(amount));
    }

    [Theory]
    [InlineData("007", "SEVEN DOLLARS")]
    [InlineData("01", "ONE DOLLAR")]
    [InlineData("000123.45", "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS")]
    [InlineData("0001000", "ONE THOUSAND DOLLARS")]
    [InlineData("-007", "MINUS SEVEN DOLLARS")]
    public void Convert_ignores_leading_zeros(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData(" 123.45 ", "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS")]
    [InlineData("\t12\r\n", "TWELVE DOLLARS")]
    [InlineData("  -5", "MINUS FIVE DOLLARS")]
    public void Convert_trims_whitespace_around_the_amount(string amount, string expected)
    {
        Assert.Equal(expected, _converter.Convert(amount));
    }

    [Theory]
    [InlineData("", "Please enter an amount.")]
    [InlineData("   ", "Please enter an amount.")]
    [InlineData(
        "abc",
        "The amount must be a plain number such as 123.45 or -123.45, with no commas, spaces or symbols and with digits on both sides of any decimal point.")]
    [InlineData(
        "1,000",
        "The amount must be a plain number such as 123.45 or -123.45, with no commas, spaces or symbols and with digits on both sides of any decimal point.")]
    [InlineData(
        "12.",
        "The amount must be a plain number such as 123.45 or -123.45, with no commas, spaces or symbols and with digits on both sides of any decimal point.")]
    [InlineData("12.505", "The amount can have at most two decimal places.")]
    public void Convert_throws_with_the_validation_message_for_invalid_amounts(string amount, string expectedMessage)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _converter.Convert(amount));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Convert_throws_for_a_missing_amount()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _converter.Convert(null!));

        Assert.Equal("Please enter an amount.", exception.Message);
    }

    [Fact]
    public void Convert_throws_for_one_undecillion()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => _converter.Convert(OneUndecillion));

        Assert.Equal(
            "The amount is too large; it must be below one undecillion dollars, which allows at most 36 digits before the decimal point.",
            exception.Message);
    }

    [Fact]
    public void Convert_throws_for_minus_one_undecillion()
    {
        Assert.Throws<ArgumentException>(() => _converter.Convert("-" + OneUndecillion));
    }

    [Fact]
    public void Convert_can_be_used_through_its_interface()
    {
        IMoneyToWordsConverter converter = new MoneyToWordsConverter();

        Assert.Equal("FIVE DOLLARS", converter.Convert("5"));
    }
}
