using Bunit;
using DigitsToWords.Core;
using DigitsToWords.Web.Client.Pages;
using DigitsToWords.Web.Client.Services;
using DigitsToWords.Web.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace DigitsToWords.Web.Tests;

/// <summary>Component tests for the home page, using fake validation and a fake API client.</summary>
public sealed class HomePageTests : BunitContext
{
    private const string ValidAmount = "123.45";

    private readonly FakeConversionApiClient apiClient = new();

    public HomePageTests()
    {
        Services.AddSingleton<IMoneyValidator>(new FakeMoneyValidator(ValidAmount));
        Services.AddSingleton<IConversionApiClient>(apiClient);
    }

    [Fact]
    public void Page_has_a_labelled_input_a_convert_button_and_live_output_areas()
    {
        var page = Render<Home>();

        Assert.Equal("amount-input", page.Find("label").GetAttribute("for"));
        Assert.Equal("Convert", page.Find("#convert-button").TextContent.Trim());
        Assert.Equal("assertive", page.Find("#error-message").GetAttribute("aria-live"));
        Assert.Equal("polite", page.Find("#result").GetAttribute("aria-live"));
    }

    [Fact]
    public void Invalid_input_shows_the_validator_message_and_does_not_call_the_api()
    {
        var page = Render<Home>();

        page.Find("#amount-input").Input("12.505");
        page.Find("form").Submit();

        Assert.Equal(FakeMoneyValidator.RejectionMessage, page.Find("#error-message").TextContent);
        Assert.Equal("true", page.Find("#amount-input").GetAttribute("aria-invalid"));
        Assert.Empty(page.Find("#result").TextContent);
        Assert.Empty(apiClient.ReceivedAmounts);
    }

    [Fact]
    public void Valid_input_shows_the_words_returned_by_the_api()
    {
        apiClient.Words = "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS";
        var page = Render<Home>();

        page.Find("#amount-input").Input(ValidAmount);
        page.Find("form").Submit();

        page.WaitForAssertion(() => Assert.Equal(apiClient.Words, page.Find("#result").TextContent));
        Assert.Empty(page.Find("#error-message").TextContent);
        Assert.Equal([ValidAmount], apiClient.ReceivedAmounts);
    }

    [Fact]
    public void Api_failure_shows_a_friendly_message()
    {
        apiClient.ExceptionToThrow = new HttpRequestException("Connection refused.");
        var page = Render<Home>();

        page.Find("#amount-input").Input(ValidAmount);
        page.Find("form").Submit();

        page.WaitForAssertion(() => Assert.Equal(Home.ApiFailureMessage, page.Find("#error-message").TextContent));
        Assert.Empty(page.Find("#result").TextContent);
    }

    [Fact]
    public void Server_rejection_shows_the_server_message()
    {
        apiClient.ExceptionToThrow = new ConversionRejectedException("The server says no.");
        var page = Render<Home>();

        page.Find("#amount-input").Input(ValidAmount);
        page.Find("form").Submit();

        page.WaitForAssertion(() => Assert.Equal("The server says no.", page.Find("#error-message").TextContent));
    }

    [Fact]
    public void A_new_submit_clears_the_previous_result()
    {
        var page = Render<Home>();
        page.Find("#amount-input").Input(ValidAmount);
        page.Find("form").Submit();
        page.WaitForAssertion(() => Assert.NotEmpty(page.Find("#result").TextContent));

        page.Find("#amount-input").Input("not a number");
        page.Find("form").Submit();

        Assert.Empty(page.Find("#result").TextContent);
        Assert.Equal(FakeMoneyValidator.RejectionMessage, page.Find("#error-message").TextContent);
    }
}
