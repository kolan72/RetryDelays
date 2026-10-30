using System;

namespace RetryDelays
{
	/// <summary>
	/// Class to get the delay value calculated linearly.
	/// </summary>
	public sealed class LinearRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="LinearRetryDelayOptions"/></param>
		public LinearRetryDelay(LinearRetryDelayOptions retryDelayOptions) : base(new LinearDelayCore(retryDelayOptions))
		{
		}

		/// <summary>
		///  Creates <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">Maximum delay value. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <param name="jitterPercent">Jitter spread as a percentage of the calculated delay (0–100). If null, the default of 25 is used. Only valid when <paramref name="useJitter"/> is true.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="jitterPercent"/> is specified without <paramref name="useJitter"/> being true.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="jitterPercent"/> is negative or greater than 100.</exception>
		public static LinearRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false, double? jitterPercent = null) => new LinearRetryDelay(baseDelay, maxDelay: maxDelay, useJitter: useJitter, jitterPercent: jitterPercent);

		/// <summary>
		///  Creates <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="slopeFactor">Slope factor to use.</param>
		/// <param name="maxDelay">Maximum delay value. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <param name="jitterPercent">Jitter spread as a percentage of the calculated delay (0–100). If null, the default of 25 is used. Only valid when <paramref name="useJitter"/> is true.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="jitterPercent"/> is specified without <paramref name="useJitter"/> being true.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="jitterPercent"/> is negative or greater than 100.</exception>
		public static LinearRetryDelay Create(TimeSpan baseDelay, double slopeFactor, TimeSpan? maxDelay = null, bool useJitter = false, double? jitterPercent = null) => new LinearRetryDelay(baseDelay, slopeFactor, maxDelay, useJitter, jitterPercent);

		internal LinearRetryDelay(TimeSpan baseDelay, double slopeFactor = RetryDelayConstants.SlopeFactor, TimeSpan? maxDelay = null, bool useJitter = false, double? jitterPercent = null) :
			 this(new LinearRetryDelayOptions() { BaseDelay = baseDelay, SlopeFactor = slopeFactor, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue, JitterPercent = JitterPercentResolver.Resolve(jitterPercent, useJitter, nameof(jitterPercent)) })
		{ }
	}
}
