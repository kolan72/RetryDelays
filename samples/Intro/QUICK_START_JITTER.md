# Quick Start: JitterPercent Feature

## 🎯 What's New?

The `JitterPercent` property lets you control how much randomization is applied to retry delays. This prevents the "thundering herd" problem where multiple clients retry at the same time.

## ⚡ Quick Examples

### Via Configuration
```json
{
  "BaseDelay": "00:00:02",
  "UseJitter": true,
  "JitterPercent": 15
}
```
Result: Delays will be randomized within ±15% of 2 seconds (1.7s to 2.3s)

### Via Code
```csharp
var options = new ConstantRetryDelayOptions
{
    BaseDelay = TimeSpan.FromSeconds(2),
    UseJitter = true,
    JitterPercent = 15
};
var delay = new ConstantRetryDelay(options);
```

### Via Factory Method
```csharp
var delay = ConstantRetryDelay.Create(
    baseDelay: TimeSpan.FromSeconds(2),
    useJitter: true,
    jitterPercent: 15
);
```

## 📊 Sample Configuration Overview

The Intro sample uses these values to demonstrate different scenarios:

```
┌──────────────┬────────────────┬─────────────────────────────┐
│ Delay Type   │ JitterPercent  │ Purpose                     │
├──────────────┼────────────────┼─────────────────────────────┤
│ Constant     │ 15%            │ Conservative jitter         │
│ Linear       │ 10%            │ Minimal jitter              │
│ TimeSeries   │ 30%            │ Moderate load spreading     │
│ Periodic     │ 20%            │ Balanced cycling delays     │
│ Exponential  │ N/A            │ Uses decorrelated jitter    │
└──────────────┴────────────────┴─────────────────────────────┘
```

## 🧪 Try It Now

1. Start the sample:
   ```bash
   cd samples/Intro
   dotnet run
   ```

2. Call an endpoint multiple times to see jitter in action:
   ```bash
   # Linux/Mac
   for i in {1..5}; do curl http://localhost:5000/constant-retry; echo ""; done
   
   # Windows PowerShell
   1..5 | ForEach-Object { (Invoke-WebRequest http://localhost:5000/constant-retry).Content }
   ```

3. Example output showing randomized delays:
   ```json
   {"type":"Constant","baseDelay":"00:00:02","useJitter":true,"jitterPercent":15,"maxDelay":"00:00:30","nextDelay":"00:00:02.1234567"}
   {"type":"Constant","baseDelay":"00:00:02","useJitter":true,"jitterPercent":15,"maxDelay":"00:00:30","nextDelay":"00:00:01.8765432"}
   {"type":"Constant","baseDelay":"00:00:02","useJitter":true,"jitterPercent":15,"maxDelay":"00:00:30","nextDelay":"00:00:02.0543210"}
   ```

## 📐 Choosing the Right Value

| Value Range | When to Use |
|-------------|-------------|
| **0-10%** | Need predictable timing (real-time systems, strict SLAs) |
| **10-25%** | General purpose (balanced randomization) ⭐ **Recommended** |
| **25-50%** | High-traffic distributed systems |
| **50-100%** | Extreme scenarios (rarely needed) |

## ✅ Rules to Remember

1. **Valid range**: 0 to 100
2. **Default value**: 25 (±25%)
3. **Only works with**: `UseJitter: true`
4. **Available for**: Constant, Linear, TimeSeries, Periodic
5. **NOT available for**: Exponential (uses decorrelated jitter)

## 🚫 Common Mistakes

❌ **Wrong**: Setting JitterPercent without UseJitter
```csharp
// This will throw ArgumentException
var delay = ConstantRetryDelay.Create(
    TimeSpan.FromSeconds(2),
    jitterPercent: 10  // ERROR: useJitter must be true!
);
```

✅ **Correct**: Enable UseJitter when using JitterPercent
```csharp
var delay = ConstantRetryDelay.Create(
    TimeSpan.FromSeconds(2),
    useJitter: true,
    jitterPercent: 10  // OK
);
```

---

❌ **Wrong**: Invalid percentage values
```csharp
options.JitterPercent = -5;   // ERROR: can't be negative
options.JitterPercent = 150;  // ERROR: can't exceed 100
```

✅ **Correct**: Use values between 0 and 100
```csharp
options.JitterPercent = 0;    // OK: no randomization
options.JitterPercent = 25;   // OK: default
options.JitterPercent = 100;  // OK: maximum spread
```

## 📚 More Information

- **Full examples**: See `JITTER_EXAMPLES.md`
- **Complete changes**: See `CHANGES_SUMMARY.md`
- **API reference**: See `README.md`

## 🎉 That's It!

You're now ready to use the `JitterPercent` feature in your retry strategies. Start with the default (25%) and adjust based on your needs.

Happy retrying! 🔄
