# RetryDelays Sample

This sample demonstrates how to use the RetryDelays library with a Minimal API and configuration options.  
It shows two configuration approaches side by side and includes a service that analyses all strategies at once.

## Project structure

| File | Purpose |
|------|---------|
| `IRetryConfiguration.cs` | Interface exposing five named delay options (`DelayOption1`–`DelayOption5`) |
| `RetryConfiguration.cs` | Concrete implementation — bound directly from the `RetryConfiguration` config section |
| `RetryDelayAnalysisService.cs` | Service that accepts `IRetryConfiguration` and builds a per-attempt delay schedule for all five strategies |
| `Program.cs` | Wires everything together and maps all endpoints |

## Configuration

### `RetryConfiguration` section — unified configuration (used by `/retry-analysis`)

All five delay options live under a single `RetryConfiguration` key and are bound to `IRetryConfiguration` as a whole object.

```json
{
  "RetryConfiguration": {
    "DelayOption1": {
      "BaseDelay": "00:00:02",
      "UseJitter": false,
      "MaxDelay": "00:00:30"
    },
    "DelayOption2": {
      "BaseDelay": "00:00:01",
      "UseJitter": true,
      "MaxDelay": "00:01:00",
      "ExponentialFactor": 2.0
    },
    "DelayOption3": {
      "BaseDelay": "00:00:01",
      "UseJitter": false,
      "MaxDelay": "00:00:30",
      "SlopeFactor": 1.5
    },
    "DelayOption4": {
      "BaseDelay": "00:00:00",
      "UseJitter": false,
      "MaxDelay": "00:01:00",
      "Times": [ "00:00:01", "00:00:02", "00:00:04", "00:00:08" ]
    },
    "DelayOption5": {
      "BaseDelay": "00:00:00",
      "UseJitter": false,
      "MaxDelay": "00:01:00",
      "Times": [ "00:00:01", "00:00:03", "00:00:10" ]
    }
  }
}
```

| Property | Delay type |
|----------|-----------|
| `DelayOption1` | Constant |
| `DelayOption2` | Exponential |
| `DelayOption3` | Linear |
| `DelayOption4` | TimeSeries |
| `DelayOption5` | Periodic |

> Note: `TimeSeries` and `Periodic` both take a `Times` array. The difference is in what happens when attempts exceed the length: TimeSeries keeps the last value, while Periodic cycles back to the first value.

### `RetryDelays` section — individual options (used by the per-strategy endpoints)

```json
{
  "RetryDelays": {
    "Constant":     { "BaseDelay": "00:00:02", "UseJitter": false, "MaxDelay": "00:00:30" },
    "Exponential":  { "BaseDelay": "00:00:01", "UseJitter": true,  "MaxDelay": "00:01:00", "ExponentialFactor": 2.0 },
    "Linear":       { "BaseDelay": "00:00:01", "UseJitter": false, "MaxDelay": "00:00:30", "SlopeFactor": 1.5 },
    "TimeSeries":   { "BaseDelay": "00:00:00", "UseJitter": false, "MaxDelay": "00:01:00", "Times": ["00:00:01","00:00:02","00:00:04","00:00:08"] },
    "Periodic":     { "BaseDelay": "00:00:00", "UseJitter": false, "MaxDelay": "00:01:00", "Times": ["00:00:01","00:00:03","00:00:10"] }
  }
}
```

## DI registration

```csharp
// Bind IRetryConfiguration from the whole "RetryConfiguration" section
var retryConfig = builder.Configuration
    .GetSection("RetryConfiguration")
    .Get<RetryConfiguration>() ?? new RetryConfiguration();

builder.Services.AddSingleton<IRetryConfiguration>(retryConfig);
builder.Services.AddSingleton<RetryDelayAnalysisService>();

// Individual options bound from the "RetryDelays" section
builder.Services.Configure<ConstantRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:Constant"));
builder.Services.Configure<ExponentialRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:Exponential"));
builder.Services.Configure<LinearRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:Linear"));
builder.Services.Configure<TimeSeriesRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:TimeSeries"));
builder.Services.Configure<PeriodicRetryDelayOptions>(
    builder.Configuration.GetSection("RetryDelays:Periodic"));
```

## Endpoints

### Per-strategy endpoints (use `RetryDelays` config section)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/constant-retry` | Delay info and next delay for attempt 1 using Constant strategy |
| `GET` | `/exponential-retry` | Delay info and next delay for attempt 1 using Exponential strategy |
| `GET` | `/linear-retry` | Delay info and next delay for attempt 1 using Linear strategy |
| `GET` | `/timeseries-retry` | Delay info and next delay for attempt 1 using TimeSeries strategy |
| `GET` | `/periodic-retry` | Delay info, configured `Times`, and a delay sample for `2 × Times.Length` attempts so the wrap-around cycling of the Periodic strategy is visible |

#### Example response — `GET /periodic-retry`

```json
{
  "type": "Periodic",
  "baseDelay": "00:00:00",
  "useJitter": false,
  "maxDelay": "00:01:00",
  "times": [ "00:00:01", "00:00:03", "00:00:10" ],
  "sampleDelays": [
    { "attempt": 0, "delay": "00:00:01" },
    { "attempt": 1, "delay": "00:00:03" },
    { "attempt": 2, "delay": "00:00:10" },
    { "attempt": 3, "delay": "00:00:01" },
    { "attempt": 4, "delay": "00:00:03" },
    { "attempt": 5, "delay": "00:00:10" }
  ],
  "nextDelay": "00:00:01"
}
```

### Analysis endpoint (uses `RetryConfiguration` via `IRetryConfiguration`)

| Method | Route | Query params | Description |
|--------|-------|--------------|-------------|
| `GET` | `/retry-analysis` | `attempts` (default `5`) | Runs `RetryDelayAnalysisService.Analyse()` across all five options and returns a full delay schedule |

#### Example response — `GET /retry-analysis?attempts=3`

```json
{
  "delayOption1": {
    "type": "Constant",
    "attempts": [
      { "attempt": 0, "delaySeconds": 2.0 },
      { "attempt": 1, "delaySeconds": 2.0 },
      { "attempt": 2, "delaySeconds": 2.0 }
    ],
    "totalWaitSeconds": 6.0
  },
  "delayOption2": {
    "type": "Exponential",
    "attempts": [
      { "attempt": 0, "delaySeconds": 1.0 },
      { "attempt": 1, "delaySeconds": 2.0 },
      { "attempt": 2, "delaySeconds": 4.0 }
    ],
    "totalWaitSeconds": 7.0
  },
  "delayOption3": {
    "type": "Linear",
    "attempts": [
      { "attempt": 0, "delaySeconds": 1.5 },
      { "attempt": 1, "delaySeconds": 3.0 },
      { "attempt": 2, "delaySeconds": 4.5 }
    ],
    "totalWaitSeconds": 9.0
  },
  "delayOption4": {
    "type": "TimeSeries",
    "attempts": [
      { "attempt": 0, "delaySeconds": 1.0 },
      { "attempt": 1, "delaySeconds": 2.0 },
      { "attempt": 2, "delaySeconds": 4.0 }
    ],
    "totalWaitSeconds": 7.0
  },
  "delayOption5": {
    "type": "Periodic",
    "attempts": [
      { "attempt": 0, "delaySeconds": 1.0 },
      { "attempt": 1, "delaySeconds": 3.0 },
      { "attempt": 2, "delaySeconds": 10.0 }
    ],
    "totalWaitSeconds": 14.0
  }
}
```

> Note: `delayOption2` uses jitter, so its values will vary between requests. With a larger `attempts` value (e.g. `attempts=5`), `delayOption5` demonstrates the periodic wrap-around — the sequence `1s, 3s, 10s` repeats as `1s, 3s, 10s, 1s, 3s`, unlike the TimeSeries strategy which would hold the last value.

## Usage

```bash
dotnet run
```

Then hit any endpoint, for example:

```bash
curl http://localhost:5000/retry-analysis?attempts=5
curl http://localhost:5000/periodic-retry
```
