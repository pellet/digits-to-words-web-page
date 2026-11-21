using DigitsToWords.Core;
using DigitsToWords.Web.Api;
using DigitsToWords.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSingleton<IMoneyValidator, MoneyValidator>();
builder.Services.AddSingleton<IMoneyToWordsConverter, MoneyToWordsConverter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapConvertEndpoint();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(DigitsToWords.Web.Client._Imports).Assembly);

app.Run();

/// <summary>Makes the entry point visible to the integration tests.</summary>
public partial class Program { }
