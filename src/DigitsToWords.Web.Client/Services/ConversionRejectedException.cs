namespace DigitsToWords.Web.Client.Services;

/// <summary>Thrown when the server refuses to convert an amount because it is not valid.</summary>
public sealed class ConversionRejectedException(string message) : Exception(message);
