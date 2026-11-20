using DigitsToWords.Core;
using DigitsToWords.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<IMoneyValidator, MoneyValidator>();
builder.Services.AddSingleton<IMoneyToWordsConverter, MoneyToWordsConverter>();

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IConversionApiClient, ConversionApiClient>();

await builder.Build().RunAsync();
