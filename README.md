# Digits to words

This is a small website that turns a dollar amount written in digits into words. For example, `123.45` becomes `ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS`.

The site is built with .NET 10 and Blazor. The page runs in the browser as WebAssembly and sends the amount to an API on the server, which checks it again and does the conversion. The conversion code is written by hand and uses no number-to-words library. The author's reasons for the main design choices are in [docs/IMPLEMENTATION-REASONS.md](docs/IMPLEMENTATION-REASONS.md).

## Prerequisites

- The .NET 10 SDK (version 10.0.100 or later in the 10.x line). The `global.json` file at the root of the repository tells the `dotnet` command to use it. Check it with `dotnet --version`.
- PowerShell (`pwsh`), which is needed only to install the browser used by the end-to-end test.

## Build

Run this from the repository root:

```
dotnet build DigitsToWords.sln
```

## Run and host it locally

Start the site with:

```
dotnet run --project src/DigitsToWords.Web
```

The site is then at http://localhost:5076 (the first profile in `launchSettings.json`). Open that address in a browser to use it, and stop the site with Ctrl+C.

If you use Claude Code, you can instead type `/launch-site` or ask Claude to launch the site. The project's `launch-site` skill, in `.claude/skills/launch-site/SKILL.md`, builds and starts the site in the background, waits until it answers, and opens it in your default browser. Ask Claude to stop the site when you have finished.

To publish it for hosting, run this from the repository root:

```
dotnet publish src/DigitsToWords.Web -c Release -o publish
```

The `publish` folder then holds everything the site needs, including the WebAssembly files. On the hosting machine, install the ASP.NET Core 8 runtime and start it from inside that folder:

```
dotnet DigitsToWords.Web.dll
```

By default the published site listens on http://localhost:5000. Set the `ASPNETCORE_URLS` environment variable to change the address, for example `ASPNETCORE_URLS=http://localhost:5080`.

For public use, run the site behind a reverse proxy such as nginx, IIS or a cloud load balancer. The proxy should accept HTTPS with a real certificate and pass requests on to the site over local HTTP. The site itself does not set up HTTPS or any forwarded-header handling, so if the proxy must pass the original scheme or client address along, that setting has to be added to the site first.

## How to use it

1. Open the site in a browser.
2. Type an amount into the "Amount in dollars" box.
3. Click Convert. The words appear below the box. If the amount is not acceptable, a message explaining why appears instead.

The input rules are:

- Use digits only, with an optional minus sign at the start and at most one decimal point.
- Give at most two decimal places. A decimal point must have digits on both sides, so `12.` and `.5` are refused.
- Do not use commas, dollar signs, plus signs, exponents or spaces inside the number. Spaces at the start or end are ignored.
- The amount must be below one undecillion (a 1 followed by 36 zeros), so at most 36 digits before the decimal point. Leading zeros are ignored.

These examples are the actual output of the running site:

| Input | Output |
| --- | --- |
| `123.45` | ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS |
| `0.45` | FORTY-FIVE CENTS |
| `1.01` | ONE DOLLAR AND ONE CENT |
| `12.5` | TWELVE DOLLARS AND FIFTY CENTS |
| `1005` | ONE THOUSAND AND FIVE DOLLARS |
| `-5` | MINUS FIVE DOLLARS |
| `-0.45` | MINUS FORTY-FIVE CENTS |
| `12.505` | (refused) The amount can have at most two decimal places. |

The word "AND" appears before a last group of fewer than one hundred (`1005` is `ONE THOUSAND AND FIVE DOLLARS`), but not between larger groups, so `1005000` reads `ONE MILLION FIVE THOUSAND DOLLARS`.

## Run the tests

The tests run locally. No GitHub workflow or other automated pipeline runs them.

To run the unit and API tests (everything except the browser test), run this from the repository root:

```
dotnet test DigitsToWords.sln --filter "Category!=E2E"
```

To run the end-to-end test on its own, first build it and install the Chromium browser it uses (this only needs doing once per machine):

```
dotnet build tests/DigitsToWords.E2E
pwsh tests/DigitsToWords.E2E/bin/Debug/net10.0/playwright.ps1 install chromium
```

Then run it:

```
dotnet test tests/DigitsToWords.E2E
```

The end-to-end test starts the site itself on http://localhost:5199, so make sure nothing else is using port 5199 and that no copy of the site is already running. It stops the site when it finishes. Starting the site can take a while the first time because it builds first.

The end-to-end test is marked with the trait `Category=E2E`. Running `dotnet test DigitsToWords.sln` with no filter runs all the tests including it. To run only the tests with that trait, use `dotnet test DigitsToWords.sln --filter "Category=E2E"`. The other test projects report that no test matches, which is expected.

## Test plan

### What each test project covers

- `tests/DigitsToWords.Core.Tests` (225 tests) checks the validator and the converter on their own, with no website involved.
- `tests/DigitsToWords.Web.Tests` (13 tests) checks the page with bUnit (a library that renders Blazor components in a test) using a fake API client, checks the client that calls the API, and checks the `POST /api/convert` endpoint through an in-memory copy of the site, using fake validator and converter classes.
- `tests/DigitsToWords.E2E` (1 test) uses Playwright to drive a real headless Chromium browser against the real site.

### Cases tested

Valid conversions (Core):

- The example from the original request, zero, `0.00`, and values with leading zeros.
- Singular `ONE DOLLAR` and `ONE CENT` against plural forms, amounts under one dollar written as cents only (`0.45` is `FORTY-FIVE CENTS`), one decimal place (`12.5` is fifty cents), and no cents part when the cents are zero.
- Every single digit, `10` to `19`, every exact ten, hyphenated tens and units, and hundreds with and without "AND".
- "AND" before a last group under one hundred, and no "AND" between larger groups.
- Every scale word from THOUSAND to DECILLION, including tens and hundreds of each, and one amount that uses them all in order.
- Negative amounts starting with `MINUS` (`-0.45` is `MINUS FORTY-FIVE CENTS`), and `-0` and `-0.00` written as plain zero.
- Whitespace around the amount is ignored.

Boundaries (Core):

- The largest allowed amount (36 nines with `.99`) and its negative are accepted and converted.
- One undecillion and minus one undecillion are rejected, including when written with cents or leading zeros, and amounts far above the limit.
- Leading zeros do not count towards the 36-digit limit.

Rejections (Core):

- Empty, blank and missing input.
- Text that is not a plain number: letters, commas, dollar signs, inner spaces, plus signs, double minus signs, a lone minus or decimal point, `12.`, `.5`, two decimal points, exponents, hexadecimal, non-ASCII digits, `NaN` and `Infinity`.
- More than two decimal places, and the rule that this message is given before the size message when both apply.
- The exact message for each rule.

Front-end behaviour (Web.Tests):

- The page has a labelled input, a Convert button, and result and error areas that announce changes to screen readers.
- Invalid input shows the validator's message and does not call the API.
- Valid input shows the words returned by the API.
- A failing API shows a friendly message, and a rejection from the server shows the server's message.
- A new submit clears the previous result.
- The API client sends the amount, returns the words, and turns a 400 reply or a server error into the right failure.

API behaviour (Web.Tests):

- A valid amount returns 200 with the words.
- Invalid amounts return 400 with the validator's message.
- A request with no amount returns 400.

End-to-end happy path (E2E):

- In a real browser, type `123.45`, click Convert, and see `ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS` in the result area.

### How this supports production readiness

The site runs on .NET 10, which Microsoft supports with security fixes, and a dependency scan found no NuGet packages with known vulnerabilities in any project. Repeat the scan before each release with this command:

```
dotnet list DigitsToWords.sln package --vulnerable --include-transitive
```

The conversion rules, which are the part most likely to be wrong in a way users notice, are tested across every word, every boundary and every rejection rule. The server validates every request itself, and the API tests show it refuses bad input with a clear message whatever the page does. The page tests show that users see an error or a friendly failure message instead of a broken screen. The end-to-end test shows the pieces are wired together: the WebAssembly page loads, calls the real API and shows the result. Together they give good confidence that the site does what it says for normal use. They do not, on their own, prove it is ready for heavy or hostile public traffic, as the list below shows.

### Not yet covered

- Load, stress and soak testing. Nothing measures how the site behaves under many users or very large request volumes.
- Cross-browser and mobile testing. The end-to-end test runs only in Chromium.
- An accessibility audit. The page uses a label and live regions, but it has not been checked with a tool or a screen reader.
- Deeper security testing, such as a penetration test, and a review of rate limiting, request size limits and security headers.
- Only one end-to-end test exists, so error messages and other cases are not checked in a real browser.
- Tests of the published Release build, of HTTPS behind a reverse proxy, and of monitoring and logging.
- Tests of slow or lost network connections, and of the WebAssembly download size and start-up time.
- Automatic runs. The tests run only when someone runs them locally, so nothing stops a change from being merged without them.
