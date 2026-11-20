using System.Diagnostics;
using System.Net.Http;

namespace DigitsToWords.E2E;

/// <summary>Starts the real website on a fixed local port for the tests and stops it, with all its child processes, afterwards.</summary>
public sealed class SiteFixture : IAsyncLifetime
{
    public const string BaseUrl = "http://localhost:5199";

    private Process? _process;

    public async Task InitializeAsync()
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = FindRepositoryRoot(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (var argument in new[] { "run", "--project", "src/DigitsToWords.Web", "--no-launch-profile", "--urls", BaseUrl })
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("The website process did not start.");
        // Drain the output so the process never blocks on a full pipe.
        _process.OutputDataReceived += (_, _) => { };
        _process.ErrorDataReceived += (_, _) => { };
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        await WaitUntilRespondingAsync();
    }

    public Task DisposeAsync()
    {
        if (_process is { HasExited: false })
        {
            _process.Kill(entireProcessTree: true);
            _process.WaitForExit(10_000);
        }
        _process?.Dispose();
        return Task.CompletedTask;
    }

    private async Task WaitUntilRespondingAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var deadline = DateTime.UtcNow.AddSeconds(120);
        while (DateTime.UtcNow < deadline)
        {
            if (_process!.HasExited)
            {
                throw new InvalidOperationException($"The website stopped early with exit code {_process.ExitCode}. Is port 5199 already in use?");
            }
            try
            {
                using var response = await client.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }
            await Task.Delay(500);
        }
        throw new TimeoutException("The website did not respond within 120 seconds.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DigitsToWords.sln")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find DigitsToWords.sln above the test output folder.");
    }
}
