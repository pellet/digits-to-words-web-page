using DigitsToWords.Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DigitsToWords.Web.Api;

/// <summary>Maps the <c>POST /api/convert</c> endpoint that turns a dollar amount into words.</summary>
public static class ConvertEndpoint
{
    /// <summary>Adds the conversion endpoint to the application's routes.</summary>
    public static IEndpointRouteBuilder MapConvertEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/convert", Convert);
        return endpoints;
    }

    private static Results<Ok<ConvertResponse>, BadRequest<ConvertError>> Convert(
        ConvertRequest? request,
        IMoneyValidator validator,
        IMoneyToWordsConverter converter)
    {
        var validation = validator.Validate(request?.Amount);
        if (!validation.IsValid)
        {
            return TypedResults.BadRequest(new ConvertError(validation.ErrorMessage ?? "The amount is not valid."));
        }

        try
        {
            var words = converter.Convert(validation.NormalisedValue ?? request!.Amount);
            return TypedResults.Ok(new ConvertResponse(words));
        }
        catch (ArgumentException)
        {
            // The converter rejected an amount the validator accepted, so report it as invalid input.
            return TypedResults.BadRequest(new ConvertError("The amount could not be converted."));
        }
    }
}
