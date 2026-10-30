using System;

namespace RetryDelays
{
	/// <summary>
	/// Class to get the constant delay value.
	/// </summary>
	public sealed class ConstantRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ConstantRetryDelayOptions"/></param>
		public ConstantRetryDelay(ConstantRetryDelayOptions retryDelayOptions) : base(new ConstantDelayCore(retryDelayOptions))
		{
			if (retryDelayOptions.UseJitter && retryDelayOptions.MaxDelay < retryDelayOptions.BaseDelay)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelayOptions), "MaxDelay must be greater than or equal to BaseDelay.");
			}
		}

		/// <summary>
		/// Creates <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">Maximum delay value. Only used if <paramref name="useJitter"/> is true. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <param name="jitterPercent">Jitter spread as a percentage of the calculated delay (0–100). If null, the default of 25 is used. Only valid when <paramref name="useJitter"/> is true.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="jitterPercent"/> is specified without <paramref name="useJitter"/> being true.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="jitterPercent"/> is negative or greater than 100.</exception>
		public static ConstantRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false, double? jitterPercent = null) => new ConstantRetryDelay(baseDelay, maxDelay, useJitter, jitterPercent);

		internal ConstantRetryDelay(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false, double? jitterPercent = null) : this(new ConstantRetryDelayOptions() { BaseDelay = baseDelay, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue, JitterPercent = JitterPercentResolver.Resolve(jitterPercent, useJitter, nameof(jitterPercent)) }) { }
	}
}
