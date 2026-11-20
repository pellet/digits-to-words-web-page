namespace DigitsToWords.Core;

/// <summary>Turns a dollar amount into capitalised English words by reading the digits of the text one position at a time.</summary>
public sealed class MoneyToWordsConverter : IMoneyToWordsConverter
{
    private const int DigitsPerGroup = 3;

    private static readonly string[] UnitWords =
    [
        "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE",
    ];

    private static readonly string[] TeenWords =
    [
        "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN",
    ];

    private static readonly string[] TensWords =
    [
        "", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY",
    ];

    /// <summary>The word for each group of three digits, counted from the right, where group zero has no word.</summary>
    private static readonly string[] ScaleWords =
    [
        "", "THOUSAND", "MILLION", "BILLION", "TRILLION", "QUADRILLION", "QUINTILLION",
        "SEXTILLION", "SEPTILLION", "OCTILLION", "NONILLION", "DECILLION",
    ];

    private static readonly MoneyValidator Validator = new();

    /// <summary>
    /// Converts an amount such as "123.45" to "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS".
    /// Throws <see cref="ArgumentException"/> carrying the validator's message when the amount is invalid.
    /// </summary>
    public string Convert(string amount)
    {
        MoneyValidationResult validation = Validator.Validate(amount);
        if (!validation.IsValid)
        {
            throw new ArgumentException(validation.ErrorMessage);
        }

        // The validated value always looks like "-123.45": an optional minus sign, the dollars, a point and two cent digits.
        string value = validation.NormalisedValue!;
        bool isNegative = value[0] == '-';
        string unsignedValue = isNegative ? value[1..] : value;
        int pointIndex = unsignedValue.IndexOf('.');
        string dollarDigits = unsignedValue[..pointIndex];
        string centDigits = unsignedValue[(pointIndex + 1)..];

        var words = new List<string>();
        if (isNegative)
        {
            words.Add("MINUS");
        }

        words.AddRange(WholeNumberWords(dollarDigits));
        words.Add(dollarDigits == "1" ? "DOLLAR" : "DOLLARS");

        if (centDigits != "00")
        {
            words.Add("AND");
            words.Add(WordsBelowOneHundred(centDigits[0], centDigits[1]));
            words.Add(centDigits == "01" ? "CENT" : "CENTS");
        }

        return string.Join(' ', words);
    }

    /// <summary>Returns the words for a whole number of any length that has no leading zeros.</summary>
    private static List<string> WholeNumberWords(string digits)
    {
        if (digits == "0")
        {
            return [UnitWords[0]];
        }

        var words = new List<string>();
        int groupCount = (digits.Length + DigitsPerGroup - 1) / DigitsPerGroup;

        for (int groupIndex = groupCount - 1; groupIndex >= 0; groupIndex--)
        {
            int unitsPlace = groupIndex * DigitsPerGroup;
            char hundreds = DigitFromRight(digits, unitsPlace + 2);
            char tens = DigitFromRight(digits, unitsPlace + 1);
            char units = DigitFromRight(digits, unitsPlace);

            if (hundreds == '0' && tens == '0' && units == '0')
            {
                continue;
            }

            // A last group below one hundred is joined to the larger groups with "AND", as in "ONE THOUSAND AND FIVE".
            bool isLastGroupBelowOneHundred = groupIndex == 0 && hundreds == '0';
            if (isLastGroupBelowOneHundred && words.Count > 0)
            {
                words.Add("AND");
            }

            words.AddRange(ThreeDigitGroupWords(hundreds, tens, units));

            string scaleWord = ScaleWord(groupIndex);
            if (scaleWord.Length > 0)
            {
                words.Add(scaleWord);
            }
        }

        return words;
    }

    /// <summary>Returns the digit that sits the given number of places from the right-hand end, or '0' when the number is shorter than that.</summary>
    private static char DigitFromRight(string digits, int placesFromRight)
    {
        int index = digits.Length - 1 - placesFromRight;
        return index >= 0 ? digits[index] : '0';
    }

    /// <summary>Returns the words for one group of three digits, such as "ONE HUNDRED AND TWENTY-THREE", or nothing when all three are zero.</summary>
    private static List<string> ThreeDigitGroupWords(char hundreds, char tens, char units)
    {
        var words = new List<string>();
        bool hasHundreds = hundreds != '0';
        bool hasTensOrUnits = tens != '0' || units != '0';

        if (hasHundreds)
        {
            words.Add(UnitWords[DigitValue(hundreds)]);
            words.Add("HUNDRED");
        }

        if (hasHundreds && hasTensOrUnits)
        {
            words.Add("AND");
        }

        if (hasTensOrUnits)
        {
            words.Add(WordsBelowOneHundred(tens, units));
        }

        return words;
    }

    /// <summary>Returns the word for a two-digit number from its tens digit and units digit, such as "FORTY-FIVE", "TWELVE" or "FIVE".</summary>
    private static string WordsBelowOneHundred(char tens, char units)
    {
        if (tens == '0')
        {
            return UnitWords[DigitValue(units)];
        }

        if (tens == '1')
        {
            return TeenWords[DigitValue(units)];
        }

        string tensWord = TensWords[DigitValue(tens)];
        return units == '0' ? tensWord : $"{tensWord}-{UnitWords[DigitValue(units)]}";
    }

    /// <summary>Returns the scale word for a group of three digits counted from the right, which is empty for group zero and "THOUSAND" for group one.</summary>
    private static string ScaleWord(int groupIndex) => ScaleWords[groupIndex];

    /// <summary>Converts a digit character such as '7' to its number, 7.</summary>
    private static int DigitValue(char digit) => digit - '0';
}
