**RetryDelays** is a lightweight .NET library for generating retry delay strategies.  
Simplifies retry logic by removing the need for manual delay calculations - just use the appropriate `RetryDelay` or `RetryDelayOptions`.
Supports common retry patterns such as **constant**, **linear**, **exponential**, and **time-series delays**, including **jitter** and **decorrelated jitter**.

---
## 🚀 Quick start

Create a retry loop and let `RetryDelay` calculate delays between attempts.

```csharp
var delay = ExponentialRetryDelay.Create(
    baseDelay: TimeSpan.FromMilliseconds(500),
    maxDelay: TimeSpan.FromSeconds(10),
    useJitter: true);

int attempt = 0;

while (attempt <= 10)
{
    try
    {
        var result = func();
        break;
    }
    catch (Exception)
    {
        attempt++;

        var nextDelay = delay.GetDelay(attempt);
        await Task.Delay(nextDelay, token);
    }
}
````
---

## ⭐ Key Features

- ⚙️ Supports **multiple retry delay strategies** - constant, linear, exponential, and time series  
- 🎲 Built-in **jitter support** 
- 📈 **Decorrelated jitter** implementation for exponential backoff, adapted from the [Polly](https://github.com/App-vNext/Polly)  
- 🧩 **Flexible configuration** via `RetryDelayOptions`  
- 🔄 **Implicit conversion** from `RetryDelayOptions` to `RetryDelay`
- 🧱 Ability to create **custom delay strategies** using `Func<int, TimeSpan>`

---

## 💡 Key Concepts

* **`RetryDelay`**: The base class responsible for calculating the exact `TimeSpan` to wait before the next attempt.
* **`RetryDelayOptions`**: Configuration objects (like `LinearRetryDelayOptions` or `ConstantRetryDelayOptions`) used to define the rules of your delay strategy. 
* **Attempts**: Current retry attempt (an integer count starting from 0).

---

## ✨ Available delay types

| Type          | Description                                                                                   |
|---------------|-----------------------------------------------------------------------------------------------|
| **Constant**  | Same delay for every attempt.       |
| **Linear**    | Delay increases linearly: `(attempt + 1) * slopeFactor * baseDelay`.                          |
| **Exponential** | Delay grows exponentially: `baseDelay * (exponentialFactor ^ attempt)`. Decorrelated jitter available. |
| **TimeSeries** | Uses a predefined sequence of delays. Once exhausted, the last value repeats.                |

All types respect the optional `MaxDelay` limit.

---

## 🎲 Jitter Strategies

**Standard Jitter** - Applies ±25% randomization to calculated delays to prevent synchronized retries across distributed systems.

**Decorrelated Jitter** - Available for exponential delays. Uses an advanced formula that produces smoother, more evenly distributed delays with fewer spikes than standard jitter. Based on techniques from [Polly](https://github.com/App-vNext/Polly).

---

## 🛠️ Passing options where a `RetryDelay` is expected

Thanks to implicit conversion, you can write methods that accept a `RetryDelay` but pass configuration options directly:

```csharp
// Method that accepts a RetryDelay
async Task<T> ExecuteWithRetry<T>(
    Func<Task<T>> action,
    RetryDelay retryDelay,
    CancellationToken cancellationToken = default)
{
    int attempt = 0;
    while (true)
    {
        try
        {
            return await action();
        }
        catch
        {
            attempt++;
            var delay = retryDelay.GetDelay(attempt);
            await Task.Delay(delay, cancellationToken);
        }
    }
}

// Call the method with an options object - it is implicitly converted to the corresponding RetryDelay
var options = new ExponentialRetryDelayOptions
{
    BaseDelay = TimeSpan.FromSeconds(1),
    ExponentialFactor = 2.0,
    UseJitter = true,
    MaxDelay = TimeSpan.FromSeconds(30)
};

var result = await ExecuteWithRetry(() => FetchDataAsync(), options);
```

The options are automatically converted to an `ExponentialRetryDelay` instance, so the method receives a fully functional delay calculator.

---

## 💡 Custom Delays

The `RetryDelay` base class is highly flexible. You can even create a delay strategy directly from a simple function:

> **Note**: The `RetryDelay` base class can be instantiated from a `Func<int, TimeSpan>`, allowing for completely custom logic without sub-classing.

```csharp
// Custom logic: delay is always (attempt * 2) seconds, but never more than 1 minute
RetryDelay customDelay = new RetryDelay(attempt => 
    TimeSpan.FromSeconds(Math.Min((attempt + 1) * 2, 60))
);
```

---

## 📌 When to use RetryDelays

`RetryDelays` focuses solely on **delay calculation**, allowing you to integrate it with any retry implementation or resilience framework.

---

## 🧪 Sample

The `samples/` directory contains an ASP.NET Core Minimal API that demonstrates library usage with full configuration support.

Key highlights:

- **`IRetryConfiguration`** — interface that groups four named delay options (`DelayOption1`–`DelayOption4`, covering Constant, Exponential, Linear, and TimeSeries strategies)
- **`RetryConfiguration`** — concrete implementation bound from a single `RetryConfiguration` section in `appsettings.json`
- **`RetryDelayAnalysisService`** — service that accepts `IRetryConfiguration` and produces a per-attempt delay schedule and total wait time for all four strategies
- **`GET /retry-analysis?attempts=N`** — endpoint that runs the analysis and returns the full schedule as JSON

See samples folder for concrete example.

---