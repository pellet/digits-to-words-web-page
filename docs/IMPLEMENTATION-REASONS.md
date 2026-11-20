# Implementation reasons

## 1. Overview of the approach

- In a few sentences, how does the solution work from the moment a number is typed to the moment the words are shown?
- What was the guiding idea behind the design?

_Your answer here._

## 2. Why .NET 8 and Blazor

I went with .NET since c# was preferred, .NET 8 is relatively modern and stable and production ready, I also have experience using it already.
I went with Blazor instead of React so all code and the tests could be written using c#/xunit(rather than JS) on both backend and frontend.  Other option was to go with React but it doesn't share the tooling as much so would make the solution more complex.

## 3. Project structure and separation of concerns

The solution is broken into three projects/components.
- The 'Core' project is a portable c# project which is low on dependencies and consumed by the backend project.  It has a corresponding 'Core.Tests' project for its tests.
- The 'Web' project is the backend AspNetCore project which creates the '/api/convert' endpoint for communicating with front end. It has a 
- The 'Web.Client' project is the Blazor project which contains the frontend logic and dependencies.

## 4. The conversion algorithm

- Why does the algorithm work on the number as a string and use each digit's position counted from the right, instead of parsing it into a numeric type?
- Why did you write it yourself with no libraries or NuGet packages?
- How are the scale words (THOUSAND, MILLION and so on) and the word "AND" handled?

_Your answer here._

## 5. Validation rules and where validation happens

- Which rules does the validator enforce, and why those?
- Why is validation done in both the browser (client) and the API (server)?

_Your answer here._

## 6. Interpretation decisions on ambiguous requirements

The brief left some points open. For each default below, say why you accepted it or what you would change.

- **At most two decimal places** ("12.5" and "12.50" are valid, "12.505" is not; "12." and ".5" are rejected).

  _Your answer here._

- **Singular DOLLAR and CENT** ("ONE DOLLAR", "ONE CENT").

  _Your answer here._

- **"MINUS" prefix for negative values** ("-5" gives "MINUS FIVE DOLLARS", while "-0" gives "ZERO DOLLARS").

  _Your answer here._

- **"ZERO DOLLARS"** for zero ("0" gives "ZERO DOLLARS", and "0.45" gives "ZERO DOLLARS AND FORTY-FIVE CENTS").

  _Your answer here._

- **Placement of "AND"** (after the hundreds, and before a final group under one hundred when larger groups come first).

  _Your answer here._

- **No cents part when the cents are zero** ("12.00" gives "TWELVE DOLLARS").

  _Your answer here._

- **Rejecting commas and the "$" sign** in the input.

  _Your answer here._

- **Upper limit** (values must be below one undecillion).

  _Your answer here._

## 7. Testing strategy

- Why these tools: xUnit for the logic, bUnit for the page, WebApplicationFactory for the API, and Playwright for the end-to-end check?
- Why keep the end-to-end test minimal, and why are the tests run locally only rather than on GitHub?

_Your answer here._

## 8. Alternatives considered and rejected

- Which other approaches did you consider (for example a number-to-words library, Blazor Server, or parsing into a numeric type)?
- Why did you reject each one?

_Your answer here._

## 9. Known limitations and future improvements

- What does the solution not do, or not do well?
- What would you add or change with more time?

_Your answer here._

## 10. Time spent and trade-offs

- How long did you spend, and where did the time go?
- What did you trade away to finish in that time?

_Your answer here._
