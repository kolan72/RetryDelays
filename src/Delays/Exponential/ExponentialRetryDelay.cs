using System;

namespace RetryDelays
{
	/// <summary>
	///  Class to get the delay value calculated exponentially.
	/// </summary>
	public sealed class ExponentialRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ExponentialRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ExponentialRetryDelayOptions"/></param>
		public ExponentialRetryDelay(ExponentialRetryDelayOptions retryDelayOptions): base(new ExponentialDelayCore(retryDelayOptions))
		{
		}

		/// <summary>
		/// Creates <see cref="ExponentialRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">>Maximum delay value. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static ExponentialRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) => new ExponentialRetryDelay(baseDelay, maxDelay: maxDelay, useJitter: useJitter);

		/// <summary>
		/// Creates <see cref="ExponentialRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="exponentialFactor">Exponential factor to use.</param>
		/// <param name="maxDelay">>Maximum delay value.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static ExponentialRetryDelay Create(TimeSpan baseDelay, double exponentialFactor, TimeSpan? maxDelay = null, bool useJitter = false) => new ExponentialRetryDelay(baseDelay, exponentialFactor, maxDelay, useJitter);

		internal ExponentialRetryDelay(TimeSpan baseDelay, double exponentialFactor = RetryDelayConstants.ExponentialFactor, TimeSpan? maxDelay = null, bool useJitter = false) :
			this(new ExponentialRetryDelayOptions() { BaseDelay = baseDelay, ExponentialFactor = exponentialFactor, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue })
		{ }

	}
}
