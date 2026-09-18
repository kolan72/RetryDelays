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