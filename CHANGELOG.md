## 0.3.0

- Add `JitterPercent` property (default 25, valid range 0–100) to `ConstantRetryDelayOptions`, `LinearRetryDelayOptions`, `PeriodicRetryDelayOptions` and `TimeSeriesRetryDelayOptions`, via the new `JitteredRetryDelayOptions` base class. The spread of the standard jitter is now configurable as a percentage of the calculated delay.

## 0.2.0

- Make `RetryDelayOptions.DelayType` public and use it in sample.
- Optimized retry delay hot-path methods with aggressive inlining
- Target net9.0 in RetryDelays.Tests and update NUnit packages.
- Update coverlet.collector package for RetryDelays.Tests.csproj.
- Update Microsoft.NET.Test.Sdk for RetryDelays.Tests.csproj.
- Add CHANGELOG.md.