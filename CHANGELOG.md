## 0.5.0

- Add configurable `JitterPercent` to standard-jitter delays.
- New `JitteredRetryDelayOptions` base class (default 25%, range 0–100).
- Constant/Linear/Periodic/TimeSeries options inherit it; Exponential unchanged.
- Parameterize `StandardJitter`; update cores, tests, README and samples.
- All `Create` methods of `ConstantRetryDelay`, `LinearRetryDelay`, `PeriodicRetryDelay` and `TimeSeriesRetryDelay` now accept an optional trailing `jitterPercent` parameter (0–100, `null` = default 25) to configure the standard-jitter spread directly from the factory API. Specifying it without `useJitter: true` throws `ArgumentException`; out-of-range values throw `ArgumentOutOfRangeException`. **Breaking:** the `Create` signatures changed.
- Demonstrate `JitterPercent` feature in Intro sample
- Update Microsoft.NET.Test.Sdk for RetryDelays.Tests.csproj.
- Update NUnit3TestAdapter.
- Update coverlet.collector package for RetryDelays.Tests.csproj.
- Add NuGet package badge and logo to README.


## 0.3.0

- Add Periodic retry delay strategy that cycles through times array.
- Fix off-by-one in retry loop examples: call `GetDelay` before incrementing attempt.
- Update NUnit packages.
- Demonstrate PeriodicRetryDelay in Intro sample.
- Update Microsoft.NET.Test.Sdk for RetryDelays.Tests.csproj.
- Update README and NuGet docs with Periodic retry delay.


## 0.2.0

- Make `RetryDelayOptions.DelayType` public and use it in sample.
- Optimized retry delay hot-path methods with aggressive inlining
- Target net9.0 in RetryDelays.Tests and update NUnit packages.
- Update coverlet.collector package for RetryDelays.Tests.csproj.
- Update Microsoft.NET.Test.Sdk for RetryDelays.Tests.csproj.
- Add CHANGELOG.md.