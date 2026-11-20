---
name: launch-site
description: Build and start the digits-to-words site locally, wait until it answers, and open it in the user's browser so they can try it. Use when the user says "launch the site", "run the site", "start the site", "let me try it", or "open the site". Also stops the site when asked.
---

# Launch the site locally

Starts the Blazor site from `src/DigitsToWords.Web` on http://localhost:5076 and opens it in the default browser. Run every command from the repository root.

## Starting

1. **Check whether it is already running.** In PowerShell, run `Get-NetTCPConnection -LocalPort 5076 -State Listen -ErrorAction SilentlyContinue`. If something is listening and `Invoke-WebRequest http://localhost:5076 -UseBasicParsing` returns status 200, the site is already up: skip to step 4. If something else holds the port, tell the user which process it is (`Get-Process -Id <OwningProcess>`) and stop; do not kill it without asking.
2. **Start the site in the background** with the Bash or PowerShell tool and `run_in_background: true`:

   ```
   dotnet run --project src/DigitsToWords.Web --launch-profile http
   ```

   This builds the site first, so the first start can take a minute. Keep the background task ID; it is needed to stop the site.
3. **Wait until it answers.** Poll http://localhost:5076 until it returns status 200, for up to 120 seconds, using a single PowerShell loop:

   ```powershell
   $deadline = (Get-Date).AddSeconds(120)
   do {
     try { if ((Invoke-WebRequest http://localhost:5076 -UseBasicParsing -TimeoutSec 5).StatusCode -eq 200) { 'ready'; break } } catch {}
     Start-Sleep -Seconds 2
   } while ((Get-Date) -lt $deadline)
   ```

   If it never prints `ready`, read the background task's output for the build or start-up error, report it, and stop.
4. **Open the browser** with `Start-Process http://localhost:5076`.
5. **Tell the user** in two or three lines: the URL, that the form appears once the in-browser app has loaded, one example to try ("123.45" gives "ONE HUNDRED AND TWENTY-THREE DOLLARS AND FORTY-FIVE CENTS"), and that they can ask you to stop the site.

## Stopping

When the user asks to stop the site, end the background task with `TaskStop` using the saved task ID. Then check that nothing is still listening on port 5076; if a leftover `DigitsToWords.Web` process is, stop that process and say so.

## Notes

- The Playwright end-to-end test uses port 5199, not 5076, so the site can stay running while the tests run.
- Do not use the `https` launch profile; it needs a trusted development certificate that may not be installed.
