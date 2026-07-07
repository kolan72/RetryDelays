using RetryDelays;

namespace Intro;

/// <summary>
/// Concrete implementation of <see cref="IRetryConfiguration"/>.
/// Bound from the "RetryConfiguration" section of appsettings.json as a whole object.
/// </summary>
public class RetryConfiguration : IRetryConfiguration
{
    /// <inheritdoc/>
    public ConstantRetryDelayOptions DelayOption1 { get; set; } = new();

    /// <inheritdoc/>
    public ExponentialRetryDelayOptions DelayOption2 { get; set; } = new();

    /// <inheritdoc/>
    public LinearRetryDelayOptions DelayOption3 { get; set; } = new();

    /// <inheritdoc/>
    public TimeSeriesRetryDelayOptions DelayOption4 { get; set; } = new();
}
