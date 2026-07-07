using Microsoft.Extensions.Options;
using RetryDelays;
using Intro;

var builder = WebApplication.CreateBuilder(args);

// Bind IRetryConfiguration from the "RetryConfiguration" section as a whole object
var retryConfig = builder.Configuration
    .GetSection("RetryConfiguration")
    .Get<RetryConfiguration>() ?? new RetryConfiguration();

builder.Services.AddSingleton<IRetryConfiguration>(retryConfig);
builder.Services.AddSingleton<RetryDelayAnalysisService>();

// Add individual options (kept for the existing endpoints)
builder.Services.Configure<ConstantRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:Constant"));
builder.Services.Configure<ExponentialRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:Exponential"));
builder.Services.Configure<LinearRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:Linear"));
builder.Services.Configure<TimeSeriesRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:TimeSeries"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Constant retry delay endpoint
app.MapGet("/constant-retry", (IOptions<ConstantRetryDelayOptions> options) =>
{
    var delay = new ConstantRetryDelay(options.Value);
    return new
    {
        Type = "Constant",
		options.Value.BaseDelay,
		options.Value.UseJitter,
		options.Value.MaxDelay,
        NextDelay = delay.GetDelay(1)
    };
});

// Exponential retry delay endpoint
app.MapGet("/exponential-retry", (IOptions<ExponentialRetryDelayOptions> options) =>
{
    var delay = new ExponentialRetryDelay(options.Value);
    return new
    {
        Type = "Exponential",
		options.Value.BaseDelay,
        options.Value.ExponentialFactor,
        options.Value.UseJitter,
        options.Value.MaxDelay,
        NextDelay = delay.GetDelay(1)
    };
});

// Linear retry delay endpoint
app.MapGet("/linear-retry", (IOptions<LinearRetryDelayOptions> options) =>
{
    var delay = new LinearRetryDelay(options.Value);
    return new
    {
        Type = "Linear",
        options.Value.BaseDelay,
        options.Value.SlopeFactor,
        options.Value.UseJitter,
        options.Value.MaxDelay,
        NextDelay = delay.GetDelay(1)
    };
});

// TimeSeries retry delay endpoint
app.MapGet("/timeseries-retry", (IOptions<TimeSeriesRetryDelayOptions> options) =>
{
    var delay = new TimeSeriesRetryDelay(options.Value);
    return new
    {
        Type = "TimeSeries",
        options.Value.BaseDelay,
        options.Value.UseJitter,
        options.Value.MaxDelay,
        options.Value.Times,
        NextDelay = delay.GetDelay(1)
    };
});

// Retry analysis endpoint — uses IRetryConfiguration via RetryDelayAnalysisService
app.MapGet("/retry-analysis", (RetryDelayAnalysisService analysisService, int attempts = 5) =>
{
    var result = analysisService.Analyse(attempts);
    return result;
});

await app.RunAsync();