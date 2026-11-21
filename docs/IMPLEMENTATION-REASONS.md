# Implementation reasons (All manually typed/no AI help beyond original template for this file)

## 1. Overview of the approach

Take a look at the 'initial-prompt.md' file to see the high level of my approach/design.

Use of AI
- I went with an agent orchestration technique I am now using which gives a good result fast whilst being efficient with tokens, this can be seen in the transcript.  
- It took roughly 45 minutes which included testing - it could have been quicker if I dropped the loop poll lower than 15 minutes and/or told it to kick off each task ASAP without waiting for the loop. 
- Fable high effort was used for transforming my original prompt shown in initial-prompt.md.  I've checked in the 'schedule-deferred-tasks' skill and agents used by claude into this repo.  
- It used a mix of models/efforts, the models it ended up using for the subagents were Fable, Opus and Sonnet.

- I also created a skill to make it easy for someone to interact with the final product, which I used myself to do some freestyle testing.

## 2. Why .NET 8 and Blazor

I went with .NET since c# was preferred, .NET 8 is relatively modern and stable and production ready, I also have experience using it already.
I went with Blazor instead of React so all code and the tests could be written using c#/xunit(rather than JS) on both backend and frontend.  Other option was to go with React but it doesn't share the tooling as much so would make the solution more complex.

## 3. Project structure and separation of concerns

The solution is broken into three projects/components.
- The 'Core' project is a portable c# project which is low on dependencies and consumed by the backend project.  It has a corresponding 'Core.Tests' project for its tests.
- The 'Web' project is the backend AspNetCore project which creates the '/api/convert' endpoint for communicating with front end. It has a 
- The 'Web.Client' project is the Blazor project which contains the frontend logic and dependencies.

## 4. The conversion algorithm

- Since I had to come up with my own opinionated design of the algorithmn, I thought about how the numbers would map to english words. I decided to use the decimal point and/or last index of the number as an anchor.
- I went with using a string so it was easy for parsing the number by its index, it also made the specification more specific so Claude could follow my direction, the code would also be more readable than using bitwise operations on number types and have less concern about the upper and lower limit of the number to support.
- I decided to only support under undecillion dollars, since I thought this amount was reasonable when talking about dollars and cents, and it was not straight forward for me on how to support words dynamically so that there would be no upper limit.
- I went over the final implementation by reading it and then running some tests with the debugger to step through the algorithmn and understand exactly how the conditional logic worked.

## 5. Validation rules and where validation happens

- I wanted some basic helpful validation to be put on the frontend so the user would have an idea of what input was supported and could modify the input to get a result.
- The backend also has some validation but leaves the user readable errors to the front end to not duplicate logic or load the server.

## 7. Testing strategy

- I went with xunit as much as possible since I have a lot of experience using it.
- A 'pyramid' like structure of tests was made with the projects with least dependencies and quickest to test having the most tests and projects with the most dependencies and slowest to run having the fewest.
- I tried to cover the core business logic since this could change and not be obvious to a developer/LLM even though it had essentially broken the site's behaviour.