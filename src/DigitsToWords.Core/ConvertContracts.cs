namespace DigitsToWords.Core;

/// <summary>The body of a request to the conversion API.</summary>
public sealed record ConvertRequest(string Amount);

/// <summary>The body of a successful response from the conversion API.</summary>
public sealed record ConvertResponse(string Words);

/// <summary>The body of an error response from the conversion API.</summary>
public sealed record ConvertError(string Error);
