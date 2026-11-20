using Microsoft.Playwright;

namespace DigitsToWords.E2E;

public sealed class HappyPathTests : IClassFixture<SiteFixture>
{
    [Fact]
    [Trait("Category", "E2E")]
    public async Task Entering_an_amount_and_clicking_convert_shows_the_words()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync(SiteFixture.BaseUrl);

        // The form only exists once the WebAssembly app has loaded, so wait for it first.
        var input = page.GetByTestId("amount-input");
        await input.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 60_000 });
        await Assertions.Expect(input).ToBeEnabledAsync(new() { Timeout = 60_000 });

        await input.FillAsync("123.45");
        await page.GetByTestId("convert-button").ClickAsync();

        await Assertions.Expect(page.GetByTestId("result"))
            .ToContainTextAsync("ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS", new() { Timeout = 30_000 });
    }
}
