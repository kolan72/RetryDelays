# JitterPercent Feature Examples

This document demonstrates the usage of the new `JitterPercent` parameter added to the RetryDelays library.

## Overview

The `JitterPercent` property allows you to control the spread of standard jitter as a percentage of the calculated delay. This helps prevent synchronized retries across distributed systems while giving you fine-grained control over the randomization range.

## Key Features

- **Default value**: 25 (±25% of the calculated delay)
- **Valid range**: 0–100
- **Applies to**: Constant, Linear, TimeSeries, and Periodic delay types
- **Only active when**: `UseJitter` is `true`

> **Note**: Exponential delays use decorrelated jitter and do not have a `JitterPercent` property.

## Configuration Examples

### Via Options Classes

```csharp
// Constant delay with 15% jitter
var constantOptions = new ConstantRetryDelayOptions
{
    BaseDelay = TimeSpan.FromSeconds(2),
    UseJitter = true,
    JitterPercent = 15,  // Randomizes delays within ±15% (1.7s–2.3s)
    MaxDelay = TimeSpan.FromSeconds(30)
};
var constantDelay = new ConstantRetryDelay(constantOptions);

// Linear delay with 10% jitter
var linearOptions = new LinearRetryDelayOptions
{
    BaseDelay = TimeSpan.FromSeconds(1),
    SlopeFactor = 1.5,
    UseJitter = true,
    JitterPercent = 10,  // Randomizes delays within ±10%
    MaxDelay = TimeSpan.FromSeconds(30)
};
var linearDelay = new LinearRetryDelay(linearOptions);

// TimeSeries delay with 30% jitter
var timeSeriesOptions = new TimeSeriesRetryDelayOptions
{
    BaseDelay = TimeSpan.Zero,
    UseJitter = true,
    JitterPercent = 30,  // Randomizes each time value within ±30%
    MaxDelay = TimeSpan.FromMinutes(1),
    Times = new[]
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4),
        TimeSpan.FromSeconds(8)
    }
};
var timeSeriesDelay = new TimeSeriesRetryDelay(timeSeriesOptions);

// Periodic delay with 20% jitter
var periodicOptions = new PeriodicRetryDelayOptions
{
    BaseDelay = TimeSpan.Zero,
    UseJitter = true,
    JitterPercent = 20,  // Randomizes each cycled delay within ±20%
    MaxDelay = TimeSpan.FromMinutes(1),
    Times = new[]
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(10)
    }
};
var periodicDelay = new PeriodicRetryDelay(periodicOptions);
```

### Via Factory Methods

```csharp
// Constant delay with custom jitter percent
var delay = ConstantRetryDelay.Create(
    baseDelay: TimeSpan.FromSeconds(2),
    useJitter: true,
    jitterPercent: 10  // ±10% randomization (1.8s–2.2s)
);

// Zero jitter (no randomization, but jitter is technically enabled)
var exactDelay = ConstantRetryDelay.Create(
    baseDelay: TimeSpan.FromMilliseconds(500),
    useJitter: true,
    jitterPercent: 0  // No randomization (exactly 500ms)
);
```

### Via Configuration (appsettings.json)

```json
{
  "RetryDelays": {
    "Constant": {
      "BaseDelay": "00:00:02",
      "UseJitter": true,
      "JitterPercent": 15,
      "MaxDelay": "00:00:30"
    },
    "Linear": {
      "BaseDelay": "00:00:01",
      "UseJitter": true,
      "JitterPercent": 10,
      "MaxDelay": "00:00:30",
      "SlopeFactor": 1.5
    },
    "TimeSeries": {
      "BaseDelay": "00:00:00",
      "UseJitter": true,
      "JitterPercent": 30,
      "MaxDelay": "00:01:00",
      "Times": ["00:00:01", "00:00:02", "00:00:04", "00:00:08"]
    },
    "Periodic": {
      "BaseDelay": "00:00:00",
      "UseJitter": true,
      "JitterPercent": 20,
      "MaxDelay": "00:01:00",
      "Times": ["00:00:01", "00:00:03", "00:00:10"]
    }
  }
}
```

## Validation Rules

The `JitterPercent` property includes built-in validation:

```csharp
// ✅ Valid values
options.JitterPercent = 0;    // No jitter
options.JitterPercent = 25;   // Default (±25%)
options.JitterPercent = 50;   // Wide spread (±50%)
options.JitterPercent = 100;  // Maximum spread (±100%)

// ❌ Invalid values (throw ArgumentOutOfRangeException)
options.JitterPercent = -1;   // Negative values not allowed
options.JitterPercent = 101;  // Values over 100 not allowed
```

## Factory Method Rules

When using the `Create` factory method:

```csharp
// ✅ Valid: jitterPercent with useJitter=true
var delay1 = ConstantRetryDelay.Create(
    TimeSpan.FromSeconds(2),
    useJitter: true,
    jitterPercent: 10
);

// ✅ Valid: jitterPercent=null with useJitter=false (treated as omitted)
var delay2 = ConstantRetryDelay.Create(
    TimeSpan.FromSeconds(2),
    useJitter: false,
    jitterPercent: null
);

// ❌ Invalid: jitterPercent without useJitter=true (throws ArgumentException)
var delay3 = ConstantRetryDelay.Create(
    TimeSpan.FromSeconds(2),
    jitterPercent: 10  // ERROR: useJitter must be true
);
```

## Practical Examples

### Example 1: Conservative Jitter (±10%)

Good for systems that need predictable timing with minimal variation:

```csharp
var options = new ConstantRetryDelayOptions
{
    BaseDelay = TimeSpan.FromSeconds(5),
    UseJitter = true,
    JitterPercent = 10  // Delays will range from 4.5s to 5.5s
};
```

### Example 2: Default Jitter (±25%)

Balanced approach for most use cases:

```csharp
var options = new LinearRetryDelayOptions
{
    BaseDelay = TimeSpan.FromSeconds(2),
    SlopeFactor = 1.5,
    UseJitter = true
    // JitterPercent defaults to 25 (±25%)
    // First attempt: 2s ± 25% = 1.5s–2.5s
    // Second attempt: 3.5s ± 25% = 2.625s–4.375s
};
```

### Example 3: Aggressive Jitter (±50%)

Maximum randomization to prevent thundering herd:

```csharp
var options = new TimeSeriesRetryDelayOptions
{
    BaseDelay = TimeSpan.Zero,
    UseJitter = true,
    JitterPercent = 50,  // Wide spread to distribute load
    Times = new[]
    {
        TimeSpan.FromSeconds(1),   // Will vary 0.5s–1.5s
        TimeSpan.FromSeconds(5),   // Will vary 2.5s–7.5s
        TimeSpan.FromSeconds(10)   // Will vary 5s–15s
    }
};
```

## Testing in the Intro Sample

The Intro sample project demonstrates JitterPercent usage across all supported delay types. Each configuration uses different jitter percentages to show the variety:

- **Constant**: 15% jitter (±15%)
- **Linear**: 10% jitter (±10%)
- **TimeSeries**: 30% jitter (±30%)
- **Periodic**: 20% jitter (±20%)

### Try It Out

1. Start the sample:
   ```bash
   cd samples/Intro
   dotnet run
   ```

2. Test different endpoints to see jitter in action:
   ```bash
   # Constant delay with 15% jitter
   curl http://localhost:5000/constant-retry
   
   # Linear delay with 10% jitter
   curl http://localhost:5000/linear-retry
   
   # TimeSeries delay with 30% jitter
   curl http://localhost:5000/timeseries-retry
   
   # Periodic delay with 20% jitter (shows cycling behavior)
   curl http://localhost:5000/periodic-retry
   
   # Full analysis of all strategies
   curl http://localhost:5000/retry-analysis?attempts=5
   ```

3. Call the same endpoint multiple times to observe jitter variations:
   ```bash
   # Run 5 times to see different jittered values
   for i in {1..5}; do curl http://localhost:5000/constant-retry; echo ""; done
   ```

## Why Use Different JitterPercent Values?

Different scenarios call for different jitter strategies:

| JitterPercent | Use Case |
|---------------|----------|
| **0–10%** | Systems requiring tight timing control (e.g., real-time systems, strict SLAs) |
| **15–25%** | General purpose applications (good balance of predictability and randomization) |
| **30–50%** | High-traffic distributed systems prone to thundering herd problems |
| **50–100%** | Extreme load spreading scenarios (rarely needed, can make delays unpredictable) |

## Related Classes

- `JitteredRetryDelayOptions` - Base class that provides the `JitterPercent` property
- `StandardJitter` - Internal class that performs the jitter calculation
- `JitterPercentResolver` - Internal helper for factory method validation

## See Also

- Main README: `README.md`
- Configuration examples: `appsettings.json`
- API endpoints: `Program.cs`
