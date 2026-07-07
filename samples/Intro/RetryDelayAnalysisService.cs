using RetryDelays;

namespace Intro;

/// <summary>
/// Analyses all four delay options from <see cref="IRetryConfiguration"/> and produces
/// a per-attempt delay schedule for each strategy over a configurable number of attempts.
/// </summary>
public class RetryDelayAnalysisService
{
    private readonly IRetryConfiguration _config;

    public RetryDelayAnalysisService(IRetryConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Builds a delay schedule for every strategy across <paramref name="attempts"/> attempts.
    /// </summary>
    public RetryDelayAnalysisResult Analyse(int attempts = 5)
    {
        if (attempts < 1) attempts = 1;

        return new RetryDelayAnalysisResult
        {
            DelayOption1 = BuildSchedule("Constant",     new ConstantRetryDelay(_config.DelayOption1),     attempts),
            DelayOption2 = BuildSchedule("Exponential",  new ExponentialRetryDelay(_config.DelayOption2),  attempts),
            DelayOption3 = BuildSchedule("Linear",       new LinearRetryDelay(_config.DelayOption3),       attempts),
            DelayOption4 = BuildSchedule("TimeSeries",   new TimeSeriesRetryDelay(_config.DelayOption4),   attempts),
        };
    }

    private static DelaySchedule BuildSchedule(string type, RetryDelay delay, int attempts)
    {
        var delays = Enumerable.Range(0, attempts)
            .Select(attempt => new AttemptDelay
            {
                Attempt = attempt,
                DelaySeconds = delay.GetDelay(attempt).TotalSeconds
            })
            .ToList();

        return new DelaySchedule
        {
            Type = type,
            Attempts = delays,
            TotalWaitSeconds = delays.Sum(d => d.DelaySeconds)
        };
    }
}

public record RetryDelayAnalysisResult
{
    public DelaySchedule DelayOption1 { get; init; } = new();
    public DelaySchedule DelayOption2 { get; init; } = new();
    public DelaySchedule DelayOption3 { get; init; } = new();
    public DelaySchedule DelayOption4 { get; init; } = new();
}

public record DelaySchedule
{
    public string Type { get; init; } = string.Empty;
    public List<AttemptDelay> Attempts { get; init; } = new();
    public double TotalWaitSeconds { get; init; }
}

public record AttemptDelay
{
    public int Attempt { get; init; }
    public double DelaySeconds { get; init; }
}
