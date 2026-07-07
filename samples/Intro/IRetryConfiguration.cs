using RetryDelays;

namespace Intro;

/// <summary>
/// Holds four named retry delay option sets that together describe a full retry configuration.
/// </summary>
public interface IRetryConfiguration
{
    /// <summary>First delay option — constant delay.</summary>
    ConstantRetryDelayOptions DelayOption1 { get; }

    /// <summary>Second delay option — exponential delay.</summary>
    ExponentialRetryDelayOptions DelayOption2 { get; }

    /// <summary>Third delay option — linear delay.</summary>
    LinearRetryDelayOptions DelayOption3 { get; }

    /// <summary>Fourth delay option — time-series delay.</summary>
    TimeSeriesRetryDelayOptions DelayOption4 { get; }
}
